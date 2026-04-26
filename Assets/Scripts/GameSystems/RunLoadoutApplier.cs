using UnityEngine;
using UnityEngine.SceneManagement;

public class RunLoadoutApplier : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static RunLoadoutApplier _instance;

    private bool _applied;
    private bool _announcementShown;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("RunLoadoutApplier");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<RunLoadoutApplier>();
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

        _applied = false;
        _announcementShown = false;
        Debug.Log($"RunLoadoutApplier armed for scene: {scene.name}");
    }

    private void Update()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != GameplaySceneName)
            return;

        if (!_applied)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return;

            ApplyToPlayer(player);
            _applied = true;
        }

        if (_announcementShown)
            return;

        if (RunAnnouncementUI.Instance != null)
        {
            RunAnnouncementUI.Instance.ShowMessage(
                $"{RunLoadoutState.GetWeaponName(RunLoadoutState.WeaponChoice)} | {RunLoadoutState.GetBombName(RunLoadoutState.BombChoice)}\n{RunLoadoutState.GetSkillName(RunLoadoutState.SkillChoice)} | {RunLoadoutState.GetPassiveName(RunLoadoutState.PassiveChoice)}",
                4f);
            _announcementShown = true;
        }
    }

    private static void ApplyToPlayer(GameObject player)
    {
        PlayerShooting shooting = player.GetComponent<PlayerShooting>();
        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        string passiveSummary;

        if (shooting != null)
            ApplyWeaponPreset(shooting);

        if (stats != null)
            ApplyPassive(stats, health, out passiveSummary);
        else
            passiveSummary = "Passive unavailable";

        PlayerActiveBomb bomb = player.GetComponent<PlayerActiveBomb>();
        if (bomb == null)
            bomb = player.AddComponent<PlayerActiveBomb>();

        bomb.Configure(RunLoadoutState.BombChoice);

        PlayerSecondaryActiveSkill skill = player.GetComponent<PlayerSecondaryActiveSkill>();
        if (skill == null)
            skill = player.AddComponent<PlayerSecondaryActiveSkill>();

        skill.Configure(RunLoadoutState.SkillChoice);

        Debug.Log($"LOADOUT APPLIED: {RunLoadoutState.BuildSummary()}");
        Debug.Log($"PASSIVE APPLIED: {passiveSummary}");
    }

    private static void ApplyWeaponPreset(PlayerShooting shooting)
    {
        WeaponDefinition baseWeapon = shooting.GetWeaponDefinition();
        if (baseWeapon == null)
            return;

        WeaponDefinition runtimeWeapon = Object.Instantiate(baseWeapon);
        runtimeWeapon.name = $"{baseWeapon.name}_RuntimeLoadout";
        runtimeWeapon.displayName = RunLoadoutState.GetWeaponName(RunLoadoutState.WeaponChoice);

        switch (RunLoadoutState.WeaponChoice)
        {
            case StartingWeaponChoice.FrostLance:
                runtimeWeapon.element = DamageElement.Frost;
                runtimeWeapon.onHitEffect = StatusEffect.Slow;
                runtimeWeapon.effectChance = 0.35f;
                runtimeWeapon.statusDuration = 2.5f;
                runtimeWeapon.statusStrength = 0.35f;
                runtimeWeapon.baseDamage = 13;
                runtimeWeapon.shotsPerSecond = 4.6f;
                runtimeWeapon.bulletSpeed = 22f;
                runtimeWeapon.splashRadius = 0f;
                runtimeWeapon.pierce = 1;
                break;

            case StartingWeaponChoice.VenomCaster:
                runtimeWeapon.element = DamageElement.Poison;
                runtimeWeapon.onHitEffect = StatusEffect.Poison;
                runtimeWeapon.effectChance = 0.45f;
                runtimeWeapon.statusDuration = 3f;
                runtimeWeapon.statusStrength = 0.3f;
                runtimeWeapon.baseDamage = 10;
                runtimeWeapon.shotsPerSecond = 5.6f;
                runtimeWeapon.bulletSpeed = 18f;
                runtimeWeapon.splashRadius = 0.55f;
                runtimeWeapon.pierce = 0;
                break;

            case StartingWeaponChoice.StormNeedler:
                runtimeWeapon.element = DamageElement.Lightning;
                runtimeWeapon.onHitEffect = StatusEffect.Shock;
                runtimeWeapon.effectChance = 0.28f;
                runtimeWeapon.statusDuration = 1.8f;
                runtimeWeapon.statusStrength = 0.3f;
                runtimeWeapon.baseDamage = 12;
                runtimeWeapon.shotsPerSecond = 6f;
                runtimeWeapon.bulletSpeed = 28f;
                runtimeWeapon.splashRadius = 0f;
                runtimeWeapon.pierce = 0;
                break;

            default:
                runtimeWeapon.element = DamageElement.Fire;
                runtimeWeapon.onHitEffect = StatusEffect.Burn;
                runtimeWeapon.effectChance = 0.32f;
                runtimeWeapon.statusDuration = 2.3f;
                runtimeWeapon.statusStrength = 0.28f;
                runtimeWeapon.baseDamage = 10;
                runtimeWeapon.shotsPerSecond = 7.2f;
                runtimeWeapon.bulletSpeed = 22f;
                runtimeWeapon.splashRadius = 0f;
                runtimeWeapon.pierce = 0;
                break;
        }

        shooting.SetWeaponDefinition(runtimeWeapon);
        shooting.ResetCooldown();
    }

    private static void ApplyPassive(PlayerStats stats, PlayerHealth health, out string passiveSummary)
    {
        switch (RunLoadoutState.PassiveChoice)
        {
            case StartingPassiveChoice.Swiftness:
                stats.AddMoveSpeedPercent(0.18f);
                passiveSummary = "Swiftness (+18% move speed)";
                break;

            case StartingPassiveChoice.Magnetism:
                stats.AddPickupRadius(2f);
                passiveSummary = "Magnetism (+2 pickup radius)";
                break;

            case StartingPassiveChoice.Overclock:
                stats.AddFireRatePercent(0.2f);
                passiveSummary = "Overclock (+20% fire rate)";
                break;

            default:
                if (health != null)
                    health.IncreaseMaxHP(25, true);
                passiveSummary = "Vitality (+25 max HP)";
                break;
        }
    }
}
