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

public sealed class RunUpgradeNodeDisplayInfo
{
    public readonly string StageText;
    public readonly string Title;
    public readonly string RequirementText;
    public readonly string EffectText;
    public readonly Color AccentColor;

    public RunUpgradeNodeDisplayInfo(
        string stageText,
        string title,
        string requirementText,
        string effectText,
        Color accentColor)
    {
        StageText = stageText;
        Title = title;
        RequirementText = requirementText;
        EffectText = effectText;
        AccentColor = accentColor;
    }
}

public sealed class RunUpgradeChainDisplayInfo
{
    public readonly string ChainName;
    public readonly RunUpgradeNodeDisplayInfo[] Nodes;

    public RunUpgradeChainDisplayInfo(string chainName, RunUpgradeNodeDisplayInfo[] nodes)
    {
        ChainName = chainName;
        Nodes = nodes;
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

    public static RunUpgradeChainDisplayInfo[] BuildCurrentRunUpgradeChains()
    {
        return BuildRunUpgradeChains(CreateCurrentContext());
    }

    public static RunUpgradeChainDisplayInfo[] BuildRunUpgradeChains(TalentContext context)
    {
        return new[]
        {
            BuildPrimaryRunChain(context),
            BuildElementRunChain(context),
            BuildKitRunChain(context)
        };
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

    private static RunUpgradeChainDisplayInfo BuildPrimaryRunChain(TalentContext context)
    {
        Color accent = GetPrimaryAccentColor(context);

        return new RunUpgradeChainDisplayInfo(
            "Primary Path",
            new[]
            {
                new RunUpgradeNodeDisplayInfo(
                    "Root",
                    "Opening Form",
                    "Can appear early",
                    $"{context.PrimaryName} gains a simple baseline boost.",
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Requires Opening Form",
                    "Refined Technique",
                    "Unlocked after Root",
                    BuildPrimaryTechniqueEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Requires Refined Technique",
                    "Pressure Pattern",
                    "Unlocked after Step 2",
                    BuildPrimaryPressureEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Capstone",
                    "Signature Move",
                    "Requires full path",
                    BuildPrimaryCapstoneEffect(context),
                    accent)
            });
    }

    private static RunUpgradeChainDisplayInfo BuildElementRunChain(TalentContext context)
    {
        Color accent = GetElementAccentColor(context);
        string elementName = GetPrimaryElementName(context);

        return new RunUpgradeChainDisplayInfo(
            "Element Path",
            new[]
            {
                new RunUpgradeNodeDisplayInfo(
                    "Root",
                    $"{elementName} Spark",
                    "Can appear once element is active",
                    BuildElementRootEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Requires Spark",
                    $"{elementName} Control",
                    "Unlocked after Root",
                    BuildElementControlEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Requires Control",
                    $"{elementName} Surge",
                    "Unlocked after Step 2",
                    BuildElementSurgeEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Capstone",
                    $"{elementName} Ascendant",
                    "Requires full path",
                    BuildElementCapstoneEffect(context),
                    accent)
            });
    }

    private static RunUpgradeChainDisplayInfo BuildKitRunChain(TalentContext context)
    {
        Color accent = new Color(0.9f, 0.72f, 0.28f, 1f);

        return new RunUpgradeChainDisplayInfo(
            "Toolkit Path",
            new[]
            {
                new RunUpgradeNodeDisplayInfo(
                    "Root",
                    "Prepared Kit",
                    "Can appear early",
                    $"{context.BombName} and {context.SkillName} recover slightly faster.",
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Requires Prepared Kit",
                    "Combat Rhythm",
                    "Unlocked after Root",
                    BuildKitRhythmEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Requires Combat Rhythm",
                    "Survival Loop",
                    "Unlocked after Step 2",
                    BuildKitSurvivalEffect(context),
                    accent),
                new RunUpgradeNodeDisplayInfo(
                    "Capstone",
                    "Full Arsenal",
                    "Requires full path",
                    BuildKitCapstoneEffect(context),
                    accent)
            });
    }

    private static string BuildPrimaryTechniqueEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return $"{context.PrimaryName} gains tighter aim and higher projectile damage.";
            case PlayableCharacterChoice.HumanArcanist:
                return $"{context.PrimaryName} grows stronger in its current spell shape.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Vanguard Cleave recovers faster after hitting multiple enemies.";
        }
    }

