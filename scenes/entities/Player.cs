using System;
using System.Linq;
using Godot;

/// <summary>
/// Player character logic.
/// </summary>
public partial class Player : CharacterBody2D
{
    private bool _playerRotationControllerByMouse;
    private Vector2 _inputVelocityDirection;
    private float _sprintStrength;
    private RayCast2D _interactRay = null!;

    [Export]
    private float PlayerWalkingSpeed { get; set; } = 400;

    [Export]
    private float PlayerSprintingSpeed { get; set; } = 800;

    public Area2D DamageHitbox { get; private set; } = null!;

    public override void _Ready()
    {
        DamageHitbox = GetNode<Area2D>("DamageHitbox");
        ArgumentNullException.ThrowIfNull(DamageHitbox);

        _interactRay = GetNode<RayCast2D>("InteractRayCast");
        ArgumentNullException.ThrowIfNull(_interactRay);
    }

    public override void _UnhandledInput(InputEvent input)
    {
        var inputIsHandled = false;

        // change camera rotation input device
        if (input is InputEventMouse) { _playerRotationControllerByMouse = true; }
        else if (input is InputEventJoypadMotion or InputEventJoypadButton) { _playerRotationControllerByMouse = false; }

        // process inputs like motion, jumping, shooting, etc... 
        if (IsMotionAction(input))
        {
            var x = Input.GetAxis(InputActionNames.MoveLeft, InputActionNames.MoveRight);
            var y = Input.GetAxis(InputActionNames.MoveUp, InputActionNames.MoveDown);
            _inputVelocityDirection = new Vector2(x, y);

            // on keyboards with x=1 and y=1 we will get 1.4142 (= sqrt of 2) vector length and player might move faster diagonally 
            // on joysticks we are never out of 1 radius circle
            if (_inputVelocityDirection.Length() > 1) _inputVelocityDirection = _inputVelocityDirection.Normalized();

            inputIsHandled = true;
        }

        else if (input.IsAction(InputActionNames.Sprint))
        {
            _sprintStrength = input.GetActionStrength(InputActionNames.Sprint);
            inputIsHandled = true;
        }

        else if (input.IsActionPressed(InputActionNames.Attack))
        {
            // TODO
            inputIsHandled = true;
        }

        else if (input.IsActionPressed(InputActionNames.PrimaryAction))
        {
            TryInteract(InputActionNames.PrimaryAction);
            inputIsHandled = true;
        }

        else if (input.IsActionPressed(InputActionNames.SecondaryAction))
        {
            TryInteract(InputActionNames.SecondaryAction);
            inputIsHandled = true;
        }

        // mark input as handled so other scenes dont waste computing time on handling it again
        if (inputIsHandled) GetTree().Root.SetInputAsHandled();

        return;

        static bool IsMotionAction(InputEvent input)
        {
            return input.IsAction(InputActionNames.MoveDown)
                || input.IsAction(InputActionNames.MoveUp)
                || input.IsAction(InputActionNames.MoveLeft)
                || input.IsAction(InputActionNames.MoveRight);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndSlide();

        Velocity = CalculateVelocity();

        if (_playerRotationControllerByMouse)
            RotatePlayerByMouse();
        else
            RotatePlayerByJoystick();

        return;

        Vector2 CalculateVelocity()
        {
            if (_sprintStrength <= 0) return _inputVelocityDirection * PlayerWalkingSpeed;

            return _inputVelocityDirection.Normalized() * (PlayerWalkingSpeed + (PlayerSprintingSpeed - PlayerWalkingSpeed) * _sprintStrength);
        }
    }

    private void RotatePlayerByJoystick()
    {
        var x = Input.GetAxis(InputActionNames.JoystickCameraLeft, InputActionNames.JoystickCameraRight);
        var y = Input.GetAxis(InputActionNames.JoystickCameraUp, InputActionNames.JoystickCameraDown);
        var cameraVector = new Vector2(x, y);

        if (cameraVector.Length() < 0.35) return; // dont reset camera angle when we release left joystick

        Rotation = -cameraVector.AngleTo(Vector2.Up);
    }

    private void RotatePlayerByMouse()
    {
        var playerPosOnScreen = GetGlobalTransformWithCanvas().Origin;
        var mousePosOnScreen = GetViewport().GetMousePosition();
        var cameraVector = mousePosOnScreen - playerPosOnScreen;

        Rotation = -cameraVector.AngleTo(Vector2.Up);
    }

    private void TryInteract(string method)
    {
        var collision = _interactRay.GetCollider();
        if (collision is Area2D area2D)
        {
            var areaParent = area2D.GetParent();
            if (areaParent is IInteractable interactable && interactable.AvailableActionMethods.Contains(method))
            {
                interactable.Interact(this, method);
            }
        }
    }
}