using System;
using System.Collections.Generic;
using UnityEngine;

public enum TalentTag
{
    Human,
    Angel,
    Demon,
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
    public readonly PlayableFactionChoice Faction;
    public readonly PlayableCharacterChoice Character;
    public readonly StartingWeaponChoice Weapon;
    public readonly StartingBombChoice Bomb;
    public readonly StartingSkillChoice Skill;
    public readonly StartingPassiveChoice Passive;

    public TalentContext(
        PlayableFactionChoice faction,
        PlayableCharacterChoice character,
        StartingWeaponChoice weapon,
        StartingBombChoice bomb,
        StartingSkillChoice skill,
        StartingPassiveChoice passive)
    {
        Faction = faction;
        Character = character;
        Weapon = weapon;
        Bomb = bomb;
        Skill = skill;
        Passive = passive;
    }

    public string CharacterName => RunLoadoutState.GetCharacterName(Character);
    public string FactionName => RunLoadoutState.GetFactionName(Faction);
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

public enum RunTalentCategory
{
    Attack,
    Defense
}

public sealed class RunTalentDefinition
{
    private readonly Func<TalentContext, RunTalentState, PlayerUpgradeOption> _optionBuilder;
    private readonly Func<TalentContext, string> _effectBuilder;

    public readonly string Id;
    public readonly string RowId;
    public readonly string RowName;
    public readonly RunTalentCategory Category;
    public readonly string ParentId;
    public readonly string Title;
    public readonly int MaxPoints;
    public readonly int Tier;
    public readonly int RequiredRowPoints;
    public readonly string[] MutuallyExclusiveWithIds;
    public readonly Color AccentColor;

    public RunTalentDefinition(
        string id,
        string rowId,
        string rowName,
        RunTalentCategory category,
        string parentId,
        string title,
        int maxPoints,
        int tier,
        int requiredRowPoints,
        string[] mutuallyExclusiveWithIds,
        Color accentColor,
        Func<TalentContext, string> effectBuilder,
        Func<TalentContext, RunTalentState, PlayerUpgradeOption> optionBuilder)
    {
        Id = id;
        RowId = rowId;
        RowName = rowName;
        Category = category;
        ParentId = parentId;
        Title = title;
        MaxPoints = Mathf.Max(1, maxPoints);
        Tier = Mathf.Max(1, tier);
        RequiredRowPoints = Mathf.Max(0, requiredRowPoints);
        MutuallyExclusiveWithIds = mutuallyExclusiveWithIds;
        AccentColor = accentColor;
        _effectBuilder = effectBuilder;
        _optionBuilder = optionBuilder;
    }

    public bool IsRoot => string.IsNullOrWhiteSpace(ParentId);

    public bool IsUnlocked(RunTalentState state, int rowPoints)
    {
        if (state == null)
            return IsRoot && RequiredRowPoints == 0;

        if (!IsRoot && !state.HasPoints(ParentId))
            return false;

        if (rowPoints < RequiredRowPoints)
            return false;

        return !IsLockedByMutex(state);
    }

    public bool IsLockedByMutex(RunTalentState state)
    {
        if (state == null || MutuallyExclusiveWithIds == null)
            return false;

        for (int i = 0; i < MutuallyExclusiveWithIds.Length; i++)
        {
            if (state.HasPoints(MutuallyExclusiveWithIds[i]))
                return true;
        }

        return false;
    }

    public bool IsMaxed(RunTalentState state)
    {
        return state != null && state.GetPoints(Id) >= MaxPoints;
    }

    public string BuildEffectText(TalentContext context)
    {
        return _effectBuilder != null ? _effectBuilder(context) : string.Empty;
    }

