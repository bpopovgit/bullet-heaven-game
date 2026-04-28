using System;
using UnityEngine;

[Serializable]
public class EnemyWaveStage
{
    [Min(0f)]
    public float startTimeSeconds = 0f;
    [Min(0)]
    public int maxAlive = 8;
    [Min(0.05f)]
    public float respawnDelay = 4f;
    public bool fillImmediately = true;
    public GameObject[] enemyPrefabs;
    public EnemySpawnRegion allowedSpawnRegions = EnemySpawnRegion.Any;
    public FactionSpawnRule[] factionSpawnRules;
}

public class EnemyWaveDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunTimer runTimer;
    [SerializeField] private EnemyRespawnManager respawnManager;

    [Header("Stages")]
    [SerializeField] private bool useGeneratedFactionDefaults = true;
    [SerializeField] private EnemyWaveStage[] stages =
    {
        new EnemyWaveStage { startTimeSeconds = 0f, maxAlive = 8, respawnDelay = 4f, allowedSpawnRegions = EnemySpawnRegion.Any },
        new EnemyWaveStage { startTimeSeconds = 60f, maxAlive = 10, respawnDelay = 3.5f, allowedSpawnRegions = EnemySpawnRegion.Any },
        new EnemyWaveStage { startTimeSeconds = 120f, maxAlive = 12, respawnDelay = 3f, allowedSpawnRegions = EnemySpawnRegion.Any },
        new EnemyWaveStage { startTimeSeconds = 180f, maxAlive = 15, respawnDelay = 2.5f, allowedSpawnRegions = EnemySpawnRegion.Any }
    };

    private int _activeStageIndex = -1;

    private void OnEnable()
    {
        FindReferencesIfNeeded();

        if (runTimer != null)
            runTimer.WholeSecondChanged += HandleWholeSecondChanged;
    }

    private void Start()
    {
        FindReferencesIfNeeded();

        int wholeSecond = runTimer != null ? runTimer.WholeSeconds : 0;
        ApplyStageForTime(wholeSecond, force: true);
    }

    private void OnDisable()
    {
        if (runTimer != null)
            runTimer.WholeSecondChanged -= HandleWholeSecondChanged;
    }

    private void HandleWholeSecondChanged(int wholeSecond)
    {
        ApplyStageForTime(wholeSecond, force: false);
    }

    private void ApplyStageForTime(int wholeSecond, bool force)
    {
        if (respawnManager == null || stages == null || stages.Length == 0)
            return;

        int stageIndex = GetStageIndexForTime(wholeSecond);

        if (stageIndex < 0 || (!force && stageIndex == _activeStageIndex))
            return;

        _activeStageIndex = stageIndex;
        EnemyWaveStage stage = stages[stageIndex];
        FactionSpawnRule[] factionRules = GetFactionRulesForStage(stage, stageIndex);

        if (HasAnyFactionSpawnRules(factionRules))
        {
            respawnManager.ApplyFactionWaveSettings(
                factionRules,
                stage.respawnDelay,
                stage.fillImmediately,
                stage.allowedSpawnRegions);

            Debug.Log(
                $"WAVE STAGE {stageIndex + 1}: factionRules={factionRules.Length}, totalCap={GetFactionRuleTotalCap(factionRules)}, respawnDelay={stage.respawnDelay:0.##}, regions={stage.allowedSpawnRegions}");
            return;
        }

        respawnManager.ApplyWaveSettings(
            stage.enemyPrefabs,
            stage.maxAlive,
            stage.respawnDelay,
            stage.fillImmediately,
            stage.allowedSpawnRegions);

        Debug.Log(
            $"WAVE STAGE {stageIndex + 1}: maxAlive={stage.maxAlive}, respawnDelay={stage.respawnDelay:0.##}, regions={stage.allowedSpawnRegions}");
    }

    private int GetStageIndexForTime(int wholeSecond)
    {
        int bestIndex = -1;
        float bestStartTime = float.MinValue;

        for (int i = 0; i < stages.Length; i++)
        {
            EnemyWaveStage stage = stages[i];

            if (stage == null || stage.startTimeSeconds > wholeSecond)
                continue;

            if (stage.startTimeSeconds >= bestStartTime)
            {
                bestStartTime = stage.startTimeSeconds;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void FindReferencesIfNeeded()
    {
        if (runTimer == null)
            runTimer = RunTimer.Instance != null ? RunTimer.Instance : FindObjectOfType<RunTimer>();

        if (respawnManager == null)
            respawnManager = FindObjectOfType<EnemyRespawnManager>();
    }

    private FactionSpawnRule[] GetFactionRulesForStage(EnemyWaveStage stage, int stageIndex)
    {
        if (stage != null && HasAnyFactionSpawnRules(stage.factionSpawnRules))
            return stage.factionSpawnRules;

        if (!useGeneratedFactionDefaults)
            return null;

        return CreateGeneratedFactionRules(stageIndex);
    }

    private FactionSpawnRule[] CreateGeneratedFactionRules(int stageIndex)
    {
        switch (stageIndex)
        {
            case 0:
                return new[]
                {
                    CreateRule(FactionUnitArchetypeType.ZombieMelee, 7, EnemySpawnRegion.Any),
                    CreateRule(FactionUnitArchetypeType.ZombieRanged, 1, EnemySpawnRegion.East | EnemySpawnRegion.West)
                };
            case 1:
                return new[]
                {
                    CreateRule(FactionUnitArchetypeType.ZombieMelee, 8, EnemySpawnRegion.Any),
                    CreateRule(FactionUnitArchetypeType.ZombieRanged, 2, EnemySpawnRegion.East | EnemySpawnRegion.West),
                    CreateRule(FactionUnitArchetypeType.DemonMelee, 2, EnemySpawnRegion.North | EnemySpawnRegion.East)
                };
            case 2:
                return new[]
                {
                    CreateRule(FactionUnitArchetypeType.ZombieMelee, 9, EnemySpawnRegion.Any),
                    CreateRule(FactionUnitArchetypeType.ZombieRanged, 2, EnemySpawnRegion.East | EnemySpawnRegion.West),
                    CreateRule(FactionUnitArchetypeType.DemonMelee, 3, EnemySpawnRegion.North | EnemySpawnRegion.East),
                    CreateRule(FactionUnitArchetypeType.DemonRanged, 1, EnemySpawnRegion.East),
                    CreateRule(FactionUnitArchetypeType.AngelMelee, 2, EnemySpawnRegion.North | EnemySpawnRegion.West),
                    CreateRule(FactionUnitArchetypeType.AngelRanged, 1, EnemySpawnRegion.West)
                };
            default:
                return new[]
                {
                    CreateRule(FactionUnitArchetypeType.ZombieMelee, 10, EnemySpawnRegion.Any),
                    CreateRule(FactionUnitArchetypeType.ZombieRanged, 3, EnemySpawnRegion.East | EnemySpawnRegion.West),
                    CreateRule(FactionUnitArchetypeType.DemonMelee, 4, EnemySpawnRegion.North | EnemySpawnRegion.East),
                    CreateRule(FactionUnitArchetypeType.DemonRanged, 2, EnemySpawnRegion.East | EnemySpawnRegion.South),
                    CreateRule(FactionUnitArchetypeType.AngelMelee, 3, EnemySpawnRegion.North | EnemySpawnRegion.West),
                    CreateRule(FactionUnitArchetypeType.AngelRanged, 2, EnemySpawnRegion.West | EnemySpawnRegion.South)
                };
        }
    }

    private FactionSpawnRule CreateRule(
        FactionUnitArchetypeType archetype,
        int maxAlive,
        EnemySpawnRegion allowedSpawnRegions)
    {
        return new FactionSpawnRule
        {
            archetype = archetype,
            maxAlive = Mathf.Max(0, maxAlive),
            allowedSpawnRegions = allowedSpawnRegions == EnemySpawnRegion.None
                ? EnemySpawnRegion.Any
                : allowedSpawnRegions,
            rewardsEnabled = FactionUnitArchetype.GetFaction(archetype) == FactionType.Zombie
        };
    }

    private bool HasAnyFactionSpawnRules(FactionSpawnRule[] rules)
    {
        if (rules == null || rules.Length == 0)
            return false;

        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i] != null && rules[i].maxAlive > 0)
                return true;
        }

        return false;
    }

    private int GetFactionRuleTotalCap(FactionSpawnRule[] rules)
    {
        if (rules == null)
            return 0;

        int total = 0;
        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i] != null)
                total += Mathf.Max(0, rules[i].maxAlive);
        }

        return total;
    }

    private void OnValidate()
    {
        if (stages == null)
            return;

        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] == null)
                continue;

            stages[i].startTimeSeconds = Mathf.Max(0f, stages[i].startTimeSeconds);
            stages[i].maxAlive = Mathf.Max(0, stages[i].maxAlive);
            stages[i].respawnDelay = Mathf.Max(0.05f, stages[i].respawnDelay);

            if (stages[i].allowedSpawnRegions == EnemySpawnRegion.None)
                stages[i].allowedSpawnRegions = EnemySpawnRegion.Any;

            if (stages[i].factionSpawnRules == null)
                continue;

            for (int j = 0; j < stages[i].factionSpawnRules.Length; j++)
            {
                FactionSpawnRule rule = stages[i].factionSpawnRules[j];
                if (rule == null)
                    continue;

                rule.maxAlive = Mathf.Max(0, rule.maxAlive);

                if (rule.allowedSpawnRegions == EnemySpawnRegion.None)
                    rule.allowedSpawnRegions = EnemySpawnRegion.Any;
            }
        }
    }
}
