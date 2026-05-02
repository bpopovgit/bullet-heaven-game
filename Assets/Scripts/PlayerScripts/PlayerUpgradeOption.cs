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
    SplashRadius,
    MeleeRadius,
    MeleeArcAngle,
    MeleeCooldownReduction,
    MagicRange,
    MagicBeamWidth,
    MagicCooldownReduction,
    MagicStatusChance,
    BombCooldownReduction,
    BombRadius,
    BombDamagePercent,
    SkillCooldownReduction,
    SkillRadius,
    SkillDuration,
    ExecuteThreshold,
    ExecuteBonusDamage,
    BurstShotFrequency,
    OnKillStatusSpreadRadius,
    OnKillStatusSpreadStrength,
    SkillElementBurstDamage,
    BombSecondaryBlastFraction,
    SkillBlinkDistance,
    MeleeWhirlwindUnlock
}

public enum PlayerUpgradeScope
{
    All,
    Ranger,
    Vanguard,
    Arcanist
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
    [SerializeField] private PlayerUpgradeScope scope = PlayerUpgradeScope.All;
    [SerializeField] private string runTalentId;
    [SerializeField] private int runTalentPointsBefore;
    [SerializeField] private int runTalentMaxPoints;

    public string Title => title;
    public string Description => description;
    public string RunTalentId => runTalentId;
    public int RunTalentPointsBefore => runTalentPointsBefore;
    public int RunTalentMaxPoints => runTalentMaxPoints;
    public int RunTalentPointsAfter => runTalentMaxPoints > 0
        ? Mathf.Min(runTalentMaxPoints, runTalentPointsBefore + 1)
        : runTalentPointsBefore;
    public bool IsRunTalent => !string.IsNullOrWhiteSpace(runTalentId);
    public string DisplayTitle => IsRunTalent && runTalentMaxPoints > 0
        ? $"{title} ({runTalentPointsBefore}/{runTalentMaxPoints} -> {RunTalentPointsAfter}/{runTalentMaxPoints})"
        : title;
    public string DisplayDescription => description;

