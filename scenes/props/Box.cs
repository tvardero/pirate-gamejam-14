using System;
using Godot;

public partial class Box : StaticBody2D, IInteractable, IKillable
{
    private Area2D _hitbox = null!;

    public Area2D InteractivityHitbox => _hitbox;
    public string[] AvailableActionMethods { get; } = { InputActionNames.PrimaryAction, InputActionNames.SecondaryAction };

    public Area2D DamageHitbox => _hitbox;

    public override void _Ready()
    {
        _hitbox = GetNode<Area2D>("Hitbox");
        ArgumentNullException.ThrowIfNull(_hitbox);
    }

    public void Interact(Node initiator, string? interactionMethod = null)
    {
        switch (interactionMethod)
        {
            case InputActionNames.PrimaryAction when initiator is Player player:
                // for testing only: on primary move the box 100px away from player
                Position += (Position - player.Position).Normalized() * 100;
                break;

            case InputActionNames.SecondaryAction:
                // for testing only: remove the box from level on secondary action
                QueueFree();
                break;

            case null: return;

            default: throw new ArgumentException("Unrecognized interaction method", nameof(interactionMethod));
        }
    }

    public void TakeDamage(int damage) => QueueFree();
}