using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FactionSpawnRule
{
    public FactionUnitArchetypeType archetype = FactionUnitArchetypeType.ZombieMelee;
    [Min(0)]
    public int maxAlive = 4;
    public GameObject prefabOverride;
    public EnemySpawnRegion allowedSpawnRegions = EnemySpawnRegion.Any;
    public bool rewardsEnabled = true;
}

public class EnemyRespawnManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Points (optional - auto finds if empty)")]
    [SerializeField] private EnemySpawnPoint[] spawnPoints;

    [Header("Rules")]
    [SerializeField] private int maxAlive = 8;
    [SerializeField] private float respawnDelay = 4f;
    [SerializeField] private float minDistanceFromPlayer = 6f;
    [SerializeField] private float minDistanceBetweenEnemies = 3f;
    [SerializeField] private bool preferFarthestSpawn = true;
    [SerializeField] private int triesPerSpawn = 12;

    [Header("Dynamic Spawn (disabled for now)")]
    [SerializeField] private bool useDynamicSpawn = false;
    [SerializeField] private float spawnRadiusMin = 8f;
    [SerializeField] private float spawnRadiusMax = 12f;

    [Header("Faction Spawn Rules (optional)")]
    [SerializeField] private bool useFactionSpawnRules = false;
    [SerializeField] private FactionSpawnRule[] factionSpawnRules;

    private readonly HashSet<GameObject> _alive = new HashSet<GameObject>();
    private readonly Dictionary<GameObject, int> _factionRuleByEnemy = new Dictionary<GameObject, int>();
    private readonly Dictionary<FactionUnitArchetypeType, GameObject> _factionPrefabCache = new Dictionary<FactionUnitArchetypeType, GameObject>();
    private Transform _player;
    private EnemySpawnRegion _allowedSpawnRegions = EnemySpawnRegion.Any;
    private Coroutine _fillRoutine;

    public int MaxAlive => useFactionSpawnRules ? GetFactionRuleTotalCap() : maxAlive;
    public float RespawnDelay => respawnDelay;

    public GameObject[] GetEnemyPrefabs()
    {
        return enemyPrefabs;
    }

    private void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        RefreshSpawnPoints();

        if (FindObjectOfType<EnemyWaveDirector>() != null)
            return;

        FillToCap();
    }

    private void OnEnable()
    {
        if (_fillRoutine == null)
            _fillRoutine = StartCoroutine(ContinuousFillRoutine());
    }

    private void OnDisable()
    {
        if (_fillRoutine != null)
        {
            StopCoroutine(_fillRoutine);
            _fillRoutine = null;
        }
    }

    private void FillToCap()
    {
        RemoveMissingEnemies();

        if (useFactionSpawnRules)
        {
            FillFactionRulesToCaps();
            return;
        }

        while (_alive.Count < maxAlive)
        {
            if (!TrySpawnOne())
                break;
        }
    }

    public void ApplyWaveSettings(
        GameObject[] waveEnemyPrefabs,
        int waveMaxAlive,
        float waveRespawnDelay,
        bool fillImmediately,
        EnemySpawnRegion allowedSpawnRegions = EnemySpawnRegion.Any)
    {
        if (waveEnemyPrefabs != null && waveEnemyPrefabs.Length > 0)
            enemyPrefabs = waveEnemyPrefabs;

        useFactionSpawnRules = false;
        _factionRuleByEnemy.Clear();
        maxAlive = Mathf.Max(0, waveMaxAlive);
        respawnDelay = Mathf.Max(0.05f, waveRespawnDelay);
        _allowedSpawnRegions = allowedSpawnRegions == EnemySpawnRegion.None
            ? EnemySpawnRegion.Any
            : allowedSpawnRegions;

        if (fillImmediately)
            FillToCap();
    }

    public void ApplyFactionWaveSettings(
        FactionSpawnRule[] waveFactionSpawnRules,
        float waveRespawnDelay,
        bool fillImmediately,
        EnemySpawnRegion fallbackAllowedSpawnRegions = EnemySpawnRegion.Any)
    {
        factionSpawnRules = waveFactionSpawnRules;
        useFactionSpawnRules = HasAnyFactionSpawnRules(factionSpawnRules);
        respawnDelay = Mathf.Max(0.05f, waveRespawnDelay);
        _allowedSpawnRegions = fallbackAllowedSpawnRegions == EnemySpawnRegion.None
            ? EnemySpawnRegion.Any
            : fallbackAllowedSpawnRegions;
        maxAlive = GetFactionRuleTotalCap();

        AssignExistingFactionEnemiesToRules();

        if (fillImmediately)
            FillToCap();
    }

    private bool TrySpawnOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return false;

        GameObject prefab = GetRandomEnemyPrefab();
        return TrySpawnPrefab(prefab, out _);
    }

    public GameObject GetRandomEnemyPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return null;

        return enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
    }

    public bool TrySpawnSpecial(
        GameObject prefab,
        out GameObject spawnedEnemy,
        bool ignoreEnemySpacing = false,
        bool ignorePlayerDistance = false)
    {
        return TrySpawnPrefab(prefab, out spawnedEnemy, ignoreEnemySpacing, ignorePlayerDistance);
    }

    private bool TrySpawnPrefab(
        GameObject prefab,
        out GameObject spawnedEnemy,
        bool ignoreEnemySpacing = false,
        bool ignorePlayerDistance = false,
        EnemySpawnRegion allowedSpawnRegionsOverride = EnemySpawnRegion.None)
    {
        spawnedEnemy = null;

        if (prefab == null)
            return false;

        Vector2 spawnPos;
        bool foundSpawn;

        // ACTIVE MODE:
        // Use spawn points for now because this is easier to balance.
        foundSpawn = preferFarthestSpawn
            ? TryGetFarthestValidSpawn(out spawnPos, ignoreEnemySpacing, ignorePlayerDistance, allowedSpawnRegionsOverride)
            : TryGetRandomValidSpawn(out spawnPos, ignoreEnemySpacing, ignorePlayerDistance, allowedSpawnRegionsOverride);

        /*
        // FUTURE MODE:
        // Re-enable this later if you want dynamic spawning again.
        bool foundSpawn = useDynamicSpawn
            ? TryGetDynamicSpawn(out spawnPos)
            : (preferFarthestSpawn
                ? TryGetFarthestValidSpawn(out spawnPos)
                : TryGetRandomValidSpawn(out spawnPos));
        */

        if (!foundSpawn)
            return false;

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        spawnedEnemy = go;
        _alive.Add(go);

        EnemyHealth hp = go.GetComponent<EnemyHealth>();
        if (hp != null)
            hp.Died += OnEnemyDied;

        return true;
    }

    private void FillFactionRulesToCaps()
    {
        if (!HasAnyFactionSpawnRules(factionSpawnRules))
            return;

        for (int i = 0; i < factionSpawnRules.Length; i++)
        {
            FactionSpawnRule rule = factionSpawnRules[i];
            if (rule == null || rule.maxAlive <= 0)
                continue;

            int aliveForRule = CountAliveForFactionRule(i);
            int failedAttempts = 0;

            while (aliveForRule < rule.maxAlive)
            {
                if (TrySpawnFactionRule(i))
                {
                    aliveForRule++;
                    failedAttempts = 0;
                    continue;
                }

                failedAttempts++;
                if (failedAttempts >= triesPerSpawn)
                    break;
            }
        }
    }

    private bool TrySpawnFactionRule(int ruleIndex)
    {
        if (factionSpawnRules == null || ruleIndex < 0 || ruleIndex >= factionSpawnRules.Length)
            return false;

        FactionSpawnRule rule = factionSpawnRules[ruleIndex];
        if (rule == null || rule.maxAlive <= 0)
            return false;

        GameObject prefab = ResolveFactionPrefab(rule);
        if (prefab == null)
        {
            Debug.LogWarning($"No faction prefab found for {rule.archetype}. Run Tools > Bullet Heaven > Factions > Create Starter Prefabs, or assign a prefab override.", this);
            return false;
        }

        EnemySpawnRegion allowedRegions = rule.allowedSpawnRegions == EnemySpawnRegion.None
            ? _allowedSpawnRegions
            : rule.allowedSpawnRegions;

        if (!TrySpawnPrefab(prefab, out GameObject spawnedEnemy, allowedSpawnRegionsOverride: allowedRegions))
            return false;

        FactionUnitArchetype.ApplyTo(spawnedEnemy, rule.archetype, rule.rewardsEnabled);
        _factionRuleByEnemy[spawnedEnemy] = ruleIndex;
        return true;
    }

    private GameObject ResolveFactionPrefab(FactionSpawnRule rule)
    {
        if (rule == null)
            return null;

        if (rule.prefabOverride != null)
            return rule.prefabOverride;

        if (_factionPrefabCache.TryGetValue(rule.archetype, out GameObject cachedPrefab) && cachedPrefab != null)
            return cachedPrefab;

        GameObject prefab = LoadFactionPrefab(rule.archetype);
        _factionPrefabCache[rule.archetype] = prefab;
        return prefab;
    }

    private GameObject LoadFactionPrefab(FactionUnitArchetypeType archetype)
    {
        string primaryPath = GetFactionPrefabPath(archetype);
        GameObject prefab = Resources.Load<GameObject>(primaryPath);

        if (prefab != null)
            return prefab;

        string fallbackPath = GetFactionPrefabFallbackPath(archetype);
        return string.IsNullOrEmpty(fallbackPath)
            ? null
            : Resources.Load<GameObject>(fallbackPath);
    }

    private string GetFactionPrefabPath(FactionUnitArchetypeType archetype)
    {
        switch (archetype)
        {
            case FactionUnitArchetypeType.HumanMeleeAlly:
                return "Prefabs/Factions/HumanAlly_Melee";
            case FactionUnitArchetypeType.HumanSupport:
            case FactionUnitArchetypeType.HumanRangedAlly:
                return "Prefabs/Factions/HumanAlly_Ranged";
            case FactionUnitArchetypeType.AngelMelee:
                return "Prefabs/Factions/Angel_Melee";
            case FactionUnitArchetypeType.AngelMarksman:
            case FactionUnitArchetypeType.AngelRanged:
                return "Prefabs/Factions/Angel_Ranged";
            case FactionUnitArchetypeType.DemonRaider:
            case FactionUnitArchetypeType.DemonMelee:
                return "Prefabs/Factions/Demon_Melee";
            case FactionUnitArchetypeType.DemonRanged:
                return "Prefabs/Factions/Demon_Ranged";
            case FactionUnitArchetypeType.ZombieRanged:
                return "Prefabs/Factions/Zombie_Ranged";
            case FactionUnitArchetypeType.ZombieGrunt:
            case FactionUnitArchetypeType.ZombieMelee:
            default:
                return "Prefabs/Factions/Zombie_Melee";
        }
    }

    private string GetFactionPrefabFallbackPath(FactionUnitArchetypeType archetype)
    {
        switch (archetype)
        {
            case FactionUnitArchetypeType.HumanMeleeAlly:
            case FactionUnitArchetypeType.HumanSupport:
            case FactionUnitArchetypeType.HumanRangedAlly:
                return "Prefabs/Factions/HumanAlly";
            case FactionUnitArchetypeType.AngelMelee:
            case FactionUnitArchetypeType.AngelMarksman:
            case FactionUnitArchetypeType.AngelRanged:
                return "Prefabs/Factions/AngelTestUnit";
            case FactionUnitArchetypeType.DemonRaider:
            case FactionUnitArchetypeType.DemonMelee:
            case FactionUnitArchetypeType.DemonRanged:
                return "Prefabs/Factions/DemonTestUnit";
            case FactionUnitArchetypeType.ZombieGrunt:
            case FactionUnitArchetypeType.ZombieMelee:
            case FactionUnitArchetypeType.ZombieRanged:
            default:
                return "Prefabs/Factions/ZombieTestUnit";
        }
    }

    /*
    private bool TryGetDynamicSpawn(out Vector2 spawnPos)
    {
        spawnPos = default;

        if (_player == null)
            return false;

        if (spawnRadiusMax < spawnRadiusMin)
            spawnRadiusMax = spawnRadiusMin;

        for (int i = 0; i < triesPerSpawn; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;

            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.right;

            float dist = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector2 pos = (Vector2)_player.position + dir * dist;

            if (!IsSpawnValid(pos))
                continue;

            spawnPos = pos;
            return true;
        }

        return false;
    }
    */

    private bool TryGetRandomValidSpawn(
        out Vector2 spawnPos,
        bool ignoreEnemySpacing = false,
        bool ignorePlayerDistance = false,
        EnemySpawnRegion allowedSpawnRegionsOverride = EnemySpawnRegion.None)
    {
        spawnPos = default;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;

        List<EnemySpawnPoint> candidates = GetCandidateSpawnPoints(allowedSpawnRegionsOverride);
        if (candidates.Count == 0)
            return false;

        for (int i = 0; i < triesPerSpawn; i++)
        {
            EnemySpawnPoint sp = candidates[Random.Range(0, candidates.Count)];
            if (sp == null)
                continue;

            Vector2 pos = sp.Position;

            if (!IsSpawnValid(pos, ignoreEnemySpacing, ignorePlayerDistance))
                continue;

            spawnPos = pos;
            return true;
        }

        return false;
    }

    private bool TryGetFarthestValidSpawn(
        out Vector2 spawnPos,
        bool ignoreEnemySpacing = false,
        bool ignorePlayerDistance = false,
        EnemySpawnRegion allowedSpawnRegionsOverride = EnemySpawnRegion.None)
    {
        spawnPos = default;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;

        List<EnemySpawnPoint> candidates = GetCandidateSpawnPoints(allowedSpawnRegionsOverride);
        if (candidates.Count == 0)
            return false;

        float bestScore = float.MinValue;
        bool found = false;

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i] == null)
                continue;

            Vector2 pos = candidates[i].Position;

            if (!IsSpawnValid(pos, ignoreEnemySpacing, ignorePlayerDistance))
                continue;

            float distToPlayer = _player == null
                ? 0f
                : Vector2.Distance(pos, _player.position);

            if (distToPlayer > bestScore)
            {
                bestScore = distToPlayer;
                spawnPos = pos;
                found = true;
            }
        }

        return found;
    }

    private List<EnemySpawnPoint> GetCandidateSpawnPoints(EnemySpawnRegion allowedSpawnRegionsOverride = EnemySpawnRegion.None)
    {
        List<EnemySpawnPoint> candidates = new List<EnemySpawnPoint>();

        if (spawnPoints == null || spawnPoints.Length == 0)
            return candidates;

        EnemySpawnRegion allowedRegions = allowedSpawnRegionsOverride == EnemySpawnRegion.None
            ? _allowedSpawnRegions
            : allowedSpawnRegionsOverride;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            EnemySpawnPoint spawnPoint = spawnPoints[i];
            if (spawnPoint == null)
                continue;

            if (!IsSpawnPointAllowed(spawnPoint, allowedRegions))
                continue;

            candidates.Add(spawnPoint);
        }

        if (candidates.Count > 0)
            return candidates;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null)
                candidates.Add(spawnPoints[i]);
        }

        return candidates;
    }

    private bool IsSpawnPointAllowed(EnemySpawnPoint spawnPoint, EnemySpawnRegion allowedRegions)
    {
        if (allowedRegions == EnemySpawnRegion.Any)
            return true;

        return (spawnPoint.SpawnRegions & allowedRegions) != 0;
    }

    private void OnEnemyDied(EnemyHealth hp)
    {
        if (hp == null)
            return;

        hp.Died -= OnEnemyDied;
        _alive.Remove(hp.gameObject);
        _factionRuleByEnemy.Remove(hp.gameObject);
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0.05f, respawnDelay));
        FillToCap();
    }

    private IEnumerator ContinuousFillRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.05f, respawnDelay));
            FillToCap();
        }
    }

    private void RemoveMissingEnemies()
    {
        _alive.RemoveWhere(enemy => enemy == null);

        if (_factionRuleByEnemy.Count == 0)
            return;

        List<GameObject> missing = null;
        foreach (GameObject enemy in _factionRuleByEnemy.Keys)
        {
            if (enemy != null)
                continue;

            if (missing == null)
                missing = new List<GameObject>();

            missing.Add(enemy);
        }

        if (missing == null)
            return;

        for (int i = 0; i < missing.Count; i++)
            _factionRuleByEnemy.Remove(missing[i]);
    }

    private void AssignExistingFactionEnemiesToRules()
    {
        _factionRuleByEnemy.Clear();

        if (!HasAnyFactionSpawnRules(factionSpawnRules))
            return;

        foreach (GameObject enemy in _alive)
        {
            if (enemy == null)
                continue;

            FactionUnitArchetype archetype = enemy.GetComponent<FactionUnitArchetype>();
            if (archetype == null)
                continue;

            int ruleIndex = FindFirstRuleIndexForArchetype(archetype.Archetype);
            if (ruleIndex >= 0)
                _factionRuleByEnemy[enemy] = ruleIndex;
        }
    }

    private int CountAliveForFactionRule(int ruleIndex)
    {
        int count = 0;

        foreach (var pair in _factionRuleByEnemy)
        {
            if (pair.Key == null || pair.Value != ruleIndex)
                continue;

            count++;
        }

        return count;
    }

    private int FindFirstRuleIndexForArchetype(FactionUnitArchetypeType archetype)
    {
        if (factionSpawnRules == null)
            return -1;

        for (int i = 0; i < factionSpawnRules.Length; i++)
        {
            FactionSpawnRule rule = factionSpawnRules[i];
            if (rule != null && rule.archetype == archetype)
                return i;
        }

        return -1;
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

    private int GetFactionRuleTotalCap()
    {
        if (factionSpawnRules == null)
            return 0;

        int total = 0;
        for (int i = 0; i < factionSpawnRules.Length; i++)
        {
            if (factionSpawnRules[i] != null)
                total += Mathf.Max(0, factionSpawnRules[i].maxAlive);
        }

        return total;
    }

    private void RefreshSpawnPoints()
    {
        EnemySpawnPoint[] sceneSpawnPoints = FindObjectsOfType<EnemySpawnPoint>();

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            spawnPoints = sceneSpawnPoints;
            return;
        }

        List<EnemySpawnPoint> merged = new List<EnemySpawnPoint>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] != null && !merged.Contains(spawnPoints[i]))
                merged.Add(spawnPoints[i]);
        }

        for (int i = 0; i < sceneSpawnPoints.Length; i++)
        {
            if (sceneSpawnPoints[i] != null && !merged.Contains(sceneSpawnPoints[i]))
                merged.Add(sceneSpawnPoints[i]);
        }

        spawnPoints = merged.ToArray();
    }

    private bool IsSpawnValid(
        Vector2 pos,
        bool ignoreEnemySpacing = false,
        bool ignorePlayerDistance = false)
    {
        if (!ignorePlayerDistance && _player != null)
        {
            float distToPlayer = Vector2.Distance(pos, _player.position);
            if (distToPlayer < minDistanceFromPlayer)
                return false;
        }

        if (ignoreEnemySpacing)
            return true;

        foreach (GameObject enemy in _alive)
        {
            if (enemy == null)
                continue;

            float distToEnemy = Vector2.Distance(pos, enemy.transform.position);
            if (distToEnemy < minDistanceBetweenEnemies)
                return false;
        }

        return true;
    }
}
