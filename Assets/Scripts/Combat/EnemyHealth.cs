using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 20;
    private int currentHealth;

    private EnemyResistances _resists;

    private void Awake()
    {
        currentHealth = maxHealth;
        _resists = GetComponent<EnemyResistances>();
    }

    public void TakeDamage(DamagePacket packet)
    {
        float multiplier = 1f;

        if (_resists != null)
        {
            // Assuming EnemyResistances has something like:
            // public float GetMultiplier(DamageElement element)
            multiplier = _resists.GetMultiplier(packet.element);
        }

        int finalDamage = Mathf.RoundToInt(packet.amount * multiplier);
        currentHealth -= finalDamage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // TODO: add death VFX, score, etc.
        Destroy(gameObject);
    }
}
