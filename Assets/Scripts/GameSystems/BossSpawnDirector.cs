using UnityEngine;

public class BossSpawnDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunTimer runTimer;
    [SerializeField] private EnemyRespawnManager respawnManager;

    [Header("Schedule")]
    [SerializeField] private bool spawnBoss = true;
    [SerializeField] private float firstBossTimeSeconds = 20f;
    [SerializeField] private bool spawnOnlyOnce = true;

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

    private bool _hasSpawned;
    private GameObject _activeBoss;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
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

    private void OnDisable()
    {
        if (runTimer != null)
            runTimer.WholeSecondChanged -= HandleWholeSecondChanged;
    }

    private void HandleWholeSecondChanged(int wholeSecond)
    {
        if (!spawnBoss || respawnManager == null)
            return;

        if (_activeBoss != null)
            return;

        if (spawnOnlyOnce && _hasSpawned)
            return;

        if (wholeSecond < Mathf.FloorToInt(Mathf.Max(0f, firstBossTimeSeconds)))
            return;

        TrySpawnBoss();
    }

    private void TrySpawnBoss()
    {
        GameObject prefab = ResolveBossPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("BossSpawnDirector could not find a suitable boss prefab.");
            return;
        }

        if (!respawnManager.TrySpawnSpecial(prefab, out GameObject spawnedBoss) || spawnedBoss == null)
            return;

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

        ShowAnnouncement(bossSpawnMessage);
        GameAudio.PlayEliteSpawn();
        Debug.Log($"BOSS SPAWNED: {spawnedBoss.name}");
    }

    private void HandleBossDied(EnemyHealth health)
    {
        if (health == null)
            return;

        health.Died -= HandleBossDied;
        _activeBoss = null;

        ShowAnnouncement(bossDefeatedMessage);
        GameAudio.PlayEliteDefeated();
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

    private void FindReferencesIfNeeded()
    {
        if (runTimer == null)
            runTimer = RunTimer.Instance != null ? RunTimer.Instance : FindObjectOfType<RunTimer>();

        if (respawnManager == null)
            respawnManager = FindObjectOfType<EnemyRespawnManager>();
    }

    private void OnValidate()
    {
        firstBossTimeSeconds = Mathf.Max(0f, firstBossTimeSeconds);
        healthMultiplier = Mathf.Max(1f, healthMultiplier);
        rewardMultiplier = Mathf.Max(1f, rewardMultiplier);
        scaleMultiplier = Mathf.Max(1f, scaleMultiplier);
        specialVolleyCooldown = Mathf.Max(0.5f, specialVolleyCooldown);
        volleyProjectileCount = Mathf.Max(3, volleyProjectileCount);
        volleySpreadAngle = Mathf.Clamp(volleySpreadAngle, 5f, 180f);
        projectileSpeedMultiplier = Mathf.Max(0.1f, projectileSpeedMultiplier);
    }
}
