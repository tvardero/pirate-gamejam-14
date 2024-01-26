using Godot;

/// <summary>
/// Something that has a hitbox and could be interacted with.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Hitbox to collide with when trying to interact with the entity.
    /// </summary>
    Area2D InteractivityHitbox { get; } 

    /// <summary>
    /// Interact with the entity.
    /// </summary>
    /// <param name="initiator">The object that tries to interact with the entity.</param>
    void Interact(Node initiator);
}