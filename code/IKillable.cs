/// <summary>
/// Something that has a hitbox and could take damage.
/// </summary>
public interface IKillable
{
    /// <summary>
    /// Translates damage to the entity. Negative values are allowed and have meaning of healing.
    /// </summary>
    /// <param name="damage">Amount of damage to inflict.</param>
    void TakeDamage(int damage);
}