using System;
using UnityEngine;

public enum EnemyRewardMode
{
    Disabled,
    Always,
    HumanOnly
}

public class EnemyHealth : MonoBehaviour
{
    private const int DefaultPointsOnDeath = 10;
    private const int DefaultExperienceOnDeath = 1;

    [Header("Health")]
    [SerializeField] private int maxHealth = 20;

    [Header("Reward")]
    [SerializeField] private bool awardRewardsOnDeath = true;
    [SerializeField] private EnemyRewardMode rewardMode = EnemyRewardMode.Always;
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
    private StatusReceiver _statusReceiver;
    private bool _hasLastDamageSourceFaction;
    private FactionType _lastDamageSourceFaction;

    public event Action<EnemyHealth> Died;
    public bool IsDead { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void ConfigureHealth(int health, bool refill = true)
    {
        maxHealth = Mathf.Max(1, health);

        if (refill || currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    public void SetRewardsEnabled(bool enabled)
    {
        awardRewardsOnDeath = enabled;
        rewardMode = enabled ? EnemyRewardMode.Always : EnemyRewardMode.Disabled;
    }

    public void SetRewardMode(EnemyRewardMode mode)
    {
        rewardMode = mode;
        awardRewardsOnDeath = mode != EnemyRewardMode.Disabled;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        FactionMember.Ensure(gameObject, FactionType.Zombie);
        FactionVisualIdentity.Ensure(gameObject);
        _resists = GetComponent<EnemyResistances>();
        _statusReceiver = GetComponent<StatusReceiver>();

        if (_statusReceiver == null)
            _statusReceiver = gameObject.AddComponent<StatusReceiver>();
    }

    public void TakeDamage(DamagePacket packet, FactionMember attacker = null)
    {
        if (IsDead)
            return;

        if (attacker != null)
        {
            _hasLastDamageSourceFaction = true;
            _lastDamageSourceFaction = attacker.Faction;
        }

        packet.Clamp();

        float multiplier = 1f;

        if (_resists != null)
            multiplier = _resists.GetMultiplier(packet.element);

        int finalDamage = Mathf.RoundToInt(packet.amount * multiplier);

        if (IsPlayerSourcedDamage(attacker))
        {
            PlayerCombatModifiers modifiers = PlayerCombatModifiers.Instance;
            if (modifiers != null && maxHealth > 0)
            {
                float hpFraction = (float)currentHealth / maxHealth;
                finalDamage = modifiers.ApplyExecuteIfApplicable(finalDamage, hpFraction);
            }
        }

        currentHealth -= finalDamage;

        if (_statusReceiver != null && packet.HasStatus && currentHealth > 0)
            _statusReceiver.ApplyStatus(packet);

        if (currentHealth <= 0)
            Die();
    }

    private static bool IsPlayerSourcedDamage(FactionMember attacker)
    {
        return attacker == null || attacker.Faction == FactionType.Human;
    }

    public void ApplyEliteModifiers(float healthMultiplier, float rewardMultiplier, float pickupDropChanceBonus)
    {
        if (IsDead)
            return;

        float safeHealthMultiplier = Mathf.Max(1f, healthMultiplier);
        float safeRewardMultiplier = Mathf.Max(1f, rewardMultiplier);

        maxHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * safeHealthMultiplier));
        currentHealth = maxHealth;

        pointsOnDeath = Mathf.Max(1, Mathf.RoundToInt(pointsOnDeath * safeRewardMultiplier));
        experienceOnDeath = Mathf.Max(1, Mathf.RoundToInt(experienceOnDeath * safeRewardMultiplier));

        healthDropChance = Mathf.Clamp01(healthDropChance + pickupDropChanceBonus);
        magnetDropChance = Mathf.Clamp01(magnetDropChance + pickupDropChanceBonus);
        bombDropChance = Mathf.Clamp01(bombDropChance + pickupDropChanceBonus);
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;

        if (ShouldAwardRewardsOnDeath())
        {
            int reward = pointsOnDeath > 0 ? pointsOnDeath : DefaultPointsOnDeath;

            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddScore(reward);
            else
                Debug.LogWarning($"{name} died, but no ScoreManager was found in the scene.", this);

            DropExperience();
            DropPickups();
        }

        Died?.Invoke(this);
        GameAudio.PlayEnemyDeath();

        TrySpreadStatusOnKill();

        // TODO: add death VFX, score popup, loot drop, etc.
        Destroy(gameObject);
    }

    private void TrySpreadStatusOnKill()
    {
        PlayerCombatModifiers modifiers = PlayerCombatModifiers.Instance;
        if (modifiers == null || modifiers.OnKillStatusSpreadRadius <= 0f)
            return;

        if (_statusReceiver == null || !_statusReceiver.HasActiveStatus)
            return;

        modifiers.TrySpreadStatusOnKill(
            transform.position,
            _statusReceiver.MostRecentStatus,
            _statusReceiver.MostRecentStatusDuration,
            _statusReceiver.MostRecentStatusStrength);
    }

    private bool ShouldAwardRewardsOnDeath()
    {
        if (!awardRewardsOnDeath || rewardMode == EnemyRewardMode.Disabled)
            return false;

        if (rewardMode == EnemyRewardMode.Always)
            return true;

        if (rewardMode == EnemyRewardMode.HumanOnly)
            return !_hasLastDamageSourceFaction || _lastDamageSourceFaction == FactionType.Human;

        return false;
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
