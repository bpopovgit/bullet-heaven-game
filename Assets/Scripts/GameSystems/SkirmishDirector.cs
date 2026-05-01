using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkirmishDirector : MonoBehaviour
{
    private const string GameSceneName = "Game";
    private const float InitialDelay = 4f;
    private const float SpawnInterval = 22f;
    private const float MinPlayerDistance = 12f;
    private const float MaxAnchorDistance = 26f;
    private const int MaxActiveSkirmishes = 3;
    private const int UnitsPerSide = 4;
    private const float UnitSpread = 1.6f;
    private const string PrefabRoot = "Prefabs/Factions/";

    private static readonly (FactionType a, FactionType b)[] Matchups =
    {
        (FactionType.Angel, FactionType.Demon),
        (FactionType.Angel, FactionType.Demon),
        (FactionType.Angel, FactionType.Zombie),
        (FactionType.Demon, FactionType.Zombie),
        (FactionType.Human, FactionType.Demon),
        (FactionType.Human, FactionType.Zombie)
    };

    public static SkirmishDirector Instance { get; private set; }

    public IReadOnlyCollection<FactionSkirmish> Skirmishes => _skirmishes.Values;

    private readonly Dictionary<string, FactionSkirmish> _skirmishes = new Dictionary<string, FactionSkirmish>();
    private readonly Dictionary<string, SkirmishMarker> _markers = new Dictionary<string, SkirmishMarker>();
    private float _nextSpawnAt;
    private int _idCounter;
    private Transform _player;

    private static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_sceneHookRegistered)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        _sceneHookRegistered = true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid() || scene.name != GameSceneName)
            return;

        if (!GameTuning.Instance.skirmishesEnabled)
            return;

        if (FindObjectOfType<SkirmishDirector>() != null)
            return;

        GameObject host = new GameObject("SkirmishDirector");
        host.AddComponent<SkirmishDirector>();
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        _nextSpawnAt = Time.time + InitialDelay;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        ResolveFinishedSkirmishes();

        if (Time.time < _nextSpawnAt)
            return;

        if (CountActive() >= MaxActiveSkirmishes)
        {
            _nextSpawnAt = Time.time + 2f;
            return;
        }

        if (TrySpawnSkirmish())
            _nextSpawnAt = Time.time + SpawnInterval;
        else
            _nextSpawnAt = Time.time + 4f;
    }

    public void OnPlayerDamagedSkirmishUnit(FactionSkirmishUnit unit, int damage)
    {
        if (unit == null || string.IsNullOrEmpty(unit.SkirmishId)) return;
        if (!_skirmishes.TryGetValue(unit.SkirmishId, out FactionSkirmish skirmish)) return;

        if (unit.IsSideA)
            skirmish.RecordPlayerDamageToA(damage);
        else
            skirmish.RecordPlayerDamageToB(damage);
    }

    public void OnSkirmishUnitKilled(FactionSkirmishUnit unit, bool killedByPlayer)
    {
        if (unit == null || string.IsNullOrEmpty(unit.SkirmishId)) return;
        if (!_skirmishes.TryGetValue(unit.SkirmishId, out FactionSkirmish skirmish)) return;

        if (killedByPlayer)
        {
            if (unit.IsSideA) skirmish.RecordPlayerKillOnA();
            else skirmish.RecordPlayerKillOnB();
        }
    }

    private int CountActive()
    {
        int count = 0;
        foreach (FactionSkirmish s in _skirmishes.Values)
            if (s.State == SkirmishState.Active) count++;
        return count;
    }

    private void ResolveFinishedSkirmishes()
    {
        List<string> toRemove = null;

        foreach (FactionSkirmish skirmish in _skirmishes.Values)
        {
            if (skirmish.State != SkirmishState.Active)
                continue;

            if (!skirmish.TryResolve())
                continue;

            ApplySkirmishOutcome(skirmish);

            if (_markers.TryGetValue(skirmish.Id, out SkirmishMarker marker) && marker != null)
                marker.FadeOut();

            toRemove ??= new List<string>();
            toRemove.Add(skirmish.Id);
        }

        if (toRemove != null)
        {
            foreach (string id in toRemove)
            {
                _skirmishes.Remove(id);
                _markers.Remove(id);
            }
        }
    }

    private void ApplySkirmishOutcome(FactionSkirmish skirmish)
    {
        FactionAffinityTracker affinity = FactionAffinityTracker.Instance;
        if (affinity == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                affinity = player.AddComponent<FactionAffinityTracker>();
        }

        if (skirmish.Winner == FactionType.Neutral)
        {
            Debug.Log($"SKIRMISH RESOLVED: {skirmish.Id} ended in mutual annihilation.");
            Announce("Mutual annihilation.", 1.6f);
            return;
        }

        FactionType loser = skirmish.Winner == skirmish.SideAFaction ? skirmish.SideBFaction : skirmish.SideAFaction;

        if (skirmish.PlayerHelped(skirmish.Winner))
        {
            affinity?.RecordHelped(skirmish.Winner);
            affinity?.RecordOpposed(loser);
            SpawnInterventionAlly(skirmish);
            Debug.Log($"SKIRMISH RESOLVED: {skirmish.Id} — player helped {skirmish.Winner} defeat {loser}.");
            Announce($"{FormatFaction(skirmish.Winner).ToUpperInvariant()} OWE YOU A DEBT\n+3% damage, ally arrives", 2.4f);
        }
        else if (skirmish.PlayerWasIdle)
        {
            SpawnPenaltySquad(skirmish);
            Debug.Log($"SKIRMISH RESOLVED: {skirmish.Id} — player ignored, {skirmish.Winner} grows bolder.");
            Announce($"{FormatFaction(skirmish.Winner).ToUpperInvariant()} ADVANCE — A SQUAD APPROACHES", 2.2f);
        }
        else
        {
            Debug.Log($"SKIRMISH RESOLVED: {skirmish.Id} — {skirmish.Winner} won; player split fire.");
            Announce($"{FormatFaction(skirmish.Winner).ToUpperInvariant()} held the field", 1.6f);
        }
    }

    private static void Announce(string message, float duration)
    {
        if (RunAnnouncementUI.Instance == null || string.IsNullOrWhiteSpace(message))
            return;
        RunAnnouncementUI.Instance.ShowMessage(message, duration);
    }

    private static string FormatFaction(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Angel: return "Angels";
            case FactionType.Demon: return "Demons";
            case FactionType.Human: return "Humans";
            case FactionType.Zombie: return "Zombies";
            default: return "Neutrals";
        }
    }

    private bool TrySpawnSkirmish()
    {
        Vector2 anchor;
        if (!TryPickAnchor(out anchor))
            return false;

        (FactionType a, FactionType b) matchup = Matchups[Random.Range(0, Matchups.Length)];

        string id = $"skirmish_{++_idCounter}";
        FactionSkirmish skirmish = new FactionSkirmish(id, matchup.a, matchup.b, anchor, UnitSpread * 1.4f);

        Vector2 sideAOrigin = anchor + new Vector2(-UnitSpread * 1.2f, 0f);
        Vector2 sideBOrigin = anchor + new Vector2(UnitSpread * 1.2f, 0f);

        SpawnSide(skirmish, matchup.a, sideAOrigin, isSideA: true);
        SpawnSide(skirmish, matchup.b, sideBOrigin, isSideA: false);

        if (skirmish.SideAUnits.Count == 0 || skirmish.SideBUnits.Count == 0)
        {
            CleanupAbortedSkirmish(skirmish);
            return false;
        }

        skirmish.MarkActive();
        _skirmishes[id] = skirmish;

        SkirmishMarker marker = SkirmishMarker.Spawn(anchor, matchup.a, matchup.b, skirmish.AnchorRadius);
        if (marker != null)
            _markers[id] = marker;

        Announce($"BATTLE — {FormatFaction(matchup.a).ToUpperInvariant()} vs {FormatFaction(matchup.b).ToUpperInvariant()}", 1.4f);

        Debug.Log($"SKIRMISH SPAWNED: {id} {matchup.a} vs {matchup.b} at {anchor}.");
        return true;
    }

    private void SpawnSide(FactionSkirmish skirmish, FactionType faction, Vector2 origin, bool isSideA)
    {
        for (int i = 0; i < UnitsPerSide; i++)
        {
            bool ranged = i % 2 == 1;
            GameObject prefab = LoadFactionPrefab(faction, ranged);
            if (prefab == null)
                continue;

            Vector2 jitter = Random.insideUnitCircle * UnitSpread;
            Vector3 spawnPos = new Vector3(origin.x + jitter.x, origin.y + jitter.y, 0f);

            GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity);
            FactionMember.Ensure(unit, faction);

            FactionSkirmishUnit link = unit.GetComponent<FactionSkirmishUnit>();
            if (link == null)
                link = unit.AddComponent<FactionSkirmishUnit>();
            link.Bind(skirmish.Id, isSideA);

            SkirmishUnitBadge.Attach(unit, faction);

            if (isSideA) skirmish.SideAUnits.Add(unit);
            else skirmish.SideBUnits.Add(unit);
        }
    }

    private static GameObject LoadFactionPrefab(FactionType faction, bool ranged)
    {
        string role = ranged ? "_Ranged" : "_Melee";
        string primary = PrefabRoot + faction switch
        {
            FactionType.Human => "HumanAlly" + role,
            FactionType.Angel => "Angel" + role,
            FactionType.Demon => "Demon" + role,
            FactionType.Zombie => "Zombie" + role,
            _ => null
        };

        if (primary == null) return null;

        GameObject prefab = Resources.Load<GameObject>(primary);
        if (prefab != null) return prefab;

        string fallback = PrefabRoot + faction switch
        {
            FactionType.Human => "HumanAlly",
            FactionType.Angel => "AngelTestUnit",
            FactionType.Demon => "DemonTestUnit",
            FactionType.Zombie => "ZombieTestUnit",
            _ => null
        };

        return fallback == null ? null : Resources.Load<GameObject>(fallback);
    }

    private bool TryPickAnchor(out Vector2 anchor)
    {
        anchor = Vector2.zero;
        Transform player = GetPlayerTransform();
        if (player == null) return false;

        EnemySpawnPoint[] spawnPoints = FindObjectsOfType<EnemySpawnPoint>();
        Vector2 playerPos = player.position;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int attempts = 8;
            for (int i = 0; i < attempts; i++)
            {
                EnemySpawnPoint candidate = spawnPoints[Random.Range(0, spawnPoints.Length)];
                if (candidate == null) continue;

                Vector2 pos = candidate.Position;
                float distance = Vector2.Distance(pos, playerPos);
                if (distance >= MinPlayerDistance && distance <= MaxAnchorDistance)
                {
                    anchor = pos;
                    return true;
                }
            }
        }

        for (int i = 0; i < 6; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float distance = Random.Range(MinPlayerDistance + 4f, MaxAnchorDistance);
            anchor = playerPos + randomDir * distance;
            return true;
        }

        return false;
    }

    private Transform GetPlayerTransform()
    {
        if (_player != null) return _player;

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            _player = playerGO.transform;

        return _player;
    }

    private void CleanupAbortedSkirmish(FactionSkirmish skirmish)
    {
        for (int i = 0; i < skirmish.SideAUnits.Count; i++)
            if (skirmish.SideAUnits[i] != null) Destroy(skirmish.SideAUnits[i]);
        for (int i = 0; i < skirmish.SideBUnits.Count; i++)
            if (skirmish.SideBUnits[i] != null) Destroy(skirmish.SideBUnits[i]);
    }

    private void SpawnInterventionAlly(FactionSkirmish skirmish)
    {
        Transform player = GetPlayerTransform();
        if (player == null) return;

        GameObject prefab = LoadFactionPrefab(FactionType.Human, ranged: false);
        if (prefab != null)
        {
            Vector3 spawnPos = player.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0f);
            GameObject ally = Instantiate(prefab, spawnPos, Quaternion.identity);
            FactionMember.Ensure(ally, FactionType.Human);

            FriendlyAlly friendly = ally.GetComponent<FriendlyAlly>();
            if (friendly != null)
                friendly.Configure(player, new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)));

            Destroy(ally, 30f);
        }

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.AddDamagePercent(0.03f);
            Debug.Log($"INTERVENTION REWARD: +3% damage from helping {skirmish.Winner}.");
        }
    }

    private void SpawnPenaltySquad(FactionSkirmish skirmish)
    {
        Transform player = GetPlayerTransform();
        if (player == null) return;

        Vector2 playerPos = player.position;
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 origin = playerPos + dir * (MinPlayerDistance + 2f);

        for (int i = 0; i < 3; i++)
        {
            bool ranged = i % 2 == 1;
            GameObject prefab = LoadFactionPrefab(skirmish.Winner, ranged);
            if (prefab == null) continue;

            Vector2 jitter = Random.insideUnitCircle * UnitSpread;
            Vector3 spawnPos = new Vector3(origin.x + jitter.x, origin.y + jitter.y, 0f);
            GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity);
            FactionMember.Ensure(unit, skirmish.Winner);
        }
    }
}
