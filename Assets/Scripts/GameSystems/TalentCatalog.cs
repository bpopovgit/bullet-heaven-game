using System;
using System.Collections.Generic;
using UnityEngine;

public enum TalentTag
{
    Human,
    Melee,
    Projectile,
    Spell,
    Physical,
    Fire,
    Frost,
    Poison,
    Lightning,
    Beam,
    Cone,
    Cloud,
    Chain,
    Bomb,
    ActiveSkill,
    Passive,
    Allies
}

public readonly struct TalentContext
{
    public readonly PlayableCharacterChoice Character;
    public readonly StartingWeaponChoice Weapon;
    public readonly StartingBombChoice Bomb;
    public readonly StartingSkillChoice Skill;
    public readonly StartingPassiveChoice Passive;

    public TalentContext(
        PlayableCharacterChoice character,
        StartingWeaponChoice weapon,
        StartingBombChoice bomb,
        StartingSkillChoice skill,
        StartingPassiveChoice passive)
    {
        Character = character;
        Weapon = weapon;
        Bomb = bomb;
        Skill = skill;
        Passive = passive;
    }

    public string CharacterName => RunLoadoutState.GetCharacterName(Character);
    public string PrimaryName => RunLoadoutState.GetPrimaryAttackName(Character, Weapon);
    public string BombName => RunLoadoutState.GetBombName(Bomb);
    public string SkillName => RunLoadoutState.GetSkillName(Skill);
    public string PassiveName => RunLoadoutState.GetPassiveName(Passive);
}

public sealed class TalentDisplayInfo
{
    public readonly string Title;
    public readonly string TreeName;
    public readonly string UnlockText;
    public readonly string RequirementText;
    public readonly string EffectText;
    public readonly bool IsUnlocked;
    public readonly Color AccentColor;

    public TalentDisplayInfo(
        string title,
        string treeName,
        string unlockText,
        string requirementText,
        string effectText,
        bool isUnlocked,
        Color accentColor)
    {
        Title = title;
        TreeName = treeName;
        UnlockText = unlockText;
        RequirementText = requirementText;
        EffectText = effectText;
        IsUnlocked = isUnlocked;
        AccentColor = accentColor;
    }
}

public sealed class TalentDefinition
{
    private readonly Func<TalentContext, string> _requirementBuilder;
    private readonly Func<TalentContext, string> _effectBuilder;

    public readonly string Title;
    public readonly string TreeName;
    public readonly int UnlockProfileLevel;
    public readonly Color AccentColor;

    public TalentDefinition(
        string title,
        string treeName,
        int unlockProfileLevel,
        Color accentColor,
        Func<TalentContext, string> requirementBuilder,
        Func<TalentContext, string> effectBuilder)
    {
        Title = title;
        TreeName = treeName;
        UnlockProfileLevel = Mathf.Max(1, unlockProfileLevel);
        AccentColor = accentColor;
        _requirementBuilder = requirementBuilder;
        _effectBuilder = effectBuilder;
    }

    public TalentDisplayInfo BuildDisplayInfo(TalentContext context, int profileLevel)
    {
        bool unlocked = profileLevel >= UnlockProfileLevel;
        string unlockText = unlocked ? "Unlocked" : $"Unlocks at Profile Lv {UnlockProfileLevel}";

        return new TalentDisplayInfo(
            Title,
            TreeName,
            unlockText,
            _requirementBuilder(context),
            _effectBuilder(context),
            unlocked,
            AccentColor);
    }
}

public static class TalentCatalog
{
    public const int PreviewProfileLevel = 1;

