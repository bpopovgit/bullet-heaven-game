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

    public string Title => title;
    public string Description => description;

    public PlayerUpgradeOption(
        string title,
        string description,
        PlayerUpgradeType upgradeType,
        float amount = 0f,
        int intAmount = 0)
    {
        this.title = title;
        this.description = description;
        this.upgradeType = upgradeType;
        this.amount = amount;
        this.intAmount = intAmount;
    }

    public void Apply(GameObject player)
    {
        if (player == null)
            return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        switch (upgradeType)
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

        Debug.Log($"UPGRADE APPLIED: {title}");
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
}
