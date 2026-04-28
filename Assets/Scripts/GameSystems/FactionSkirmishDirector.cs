using UnityEngine;
using UnityEngine.SceneManagement;

public class FactionSkirmishDirector : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static FactionSkirmishDirector _instance;

    [Header("Starter Skirmish")]
    [SerializeField] private bool spawnStarterSkirmish = true;
    [SerializeField] private float spawnDelay = 1.25f;
    [SerializeField] private int angelMeleeToSpawn = 1;
    [SerializeField] private int angelRangedToSpawn = 1;
    [SerializeField] private int demonMeleeToSpawn = 1;
    [SerializeField] private int demonRangedToSpawn = 1;
    [SerializeField] private int zombieMeleeToSpawn = 2;
    [SerializeField] private int zombieRangedToSpawn = 1;
    [SerializeField] private float groupDistanceFromPlayer = 6f;
    [SerializeField] private float unitSpacing = 1.15f;
    [SerializeField] private bool rewardsEnabled = false;

    private bool _spawned;
    private float _readyTime;
    private GameObject _angelMeleePrefab;
    private GameObject _angelRangedPrefab;
    private GameObject _demonMeleePrefab;
    private GameObject _demonRangedPrefab;
    private GameObject _zombieMeleePrefab;
    private GameObject _zombieRangedPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("FactionSkirmishDirector");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<FactionSkirmishDirector>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid() || scene.name != GameplaySceneName)
            return;

        _spawned = false;
        _readyTime = Time.time + Mathf.Max(0f, spawnDelay);
    }

    private void Update()
    {
        if (_spawned || !spawnStarterSkirmish)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != GameplaySceneName)
            return;

        if (Time.time < _readyTime)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        SpawnSkirmish(player.transform);
        _spawned = true;
    }

    private void SpawnSkirmish(Transform player)
    {
        Vector3 center = player.position;
        Vector3 angelAnchor = center + new Vector3(-groupDistanceFromPlayer, groupDistanceFromPlayer, 0f);
        Vector3 demonAnchor = center + new Vector3(groupDistanceFromPlayer, groupDistanceFromPlayer, 0f);
        Vector3 zombieAnchor = center + new Vector3(0f, -groupDistanceFromPlayer, 0f);

        SpawnGroup(FactionUnitArchetypeType.AngelMelee, angelMeleeToSpawn, angelAnchor + new Vector3(0.45f, -0.4f, 0f));
        SpawnGroup(FactionUnitArchetypeType.AngelRanged, angelRangedToSpawn, angelAnchor + new Vector3(-0.65f, 0.45f, 0f));
        SpawnGroup(FactionUnitArchetypeType.DemonMelee, demonMeleeToSpawn, demonAnchor + new Vector3(-0.45f, -0.4f, 0f));
        SpawnGroup(FactionUnitArchetypeType.DemonRanged, demonRangedToSpawn, demonAnchor + new Vector3(0.65f, 0.45f, 0f));
        SpawnGroup(FactionUnitArchetypeType.ZombieMelee, zombieMeleeToSpawn, zombieAnchor);
        SpawnGroup(FactionUnitArchetypeType.ZombieRanged, zombieRangedToSpawn, zombieAnchor + new Vector3(0f, -0.7f, 0f));

        Debug.Log("Faction starter skirmish spawned.");
    }

    private void SpawnGroup(FactionUnitArchetypeType archetype, int count, Vector3 anchor)
    {
        int safeCount = Mathf.Clamp(count, 0, 8);

        for (int i = 0; i < safeCount; i++)
        {
            Vector3 offset = GetGroupOffset(i, safeCount);
            GameObject actor = CreateFactionActor(archetype, anchor + offset, i + 1);
            EnsureFactionActorSetup(actor, archetype);
        }
    }

    private GameObject CreateFactionActor(FactionUnitArchetypeType archetype, Vector3 position, int index)
    {
        FactionType faction = FactionUnitArchetype.GetFaction(archetype);
        GameObject prefab = GetPrefab(archetype);
        string displayName = $"{archetype} {index}";

        if (prefab != null)
        {
            GameObject actor = Instantiate(prefab, position, Quaternion.identity);
            actor.name = displayName;
            return actor;
        }

        GameObject fallback = new GameObject(displayName);
        fallback.transform.position = position;
        fallback.transform.localScale = Vector3.one * GetScale(faction);
        PickupSpriteFactory.AddDefaultRenderer(fallback, GetColor(faction), sortingOrder: 2);
        return fallback;
    }

    private void EnsureFactionActorSetup(GameObject actor, FactionUnitArchetypeType archetype)
    {
        if (actor == null)
            return;

        FactionUnitArchetype.ApplyTo(actor, archetype, rewardsEnabled);
    }

    private GameObject GetPrefab(FactionUnitArchetypeType archetype)
    {
        switch (archetype)
        {
            case FactionUnitArchetypeType.AngelMelee:
                return LoadCachedPrefab(ref _angelMeleePrefab, "Prefabs/Factions/Angel_Melee", "Prefabs/Factions/AngelTestUnit");
            case FactionUnitArchetypeType.AngelMarksman:
            case FactionUnitArchetypeType.AngelRanged:
                return LoadCachedPrefab(ref _angelRangedPrefab, "Prefabs/Factions/Angel_Ranged", "Prefabs/Factions/AngelTestUnit");
            case FactionUnitArchetypeType.DemonRaider:
            case FactionUnitArchetypeType.DemonMelee:
                return LoadCachedPrefab(ref _demonMeleePrefab, "Prefabs/Factions/Demon_Melee", "Prefabs/Factions/DemonTestUnit");
            case FactionUnitArchetypeType.DemonRanged:
                return LoadCachedPrefab(ref _demonRangedPrefab, "Prefabs/Factions/Demon_Ranged", "Prefabs/Factions/DemonTestUnit");
            case FactionUnitArchetypeType.ZombieGrunt:
            case FactionUnitArchetypeType.ZombieMelee:
                return LoadCachedPrefab(ref _zombieMeleePrefab, "Prefabs/Factions/Zombie_Melee", "Prefabs/Factions/ZombieTestUnit");
            case FactionUnitArchetypeType.ZombieRanged:
                return LoadCachedPrefab(ref _zombieRangedPrefab, "Prefabs/Factions/Zombie_Ranged", "Prefabs/Factions/ZombieTestUnit");

            default:
                return null;
        }
    }

    private GameObject LoadCachedPrefab(ref GameObject cache, string resourcePath, string fallbackResourcePath)
    {
        if (cache != null)
            return cache;

        cache = Resources.Load<GameObject>(resourcePath);

        if (cache == null && !string.IsNullOrWhiteSpace(fallbackResourcePath))
            cache = Resources.Load<GameObject>(fallbackResourcePath);

        return cache;
    }

    private Vector3 GetGroupOffset(int index, int count)
    {
        if (count <= 1)
            return Vector3.zero;

        float centeredIndex = index - (count - 1) * 0.5f;
        return new Vector3(centeredIndex * unitSpacing, Random.Range(-0.35f, 0.35f), 0f);
    }

    private float GetScale(FactionType faction)
    {
        return faction == FactionType.Demon ? 0.78f : 0.72f;
    }

    private Color GetColor(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Angel:
                return new Color(1f, 0.96f, 0.65f, 1f);
            case FactionType.Demon:
                return new Color(0.9f, 0.12f, 0.16f, 1f);
            case FactionType.Zombie:
                return new Color(0.45f, 0.85f, 0.28f, 1f);
            default:
                return Color.white;
        }
    }
}
