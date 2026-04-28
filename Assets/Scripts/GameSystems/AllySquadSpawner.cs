using UnityEngine;
using UnityEngine.SceneManagement;

public class AllySquadSpawner : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static AllySquadSpawner _instance;

    [Header("Runtime Test Squad")]
    [SerializeField] private bool spawnSquad = true;
    [SerializeField] private bool useSelectedCharacterSquad = true;
    [SerializeField] private int allyCount = 3;
    [SerializeField] private int meleeAllyCount = 1;
    [SerializeField] private int allyHealth = 55;
    [SerializeField] private float formationRadius = 1.45f;
    [SerializeField] private Color allyColor = new Color(0.2f, 0.78f, 1f, 1f);

    [Header("Prefab Overrides")]
    [SerializeField] private GameObject allyPrefab;
    [SerializeField] private GameObject meleeAllyPrefab;
    [SerializeField] private GameObject rangedAllyPrefab;
    [SerializeField] private string allyPrefabResourcePath = "Prefabs/Factions/HumanAlly_Ranged";
    [SerializeField] private string meleeAllyPrefabResourcePath = "Prefabs/Factions/HumanAlly_Melee";
    [SerializeField] private string rangedAllyPrefabResourcePath = "Prefabs/Factions/HumanAlly_Ranged";

    private bool _spawned;
    private GameObject _cachedLegacyAllyPrefab;
    private GameObject _cachedMeleeAllyPrefab;
    private GameObject _cachedRangedAllyPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("AllySquadSpawner");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<AllySquadSpawner>();
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
        if (scene.IsValid() && scene.name == GameplaySceneName)
            _spawned = false;
    }

    private void Update()
    {
        if (_spawned || !spawnSquad)
            return;

        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != GameplaySceneName)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        SpawnSquad(player.transform);
        _spawned = true;
    }

    private void SpawnSquad(Transform player)
    {
        int meleeCount = GetMeleeAllyCount();
        int rangedCount = GetRangedAllyCount();
        int count = Mathf.Clamp(meleeCount + rangedCount, 0, 8);
        if (count <= 0)
            return;

        meleeCount = Mathf.Clamp(meleeCount, 0, count);
        float radius = GetFormationRadius();

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = GetFormationOffset(i, count, radius);
            FactionUnitArchetypeType archetype = i < meleeCount
                ? FactionUnitArchetypeType.HumanMeleeAlly
                : FactionUnitArchetypeType.HumanRangedAlly;
            GameObject ally = CreateAlly(player.position + (Vector3)offset, i + 1, archetype);

            FriendlyAlly friendlyAlly = ally.GetComponent<FriendlyAlly>();
            if (friendlyAlly != null)
                friendlyAlly.Configure(player, offset);
        }

        Debug.Log($"Spawned {count} Human allies near {RunLoadoutState.GetCharacterName(RunLoadoutState.CharacterChoice)}.");
    }

    private GameObject CreateAlly(Vector3 position, int index, FactionUnitArchetypeType archetype)
    {
        GameObject prefab = GetAllyPrefab(archetype);
        if (prefab != null)
            return CreatePrefabAlly(prefab, position, index, archetype);

        GameObject ally = new GameObject($"{archetype} {index}");
        ally.transform.position = position;
        ally.transform.localScale = Vector3.one * 0.72f;

        PickupSpriteFactory.AddDefaultRenderer(ally, GetAllyColor(), sortingOrder: 2);

        FactionUnitArchetype.ApplyTo(ally, archetype, rewardsEnabled: false);

        EnemyHealth health = ally.GetComponent<EnemyHealth>();
        if (health != null)
            health.ConfigureHealth(allyHealth);

        return ally;
    }

    private GameObject CreatePrefabAlly(GameObject prefab, Vector3 position, int index, FactionUnitArchetypeType archetype)
    {
        GameObject ally = Instantiate(prefab, position, Quaternion.identity);
        ally.name = $"{archetype} {index}";
        EnsureAllySetup(ally, archetype);
        return ally;
    }

    private GameObject GetAllyPrefab(FactionUnitArchetypeType archetype)
    {
        if (archetype == FactionUnitArchetypeType.HumanMeleeAlly)
        {
            if (meleeAllyPrefab != null)
                return meleeAllyPrefab;

            if (_cachedMeleeAllyPrefab == null && !string.IsNullOrWhiteSpace(meleeAllyPrefabResourcePath))
                _cachedMeleeAllyPrefab = Resources.Load<GameObject>(meleeAllyPrefabResourcePath);

            return _cachedMeleeAllyPrefab != null ? _cachedMeleeAllyPrefab : GetLegacyAllyPrefab();
        }

        if (rangedAllyPrefab != null)
            return rangedAllyPrefab;

        if (_cachedRangedAllyPrefab == null && !string.IsNullOrWhiteSpace(rangedAllyPrefabResourcePath))
            _cachedRangedAllyPrefab = Resources.Load<GameObject>(rangedAllyPrefabResourcePath);

        return _cachedRangedAllyPrefab != null ? _cachedRangedAllyPrefab : GetLegacyAllyPrefab();
    }

    private GameObject GetLegacyAllyPrefab()
    {
        if (allyPrefab != null)
            return allyPrefab;

        if (_cachedLegacyAllyPrefab == null && !string.IsNullOrWhiteSpace(allyPrefabResourcePath))
            _cachedLegacyAllyPrefab = Resources.Load<GameObject>(allyPrefabResourcePath);

        return _cachedLegacyAllyPrefab;
    }

    private void EnsureAllySetup(GameObject ally, FactionUnitArchetypeType archetype)
    {
        if (ally == null)
            return;

        FactionUnitArchetype.ApplyTo(ally, archetype, rewardsEnabled: false);

        EnemyHealth health = ally.GetComponent<EnemyHealth>();
        if (health != null)
            health.ConfigureHealth(allyHealth);
    }

    private int GetMeleeAllyCount()
    {
        if (!useSelectedCharacterSquad)
            return Mathf.Clamp(meleeAllyCount, 0, 8);

        return RunLoadoutState.GetCharacterMeleeAllyCount(RunLoadoutState.CharacterChoice);
    }

    private int GetRangedAllyCount()
    {
        if (!useSelectedCharacterSquad)
            return Mathf.Clamp(allyCount - meleeAllyCount, 0, 8);

        return RunLoadoutState.GetCharacterRangedAllyCount(RunLoadoutState.CharacterChoice);
    }

    private float GetFormationRadius()
    {
        if (!useSelectedCharacterSquad)
            return formationRadius;

        return RunLoadoutState.GetCharacterFormationRadius(RunLoadoutState.CharacterChoice);
    }

    private Color GetAllyColor()
    {
        if (!useSelectedCharacterSquad)
            return allyColor;

        return RunLoadoutState.GetCharacterTint(RunLoadoutState.CharacterChoice);
    }

    private Vector2 GetFormationOffset(int index, int count, float radius)
    {
        if (count == 1)
            return Vector2.down * radius;

        float startAngle = 210f;
        float endAngle = 330f;
        float t = count <= 1 ? 0.5f : index / (float)(count - 1);
        float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