    public PlayerUpgradeOption BuildOption(TalentContext context, RunTalentState state)
    {
        return _optionBuilder?.Invoke(context, state);
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

    private static readonly string[] AttackPrimaryCapstoneMutex = { "atk_primary_overcharge", "atk_primary_annihilator" };
    private static readonly string[] AttackElementCapstoneMutex = { "atk_element_avatar", "atk_element_unleashed" };
    private static readonly string[] AttackBombCapstoneMutex = { "atk_bomb_devastator", "atk_bomb_chained" };
    private static readonly string[] DefenseVitalCapstoneMutex = { "def_vital_eternal", "def_vital_indomitable" };
    private static readonly string[] DefenseFieldCapstoneMutex = { "def_field_dominion", "def_field_phaseshift" };
    private static readonly string[] DefenseCommandCapstoneMutex = { "def_command_warbanner", "def_command_phalanx" };

    private static string[] MutexAgainst(string[] group, string self)
    {
        List<string> others = new List<string>(group.Length);
        for (int i = 0; i < group.Length; i++)
        {
            if (group[i] != self)
                others.Add(group[i]);
        }
        return others.ToArray();
    }

    private static readonly RunTalentDefinition[] RunTalentDefinitions =
    {
        // Attack Primary tree
        new RunTalentDefinition("atk_primary_power", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, null, "Power Shot", 5, 1, 0, null, new Color(0.9f, 0.68f, 0.24f, 1f), context => $"{context.PrimaryName} gains +8% damage per point.", BuildPrimaryPowerOption),
        new RunTalentDefinition("atk_primary_heavy", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_power", "Big Shot", 5, 2, 1, null, new Color(1f, 0.76f, 0.32f, 1f), BuildPrimaryHeavyEffect, BuildPrimaryHeavyOption),
        new RunTalentDefinition("atk_primary_splinter", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_power", "Splinter", 5, 2, 1, null, new Color(1f, 0.84f, 0.42f, 1f), BuildPrimarySplinterEffect, BuildPrimarySplinterOption),
        new RunTalentDefinition("atk_primary_reaper", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_power", "Reaper Rounds", 5, 2, 1, null, new Color(0.96f, 0.62f, 0.2f, 1f), BuildPrimaryReaperEffect, BuildPrimaryReaperOption),
        new RunTalentDefinition("atk_primary_focus", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_heavy", "Focused Strike", 3, 3, 4, null, new Color(1f, 0.7f, 0.28f, 1f), BuildPrimaryFocusEffect, BuildPrimaryFocusOption),
        new RunTalentDefinition("atk_primary_volley", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_splinter", "Pressure Volley", 3, 3, 4, null, new Color(1f, 0.78f, 0.36f, 1f), BuildPrimaryVolleyEffect, BuildPrimaryVolleyOption),
        new RunTalentDefinition("atk_primary_finisher", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_reaper", "Killing Stroke", 3, 3, 4, null, new Color(0.92f, 0.56f, 0.18f, 1f), BuildPrimaryFinisherEffect, BuildPrimaryFinisherOption),
        new RunTalentDefinition("atk_primary_overcharge", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_focus", "Overcharge", 2, 4, 9, MutexAgainst(AttackPrimaryCapstoneMutex, "atk_primary_overcharge"), new Color(1f, 0.86f, 0.42f, 1f), BuildPrimaryOverchargeEffect, BuildPrimaryOverchargeOption),
        new RunTalentDefinition("atk_primary_annihilator", "attack_primary", "Primary Damage Tree", RunTalentCategory.Attack, "atk_primary_finisher", "Annihilator", 2, 4, 9, MutexAgainst(AttackPrimaryCapstoneMutex, "atk_primary_annihilator"), new Color(0.9f, 0.46f, 0.16f, 1f), BuildPrimaryAnnihilatorEffect, BuildPrimaryAnnihilatorOption),

        // Attack Element tree
        new RunTalentDefinition("atk_element_spark", "attack_element", "Elemental Tree", RunTalentCategory.Attack, null, "Elemental Spark", 5, 1, 0, null, new Color(0.62f, 0.88f, 1f, 1f), context => $"{GetPrimaryElementName(context)} effects gain +6% damage per point.", BuildElementSparkOption),
        new RunTalentDefinition("atk_element_control", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_spark", "Elemental Control", 5, 2, 1, null, GetElementPreviewColor(), BuildElementControlEffect, BuildElementControlOption),
        new RunTalentDefinition("atk_element_surge", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_spark", "Elemental Surge", 5, 2, 1, null, GetElementPreviewColor(), BuildElementSurgeEffect, BuildElementSurgeOption),
        new RunTalentDefinition("atk_element_ascendant", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_spark", "Ascendant", 5, 2, 1, null, GetElementPreviewColor(), BuildElementCapstoneEffect, BuildElementAscendantOption),
        new RunTalentDefinition("atk_element_master", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_control", "Element Mastery", 3, 3, 4, null, GetElementPreviewColor(), BuildElementMasteryDeepEffect, BuildElementMasteryDeepOption),
        new RunTalentDefinition("atk_element_overflow", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_surge", "Element Overflow", 3, 3, 4, null, GetElementPreviewColor(), BuildElementOverflowEffect, BuildElementOverflowOption),
        new RunTalentDefinition("atk_element_apex", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_ascendant", "Element Apex", 3, 3, 4, null, GetElementPreviewColor(), BuildElementApexEffect, BuildElementApexOption),
        new RunTalentDefinition("atk_element_avatar", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_master", "Avatar of the Element", 2, 4, 9, MutexAgainst(AttackElementCapstoneMutex, "atk_element_avatar"), GetElementPreviewColor(), BuildElementAvatarEffect, BuildElementAvatarOption),
        new RunTalentDefinition("atk_element_unleashed", "attack_element", "Elemental Tree", RunTalentCategory.Attack, "atk_element_apex", "Unleashed", 2, 4, 9, MutexAgainst(AttackElementCapstoneMutex, "atk_element_unleashed"), GetElementPreviewColor(), BuildElementUnleashedEffect, BuildElementUnleashedOption),

        // Attack Bomb tree
        new RunTalentDefinition("atk_bomb_craft", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, null, "Bomb Craft", 5, 1, 0, null, new Color(1f, 0.48f, 0.18f, 1f), context => $"{context.BombName} cooldown is reduced by 0.45s per point.", BuildBombCraftOption),
        new RunTalentDefinition("atk_bomb_payload", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_craft", "Bigger Payload", 5, 2, 1, null, new Color(1f, 0.58f, 0.18f, 1f), context => $"{context.BombName} gains +10% damage per point.", BuildBombPayloadOption),
        new RunTalentDefinition("atk_bomb_radius", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_craft", "Blast Radius", 5, 2, 1, null, new Color(1f, 0.68f, 0.26f, 1f), context => $"{context.BombName} gains +0.25 explosion radius per point.", BuildBombRadiusOption),
        new RunTalentDefinition("atk_bomb_rhythm", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_craft", "Combat Rhythm", 5, 2, 1, null, new Color(1f, 0.78f, 0.34f, 1f), BuildKitRhythmEffect, BuildBombRhythmOption),
        new RunTalentDefinition("atk_bomb_payload_master", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_payload", "Heavy Munitions", 3, 3, 4, null, new Color(1f, 0.58f, 0.22f, 1f), context => $"{context.BombName} gains +14% damage per point.", BuildBombPayloadMasterOption),
        new RunTalentDefinition("atk_bomb_radius_master", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_radius", "Wide Detonation", 3, 3, 4, null, new Color(1f, 0.66f, 0.28f, 1f), context => $"{context.BombName} gains +0.35 radius per point.", BuildBombRadiusMasterOption),
        new RunTalentDefinition("atk_bomb_rhythm_master", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_rhythm", "Quickdraw Drill", 3, 3, 4, null, new Color(1f, 0.74f, 0.34f, 1f), context => $"-{FormatSeconds(0.4f)} bomb cooldown and -{FormatSeconds(0.35f)} skill cooldown per point.", BuildBombRhythmMasterOption),
        new RunTalentDefinition("atk_bomb_devastator", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_payload_master", "Devastator", 2, 4, 9, MutexAgainst(AttackBombCapstoneMutex, "atk_bomb_devastator"), new Color(1f, 0.5f, 0.2f, 1f), context => $"{context.BombName} gains +18% damage and +0.4 radius per point.", BuildBombDevastatorOption),
        new RunTalentDefinition("atk_bomb_chained", "attack_bomb", "Bombcraft Tree", RunTalentCategory.Attack, "atk_bomb_rhythm_master", "Chained Salvo", 2, 4, 9, MutexAgainst(AttackBombCapstoneMutex, "atk_bomb_chained"), new Color(1f, 0.7f, 0.34f, 1f), context => $"-{FormatSeconds(0.55f)} bomb cooldown and +12% bomb damage per point.", BuildBombChainedOption),

        // Defense Vital tree
        new RunTalentDefinition("def_vital_core", "defense_vital", "Survival Tree", RunTalentCategory.Defense, null, "Vital Core", 5, 1, 0, null, new Color(0.55f, 0.95f, 0.58f, 1f), context => "+12 max HP and heal 12 per point.", BuildVitalCoreOption),
        new RunTalentDefinition("def_vital_steps", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_core", "Guarded Steps", 5, 2, 1, null, new Color(0.62f, 1f, 0.66f, 1f), context => "+7% movement speed per point.", BuildGuardedStepsOption),
        new RunTalentDefinition("def_vital_secondwind", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_core", "Second Wind", 5, 2, 1, null, new Color(0.72f, 1f, 0.72f, 1f), context => "+8 max HP and +4% damage per point.", BuildSecondWindOption),
        new RunTalentDefinition("def_vital_instinct", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_core", "Survivor's Instinct", 5, 2, 1, null, new Color(0.82f, 1f, 0.78f, 1f), BuildSurvivorInstinctEffect, BuildSurvivorInstinctOption),
        new RunTalentDefinition("def_vital_steps_master", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_steps", "Sprinter's Resolve", 3, 3, 4, null, new Color(0.7f, 1f, 0.7f, 1f), context => "+9% movement speed per point.", BuildStepsMasterOption),
        new RunTalentDefinition("def_vital_secondwind_master", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_secondwind", "Iron Resolve", 3, 3, 4, null, new Color(0.78f, 1f, 0.74f, 1f), context => "+12 max HP and +6% damage per point.", BuildSecondWindMasterOption),
        new RunTalentDefinition("def_vital_instinct_master", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_instinct", "Survivor's Edge", 3, 3, 4, null, new Color(0.88f, 1f, 0.82f, 1f), context => "+10 max HP and +6% movement speed per point.", BuildInstinctMasterOption),
        new RunTalentDefinition("def_vital_eternal", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_secondwind_master", "Eternal Vigil", 2, 4, 9, MutexAgainst(DefenseVitalCapstoneMutex, "def_vital_eternal"), new Color(0.66f, 1f, 0.7f, 1f), context => "+18 max HP and +8% damage per point.", BuildEternalOption),
        new RunTalentDefinition("def_vital_indomitable", "defense_vital", "Survival Tree", RunTalentCategory.Defense, "def_vital_instinct_master", "Indomitable", 2, 4, 9, MutexAgainst(DefenseVitalCapstoneMutex, "def_vital_indomitable"), new Color(0.86f, 1f, 0.8f, 1f), context => "+18 max HP and +0.6 pickup radius per point.", BuildIndomitableOption),

        // Defense Field tree
        new RunTalentDefinition("def_field_control", "defense_field", "Field Control Tree", RunTalentCategory.Defense, null, "Field Control", 5, 1, 0, null, new Color(0.46f, 0.9f, 0.86f, 1f), context => $"{context.SkillName} cooldown is reduced by 0.4s per point.", BuildFieldRootOption),
        new RunTalentDefinition("def_field_wider", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_control", "Wider Field", 5, 2, 1, null, new Color(0.56f, 0.98f, 0.92f, 1f), context => $"{context.SkillName} gains +0.3 radius per point.", BuildFieldWiderOption),
        new RunTalentDefinition("def_field_lasting", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_control", "Lasting Ward", 5, 2, 1, null, new Color(0.66f, 1f, 0.96f, 1f), context => $"{context.SkillName} effects last +0.25s longer per point.", BuildFieldLastingOption),
        new RunTalentDefinition("def_field_magnet", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_control", "Magnet Harvest", 5, 2, 1, null, new Color(0.76f, 1f, 0.9f, 1f), context => "+0.65 pickup radius per point.", BuildFieldMagnetOption),
        new RunTalentDefinition("def_field_wider_master", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_wider", "Sweeping Field", 3, 3, 4, null, new Color(0.6f, 1f, 0.94f, 1f), context => $"{context.SkillName} gains +0.45 radius per point.", BuildFieldWiderMasterOption),
        new RunTalentDefinition("def_field_lasting_master", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_lasting", "Enduring Ward", 3, 3, 4, null, new Color(0.7f, 1f, 0.98f, 1f), context => $"{context.SkillName} effects last +0.4s longer per point.", BuildFieldLastingMasterOption),
        new RunTalentDefinition("def_field_magnet_master", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_magnet", "Greater Magnet", 3, 3, 4, null, new Color(0.82f, 1f, 0.92f, 1f), context => "+0.9 pickup radius per point.", BuildFieldMagnetMasterOption),
        new RunTalentDefinition("def_field_dominion", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_lasting_master", "Field Dominion", 2, 4, 9, MutexAgainst(DefenseFieldCapstoneMutex, "def_field_dominion"), new Color(0.6f, 1f, 0.96f, 1f), context => $"{context.SkillName} gains +0.6 radius and +0.5s duration per point.", BuildFieldDominionOption),
        new RunTalentDefinition("def_field_phaseshift", "defense_field", "Field Control Tree", RunTalentCategory.Defense, "def_field_magnet_master", "Phase Shift", 2, 4, 9, MutexAgainst(DefenseFieldCapstoneMutex, "def_field_phaseshift"), new Color(0.86f, 1f, 0.94f, 1f), context => $"-{FormatSeconds(0.55f)} {context.SkillName} cooldown and +0.7 pickup radius per point.", BuildFieldPhaseshiftOption),

        // Defense Command tree
        new RunTalentDefinition("def_command_rally", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, null, "Rallying Banner", 5, 1, 0, null, new Color(0.78f, 0.74f, 1f, 1f), context => "+5% damage and +5 max HP per point.", BuildRallyingBannerOption),
        new RunTalentDefinition("def_command_guard", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_rally", "Guard Detail", 5, 2, 1, null, new Color(0.86f, 0.82f, 1f, 1f), context => "+10 max HP per point.", BuildGuardDetailOption),
        new RunTalentDefinition("def_command_crossfire", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_rally", "Crossfire Drills", 5, 2, 1, null, new Color(0.9f, 0.86f, 1f, 1f), BuildFactionCommandEffect, BuildCrossfireDrillsOption),
        new RunTalentDefinition("def_command_supply", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_rally", "Supply Line", 5, 2, 1, null, new Color(0.82f, 0.9f, 1f, 1f), context => "+0.45 pickup radius and +4% movement speed per point.", BuildSupplyLineOption),
        new RunTalentDefinition("def_command_guard_master", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_guard", "Guard Captain", 3, 3, 4, null, new Color(0.84f, 0.8f, 1f, 1f), context => "+14 max HP per point.", BuildGuardMasterOption),
        new RunTalentDefinition("def_command_crossfire_master", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_crossfire", "Coordinated Strike", 3, 3, 4, null, new Color(0.88f, 0.84f, 1f, 1f), BuildCrossfireMasterEffect, BuildCrossfireMasterOption),
        new RunTalentDefinition("def_command_supply_master", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_supply", "Logistics Mastery", 3, 3, 4, null, new Color(0.8f, 0.88f, 1f, 1f), context => "+0.7 pickup radius and +6% movement speed per point.", BuildSupplyMasterOption),
        new RunTalentDefinition("def_command_warbanner", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_crossfire_master", "War Banner", 2, 4, 9, MutexAgainst(DefenseCommandCapstoneMutex, "def_command_warbanner"), new Color(0.9f, 0.86f, 1f, 1f), context => "+10% damage and +10 max HP per point.", BuildWarBannerOption),
        new RunTalentDefinition("def_command_phalanx", "defense_command", "Faction Command Tree", RunTalentCategory.Defense, "def_command_guard_master", "Phalanx", 2, 4, 9, MutexAgainst(DefenseCommandCapstoneMutex, "def_command_phalanx"), new Color(0.84f, 0.82f, 1f, 1f), context => "+20 max HP and +6% movement speed per point.", BuildPhalanxOption)
    };

    public static TalentContext CreateCurrentContext()
    {
        return new TalentContext(
            RunLoadoutState.FactionChoice,
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

    public static RunUpgradeChainDisplayInfo[] BuildCurrentRunTalentTrees()
    {
        return BuildRunTalentRows(CreateCurrentContext(), null);
    }

    public static RunUpgradeChainDisplayInfo[] BuildRunTalentTrees(TalentContext context)
    {
        return BuildRunTalentRows(context, null);
    }

    public static RunUpgradeChainDisplayInfo[] BuildCurrentRunTalentRows(RunTalentState state = null)
    {
        return BuildRunTalentRows(CreateCurrentContext(), state);
    }

    public static RunUpgradeChainDisplayInfo[] BuildRunTalentRows(TalentContext context, RunTalentState state)
    {
        List<RunUpgradeChainDisplayInfo> rows = new List<RunUpgradeChainDisplayInfo>();
        HashSet<string> rowIds = new HashSet<string>();

        for (int i = 0; i < RunTalentDefinitions.Length; i++)
        {
            RunTalentDefinition definition = RunTalentDefinitions[i];
            if (!rowIds.Add(definition.RowId))
                continue;

            rows.Add(BuildRunTalentRow(context, state, definition.RowId));
        }

        return rows.ToArray();
    }

    public static List<PlayerUpgradeOption> BuildAvailableRunTalentOptions(
        RunTalentState state,
        TalentContext context,
        PlayableCharacterChoice character)
    {
        List<PlayerUpgradeOption> options = new List<PlayerUpgradeOption>();
        if (state == null)
            return options;

        for (int i = 0; i < RunTalentDefinitions.Length; i++)
        {
            RunTalentDefinition definition = RunTalentDefinitions[i];
            int rowPoints = CountRowPoints(state, definition.RowId);
            if (!definition.IsUnlocked(state, rowPoints) || definition.IsMaxed(state))
                continue;

            PlayerUpgradeOption option = definition.BuildOption(context, state);
            if (option != null && option.IsAvailableFor(character))
                options.Add(option);
        }

        return options;
    }

    public static int CountRowPoints(RunTalentState state, string rowId)
    {
        if (state == null || string.IsNullOrWhiteSpace(rowId))
            return 0;

        int sum = 0;
        for (int i = 0; i < RunTalentDefinitions.Length; i++)
        {
            RunTalentDefinition definition = RunTalentDefinitions[i];
            if (definition.RowId == rowId)
                sum += state.GetPoints(definition.Id);
        }

        return sum;
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

    private static RunUpgradeChainDisplayInfo BuildRunTalentRow(TalentContext context, RunTalentState state, string rowId)
    {
        List<RunUpgradeNodeDisplayInfo> nodes = new List<RunUpgradeNodeDisplayInfo>();
        string rowName = rowId;

        for (int i = 0; i < RunTalentDefinitions.Length; i++)
        {
            RunTalentDefinition definition = RunTalentDefinitions[i];
            if (definition.RowId != rowId)
                continue;

            rowName = $"{definition.Category}: {definition.RowName}";
            nodes.Add(BuildRunTalentNode(context, state, definition));
        }

        return new RunUpgradeChainDisplayInfo(rowName, nodes.ToArray());
    }

    private static RunUpgradeNodeDisplayInfo BuildRunTalentNode(
        TalentContext context,
        RunTalentState state,
        RunTalentDefinition definition)
    {
        int points = state != null ? state.GetPoints(definition.Id) : 0;
        int rowPoints = CountRowPoints(state, definition.RowId);
        bool unlocked = definition.IsUnlocked(state, rowPoints);
        bool maxed = points >= definition.MaxPoints;
        bool mutexLocked = definition.IsLockedByMutex(state);
        string requirement = BuildRequirementText(definition);

        if (state != null)
        {
            if (mutexLocked)
                requirement = "Locked by sibling capstone";
            else if (!unlocked)
                requirement = $"Locked: {requirement}";
            else if (maxed)
                requirement = "Maxed";
        }

        PlayerUpgradeOption previewOption = definition.BuildOption(context, state);
        string effectText = previewOption != null
            ? StripRequirementFromDescription(previewOption.Description)
            : definition.BuildEffectText(context);

        return new RunUpgradeNodeDisplayInfo(
            $"{points} / {definition.MaxPoints} points",
            definition.Title,
            requirement,
            effectText,
            definition.AccentColor);
    }

    private static string BuildRequirementText(RunTalentDefinition definition)
    {
        if (definition.IsRoot && definition.RequiredRowPoints == 0)
            return $"Tier {definition.Tier} | Root talent";

        List<string> parts = new List<string>();
        if (!definition.IsRoot)
            parts.Add($"Requires {GetTalentTitle(definition.ParentId)}");
        if (definition.RequiredRowPoints > 0)
            parts.Add($"{definition.RequiredRowPoints} pts in tree");
        if (definition.MutuallyExclusiveWithIds != null && definition.MutuallyExclusiveWithIds.Length > 0)
            parts.Add("Choose one capstone");

        return $"Tier {definition.Tier} | {string.Join(", ", parts)}";
    }

    private static string GetTalentTitle(string id)
    {
        for (int i = 0; i < RunTalentDefinitions.Length; i++)
        {
            if (RunTalentDefinitions[i].Id == id)
                return RunTalentDefinitions[i].Title;
        }

        return "parent talent";
    }

    public static string BuildTagSummary(TalentContext context)
    {
        List<string> tags = new List<string>();
        AddTag(tags, GetFactionTag(context));
        AddTag(tags, GetPrimaryFormTag(context));
        AddTag(tags, GetPrimaryElementTag(context));
        AddTag(tags, GetPrimaryPatternTag(context));
        AddTag(tags, TalentTag.Bomb);
        AddTag(tags, TalentTag.ActiveSkill);
        AddTag(tags, TalentTag.Passive);
        AddTag(tags, TalentTag.Allies);

        return $"Current tags: {string.Join("  |  ", tags)}";
    }

    private static TalentTag GetFactionTag(TalentContext context)
    {
        switch (context.Faction)
        {
            case PlayableFactionChoice.Angels:
                return TalentTag.Angel;
            case PlayableFactionChoice.Demons:
                return TalentTag.Demon;
            case PlayableFactionChoice.Humans:
            default:
                return TalentTag.Human;
        }
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

    private static string BuildPrimaryHeavyEffect(TalentContext context)
    {
        return BuildPrimaryTechniqueEffect(context);
    }

    private static string BuildPrimarySplinterEffect(TalentContext context)
    {
        return BuildPrimaryPressureEffect(context);
    }

    private static string BuildPrimaryReaperEffect(TalentContext context)
    {
        return BuildPrimaryCapstoneEffect(context);
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

    private static PlayerUpgradeOption BuildPrimaryPowerOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_primary_power", context, state, PlayerUpgradeType.DamagePercent, amount: 0.08f);
    }

    private static PlayerUpgradeOption BuildPrimaryHeavyOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_heavy", context, state, PlayerUpgradeType.DamagePercent, amount: 0.08f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.SplashRadius, secondaryAmount: 0.18f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_heavy", context, state, PlayerUpgradeType.DamagePercent, amount: 0.08f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicBeamWidth, secondaryAmount: 0.08f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_heavy", context, state, PlayerUpgradeType.DamagePercent, amount: 0.08f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeRadius, secondaryAmount: 0.18f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildPrimarySplinterOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_splinter", context, state, PlayerUpgradeType.ProjectileCount, intAmount: 1, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_splinter", context, state, PlayerUpgradeType.MagicStatusChance, amount: 0.08f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_splinter", context, state, PlayerUpgradeType.MeleeArcAngle, amount: 8f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildPrimaryReaperOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_reaper", context, state, PlayerUpgradeType.DamagePercent, amount: 0.1f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.Pierce, secondaryIntAmount: 1, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_reaper", context, state, PlayerUpgradeType.DamagePercent, amount: 0.1f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicStatusChance, secondaryAmount: 0.06f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_reaper", context, state, PlayerUpgradeType.DamagePercent, amount: 0.1f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeCooldownReduction, secondaryAmount: 0.025f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildElementSparkOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_element_spark", context, state, PlayerUpgradeType.DamagePercent, amount: 0.06f);
    }

    private static PlayerUpgradeOption BuildElementControlOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_element_control", context, state, PlayerUpgradeType.SplashRadius, amount: 0.18f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_element_control", context, state, PlayerUpgradeType.MagicStatusChance, amount: 0.08f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_element_control", context, state, PlayerUpgradeType.MeleeRadius, amount: 0.15f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildElementSurgeOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_element_surge", context, state, PlayerUpgradeType.FireRatePercent, amount: 0.08f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_element_surge", context, state, PlayerUpgradeType.MagicCooldownReduction, amount: 0.04f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_element_surge", context, state, PlayerUpgradeType.MeleeCooldownReduction, amount: 0.035f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildElementAscendantOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_element_ascendant", context, state, PlayerUpgradeType.DamagePercent, amount: 0.1f);
    }

    private static PlayerUpgradeOption BuildBombCraftOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_craft", context, state, PlayerUpgradeType.BombCooldownReduction, amount: 0.45f);
    }

    private static PlayerUpgradeOption BuildBombPayloadOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_payload", context, state, PlayerUpgradeType.BombDamagePercent, amount: 0.1f);
    }

    private static PlayerUpgradeOption BuildBombRadiusOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_radius", context, state, PlayerUpgradeType.BombRadius, amount: 0.25f);
    }

    private static PlayerUpgradeOption BuildBombRhythmOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_rhythm", context, state, PlayerUpgradeType.BombCooldownReduction, amount: 0.25f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.SkillCooldownReduction, secondaryAmount: 0.25f);
    }

    private static PlayerUpgradeOption BuildVitalCoreOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_core", context, state, PlayerUpgradeType.MaxHealth, intAmount: 12);
    }

    private static PlayerUpgradeOption BuildGuardedStepsOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_steps", context, state, PlayerUpgradeType.MoveSpeedPercent, amount: 0.07f);
    }

    private static PlayerUpgradeOption BuildSecondWindOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_secondwind", context, state, PlayerUpgradeType.MaxHealth, intAmount: 8, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.DamagePercent, secondaryAmount: 0.04f);
    }

    private static PlayerUpgradeOption BuildSurvivorInstinctOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_instinct", context, state, PlayerUpgradeType.MaxHealth, intAmount: 6, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MoveSpeedPercent, secondaryAmount: 0.04f);
    }

    private static PlayerUpgradeOption BuildFieldRootOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_control", context, state, PlayerUpgradeType.SkillCooldownReduction, amount: 0.4f);
    }

    private static PlayerUpgradeOption BuildFieldWiderOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_wider", context, state, PlayerUpgradeType.SkillRadius, amount: 0.3f);
    }

    private static PlayerUpgradeOption BuildFieldLastingOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_lasting", context, state, PlayerUpgradeType.SkillDuration, amount: 0.25f);
    }

    private static PlayerUpgradeOption BuildFieldMagnetOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_magnet", context, state, PlayerUpgradeType.PickupRadius, amount: 0.65f);
    }

    private static PlayerUpgradeOption BuildRallyingBannerOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_rally", context, state, PlayerUpgradeType.DamagePercent, amount: 0.05f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MaxHealth, secondaryIntAmount: 5);
    }

    private static PlayerUpgradeOption BuildGuardDetailOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_guard", context, state, PlayerUpgradeType.MaxHealth, intAmount: 10);
    }

    private static PlayerUpgradeOption BuildCrossfireDrillsOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("def_command_crossfire", context, state, PlayerUpgradeType.FireRatePercent, amount: 0.08f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("def_command_crossfire", context, state, PlayerUpgradeType.MagicRange, amount: 0.25f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("def_command_crossfire", context, state, PlayerUpgradeType.MeleeRadius, amount: 0.12f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildSupplyLineOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_supply", context, state, PlayerUpgradeType.PickupRadius, amount: 0.45f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MoveSpeedPercent, secondaryAmount: 0.04f);
    }

    private static string BuildPrimaryFocusEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return $"{context.PrimaryName} hits harder and pierces deeper into ranks.";
            case PlayableCharacterChoice.HumanArcanist:
                return $"{context.PrimaryName} burns hotter with wider beam coverage.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Vanguard Cleave damage scales further and reach extends.";
        }
    }

    private static string BuildPrimaryVolleyEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return $"{context.PrimaryName} adds an extra projectile and tighter spread.";
            case PlayableCharacterChoice.HumanArcanist:
                return $"{context.PrimaryName} status chance climbs further with each cast.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Vanguard Cleave widens its arc into a sweeping pressure swing.";
        }
    }

    private static string BuildPrimaryFinisherEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return $"{context.PrimaryName} damage spikes and shots cut through more enemies.";
            case PlayableCharacterChoice.HumanArcanist:
                return $"{context.PrimaryName} damage spikes and triggers status more reliably.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Vanguard Cleave damage spikes and recovery speeds up dramatically.";
        }
    }

    private static string BuildPrimaryOverchargeEffect(TalentContext context)
    {
        return $"Capstone: {context.PrimaryName} unleashes overwhelming offensive output. Locks the alternate capstone.";
    }

    private static string BuildPrimaryAnnihilatorEffect(TalentContext context)
    {
        return $"Capstone: {context.PrimaryName} hits with finishing force, cutting through the toughest packs. Locks the alternate capstone.";
    }

    private static string BuildElementMasteryDeepEffect(TalentContext context)
    {
        return $"Mastery of {GetPrimaryElementName(context)} sharpens its control effects further.";
    }

    private static string BuildElementOverflowEffect(TalentContext context)
    {
        return $"{GetPrimaryElementName(context)} effects ramp faster and trigger more often.";
    }

    private static string BuildElementApexEffect(TalentContext context)
    {
        return $"{GetPrimaryElementName(context)} reaches its peak intensity, hitting harder per stack.";
    }

    private static string BuildElementAvatarEffect(TalentContext context)
    {
        return $"Capstone: become an avatar of {GetPrimaryElementName(context)}, dealing massive elemental damage. Locks the alternate capstone.";
    }

    private static string BuildElementUnleashedEffect(TalentContext context)
    {
        return $"Capstone: unleash {GetPrimaryElementName(context)} in a relentless cascade of effects. Locks the alternate capstone.";
    }

    private static string BuildCrossfireMasterEffect(TalentContext context)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Ranged allies coordinate fire — your attack speed climbs further.";
            case PlayableCharacterChoice.HumanArcanist:
                return "Spell range stretches farther as allies hold the line.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Melee range grows as allies open lanes for the Vanguard.";
        }
    }

    private static PlayerUpgradeOption BuildPrimaryFocusOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_focus", context, state, PlayerUpgradeType.DamagePercent, amount: 0.12f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.Pierce, secondaryIntAmount: 1, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_focus", context, state, PlayerUpgradeType.DamagePercent, amount: 0.12f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicBeamWidth, secondaryAmount: 0.1f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_focus", context, state, PlayerUpgradeType.DamagePercent, amount: 0.12f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeRadius, secondaryAmount: 0.22f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildPrimaryVolleyOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_volley", context, state, PlayerUpgradeType.ProjectileCount, intAmount: 1, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.FireRatePercent, secondaryAmount: 0.06f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_volley", context, state, PlayerUpgradeType.MagicStatusChance, amount: 0.1f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicCooldownReduction, secondaryAmount: 0.05f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_volley", context, state, PlayerUpgradeType.MeleeArcAngle, amount: 10f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeRadius, secondaryAmount: 0.12f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildPrimaryFinisherOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_finisher", context, state, PlayerUpgradeType.DamagePercent, amount: 0.14f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.Pierce, secondaryIntAmount: 1, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_finisher", context, state, PlayerUpgradeType.DamagePercent, amount: 0.14f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicStatusChance, secondaryAmount: 0.08f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_finisher", context, state, PlayerUpgradeType.DamagePercent, amount: 0.14f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeCooldownReduction, secondaryAmount: 0.04f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildPrimaryOverchargeOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_overcharge", context, state, PlayerUpgradeType.DamagePercent, amount: 0.22f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.FireRatePercent, secondaryAmount: 0.1f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_overcharge", context, state, PlayerUpgradeType.DamagePercent, amount: 0.22f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicCooldownReduction, secondaryAmount: 0.06f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_overcharge", context, state, PlayerUpgradeType.DamagePercent, amount: 0.22f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeCooldownReduction, secondaryAmount: 0.05f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildPrimaryAnnihilatorOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_primary_annihilator", context, state, PlayerUpgradeType.DamagePercent, amount: 0.2f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.Pierce, secondaryIntAmount: 1, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_primary_annihilator", context, state, PlayerUpgradeType.DamagePercent, amount: 0.2f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicBeamWidth, secondaryAmount: 0.12f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_primary_annihilator", context, state, PlayerUpgradeType.DamagePercent, amount: 0.2f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeRadius, secondaryAmount: 0.25f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildElementMasteryDeepOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_element_master", context, state, PlayerUpgradeType.SplashRadius, amount: 0.24f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_element_master", context, state, PlayerUpgradeType.MagicStatusChance, amount: 0.12f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_element_master", context, state, PlayerUpgradeType.MeleeRadius, amount: 0.2f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildElementOverflowOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_element_overflow", context, state, PlayerUpgradeType.FireRatePercent, amount: 0.1f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_element_overflow", context, state, PlayerUpgradeType.MagicCooldownReduction, amount: 0.06f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_element_overflow", context, state, PlayerUpgradeType.MeleeCooldownReduction, amount: 0.05f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildElementApexOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_element_apex", context, state, PlayerUpgradeType.DamagePercent, amount: 0.14f);
    }

    private static PlayerUpgradeOption BuildElementAvatarOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_element_avatar", context, state, PlayerUpgradeType.DamagePercent, amount: 0.2f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.PickupRadius, secondaryAmount: 0.3f);
    }

    private static PlayerUpgradeOption BuildElementUnleashedOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("atk_element_unleashed", context, state, PlayerUpgradeType.FireRatePercent, amount: 0.14f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.SplashRadius, secondaryAmount: 0.3f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("atk_element_unleashed", context, state, PlayerUpgradeType.MagicStatusChance, amount: 0.14f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MagicCooldownReduction, secondaryAmount: 0.08f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("atk_element_unleashed", context, state, PlayerUpgradeType.MeleeCooldownReduction, amount: 0.07f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MeleeRadius, secondaryAmount: 0.22f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildBombPayloadMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_payload_master", context, state, PlayerUpgradeType.BombDamagePercent, amount: 0.14f);
    }

    private static PlayerUpgradeOption BuildBombRadiusMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_radius_master", context, state, PlayerUpgradeType.BombRadius, amount: 0.35f);
    }

    private static PlayerUpgradeOption BuildBombRhythmMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_rhythm_master", context, state, PlayerUpgradeType.BombCooldownReduction, amount: 0.4f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.SkillCooldownReduction, secondaryAmount: 0.35f);
    }

    private static PlayerUpgradeOption BuildBombDevastatorOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_devastator", context, state, PlayerUpgradeType.BombDamagePercent, amount: 0.18f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.BombRadius, secondaryAmount: 0.4f);
    }

    private static PlayerUpgradeOption BuildBombChainedOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("atk_bomb_chained", context, state, PlayerUpgradeType.BombCooldownReduction, amount: 0.55f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.BombDamagePercent, secondaryAmount: 0.12f);
    }

    private static PlayerUpgradeOption BuildStepsMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_steps_master", context, state, PlayerUpgradeType.MoveSpeedPercent, amount: 0.09f);
    }

    private static PlayerUpgradeOption BuildSecondWindMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_secondwind_master", context, state, PlayerUpgradeType.MaxHealth, intAmount: 12, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.DamagePercent, secondaryAmount: 0.06f);
    }

    private static PlayerUpgradeOption BuildInstinctMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_instinct_master", context, state, PlayerUpgradeType.MaxHealth, intAmount: 10, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MoveSpeedPercent, secondaryAmount: 0.06f);
    }

    private static PlayerUpgradeOption BuildEternalOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_eternal", context, state, PlayerUpgradeType.MaxHealth, intAmount: 18, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.DamagePercent, secondaryAmount: 0.08f);
    }

    private static PlayerUpgradeOption BuildIndomitableOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_vital_indomitable", context, state, PlayerUpgradeType.MaxHealth, intAmount: 18, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.PickupRadius, secondaryAmount: 0.6f);
    }

    private static PlayerUpgradeOption BuildFieldWiderMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_wider_master", context, state, PlayerUpgradeType.SkillRadius, amount: 0.45f);
    }

    private static PlayerUpgradeOption BuildFieldLastingMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_lasting_master", context, state, PlayerUpgradeType.SkillDuration, amount: 0.4f);
    }

    private static PlayerUpgradeOption BuildFieldMagnetMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_magnet_master", context, state, PlayerUpgradeType.PickupRadius, amount: 0.9f);
    }

    private static PlayerUpgradeOption BuildFieldDominionOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_dominion", context, state, PlayerUpgradeType.SkillRadius, amount: 0.6f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.SkillDuration, secondaryAmount: 0.5f);
    }

    private static PlayerUpgradeOption BuildFieldPhaseshiftOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_field_phaseshift", context, state, PlayerUpgradeType.SkillCooldownReduction, amount: 0.55f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.PickupRadius, secondaryAmount: 0.7f);
    }

    private static PlayerUpgradeOption BuildGuardMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_guard_master", context, state, PlayerUpgradeType.MaxHealth, intAmount: 14);
    }

    private static PlayerUpgradeOption BuildCrossfireMasterOption(TalentContext context, RunTalentState state)
    {
        switch (context.Character)
        {
            case PlayableCharacterChoice.HumanRanger:
                return CreateTalentOption("def_command_crossfire_master", context, state, PlayerUpgradeType.FireRatePercent, amount: 0.12f, scope: PlayerUpgradeScope.Ranger);
            case PlayableCharacterChoice.HumanArcanist:
                return CreateTalentOption("def_command_crossfire_master", context, state, PlayerUpgradeType.MagicRange, amount: 0.4f, scope: PlayerUpgradeScope.Arcanist);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return CreateTalentOption("def_command_crossfire_master", context, state, PlayerUpgradeType.MeleeRadius, amount: 0.18f, scope: PlayerUpgradeScope.Vanguard);
        }
    }

    private static PlayerUpgradeOption BuildSupplyMasterOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_supply_master", context, state, PlayerUpgradeType.PickupRadius, amount: 0.7f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MoveSpeedPercent, secondaryAmount: 0.06f);
    }

    private static PlayerUpgradeOption BuildWarBannerOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_warbanner", context, state, PlayerUpgradeType.DamagePercent, amount: 0.1f, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MaxHealth, secondaryIntAmount: 10);
    }

    private static PlayerUpgradeOption BuildPhalanxOption(TalentContext context, RunTalentState state)
    {
        return CreateTalentOption("def_command_phalanx", context, state, PlayerUpgradeType.MaxHealth, intAmount: 20, hasSecondaryUpgrade: true, secondaryType: PlayerUpgradeType.MoveSpeedPercent, secondaryAmount: 0.06f);
    }

    private static PlayerUpgradeOption CreateTalentOption(
        string id,
        TalentContext context,
        RunTalentState state,
        PlayerUpgradeType type,
        float amount = 0f,
        int intAmount = 0,
        bool hasSecondaryUpgrade = false,
        PlayerUpgradeType secondaryType = PlayerUpgradeType.DamagePercent,
        float secondaryAmount = 0f,
        int secondaryIntAmount = 0,
        PlayerUpgradeScope scope = PlayerUpgradeScope.All)
    {
        RunTalentDefinition definition = FindRunTalentDefinition(id);
        if (definition == null)
            return null;

        int currentPoints = state != null ? state.GetPoints(id) : 0;
        string requirement = BuildRequirementText(definition);
        string description = $"{BuildAppliedTalentEffectText(type, amount, intAmount, hasSecondaryUpgrade, secondaryType, secondaryAmount, secondaryIntAmount)}\n{requirement}";

        return new PlayerUpgradeOption(
            definition.Title,
            description,
            type,
            amount,
            intAmount,
            hasSecondaryUpgrade,
            secondaryType,
            secondaryAmount,
            secondaryIntAmount,
            scope,
            definition.Id,
            currentPoints,
            definition.MaxPoints);
    }

    private static string StripRequirementFromDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        string[] lines = description.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
            return description.Trim();

        return string.Join(" ", lines, 0, lines.Length - 1).Trim();
    }

    private static string BuildAppliedTalentEffectText(
        PlayerUpgradeType type,
        float amount,
        int intAmount,
        bool hasSecondaryUpgrade,
        PlayerUpgradeType secondaryType,
        float secondaryAmount,
        int secondaryIntAmount)
    {
        string primary = BuildSingleAppliedEffectText(type, amount, intAmount);
        if (!hasSecondaryUpgrade)
            return $"{primary} per point.";

        string secondary = BuildSingleAppliedEffectText(secondaryType, secondaryAmount, secondaryIntAmount);
        return $"{primary} and {secondary} per point.";
    }

    private static string BuildSingleAppliedEffectText(PlayerUpgradeType type, float amount, int intAmount)
    {
        switch (type)
        {
            case PlayerUpgradeType.DamagePercent:
                return $"+{FormatPercent(amount)} player damage";
            case PlayerUpgradeType.FireRatePercent:
                return $"+{FormatPercent(amount)} attack speed";
            case PlayerUpgradeType.MoveSpeedPercent:
                return $"+{FormatPercent(amount)} movement speed";
            case PlayerUpgradeType.PickupRadius:
                return $"+{FormatDecimal(amount)} pickup radius";
            case PlayerUpgradeType.ProjectileCount:
                return $"+{Mathf.Max(1, intAmount)} projectile";
            case PlayerUpgradeType.Pierce:
                return $"+{Mathf.Max(1, intAmount)} pierce";
            case PlayerUpgradeType.MaxHealth:
                return $"+{Mathf.Max(1, intAmount)} max HP and heal {Mathf.Max(1, intAmount)}";
            case PlayerUpgradeType.SplashRadius:
                return $"+{FormatDecimal(amount)} splash radius";
            case PlayerUpgradeType.MeleeRadius:
                return $"+{FormatDecimal(amount)} melee range";
            case PlayerUpgradeType.MeleeArcAngle:
                return $"+{FormatDecimal(amount)} melee arc";
            case PlayerUpgradeType.MeleeCooldownReduction:
                return $"-{FormatSeconds(amount)} melee recovery";
            case PlayerUpgradeType.MagicRange:
                return $"+{FormatDecimal(amount)} spell range";
            case PlayerUpgradeType.MagicBeamWidth:
                return $"+{FormatDecimal(amount)} spell width";
            case PlayerUpgradeType.MagicCooldownReduction:
                return $"-{FormatSeconds(amount)} spell cooldown";
            case PlayerUpgradeType.MagicStatusChance:
                return $"+{FormatPercent(amount)} spell status chance";
            case PlayerUpgradeType.BombCooldownReduction:
                return $"-{FormatSeconds(amount)} bomb cooldown";
            case PlayerUpgradeType.BombRadius:
                return $"+{FormatDecimal(amount)} bomb radius";
            case PlayerUpgradeType.BombDamagePercent:
                return $"+{FormatPercent(amount)} bomb damage";
            case PlayerUpgradeType.SkillCooldownReduction:
                return $"-{FormatSeconds(amount)} E skill cooldown";
            case PlayerUpgradeType.SkillRadius:
                return $"+{FormatDecimal(amount)} E skill radius";
            case PlayerUpgradeType.SkillDuration:
                return $"+{FormatSeconds(amount)} E skill duration";
            default:
                return "Improve this talent";
        }
    }

    private static string FormatPercent(float value)
    {
        return Mathf.RoundToInt(value * 100f).ToString();
    }

    private static string FormatDecimal(float value)
    {
        return value.ToString("0.##");
    }

    private static string FormatSeconds(float value)
    {
        return $"{value:0.##}s";
    }

    private static RunTalentDefinition FindRunTalentDefinition(string id)
    {
        for (int i = 0; i < RunTalentDefinitions.Length; i++)
        {
            if (RunTalentDefinitions[i].Id == id)
                return RunTalentDefinitions[i];
        }

        return null;
    }

    private static Color GetElementPreviewColor()
    {
        return new Color(0.62f, 0.88f, 1f, 1f);
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