    private static readonly TalentDefinition[] Definitions =
    {
        new TalentDefinition(
            "Opening Discipline",
            "Core",
            1,
            new Color(0.36f, 0.72f, 0.28f, 1f),
            context => $"Primary: {context.PrimaryName}",
            BuildOpeningDisciplineEffect),

        new TalentDefinition(
            "Elemental Mastery",
            "Element",
            2,
            new Color(0.45f, 0.8f, 1f, 1f),
            context => $"Element: {GetPrimaryElementName(context)}",
            BuildElementalMasteryEffect),

        new TalentDefinition(
            "Bomb Craft",
            "Active Q",
            3,
            new Color(1f, 0.58f, 0.18f, 1f),
            context => $"Q: {context.BombName}",
            BuildBombCraftEffect),

        new TalentDefinition(
            "Field Control",
            "Active E",
            4,
            new Color(0.76f, 0.92f, 1f, 1f),
            context => $"E: {context.SkillName}",
            BuildFieldControlEffect),

        new TalentDefinition(
            "Survivor's Instinct",
            "Passive",
            5,
            new Color(0.9f, 0.82f, 0.34f, 1f),
            context => $"Passive: {context.PassiveName}",
            BuildSurvivorInstinctEffect),

        new TalentDefinition(
            "Faction Command",
            "Allies",
            6,
            new Color(0.82f, 0.58f, 1f, 1f),
            context => $"{RunLoadoutState.GetCharacterRole(context.Character)} allies",
            BuildFactionCommandEffect)
    };

    public static TalentContext CreateCurrentContext()
    {
        return new TalentContext(
            RunLoadoutState.CharacterChoice,
            RunLoadoutState.WeaponChoice,
            RunLoadoutState.BombChoice,
            RunLoadoutState.SkillChoice,
            RunLoadoutState.PassiveChoice);
    }

    public static TalentDisplayInfo[] BuildCurrentDisplayCards()
    {
        return BuildDisplayCards(CreateCurrentContext(), PreviewProfileLevel);
    }

    public static TalentDisplayInfo[] BuildDisplayCards(TalentContext context, int profileLevel)
    {
        List<TalentDisplayInfo> cards = new List<TalentDisplayInfo>();
        for (int i = 0; i < Definitions.Length; i++)
            cards.Add(Definitions[i].BuildDisplayInfo(context, profileLevel));

        return cards.ToArray();
    }

    public static string BuildCurrentTagSummary()
    {
        return BuildTagSummary(CreateCurrentContext());
    }

    public static string BuildTagSummary(TalentContext context)
    {
        List<string> tags = new List<string>();
        AddTag(tags, TalentTag.Human);
        AddTag(tags, GetPrimaryFormTag(context));
        AddTag(tags, GetPrimaryElementTag(context));
        AddTag(tags, GetPrimaryPatternTag(context));
        AddTag(tags, TalentTag.Bomb);
        AddTag(tags, TalentTag.ActiveSkill);
        AddTag(tags, TalentTag.Passive);
        AddTag(tags, TalentTag.Allies);

        return $"Current tags: {string.Join("  |  ", tags)}";
    }

    private static void AddTag(List<string> tags, TalentTag tag)
    {
        string label = tag.ToString();
        if (!tags.Contains(label))
            tags.Add(label);
    }

