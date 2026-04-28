using UnityEngine;
using UnityEngine.SceneManagement;

public class AllySquadSpawner : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static AllySquadSpawner _instance;

    [Header("Runtime Test Squad")]
    [SerializeField] private bool spawnSquad = true;
    [SerializeField] private int allyCount = 3;
    [SerializeField] private int allyHealth = 45;
    [SerializeField] private float formationRadius = 1.45f;
    [SerializeField] private Color allyColor = new Color(0.2f, 0.78f, 1f, 1f);

    private bool _spawned;

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
        int count = Mathf.Clamp(allyCount, 0, 8);
        if (count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            Vector2 offset = GetFormationOffset(i, count);
            GameObject ally = CreateAlly(player.position + (Vector3)offset, i + 1);

            FriendlyAlly friendlyAlly = ally.GetComponent<FriendlyAlly>();
            if (friendlyAlly != null)
                friendlyAlly.Configure(player, offset);
        }

        Debug.Log($"Spawned {count} Human allies near the player.");
    }

    private GameObject CreateAlly(Vector3 position, int index)
    {
        GameObject ally = new GameObject($"Human Ally {index}");
        ally.transform.position = position;
        ally.transform.localScale = Vector3.one * 0.72f;

        PickupSpriteFactory.AddDefaultRenderer(ally, allyColor, sortingOrder: 2);

        CircleCollider2D collider = ally.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;
        collider.isTrigger = false;

        Rigidbody2D rb = ally.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        FactionMember faction = ally.AddComponent<FactionMember>();
        faction.Configure(FactionType.Human);

        EnemyHealth health = ally.AddComponent<EnemyHealth>();
        health.ConfigureHealth(allyHealth);
        health.SetRewardsEnabled(false);

        ally.AddComponent<FriendlyAlly>();
        return ally;
    }

    private Vector2 GetFormationOffset(int index, int count)
    {
        if (count == 1)
            return Vector2.down * formationRadius;

        float startAngle = 210f;
        float endAngle = 330f;
        float t = count <= 1 ? 0.5f : index / (float)(count - 1);
        float angle = Mathf.Lerp(startAngle, endAngle, t) * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * formationRadius;
    }
}
