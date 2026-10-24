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
    [Min(0f)]
    [SerializeField] private float playerInvincibilityDuration = 1f;
    [Min(0)]
    [SerializeField] private int scoreValue = 100;

    private int currentHealth;
    private float invincibleUntil;
    private PrototypeHitFlash hitFlash;
    private PrototypeAudioEvents audioEvents;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public PrototypeFaction Faction => faction;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        hitFlash = GetComponent<PrototypeHitFlash>();
        if (hitFlash == null)
        {
            hitFlash = gameObject.AddComponent<PrototypeHitFlash>();
        }
        audioEvents = GetComponent<PrototypeAudioEvents>();
        if (audioEvents == null)
        {
            audioEvents = gameObject.AddComponent<PrototypeAudioEvents>();
        }
    }

    public void TakeDamage(int damage, GameObject source = null)
    {
        if (IsDead || damage <= 0 || (faction == PrototypeFaction.Player && Time.time < invincibleUntil))
        {
            return;
        }

        if (faction == PrototypeFaction.Player)
        {
            invincibleUntil = Time.time + playerInvincibilityDuration;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        hitFlash.Play();
        if (faction == PrototypeFaction.Player)
        {
            audioEvents.PlayPlayerHit();
        }
        else
        {
            audioEvents.PlayEnemyHit();
        }
        SendMessage("OnPrototypeDamaged", damage, SendMessageOptions.DontRequireReceiver);

        if (currentHealth == 0)
        {
            audioEvents.PlayDeath();
            if (faction == PrototypeFaction.Player)
            {
                PrototypeGameUI.GameOver("GAME OVER");
            }
            else if (faction == PrototypeFaction.Enemy)
            {
                PrototypeGameUI.AddScore(scoreValue);
            }
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
