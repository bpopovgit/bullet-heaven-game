using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private const int DefaultPointsOnDeath = 10;
    private const int DefaultExperienceOnDeath = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth = 20;

    [Header("Reward")]
    [Min(1)]
    [SerializeField] private int pointsOnDeath = 10;
    [Min(1)]
    [SerializeField] private int experienceOnDeath = 1;
    [Range(0f, 1f)]
    [SerializeField] private float experienceDropChance = 1f;
    [SerializeField] private GameObject experienceGemPrefab;

    [Header("Pickup Drops")]
    [Range(0f, 1f)]
    [SerializeField] private float healthDropChance = 0.08f;
    [SerializeField] private int healthPickupAmount = 20;
    [SerializeField] private GameObject healthPickupPrefab;
    [Range(0f, 1f)]
    [SerializeField] private float magnetDropChance = 0.03f;
    [SerializeField] private GameObject magnetPickupPrefab;
    [Range(0f, 1f)]
    [SerializeField] private float bombDropChance = 0.02f;
    [SerializeField] private int bombDamage = 50;
    [SerializeField] private float bombRadius = 6f;
    [SerializeField] private GameObject bombPickupPrefab;

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

        DropExperience();
        DropPickups();
        Died?.Invoke(this);

        // TODO: add death VFX, score popup, loot drop, etc.
        Destroy(gameObject);
    }

    private void DropExperience()
    {
        int xp = experienceOnDeath > 0 ? experienceOnDeath : DefaultExperienceOnDeath;
        float chance = experienceDropChance > 0f ? experienceDropChance : 1f;

        if (UnityEngine.Random.value > chance)
            return;

        if (experienceGemPrefab != null)
        {
            GameObject gemObject = Instantiate(experienceGemPrefab, transform.position, Quaternion.identity);

            if (gemObject.TryGetComponent<XPGem>(out var gem))
                gem.SetExperienceValue(xp);
            else
                Debug.LogWarning($"{experienceGemPrefab.name} does not have an XPGem component.", experienceGemPrefab);

            return;
        }

        XPGem.SpawnDefault(transform.position, xp);
    }

    private void DropPickups()
    {
        TryDropHealthPickup();
        TryDropMagnetPickup();
        TryDropBombPickup();
    }

    private void TryDropHealthPickup()
    {
        if (UnityEngine.Random.value > healthDropChance)
            return;

        if (healthPickupPrefab != null)
        {
            GameObject pickupObject = Instantiate(healthPickupPrefab, transform.position, Quaternion.identity);
            if (pickupObject.TryGetComponent<HealthPickup>(out var pickup))
                pickup.SetHealAmount(healthPickupAmount);
            else
                Debug.LogWarning($"{healthPickupPrefab.name} does not have a HealthPickup component.", healthPickupPrefab);

            return;
        }

        HealthPickup.SpawnDefault(transform.position, healthPickupAmount);
    }

    private void TryDropMagnetPickup()
    {
        if (UnityEngine.Random.value > magnetDropChance)
            return;

        if (magnetPickupPrefab != null)
        {
            GameObject pickupObject = Instantiate(magnetPickupPrefab, transform.position, Quaternion.identity);
            if (!pickupObject.TryGetComponent<MagnetPickup>(out _))
                Debug.LogWarning($"{magnetPickupPrefab.name} does not have a MagnetPickup component.", magnetPickupPrefab);

            return;
        }

        MagnetPickup.SpawnDefault(transform.position);
    }

    private void TryDropBombPickup()
    {
        if (UnityEngine.Random.value > bombDropChance)
            return;

        if (bombPickupPrefab != null)
        {
            GameObject pickupObject = Instantiate(bombPickupPrefab, transform.position, Quaternion.identity);
            if (pickupObject.TryGetComponent<BombPickup>(out var pickup))
                pickup.Configure(bombDamage, bombRadius);
            else
                Debug.LogWarning($"{bombPickupPrefab.name} does not have a BombPickup component.", bombPickupPrefab);

            return;
        }

        BombPickup.SpawnDefault(transform.position, bombDamage, bombRadius);
    }

    private void OnValidate()
    {
        if (pointsOnDeath <= 0)
            pointsOnDeath = DefaultPointsOnDeath;

        if (experienceOnDeath <= 0)
            experienceOnDeath = DefaultExperienceOnDeath;

        if (healthPickupAmount <= 0)
            healthPickupAmount = 20;

        if (bombDamage <= 0)
            bombDamage = 50;

        if (bombRadius <= 0f)
            bombRadius = 6f;
    }
}
