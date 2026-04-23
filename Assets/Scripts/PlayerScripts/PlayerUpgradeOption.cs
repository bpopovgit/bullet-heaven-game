using System;
using UnityEngine;

public enum PlayerUpgradeType
{
    DamagePercent,
    FireRatePercent,
    MoveSpeedPercent,
    PickupRadius,
    ProjectileCount,
    Pierce,
    MaxHealth,
    SplashRadius
}

[Serializable]
public class PlayerUpgradeOption
{
    [SerializeField] private string title;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private PlayerUpgradeType upgradeType;
    [SerializeField] private float amount;
    [SerializeField] private int intAmount;
    [SerializeField] private bool hasSecondaryUpgrade;
    [SerializeField] private PlayerUpgradeType secondaryUpgradeType;
    [SerializeField] private float secondaryAmount;
    [SerializeField] private int secondaryIntAmount;

    public string Title => title;
    public string Description => description;

    public PlayerUpgradeOption(
        string title,
        string description,
        PlayerUpgradeType upgradeType,
        float amount = 0f,
        int intAmount = 0,
        bool hasSecondaryUpgrade = false,
        PlayerUpgradeType secondaryUpgradeType = PlayerUpgradeType.DamagePercent,
        float secondaryAmount = 0f,
        int secondaryIntAmount = 0)
    {
        this.title = title;
        this.description = description;
        this.upgradeType = upgradeType;
        this.amount = amount;
        this.intAmount = intAmount;
        this.hasSecondaryUpgrade = hasSecondaryUpgrade;
        this.secondaryUpgradeType = secondaryUpgradeType;
        this.secondaryAmount = secondaryAmount;
        this.secondaryIntAmount = secondaryIntAmount;
    }

    public void Apply(GameObject player)
    {
        if (player == null)
            return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        ApplyUpgrade(player, stats, health, upgradeType, amount, intAmount);

        if (hasSecondaryUpgrade)
            ApplyUpgrade(player, stats, health, secondaryUpgradeType, secondaryAmount, secondaryIntAmount);

        Debug.Log($"UPGRADE APPLIED: {title}");
    }

    private static void ApplyUpgrade(
        GameObject player,
        PlayerStats stats,
        PlayerHealth health,
        PlayerUpgradeType type,
        float amount,
        int intAmount)
    {
        switch (type)
        {
            case PlayerUpgradeType.DamagePercent:
                if (stats != null) stats.AddDamagePercent(amount);
                break;

            case PlayerUpgradeType.FireRatePercent:
                if (stats != null) stats.AddFireRatePercent(amount);
                break;

            case PlayerUpgradeType.MoveSpeedPercent:
                if (stats != null) stats.AddMoveSpeedPercent(amount);
                break;

            case PlayerUpgradeType.PickupRadius:
                if (stats != null) stats.AddPickupRadius(amount);
                break;

            case PlayerUpgradeType.ProjectileCount:
                if (stats != null) stats.AddProjectiles(Mathf.Max(1, intAmount));
                break;

            case PlayerUpgradeType.Pierce:
                if (stats != null) stats.AddPierce(Mathf.Max(1, intAmount));
                break;

            case PlayerUpgradeType.MaxHealth:
                if (health != null) health.IncreaseMaxHP(Mathf.Max(1, intAmount), healSameAmount: true);
                break;

            case PlayerUpgradeType.SplashRadius:
                if (stats != null) stats.AddSplashRadius(amount);
                break;
        }
    }

    public static PlayerUpgradeOption[] CreateDefaultPool()
    {
        return new[]
        {
            new PlayerUpgradeOption("Sharpened Rounds", "+15% damage", PlayerUpgradeType.DamagePercent, amount: 0.15f),
            new PlayerUpgradeOption("Trigger Rhythm", "+15% fire rate", PlayerUpgradeType.FireRatePercent, amount: 0.15f),
            new PlayerUpgradeOption("Fleet Footing", "+10% movement speed", PlayerUpgradeType.MoveSpeedPercent, amount: 0.10f),
            new PlayerUpgradeOption("Magnetic Field", "+1.5 pickup radius", PlayerUpgradeType.PickupRadius, amount: 1.5f),
            new PlayerUpgradeOption("Split Shot", "+1 projectile", PlayerUpgradeType.ProjectileCount, intAmount: 1),
            new PlayerUpgradeOption("Punch Through", "+1 pierce", PlayerUpgradeType.Pierce, intAmount: 1),
            new PlayerUpgradeOption("Vital Core", "+20 max HP and heal 20", PlayerUpgradeType.MaxHealth, intAmount: 20),
            new PlayerUpgradeOption("Volatile Payload", "+0.5 splash radius", PlayerUpgradeType.SplashRadius, amount: 0.5f)
        };
    }

    public static PlayerUpgradeOption[] CreateBossRewardPool()
    {
        return new[]
        {
            new PlayerUpgradeOption(
                "Dragon Heart",
                "+40 max HP and heal 40",
                PlayerUpgradeType.MaxHealth,
                intAmount: 40),

            new PlayerUpgradeOption(
                "Inferno Chamber",
                "+1 projectile and +0.75 splash radius",
                PlayerUpgradeType.ProjectileCount,
                intAmount: 1,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.SplashRadius,
                secondaryAmount: 0.75f),

            new PlayerUpgradeOption(
                "Tyrant's Focus",
                "+35% damage and +1 pierce",
                PlayerUpgradeType.DamagePercent,
                amount: 0.35f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.Pierce,
                secondaryIntAmount: 1),

            new PlayerUpgradeOption(
                "Predator's Wings",
                "+20% movement speed and +2 pickup radius",
                PlayerUpgradeType.MoveSpeedPercent,
                amount: 0.20f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.PickupRadius,
                secondaryAmount: 2f),

            new PlayerUpgradeOption(
                "Volcanic Core",
                "+25% fire rate and +20% damage",
                PlayerUpgradeType.FireRatePercent,
                amount: 0.25f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.DamagePercent,
                secondaryAmount: 0.20f)
        };
    }
}
