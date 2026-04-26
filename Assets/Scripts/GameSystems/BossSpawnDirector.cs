using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BossSpawnDirector : MonoBehaviour
{
    private const float DefaultBossSpawnDistance = 12f;

    [Header("References")]
    [SerializeField] private RunTimer runTimer;
    [SerializeField] private EnemyRespawnManager respawnManager;
    [SerializeField] private BossSpawnPoint[] bossSpawnPoints;

    [Header("Schedule")]
    [SerializeField] private bool spawnBoss = true;
    [SerializeField] private float firstBossTimeSeconds = 20f;
    [SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private bool retrySpawnAfterThreshold = true;
    [SerializeField] private float retryIntervalSeconds = 0.5f;

    [Header("Boss Spawn")]
    [SerializeField] private bool useSceneBossSpawnPoints = true;
    [SerializeField] private float spawnNorthDistance = 12f;
    [SerializeField] private float spawnTopPadding = 1.75f;

    [Header("Boss Selection")]
    [Tooltip("Optional. If empty, the director will try to use a ranged fire enemy from the active spawn pool.")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private string preferredBossPrefabName = "RangedEnemy_Fire";

    [Header("Dragon Tuning")]
    [SerializeField] private float healthMultiplier = 10f;
    [SerializeField] private float rewardMultiplier = 8f;
    [SerializeField] private float scaleMultiplier = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float pickupDropChanceBonus = 0.75f;
    [SerializeField] private Color tintColor = new Color(1f, 0.35f, 0.15f, 1f);

    [Header("Dragon Volley")]
    [SerializeField] private GameObject overrideProjectilePrefab;
    [SerializeField] private float specialVolleyCooldown = 4.5f;
    [SerializeField] private int volleyProjectileCount = 5;
    [SerializeField] private float volleySpreadAngle = 55f;
    [SerializeField] private float projectileSpeedMultiplier = 1f;

    [Header("Announcements")]
    [SerializeField] private bool showAnnouncements = true;
    [SerializeField] private string bossSpawnMessage = "THE DRAGON DESCENDS";
    [SerializeField] private string bossDefeatedMessage = "DRAGON SLAIN";
    [SerializeField] private float announcementDuration = 2.75f;
    [SerializeField] private string bossRewardTitle = "Choose a Boss Reward";
    [SerializeField] private Color bossRewardTitleColor = new Color(1f, 0.88f, 0.35f, 1f);
    [SerializeField] private Color bossRewardPanelColor = new Color(0.18f, 0.06f, 0.08f, 0.92f);

    private bool _hasSpawned;
    private GameObject _activeBoss;
    private Transform _player;
    private float _nextRetryTime;
    private bool _thresholdReached;

    private static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_sceneHookRegistered)
            return;

        SceneManager.sceneLoaded += HandleSceneLoadedStatic;
        _sceneHookRegistered = true;
    }

    private static void HandleSceneLoadedStatic(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid() || scene.name != "Game")
            return;

        if (FindObjectOfType<BossSpawnDirector>() != null)
            return;

        if (FindObjectOfType<RunTimer>() == null || FindObjectOfType<EnemyRespawnManager>() == null)
            return;

        GameObject go = new GameObject("BossSpawnDirector");
        go.AddComponent<BossSpawnDirector>();
    }

    private void OnEnable()
    {
        FindReferencesIfNeeded();

        if (runTimer != null)
            runTimer.WholeSecondChanged += HandleWholeSecondChanged;
    }

    private void Start()
    {
        FindReferencesIfNeeded();

        int currentSecond = runTimer != null ? runTimer.WholeSeconds : 0;
        HandleWholeSecondChanged(currentSecond);
    }

    private void Update()
    {
        if (!retrySpawnAfterThreshold || !ShouldAttemptSpawn())
            return;

        if (Time.unscaledTime < _nextRetryTime)
            return;

        _nextRetryTime = Time.unscaledTime + Mathf.Max(0.1f, retryIntervalSeconds);
        TrySpawnBoss("retry");
    }

    private void OnDisable()
    {
        if (runTimer != null)
            runTimer.WholeSecondChanged -= HandleWholeSecondChanged;
    }

    private void HandleWholeSecondChanged(int wholeSecond)
    {
        if (wholeSecond < Mathf.FloorToInt(Mathf.Max(0f, firstBossTimeSeconds)))
            return;

        _thresholdReached = true;
        TrySpawnBoss("timer");
    }

    private bool ShouldAttemptSpawn()
    {
        if (!spawnBoss || respawnManager == null)
            return false;

        if (_activeBoss != null)
            return false;

        if (spawnOnlyOnce && _hasSpawned)
            return false;

        if (_thresholdReached)
            return true;

        if (runTimer == null)
            return false;

        return runTimer.ElapsedSeconds >= Mathf.Max(0f, firstBossTimeSeconds);
    }

    private void TrySpawnBoss(string source)
    {
        if (!ShouldAttemptSpawn())
            return;

        GameObject prefab = ResolveBossPrefab();
        if (prefab == null)
        {
            Debug.LogWarning($"BossSpawnDirector could not find a suitable boss prefab. Source={source}");
            return;
        }

        if (!TryGetBossSpawnPosition(out Vector3 spawnPosition))
        {
            Debug.LogWarning($"BossSpawnDirector could not resolve a spawn position. Source={source}");
            return;
        }

        GameObject spawnedBoss = Instantiate(prefab, spawnPosition, Quaternion.identity);
        if (spawnedBoss == null)
        {
            Debug.LogWarning($"BossSpawnDirector failed to instantiate boss prefab '{prefab.name}'. Source={source}");
            return;
        }

        GameObject projectilePrefab = ResolveProjectilePrefab(prefab, spawnedBoss);
        if (projectilePrefab == null)
            Debug.LogWarning("BossSpawnDirector spawned a boss, but no projectile prefab was available for the dragon volley.", spawnedBoss);

        DragonBoss dragonBoss = spawnedBoss.GetComponent<DragonBoss>();
        if (dragonBoss == null)
            dragonBoss = spawnedBoss.AddComponent<DragonBoss>();

        dragonBoss.Configure(
            healthMultiplier,
            rewardMultiplier,
            pickupDropChanceBonus,
            scaleMultiplier,
            tintColor,
            projectilePrefab,
            specialVolleyCooldown,
            volleyProjectileCount,
            volleySpreadAngle,
            projectileSpeedMultiplier);

        EnemyHealth health = spawnedBoss.GetComponent<EnemyHealth>();
        if (health != null)
            health.Died += HandleBossDied;

        _activeBoss = spawnedBoss;
        _hasSpawned = true;
        _thresholdReached = false;

        ShowAnnouncement(bossSpawnMessage);
        GameAudio.PlayEliteSpawn();
        Debug.Log($"BOSS SPAWNED: {spawnedBoss.name} at {spawnPosition} via {source}");
    }

    private void HandleBossDied(EnemyHealth health)
    {
        if (health == null)
            return;

        health.Died -= HandleBossDied;
        _activeBoss = null;

        ShowAnnouncement(bossDefeatedMessage);
        GameAudio.PlayEliteDefeated();
        OfferBossReward();
        Debug.Log($"BOSS DEFEATED: {health.name}");
    }

    private GameObject ResolveBossPrefab()
    {
        if (bossPrefab != null)
            return bossPrefab;

        GameObject[] prefabs = respawnManager != null ? respawnManager.GetEnemyPrefabs() : null;
        if (prefabs == null || prefabs.Length == 0)
            return null;

        GameObject preferred = FindPrefabContaining(prefabs, preferredBossPrefabName, requireRanged: true);
        if (preferred != null)
            return preferred;

        preferred = FindPrefabContaining(prefabs, "RangedEnemy_Fire", requireRanged: true);
        if (preferred != null)
            return preferred;

        preferred = FindFirstRangedPrefab(prefabs);
        if (preferred != null)
            return preferred;

        return prefabs[0];
    }

    private GameObject ResolveProjectilePrefab(GameObject prefab, GameObject spawnedBoss)
    {
        if (overrideProjectilePrefab != null)
            return overrideProjectilePrefab;

        RangedShooter shooter = spawnedBoss != null ? spawnedBoss.GetComponent<RangedShooter>() : null;
        if (shooter != null && shooter.EnemyProjectilePrefab != null)
            return shooter.EnemyProjectilePrefab;

        shooter = prefab != null ? prefab.GetComponent<RangedShooter>() : null;
        return shooter != null ? shooter.EnemyProjectilePrefab : null;
    }

    private static GameObject FindPrefabContaining(GameObject[] prefabs, string token, bool requireRanged)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            if (prefab == null)
                continue;

            if (requireRanged && prefab.GetComponent<RangedShooter>() == null)
                continue;

            if (prefab.name.IndexOf(token, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return prefab;
        }

        return null;
    }

    private static GameObject FindFirstRangedPrefab(GameObject[] prefabs)
    {
        for (int i = 0; i < prefabs.Length; i++)
        {
            if (prefabs[i] != null && prefabs[i].GetComponent<RangedShooter>() != null)
                return prefabs[i];
        }

        return null;
    }

    private void ShowAnnouncement(string message)
    {
        if (!showAnnouncements || string.IsNullOrWhiteSpace(message) || RunAnnouncementUI.Instance == null)
            return;

        RunAnnouncementUI.Instance.ShowMessage(message, announcementDuration);
    }

    private bool TryGetBossSpawnPosition(out Vector3 spawnPosition)
    {
        spawnPosition = default;

        if (useSceneBossSpawnPoints && TryGetSceneBossSpawnPoint(out BossSpawnPoint bossSpawnPoint))
        {
            spawnPosition = bossSpawnPoint.Position;
            return true;
        }

        if (_player == null)
        {
            Debug.LogWarning("BossSpawnDirector has no player reference when trying to calculate fallback spawn position.");
            return false;
        }

        Camera camera = Camera.main;
        float northDistance = Mathf.Max(1f, spawnNorthDistance > 0f ? spawnNorthDistance : DefaultBossSpawnDistance);
        Vector3 playerPosition = _player.position;
        float spawnY = playerPosition.y + northDistance;

        if (camera != null && camera.orthographic)
        {
            float cameraTopY = camera.transform.position.y + camera.orthographicSize + Mathf.Max(0f, spawnTopPadding);
            spawnY = Mathf.Max(spawnY, cameraTopY);
        }

        spawnPosition = new Vector3(playerPosition.x, spawnY, 0f);
        return true;
    }

    private bool TryGetSceneBossSpawnPoint(out BossSpawnPoint chosenSpawnPoint)
    {
        chosenSpawnPoint = null;

        if (bossSpawnPoints == null || bossSpawnPoints.Length == 0)
            bossSpawnPoints = FindObjectsOfType<BossSpawnPoint>();

        if (bossSpawnPoints == null || bossSpawnPoints.Length == 0)
            return false;

        float bestScore = float.MinValue;

        for (int i = 0; i < bossSpawnPoints.Length; i++)
        {
            BossSpawnPoint spawnPoint = bossSpawnPoints[i];
            if (spawnPoint == null)
                continue;

            Vector3 position = spawnPoint.Position;
            float horizontalPenalty = _player != null ? Mathf.Abs(position.x - _player.position.x) * 0.1f : 0f;
            float score = spawnPoint.Priority * 1000f + position.y - horizontalPenalty;

            if (score > bestScore)
            {
                bestScore = score;
                chosenSpawnPoint = spawnPoint;
            }
        }

        return chosenSpawnPoint != null;
    }

    private void OfferBossReward()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        List<PlayerUpgradeOption> choices = PickBossRewardChoices();
        if (choices.Count == 0)
            return;

        if (LevelUpManager.Instance != null &&
            LevelUpManager.Instance.ShowCustomChoices(
                choices,
                chosen => chosen.Apply(player),
                bossRewardTitle,
                bossRewardTitleColor,
                bossRewardPanelColor))
        {
            return;
        }

        PlayerUpgradeOption fallback = choices[0];
        Debug.Log($"No LevelUpManager found for boss reward. Auto-picking: {fallback.Title}");
        fallback.Apply(player);
    }

    private List<PlayerUpgradeOption> PickBossRewardChoices()
    {
        PlayerUpgradeOption[] pool = PlayerUpgradeOption.CreateBossRewardPool();
        List<PlayerUpgradeOption> available = new List<PlayerUpgradeOption>(pool);
        List<PlayerUpgradeOption> choices = new List<PlayerUpgradeOption>();
        int count = Mathf.Min(3, available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            choices.Add(available[index]);
            available.RemoveAt(index);
        }

        return choices;
    }

    private void FindReferencesIfNeeded()
    {
        if (runTimer == null)
            runTimer = RunTimer.Instance != null ? RunTimer.Instance : FindObjectOfType<RunTimer>();

        if (respawnManager == null)
            respawnManager = FindObjectOfType<EnemyRespawnManager>();

        if (_player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                _player = playerObject.transform;
        }

        if ((bossSpawnPoints == null || bossSpawnPoints.Length == 0) && useSceneBossSpawnPoints)
            bossSpawnPoints = FindObjectsOfType<BossSpawnPoint>();
    }

    private void OnValidate()
    {
        firstBossTimeSeconds = Mathf.Max(0f, firstBossTimeSeconds);
        retryIntervalSeconds = Mathf.Max(0.1f, retryIntervalSeconds);
        spawnNorthDistance = Mathf.Max(1f, spawnNorthDistance);
        spawnTopPadding = Mathf.Max(0f, spawnTopPadding);
        healthMultiplier = Mathf.Max(1f, healthMultiplier);
        rewardMultiplier = Mathf.Max(1f, rewardMultiplier);
        scaleMultiplier = Mathf.Max(1f, scaleMultiplier);
        specialVolleyCooldown = Mathf.Max(0.5f, specialVolleyCooldown);
        volleyProjectileCount = Mathf.Max(3, volleyProjectileCount);
        volleySpreadAngle = Mathf.Clamp(volleySpreadAngle, 5f, 180f);
        projectileSpeedMultiplier = Mathf.Max(0.1f, projectileSpeedMultiplier);
    }
}
