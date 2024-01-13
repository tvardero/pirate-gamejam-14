using System;

/// <summary>
/// Global player's data.
/// </summary>
public static class PlayerData
{
    private static float _currentHealth;
    private static float _maxHealth;

    /// <summary>
    /// Instance of player scene.
    /// </summary>
    public static Player? PlayerInstance { get; set; }
    
    /// <summary>
    /// Current player health.
    /// </summary>
    public static float CurrentHealth => _currentHealth;

    /// <summary>
    /// Maximum possible player health.
    /// </summary>
    public static float MaxHealth => _maxHealth;

    /// <summary>
    /// Flag indicating that player health less than or equal to zero.
    /// </summary>
    public static bool IsDead => _currentHealth <= 0;

    /// <summary>
    /// Event that is fired when player's health reaches zero.
    /// </summary>
    public static event Action? PlayerDied;

    /// <summary>
    /// Sets current player health amount.
    /// </summary>
    /// <param name="newCurrentHealth">New current amount.</param> 
    public static void SetCurrentHealth(float newCurrentHealth)
    {
        if (newCurrentHealth > _maxHealth)
            _currentHealth = _maxHealth;
        else if (newCurrentHealth <= 0)
        {
            _currentHealth = 0;
            PlayerDied?.Invoke();
        }
        else
            _currentHealth = newCurrentHealth;
    }

    /// <summary>
    /// Sets maximum player health amount. If player currently has more than new maximum amount - player health is reduced automatically.
    /// </summary>
    /// <param name="newMaxHealth">New maximum amount.</param>
    /// <exception cref="ArgumentException">Maximum health was less than zero.</exception>
    public static void SetMaxHealth(float newMaxHealth)
    {
        if (newMaxHealth <= 0) throw new ArgumentException("Max Health can not be less than zero");

        _maxHealth = newMaxHealth;
        if (_currentHealth > newMaxHealth) _currentHealth = _maxHealth;
    }

    /// <summary>
    /// Translates damage to the player. Negative values are allowed and have meaning of healing.
    /// </summary>
    /// <param name="damage">Amount of damage to inflict.</param>
    public static void ReduceHealth(float damage)
    {
        if (damage == 0) return;

        SetCurrentHealth(_currentHealth - damage);
    }
}