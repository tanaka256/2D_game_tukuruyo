using UnityEngine;

public enum PrototypeFaction
{
    Player,
    Enemy,
    Neutral
}

/// <summary>
/// Shared health and damage receiver for players and enemies.
/// </summary>
public sealed class PrototypeHealth : MonoBehaviour
{
    [Min(1)]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private PrototypeFaction faction = PrototypeFaction.Neutral;

    private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public PrototypeFaction Faction => faction;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, GameObject source = null)
    {
        if (IsDead || damage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        SendMessage("OnPrototypeDamaged", damage, SendMessageOptions.DontRequireReceiver);

        if (currentHealth == 0)
        {
            SendMessage("OnPrototypeDeath", source, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