    private static TalentTag GetPrimaryFormTag(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return TalentTag.Projectile;
            case PlayableCharacterChoice.HumanArcanist:
                return TalentTag.Spell;
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return TalentTag.Melee;
        }
    }

    private static TalentTag GetPrimaryPatternTag(TalentContext context)
    {
        if (context.Character == PlayableCharacterChoice.HumanVanguard)
            return TalentTag.Melee;

        if (context.Character == PlayableCharacterChoice.HumanRanger)
            return TalentTag.Projectile;

        switch (context.Weapon)
        {
            case StartingWeaponChoice.FrostLance:
                return TalentTag.Beam;
            case StartingWeaponChoice.VenomCaster:
                return TalentTag.Cloud;
            case StartingWeaponChoice.StormNeedler:
                return TalentTag.Chain;
            case StartingWeaponChoice.EmberRepeater:
            default:
                return TalentTag.Cone;
        }
    }

    private static TalentTag GetPrimaryElementTag(TalentContext context)
    {
        if (context.Character == PlayableCharacterChoice.HumanVanguard)
            return TalentTag.Physical;

        switch (context.Weapon)
        {
            case StartingWeaponChoice.FrostLance:
                return TalentTag.Frost;
            case StartingWeaponChoice.VenomCaster:
                return TalentTag.Poison;
            case StartingWeaponChoice.StormNeedler:
                return TalentTag.Lightning;
            case StartingWeaponChoice.EmberRepeater:
            default:
                return TalentTag.Fire;
        }
    }

    private static string GetPrimaryElementName(TalentContext context)
    {
        return GetPrimaryElementTag(context).ToString();
    }

    private static string BuildOpeningDisciplineEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return $"{context.PrimaryName} gains focused projectile damage without changing the weapon's identity.";
            case PlayableCharacterChoice.HumanArcanist:
                return $"{context.PrimaryName} gains spell damage while preserving its current shape and element.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Vanguard Cleave recovers faster and remains the main melee identity for the run.";
        }
    }

    private static string BuildElementalMasteryEffect(TalentContext context)
    {
        switch (GetPrimaryElementTag(context))
        {
            case TalentTag.Fire:
                return "Fire effects burn longer, giving aggressive builds a stronger cleanup trail.";
            case TalentTag.Frost:
                return "Frost effects gain a chance to briefly freeze weakened targets.";
            case TalentTag.Poison:
                return "Poison effects tick harder against enemies standing near other poisoned enemies.";
            case TalentTag.Lightning:
                return "Lightning effects can jump once farther after striking the first target.";
            case TalentTag.Physical:
            default:
                return "Physical hits build stagger pressure against nearby frontline enemies.";
        }
    }

    private static string BuildBombCraftEffect(TalentContext context)
    {
        switch (context.Bomb)
        {
            case StartingBombChoice.FrostBomb:
                return "Frost Bomb leaves a longer slow field, buying more room when surrounded.";
            case StartingBombChoice.FireBomb:
                return "Fire Bomb leaves burning ground after the initial explosion.";
            case StartingBombChoice.ShockBomb:
                return "Shock Bomb chains a smaller zap into nearby survivors of the blast.";
            case StartingBombChoice.FragBomb:
            default:
                return "Frag Bomb gains a larger outer blast for emergency crowd control.";
        }
    }

    private static string BuildFieldControlEffect(TalentContext context)
    {
        switch (context.Skill)
        {
            case StartingSkillChoice.ArcaneShield:
                return "Arcane Shield lasts longer and releases a small pulse when it ends.";
            case StartingSkillChoice.FrostNova:
                return "Frost Nova freezes slightly longer and marks frozen enemies more clearly.";
            case StartingSkillChoice.MagneticPulse:
            default:
                return "Magnetic Pulse pushes enemies farther while pulling pickups from a wider ring.";
        }
    }

    private static string BuildSurvivorInstinctEffect(TalentContext context)
    {
        switch (context.Passive)
        {
            case StartingPassiveChoice.Swiftness:
                return "Swiftness grants a brief sprint after narrowly avoiding damage.";
            case StartingPassiveChoice.Magnetism:
                return "Magnetism converts every few collected gems into a small healing spark.";
            case StartingPassiveChoice.Overclock:
                return "Overclock spikes primary attack tempo for a short window after leveling up.";
            case StartingPassiveChoice.Vitality:
            default:
                return "Vitality adds a small heal whenever you survive a dangerous HP threshold.";
        }
    }

    private static string BuildFactionCommandEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Human ranged allies fire faster while staying close to the Ranger.";
            case PlayableCharacterChoice.HumanArcanist:
                return "Human allies near Arcanist spell impacts gain a brief protective ward.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Human melee allies gain a guard stance when the Vanguard dives into combat.";
        }
    }
}
