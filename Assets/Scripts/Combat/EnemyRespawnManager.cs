using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private readonly HashSet<GameObject> _alive = new HashSet<GameObject>();
    private Transform _player;

    private void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (spawnPoints == null || spawnPoints.Length == 0)
            spawnPoints = FindObjectsOfType<EnemySpawnPoint>();

        FillToCap();
    }

    private void FillToCap()
    {
        while (_alive.Count < maxAlive)
        {
            if (!TrySpawnOne())
                break;
        }
    }

    private bool TrySpawnOne()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
            return false;

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        Vector2 spawnPos;
        bool foundSpawn;

        // ACTIVE MODE:
        // Use spawn points for now because this is easier to balance.
        foundSpawn = preferFarthestSpawn
            ? TryGetFarthestValidSpawn(out spawnPos)
            : TryGetRandomValidSpawn(out spawnPos);

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
        _alive.Add(go);

        EnemyHealth hp = go.GetComponent<EnemyHealth>();
        if (hp != null)
            hp.Died += OnEnemyDied;

        return true;
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

    private bool TryGetRandomValidSpawn(out Vector2 spawnPos)
    {
        spawnPos = default;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;

        for (int i = 0; i < triesPerSpawn; i++)
        {
            EnemySpawnPoint sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (sp == null)
                continue;

            Vector2 pos = sp.Position;

            if (!IsSpawnValid(pos))
                continue;

            spawnPos = pos;
            return true;
        }

        return false;
    }

    private bool TryGetFarthestValidSpawn(out Vector2 spawnPos)
    {
        spawnPos = default;

        if (spawnPoints == null || spawnPoints.Length == 0)
            return false;

        float bestScore = float.MinValue;
        bool found = false;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
                continue;

            Vector2 pos = spawnPoints[i].Position;

            if (!IsSpawnValid(pos))
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

    private void OnEnemyDied(EnemyHealth hp)
    {
        if (hp == null)
            return;

        hp.Died -= OnEnemyDied;
        _alive.Remove(hp.gameObject);
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        FillToCap();
    }

    private bool IsSpawnValid(Vector2 pos)
    {
        if (_player != null)
        {
            float distToPlayer = Vector2.Distance(pos, _player.position);
            if (distToPlayer < minDistanceFromPlayer)
                return false;
        }

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