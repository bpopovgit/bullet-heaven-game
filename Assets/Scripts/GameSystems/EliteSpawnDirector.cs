using System.Collections.Generic;
using UnityEngine;

public class EliteSpawnDirector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunTimer runTimer;
    [SerializeField] private EnemyRespawnManager respawnManager;

    [Header("Schedule")]
    [SerializeField] private bool spawnElites = true;
    [SerializeField] private float firstEliteTimeSeconds = 90f;
    [SerializeField] private float eliteIntervalSeconds = 90f;
    [SerializeField] private int maxElitesAlive = 1;

    [Header("Elite Prefabs")]
    [Tooltip("Optional. If empty, the director uses the respawn manager's current enemy pool.")]
    [SerializeField] private GameObject[] elitePrefabs;

    [Header("Elite Modifiers")]
    [SerializeField] private float healthMultiplier = 4f;
    [SerializeField] private float rewardMultiplier = 5f;
    [SerializeField] private float scaleMultiplier = 1.4f;
    [Range(0f, 1f)]
    [SerializeField] private float pickupDropChanceBonus = 0.25f;
    [SerializeField] private Color tintColor = new Color(1f, 0.78f, 0.15f);

    [Header("Announcements")]
    [SerializeField] private bool showAnnouncements = true;
    [SerializeField] private string eliteSpawnMessage = "ELITE INCOMING";
    [SerializeField] private string eliteDefeatedMessage = "ELITE DEFEATED";
    [SerializeField] private float announcementDuration = 2f;

    private readonly HashSet<GameObject> _aliveElites = new HashSet<GameObject>();
    private float _nextEliteTime;

    private void Awake()
    {
        if (!GameTuning.Instance.elitesEnabled)
        {
            enabled = false;
            return;
        }

        _nextEliteTime = firstEliteTimeSeconds;
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
        _nextEliteTime = Mathf.Max(0f, firstEliteTimeSeconds);
    }

    private void OnDisable()
    {
        if (runTimer != null)
            runTimer.WholeSecondChanged -= HandleWholeSecondChanged;
    }

    private void HandleWholeSecondChanged(int wholeSecond)
    {
        if (!spawnElites)
            return;

        RemoveMissingElites();

        if (wholeSecond < _nextEliteTime || _aliveElites.Count >= maxElitesAlive)
            return;

        if (TrySpawnElite())
            _nextEliteTime = wholeSecond + Mathf.Max(1f, eliteIntervalSeconds);
    }

    private bool TrySpawnElite()
    {
        if (respawnManager == null)
            return false;

        GameObject prefab = GetElitePrefab();
        if (prefab == null)
            return false;

        if (!respawnManager.TrySpawnSpecial(prefab, out GameObject spawnedEnemy))
            return false;

        EliteEnemy elite = spawnedEnemy.GetComponent<EliteEnemy>();
        if (elite == null)
            elite = spawnedEnemy.AddComponent<EliteEnemy>();

        elite.Configure(
            healthMultiplier,
            rewardMultiplier,
            pickupDropChanceBonus,
            scaleMultiplier,
            tintColor);

        _aliveElites.Add(spawnedEnemy);

        EnemyHealth health = spawnedEnemy.GetComponent<EnemyHealth>();
        if (health != null)
            health.Died += HandleEliteDied;

        ShowAnnouncement(eliteSpawnMessage);
        GameAudio.PlayEliteSpawn();
        Debug.Log($"ELITE SPAWNED: {spawnedEnemy.name}");
        return true;
    }

    private void HandleEliteDied(EnemyHealth health)
    {
        if (health == null)
            return;

        health.Died -= HandleEliteDied;
        _aliveElites.Remove(health.gameObject);

        ShowAnnouncement(eliteDefeatedMessage);
        GameAudio.PlayEliteDefeated();
        Debug.Log($"ELITE DEFEATED: {health.name}");
    }

    private GameObject GetElitePrefab()
    {
        if (elitePrefabs != null && elitePrefabs.Length > 0)
            return elitePrefabs[Random.Range(0, elitePrefabs.Length)];

        return respawnManager != null ? respawnManager.GetRandomEnemyPrefab() : null;
    }

    private void RemoveMissingElites()
    {
        _aliveElites.RemoveWhere(enemy => enemy == null);
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
        firstEliteTimeSeconds = Mathf.Max(0f, firstEliteTimeSeconds);
        eliteIntervalSeconds = Mathf.Max(1f, eliteIntervalSeconds);
        maxElitesAlive = Mathf.Max(0, maxElitesAlive);
        healthMultiplier = Mathf.Max(1f, healthMultiplier);
        rewardMultiplier = Mathf.Max(1f, rewardMultiplier);
        scaleMultiplier = Mathf.Max(1f, scaleMultiplier);
    }
}
