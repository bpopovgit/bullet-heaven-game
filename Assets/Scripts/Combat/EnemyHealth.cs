using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private const int DefaultPointsOnDeath = 10;

    [Header("Health")]
    [SerializeField] private int maxHealth = 20;

    [Header("Reward")]
    [Min(1)]
    [SerializeField] private int pointsOnDeath = 10;

    private int currentHealth;
    private EnemyResistances _resists;

    public event Action<EnemyHealth> Died;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        currentHealth = maxHealth;
        _resists = GetComponent<EnemyResistances>();
    }

    public void TakeDamage(DamagePacket packet)
    {
        if (IsDead)
            return;

        packet.Clamp();

        float multiplier = 1f;

        if (_resists != null)
            multiplier = _resists.GetMultiplier(packet.element);

        int finalDamage = Mathf.RoundToInt(packet.amount * multiplier);
        currentHealth -= finalDamage;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        int reward = pointsOnDeath > 0 ? pointsOnDeath : DefaultPointsOnDeath;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(reward);
        else
            Debug.LogWarning($"{name} died, but no ScoreManager was found in the scene.", this);

        Died?.Invoke(this);

        // TODO: add death VFX, score popup, loot drop, etc.
        Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (pointsOnDeath <= 0)
            pointsOnDeath = DefaultPointsOnDeath;
    }
}
