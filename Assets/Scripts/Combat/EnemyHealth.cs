using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;

    private EnemyResistances _resists;

    public event Action<EnemyHealth> Died;   // <-- NEW
    public bool IsDead { get; private set; } // <-- NEW

    private void Awake()
    {
        currentHealth = maxHealth;
        _resists = GetComponent<EnemyResistances>();
    }

    public void TakeDamage(DamagePacket packet)
    {
        if (IsDead) return;

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
        if (IsDead) return;
        IsDead = true;

        Died?.Invoke(this); // <-- notify respawn system

        // TODO: add death VFX, score, etc.
        Destroy(gameObject);
    }
}
