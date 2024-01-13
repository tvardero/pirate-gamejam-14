using Godot;

/// <summary>
/// Something that has a hitbox and could take damage.
/// </summary>
public interface IKillable
{
    /// <summary>
    /// Hitbox to collide with when trying to damage the entity.
    /// </summary>
    Area2D DamageHitbox { get; }
    
    /// <summary>
    /// Translates damage to the entity. Negative values are allowed and have meaning of healing.
    /// </summary>
    /// <param name="damage">Amount of damage to inflict.</param>
    void TakeDamage(int damage);
}