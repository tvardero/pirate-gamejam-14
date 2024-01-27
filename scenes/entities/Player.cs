using System;
using System.Linq;
using Gamejam.code;
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
    private AnimationTree _animationTree = null!;

    [Export]
    private float PlayerWalkingSpeed { get; set; } = 400;

    [Export]
    private float PlayerSprintingSpeed { get; set; } = 800;

    public Area2D DamageHitbox { get; private set; } = null!;

    public float PlayerRotation { get; set; }

    public override void _Ready()
    {
        _interactRay = GetNode<RayCast2D>("InteractRayCast");
        ArgumentNullException.ThrowIfNull(_interactRay);

        _animationTree = GetNode<AnimationTree>("AnimationTree");
        ArgumentNullException.ThrowIfNull(_animationTree);
    }

    public override void _UnhandledInput(InputEvent input)
    {
        var inputIsHandled = false;

        // change camera rotation input device
        if (input is InputEventMouse)
        {
            _playerRotationControllerByMouse = true;
        }
        else if (input is InputEventJoypadMotion or InputEventJoypadButton)
        {
            _playerRotationControllerByMouse = false;
        }

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
            var interacted = TryGetWater();
            if (!interacted) UseBucket();
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

        var walking = _inputVelocityDirection.LengthSquared() > 0.01f;
        var water = GameData.LevelData?.BucketAmmo > 0;

        _animationTree.Set("parameters/conditions/idle", !walking);
        _animationTree.Set("parameters/conditions/walking", walking);
        _animationTree.Set("parameters/conditions/water", water);
        _animationTree.Set("parameters/conditions/dry", !water);

        if (_inputVelocityDirection.LengthSquared() <= 0.01f) return;
        
        var blendPos = _inputVelocityDirection.Normalized();
        _animationTree.Set("parameters/idle/blend_position", blendPos);
        _animationTree.Set("parameters/idle_water/blend_position", blendPos);
        _animationTree.Set("parameters/walking/blend_position", blendPos);
        _animationTree.Set("parameters/walking_water/blend_position", blendPos);

        _interactRay.TargetPosition = blendPos * 200f;

        return;

        Vector2 CalculateVelocity()
        {
            if (_sprintStrength <= 0) return _inputVelocityDirection * PlayerWalkingSpeed;

            return _inputVelocityDirection.Normalized() *
                   (PlayerWalkingSpeed + (PlayerSprintingSpeed - PlayerWalkingSpeed) * _sprintStrength);
        }
    }

    private bool TryGetWater()
    {
        var collision = _interactRay.GetCollider();
        if (collision is not Area2D) return false;

        if (GameData.LevelData != null) GameData.LevelData.BucketAmmo = LevelData.MaxBucketAmmo;

        return true;
    }

    private void UseBucket()
    {
        if (GameData.LevelData == null) return;
        if (GameData.LevelData.BucketAmmo <= 0) return;

        GameData.LevelData.BucketAmmo--;
    }
}