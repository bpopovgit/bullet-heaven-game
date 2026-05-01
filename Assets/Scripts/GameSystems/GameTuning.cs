using UnityEngine;

[CreateAssetMenu(fileName = "GameTuning", menuName = "War of Death Metal/Game Tuning")]
public class GameTuning : ScriptableObject
{
    [Header("Spawner toggles")]
    [Tooltip("When false, SkirmishDirector won't bootstrap. Track B stays compiled but inactive.")]
    public bool skirmishesEnabled = false;

    [Tooltip("When false, EliteSpawnDirector disables itself on Awake.")]
    public bool elitesEnabled = false;

    [Tooltip("When false, BossSpawnDirector disables itself on Awake.")]
    public bool bossEnabled = false;

    [Tooltip("When false, AllySquadSpawner won't spawn the Human ally squad on player bootstrap.")]
    public bool alliesEnabled = true;

    private static GameTuning _instance;
    private static bool _instanceWarnedMissing;

    public static GameTuning Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            _instance = Resources.Load<GameTuning>("GameTuning");
            if (_instance == null)
            {
                _instance = CreateInstance<GameTuning>();
                if (!_instanceWarnedMissing)
                {
                    Debug.Log("GAME TUNING: no asset at Resources/GameTuning. Using built-in defaults — skirmishes/elites/boss OFF, allies ON.");
                    _instanceWarnedMissing = true;
                }
            }

            return _instance;
        }
    }
}