    private static string BuildPrimaryPressureEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Repeated hits mark enemies for bonus ranged damage.";
            case PlayableCharacterChoice.HumanArcanist:
                return BuildSpellPatternPressureEffect(context);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Cleave hits build guard pressure around the Vanguard.";
        }
    }

    private static string BuildPrimaryCapstoneEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Your primary fires a burst after every few clean shots.";
            case PlayableCharacterChoice.HumanArcanist:
                return BuildSpellPatternCapstoneEffect(context);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Every third Cleave sends a short shockwave forward.";
        }
    }

    private static string BuildSpellPatternPressureEffect(TalentContext context)
    {
        switch (context.Weapon)
        {
            case StartingWeaponChoice.FrostLance:
                return "Frost Ray can briefly freeze heavily slowed enemies.";
            case StartingWeaponChoice.VenomCaster:
                return "Venom Bloom leaves a small lingering poison patch.";
            case StartingWeaponChoice.StormNeedler:
                return "Storm Arc can branch to one additional nearby target.";
            case StartingWeaponChoice.EmberRepeater:
            default:
                return "Ember Wave leaves burning embers at the edge of the cone.";
        }
    }

    private static string BuildSpellPatternCapstoneEffect(TalentContext context)
    {
        switch (context.Weapon)
        {
            case StartingWeaponChoice.FrostLance:
                return "Frost Ray splits into a thinner side ray after sustained casting.";
            case StartingWeaponChoice.VenomCaster:
                return "Venom Bloom detonates again when poisoned enemies die inside it.";
            case StartingWeaponChoice.StormNeedler:
                return "Storm Arc forks into two chain paths after the first hit.";
            case StartingWeaponChoice.EmberRepeater:
            default:
                return "Ember Wave erupts into a wider flame fan after repeated casts.";
        }
    }

    private static string BuildElementRootEffect(TalentContext context)
    {
        switch (GetPrimaryElementTag(context))
        {
            case TalentTag.Fire:
                return "Burn effects last slightly longer.";
            case TalentTag.Frost:
                return "Slow effects become stronger.";
            case TalentTag.Poison:
                return "Poison effects tick a little faster.";
            case TalentTag.Lightning:
                return "Shock effects last slightly longer.";
            case TalentTag.Physical:
            default:
                return "Physical hits gain light stagger pressure.";
        }
    }

    private static string BuildElementControlEffect(TalentContext context)
    {
        switch (GetPrimaryElementTag(context))
        {
            case TalentTag.Fire:
                return "Burning enemies take bonus damage from bombs.";
            case TalentTag.Frost:
                return "Slowed enemies take bonus damage from your primary.";
            case TalentTag.Poison:
                return "Poisoned enemies spread a weaker poison on death.";
            case TalentTag.Lightning:
                return "Shocked enemies pulse damage to nearby enemies.";
            case TalentTag.Physical:
            default:
                return "Staggered enemies are pushed back by active skills.";
        }
    }

    private static string BuildElementSurgeEffect(TalentContext context)
    {
        switch (GetPrimaryElementTag(context))
        {
            case TalentTag.Fire:
                return "Fire effects can ignite a second nearby target.";
            case TalentTag.Frost:
                return "Frost effects can freeze enemies below low HP.";
            case TalentTag.Poison:
                return "Poison stacks increase damage over time.";
            case TalentTag.Lightning:
                return "Lightning effects can jump farther.";
            case TalentTag.Physical:
            default:
                return "Physical hits deal more damage to staggered packs.";
        }
    }

    private static string BuildElementCapstoneEffect(TalentContext context)
    {
        switch (GetPrimaryElementTag(context))
        {
            case TalentTag.Fire:
                return "Major fire hits create a brief burning zone.";
            case TalentTag.Frost:
                return "Major frost hits shatter frozen enemies for splash damage.";
            case TalentTag.Poison:
                return "Poisoned enemies burst into a toxic cloud on death.";
            case TalentTag.Lightning:
                return "Lightning chains can return to a previously missed target.";
            case TalentTag.Physical:
            default:
                return "Physical finishers send a short stun pulse.";
        }
    }

    private static string BuildKitRhythmEffect(TalentContext context)
    {
        switch (context.Skill)
        {
            case StartingSkillChoice.ArcaneShield:
                return "Using Arcane Shield reduces your next bomb cooldown.";
            case StartingSkillChoice.FrostNova:
                return "Frozen enemies reduce your next bomb cooldown when defeated.";
            case StartingSkillChoice.MagneticPulse:
            default:
                return "Collected pickups reduce your next bomb cooldown slightly.";
        }
    }

    private static string BuildKitSurvivalEffect(TalentContext context)
    {
        switch (context.Passive)
        {
            case StartingPassiveChoice.Swiftness:
                return "After using an active skill, gain a short speed burst.";
            case StartingPassiveChoice.Magnetism:
                return "After using an active skill, pickups pull from farther away.";
            case StartingPassiveChoice.Overclock:
                return "After using an active skill, your primary attacks faster briefly.";
            case StartingPassiveChoice.Vitality:
            default:
                return "After using an active skill, gain a small safety heal.";
        }
    }

    private static string BuildKitCapstoneEffect(TalentContext context)
    {
        return $"Using {context.BombName} and {context.SkillName} close together empowers your next {context.PrimaryName}.";
    }

    private static Color GetPrimaryAccentColor(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return new Color(0.46f, 0.86f, 0.32f, 1f);
            case PlayableCharacterChoice.HumanArcanist:
                return new Color(0.55f, 0.86f, 1f, 1f);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return new Color(1f, 0.82f, 0.32f, 1f);
        }
    }

    private static Color GetElementAccentColor(TalentContext context)
    {
        switch (GetPrimaryElementTag(context))
        {
            case TalentTag.Fire:
                return new Color(1f, 0.42f, 0.14f, 1f);
            case TalentTag.Frost:
                return new Color(0.52f, 0.9f, 1f, 1f);
            case TalentTag.Poison:
                return new Color(0.42f, 1f, 0.34f, 1f);
            case TalentTag.Lightning:
                return new Color(1f, 0.94f, 0.22f, 1f);
            case TalentTag.Physical:
            default:
                return new Color(0.92f, 0.84f, 0.64f, 1f);
        }
    }
}