    public PlayerUpgradeOption(
        string title,
        string description,
        PlayerUpgradeType upgradeType,
        float amount = 0f,
        int intAmount = 0,
        bool hasSecondaryUpgrade = false,
        PlayerUpgradeType secondaryUpgradeType = PlayerUpgradeType.DamagePercent,
        float secondaryAmount = 0f,
        int secondaryIntAmount = 0,
        PlayerUpgradeScope scope = PlayerUpgradeScope.All,
        string runTalentId = null,
        int runTalentPointsBefore = 0,
        int runTalentMaxPoints = 0)
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
        this.scope = scope;
        this.runTalentId = runTalentId;
        this.runTalentPointsBefore = Mathf.Max(0, runTalentPointsBefore);
        this.runTalentMaxPoints = Mathf.Max(0, runTalentMaxPoints);
    }

    public bool IsAvailableFor(PlayableCharacterChoice character)
    {
        if (!ScopeMatchesCharacter(scope, character))
            return false;

        if (!UpgradeTypeMatchesCharacter(upgradeType, character))
            return false;

        return !hasSecondaryUpgrade || UpgradeTypeMatchesCharacter(secondaryUpgradeType, character);
    }

    public void Apply(GameObject player)
    {
        if (player == null)
            return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerMeleeAttack melee = player.GetComponent<PlayerMeleeAttack>();
        PlayerMagicAttack magic = player.GetComponent<PlayerMagicAttack>();
        PlayerActiveBomb bomb = player.GetComponent<PlayerActiveBomb>();
        PlayerSecondaryActiveSkill secondarySkill = player.GetComponent<PlayerSecondaryActiveSkill>();
        PlayerCombatModifiers modifiers = player.GetComponent<PlayerCombatModifiers>();
        if (modifiers == null)
            modifiers = player.AddComponent<PlayerCombatModifiers>();

        ApplyUpgrade(player, stats, health, melee, magic, bomb, secondarySkill, modifiers, upgradeType, amount, intAmount);

        if (hasSecondaryUpgrade)
            ApplyUpgrade(player, stats, health, melee, magic, bomb, secondarySkill, modifiers, secondaryUpgradeType, secondaryAmount, secondaryIntAmount);

        if (IsRunTalent)
        {
            RunTalentState talentState = player.GetComponent<RunTalentState>();
            if (talentState == null)
                talentState = player.AddComponent<RunTalentState>();

            talentState.AddPoint(runTalentId, runTalentMaxPoints);
        }

        Debug.Log($"UPGRADE APPLIED: {title}");
    }

    private static void ApplyUpgrade(
        GameObject player,
        PlayerStats stats,
        PlayerHealth health,
        PlayerMeleeAttack melee,
        PlayerMagicAttack magic,
        PlayerActiveBomb bomb,
        PlayerSecondaryActiveSkill secondarySkill,
        PlayerCombatModifiers modifiers,
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

            case PlayerUpgradeType.MeleeRadius:
                if (melee != null) melee.AddRadius(amount);
                break;

            case PlayerUpgradeType.MeleeArcAngle:
                if (melee != null) melee.AddArcAngle(amount);
                break;

            case PlayerUpgradeType.MeleeCooldownReduction:
                if (melee != null) melee.ReduceCooldown(amount);
                break;

            case PlayerUpgradeType.MagicRange:
                if (magic != null) magic.AddRange(amount);
                break;

            case PlayerUpgradeType.MagicBeamWidth:
                if (magic != null) magic.AddBeamWidth(amount);
                break;

            case PlayerUpgradeType.MagicCooldownReduction:
                if (magic != null) magic.ReduceCooldown(amount);
                break;

            case PlayerUpgradeType.MagicStatusChance:
                if (magic != null) magic.AddStatusChance(amount);
                break;

            case PlayerUpgradeType.BombCooldownReduction:
                if (bomb != null) bomb.ReduceCooldown(amount);
                break;

            case PlayerUpgradeType.BombRadius:
                if (bomb != null) bomb.AddRadius(amount);
                break;

            case PlayerUpgradeType.BombDamagePercent:
                if (bomb != null) bomb.AddDamagePercent(amount);
                break;

            case PlayerUpgradeType.SkillCooldownReduction:
                if (secondarySkill != null) secondarySkill.ReduceCooldown(amount);
                break;

            case PlayerUpgradeType.SkillRadius:
                if (secondarySkill != null) secondarySkill.AddRadius(amount);
                break;

            case PlayerUpgradeType.SkillDuration:
                if (secondarySkill != null) secondarySkill.AddDuration(amount);
                break;

            case PlayerUpgradeType.ExecuteThreshold:
                if (modifiers != null) modifiers.AddExecuteThreshold(amount);
                break;

            case PlayerUpgradeType.ExecuteBonusDamage:
                if (modifiers != null) modifiers.AddExecuteBonusDamage(amount);
                break;

            case PlayerUpgradeType.BurstShotFrequency:
                if (modifiers != null) modifiers.AddBurstShotFrequency(Mathf.Max(1, intAmount));
                break;

            case PlayerUpgradeType.OnKillStatusSpreadRadius:
                if (modifiers != null) modifiers.AddOnKillStatusSpreadRadius(amount);
                break;

            case PlayerUpgradeType.OnKillStatusSpreadStrength:
                if (modifiers != null) modifiers.AddOnKillStatusSpreadStrength(amount);
                break;

            case PlayerUpgradeType.SkillElementBurstDamage:
                if (modifiers != null) modifiers.AddSkillElementBurstDamage(amount);
                break;

            case PlayerUpgradeType.BombSecondaryBlastFraction:
                if (modifiers != null) modifiers.AddBombSecondaryBlastFraction(amount);
                break;

            case PlayerUpgradeType.SkillBlinkDistance:
                if (modifiers != null) modifiers.AddSkillBlinkDistance(amount);
                break;

            case PlayerUpgradeType.MeleeWhirlwindUnlock:
                if (melee != null) melee.UnlockWhirlwind();
                break;
        }
    }

    private static bool ScopeMatchesCharacter(PlayerUpgradeScope upgradeScope, PlayableCharacterChoice character)
    {
        switch (upgradeScope)
        {
            case PlayerUpgradeScope.Ranger:
                return character == PlayableCharacterChoice.HumanRanger;
            case PlayerUpgradeScope.Vanguard:
                return character == PlayableCharacterChoice.HumanVanguard;
            case PlayerUpgradeScope.Arcanist:
                return character == PlayableCharacterChoice.HumanArcanist;
            default:
                return true;
        }
    }

    private static bool UpgradeTypeMatchesCharacter(PlayerUpgradeType type, PlayableCharacterChoice character)
    {
        switch (type)
        {
            case PlayerUpgradeType.FireRatePercent:
            case PlayerUpgradeType.ProjectileCount:
            case PlayerUpgradeType.Pierce:
            case PlayerUpgradeType.SplashRadius:
                return character == PlayableCharacterChoice.HumanRanger;

            case PlayerUpgradeType.MeleeRadius:
            case PlayerUpgradeType.MeleeArcAngle:
            case PlayerUpgradeType.MeleeCooldownReduction:
            case PlayerUpgradeType.MeleeWhirlwindUnlock:
                return character == PlayableCharacterChoice.HumanVanguard;

            case PlayerUpgradeType.MagicRange:
            case PlayerUpgradeType.MagicBeamWidth:
            case PlayerUpgradeType.MagicCooldownReduction:
            case PlayerUpgradeType.MagicStatusChance:
                return character == PlayableCharacterChoice.HumanArcanist;

            case PlayerUpgradeType.BombCooldownReduction:
            case PlayerUpgradeType.BombRadius:
            case PlayerUpgradeType.BombDamagePercent:
            case PlayerUpgradeType.SkillCooldownReduction:
            case PlayerUpgradeType.SkillRadius:
            case PlayerUpgradeType.SkillDuration:
                return true;

            default:
                return true;
        }
    }

    public static PlayerUpgradeOption[] CreateDefaultPool()
    {
        return new[]
        {
            new PlayerUpgradeOption("Sharpened Focus", "+15% damage", PlayerUpgradeType.DamagePercent, amount: 0.15f),
            new PlayerUpgradeOption("Fleet Footing", "+10% movement speed", PlayerUpgradeType.MoveSpeedPercent, amount: 0.10f),
            new PlayerUpgradeOption("Magnetic Field", "+1.5 pickup radius", PlayerUpgradeType.PickupRadius, amount: 1.5f),
            new PlayerUpgradeOption("Vital Core", "+20 max HP and heal 20", PlayerUpgradeType.MaxHealth, intAmount: 20),

            new PlayerUpgradeOption("Trigger Rhythm", "+15% fire rate", PlayerUpgradeType.FireRatePercent, amount: 0.15f, scope: PlayerUpgradeScope.Ranger),
            new PlayerUpgradeOption("Split Shot", "+1 projectile", PlayerUpgradeType.ProjectileCount, intAmount: 1, scope: PlayerUpgradeScope.Ranger),
            new PlayerUpgradeOption("Punch Through", "+1 pierce", PlayerUpgradeType.Pierce, intAmount: 1, scope: PlayerUpgradeScope.Ranger),
            new PlayerUpgradeOption("Volatile Payload", "+0.5 splash radius", PlayerUpgradeType.SplashRadius, amount: 0.5f, scope: PlayerUpgradeScope.Ranger),

            new PlayerUpgradeOption("Sweeping Edge", "+0.35 cleave range", PlayerUpgradeType.MeleeRadius, amount: 0.35f, scope: PlayerUpgradeScope.Vanguard),
            new PlayerUpgradeOption("Broad Cleave", "+18 cleave arc", PlayerUpgradeType.MeleeArcAngle, amount: 18f, scope: PlayerUpgradeScope.Vanguard),
            new PlayerUpgradeOption("Battle Tempo", "-0.06s cleave recovery", PlayerUpgradeType.MeleeCooldownReduction, amount: 0.06f, scope: PlayerUpgradeScope.Vanguard),
            new PlayerUpgradeOption(
                "Guardbreaker",
                "+15% damage and +10 cleave arc",
                PlayerUpgradeType.DamagePercent,
                amount: 0.15f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.MeleeArcAngle,
                secondaryAmount: 10f,
                scope: PlayerUpgradeScope.Vanguard),

            new PlayerUpgradeOption("Leyline Reach", "+1 spell range", PlayerUpgradeType.MagicRange, amount: 1f, scope: PlayerUpgradeScope.Arcanist),
            new PlayerUpgradeOption("Amplified Casting", "+0.18 spell size", PlayerUpgradeType.MagicBeamWidth, amount: 0.18f, scope: PlayerUpgradeScope.Arcanist),
            new PlayerUpgradeOption("Quick Chant", "-0.07s spell cooldown", PlayerUpgradeType.MagicCooldownReduction, amount: 0.07f, scope: PlayerUpgradeScope.Arcanist),
            new PlayerUpgradeOption(
                "Unstable Rune",
                "+20% status chance and +15% damage",
                PlayerUpgradeType.MagicStatusChance,
                amount: 0.20f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.DamagePercent,
                secondaryAmount: 0.15f,
                scope: PlayerUpgradeScope.Arcanist)
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
                secondaryAmount: 0.75f,
                scope: PlayerUpgradeScope.Ranger),

            new PlayerUpgradeOption(
                "Tyrant's Focus",
                "+35% damage and +1 pierce",
                PlayerUpgradeType.DamagePercent,
                amount: 0.35f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.Pierce,
                secondaryIntAmount: 1,
                scope: PlayerUpgradeScope.Ranger),

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
                secondaryAmount: 0.20f,
                scope: PlayerUpgradeScope.Ranger),

            new PlayerUpgradeOption(
                "Dragonbone Edge",
                "+0.55 cleave range and +25% damage",
                PlayerUpgradeType.MeleeRadius,
                amount: 0.55f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.DamagePercent,
                secondaryAmount: 0.25f,
                scope: PlayerUpgradeScope.Vanguard),

            new PlayerUpgradeOption(
                "War King's Sweep",
                "+28 cleave arc and -0.08s cleave recovery",
                PlayerUpgradeType.MeleeArcAngle,
                amount: 28f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.MeleeCooldownReduction,
                secondaryAmount: 0.08f,
                scope: PlayerUpgradeScope.Vanguard),

            new PlayerUpgradeOption(
                "Maelstrom Form",
                "Cleave becomes a Whirlwind, plus +0.4 cleave range.",
                PlayerUpgradeType.MeleeWhirlwindUnlock,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.MeleeRadius,
                secondaryAmount: 0.4f,
                scope: PlayerUpgradeScope.Vanguard),

            new PlayerUpgradeOption(
                "Ancient Leyline",
                "+1.5 spell range and +0.25 spell size",
                PlayerUpgradeType.MagicRange,
                amount: 1.5f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.MagicBeamWidth,
                secondaryAmount: 0.25f,
                scope: PlayerUpgradeScope.Arcanist),

            new PlayerUpgradeOption(
                "Dragon Rune",
                "+30% status chance and -0.08s spell cooldown",
                PlayerUpgradeType.MagicStatusChance,
                amount: 0.30f,
                hasSecondaryUpgrade: true,
                secondaryUpgradeType: PlayerUpgradeType.MagicCooldownReduction,
                secondaryAmount: 0.08f,
                scope: PlayerUpgradeScope.Arcanist)
        };
    }
}
