using System;
using Godot;

/// <summary>
/// Player character logic.
/// </summary>
public partial class Player : CharacterBody2D, IKillable
{
    private bool _playerRotationControllerByMouse;
    private Vector2 _inputVelocityDirection;

    private float PlayerWalkingSpeed { get; set; } = 400;

    public Area2D DamageHitbox { get; private set; } = null!;

    public override void _Ready()
    {
        if (PlayerData.PlayerInstance != this) PlayerData.PlayerInstance = this;

        DamageHitbox = GetNode<Area2D>("DamageHitbox");
        ArgumentNullException.ThrowIfNull(DamageHitbox);
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

        Velocity = _inputVelocityDirection * PlayerWalkingSpeed;
        
        if (_playerRotationControllerByMouse) RotatePlayerByMouse();
        else RotatePlayerByJoystick();
    }

    public void TakeDamage(int damage) => PlayerData.ReduceHealth(damage);

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
        var mousePosOnScreen = GetGlobalMousePosition();
        var cameraVector = mousePosOnScreen - playerPosOnScreen;

        Rotation = -cameraVector.AngleTo(Vector2.Up);
    }
}