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
}

public class EnemyWaveDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunTimer runTimer;
    [SerializeField] private EnemyRespawnManager respawnManager;

    [Header("Stages")]
    [SerializeField] private EnemyWaveStage[] stages =
    {
        new EnemyWaveStage { startTimeSeconds = 0f, maxAlive = 8, respawnDelay = 4f },
        new EnemyWaveStage { startTimeSeconds = 60f, maxAlive = 10, respawnDelay = 3.5f },
        new EnemyWaveStage { startTimeSeconds = 120f, maxAlive = 12, respawnDelay = 3f },
        new EnemyWaveStage { startTimeSeconds = 180f, maxAlive = 15, respawnDelay = 2.5f }
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

        respawnManager.ApplyWaveSettings(
            stage.enemyPrefabs,
            stage.maxAlive,
            stage.respawnDelay,
            stage.fillImmediately);

        Debug.Log($"WAVE STAGE {stageIndex + 1}: maxAlive={stage.maxAlive}, respawnDelay={stage.respawnDelay:0.##}");
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
        }
    }
}
