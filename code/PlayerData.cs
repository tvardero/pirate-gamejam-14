using System;

public static class PlayerData
{
    private const float FloatComparisonPrecision = 0.001f;
    private static float _currentHealth;
    private static float _maxHealth;

    public static Player? PlayerInstance { get; set; }
    
    public static float CurrentHealth => _currentHealth;

    public static float MaxHealth => _maxHealth;

    public static bool IsDead => _currentHealth <= 0;

    public static void SetCurrentHealth(float newCurrentHealth)
    {
        if (newCurrentHealth > _maxHealth)
            _currentHealth = _maxHealth;
        else if (newCurrentHealth <= 0)
            _currentHealth = 0;
        else
            _currentHealth = newCurrentHealth;
    }

    public static void SetMaxHealth(float newMaxHealth)
    {
        if (newMaxHealth <= 0) throw new ArgumentException("Max Health can not be less than zero");

        _maxHealth = newMaxHealth;
        if (_currentHealth > newMaxHealth) _currentHealth = _maxHealth;
    }

    public static void ReduceHealth(float damage)
    {
        if (damage == 0) return;

        SetCurrentHealth(_currentHealth - damage);
    }
}