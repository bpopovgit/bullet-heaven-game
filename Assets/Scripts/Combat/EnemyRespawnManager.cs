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
    [SerializeField] private int triesPerSpawn = 12;

    private readonly HashSet<GameObject> _alive = new HashSet<GameObject>();
    private Transform _player;

    private void Awake()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) _player = playerObj.transform;

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
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return false;
        if (spawnPoints == null || spawnPoints.Length == 0) return false;

        for (int i = 0; i < triesPerSpawn; i++)
        {
            var sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            var pos = sp.Position;

            if (!IsSpawnValid(pos)) continue;

            var go = Instantiate(prefab, pos, Quaternion.identity);
            _alive.Add(go);

            // subscribe to death
            var hp = go.GetComponent<EnemyHealth>();
            if (hp != null)
                hp.Died += OnEnemyDied;

            return true;
        }

        return false;
    }

    private void OnEnemyDied(EnemyHealth hp)
    {
        if (hp == null) return;

        // remove dead from alive list (by GO ref)
        _alive.Remove(hp.gameObject);

        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        // If you pause via Time.timeScale = 0 on Game Over,
        // respawn pauses too (often desirable).
        yield return new WaitForSeconds(respawnDelay);

        FillToCap();
    }

    private bool IsSpawnValid(Vector2 pos)
    {
        if (_player == null) return true;
        return Vector2.Distance(pos, _player.position) >= minDistanceFromPlayer;
    }
}
