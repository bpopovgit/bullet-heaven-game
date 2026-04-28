using UnityEngine;
using UnityEngine.SceneManagement;

public class FactionSkirmishDirector : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static FactionSkirmishDirector _instance;

    [Header("Starter Skirmish")]
    [SerializeField] private bool spawnStarterSkirmish = true;
    [SerializeField] private float spawnDelay = 1.25f;
    [SerializeField] private int angelsToSpawn = 2;
    [SerializeField] private int demonsToSpawn = 2;
    [SerializeField] private int zombiesToSpawn = 3;
    [SerializeField] private float groupDistanceFromPlayer = 6f;
    [SerializeField] private float unitSpacing = 1.15f;
    [SerializeField] private bool rewardsEnabled = false;

    private bool _spawned;
    private float _readyTime;
    private GameObject _angelPrefab;
    private GameObject _demonPrefab;
    private GameObject _zombiePrefab;

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

        SpawnGroup(FactionType.Angel, angelsToSpawn, center + new Vector3(-groupDistanceFromPlayer, groupDistanceFromPlayer, 0f));
        SpawnGroup(FactionType.Demon, demonsToSpawn, center + new Vector3(groupDistanceFromPlayer, groupDistanceFromPlayer, 0f));
        SpawnGroup(FactionType.Zombie, zombiesToSpawn, center + new Vector3(0f, -groupDistanceFromPlayer, 0f));

        Debug.Log("Faction starter skirmish spawned.");
    }

    private void SpawnGroup(FactionType faction, int count, Vector3 anchor)
    {
        int safeCount = Mathf.Clamp(count, 0, 8);

        for (int i = 0; i < safeCount; i++)
        {
            Vector3 offset = GetGroupOffset(i, safeCount);
            GameObject actor = CreateFactionActor(faction, anchor + offset, i + 1);
            EnsureFactionActorSetup(actor, faction);
        }
    }

    private GameObject CreateFactionActor(FactionType faction, Vector3 position, int index)
    {
        GameObject prefab = GetPrefab(faction);
        string displayName = $"{faction} Test Unit {index}";

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

    private void EnsureFactionActorSetup(GameObject actor, FactionType faction)
    {
        if (actor == null)
            return;

        FactionUnitArchetype.ApplyTo(actor, GetArchetype(faction), rewardsEnabled);
    }

    private GameObject GetPrefab(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Angel:
                if (_angelPrefab == null)
                    _angelPrefab = Resources.Load<GameObject>("Prefabs/Factions/AngelTestUnit");
                return _angelPrefab;

            case FactionType.Demon:
                if (_demonPrefab == null)
                    _demonPrefab = Resources.Load<GameObject>("Prefabs/Factions/DemonTestUnit");
                return _demonPrefab;

            case FactionType.Zombie:
                if (_zombiePrefab == null)
                    _zombiePrefab = Resources.Load<GameObject>("Prefabs/Factions/ZombieTestUnit");
                return _zombiePrefab;

            default:
                return null;
        }
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

    private FactionUnitArchetypeType GetArchetype(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Angel:
                return FactionUnitArchetypeType.AngelMarksman;
            case FactionType.Demon:
                return FactionUnitArchetypeType.DemonRaider;
            case FactionType.Zombie:
                return FactionUnitArchetypeType.ZombieGrunt;
            case FactionType.Human:
                return FactionUnitArchetypeType.HumanSupport;
            default:
                return FactionUnitArchetypeType.ZombieGrunt;
        }
    }
}
