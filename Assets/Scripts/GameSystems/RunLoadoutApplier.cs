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
                $"{RunLoadoutState.GetCharacterName(RunLoadoutState.CharacterChoice)}\n{RunLoadoutState.GetPrimaryAttackName(RunLoadoutState.CharacterChoice, RunLoadoutState.WeaponChoice)} | {RunLoadoutState.GetBombName(RunLoadoutState.BombChoice)} | {RunLoadoutState.GetSkillName(RunLoadoutState.SkillChoice)}",
                4f);
            _announcementShown = true;
        }
    }

    private static void ApplyToPlayer(GameObject player)
    {
        PlayerShooting shooting = player.GetComponent<PlayerShooting>();
        PlayerStats stats = player.GetComponent<PlayerStats>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        string characterSummary;
        string passiveSummary;

        ApplyCharacter(player, stats, health, out characterSummary);
        ApplyPrimaryCombat(player, shooting);

        if (shooting != null && shooting.enabled)
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

        if (player.GetComponent<PlayerDash>() == null)
            player.AddComponent<PlayerDash>();

        PlayerSecondaryWeapon secondaryWeapon = player.GetComponent<PlayerSecondaryWeapon>();
        if (secondaryWeapon == null)
            secondaryWeapon = player.AddComponent<PlayerSecondaryWeapon>();

        secondaryWeapon.ConfigureForCharacter(RunLoadoutState.CharacterChoice);

        if (RunLoadoutState.CharacterChoice == PlayableCharacterChoice.HumanVanguard)
        {
            if (player.GetComponent<PlayerSecondaryMelee>() == null)
                player.AddComponent<PlayerSecondaryMelee>();
        }
        else
        {
            PlayerSecondaryMelee existingMelee = player.GetComponent<PlayerSecondaryMelee>();
            if (existingMelee != null)
                Destroy(existingMelee);
        }

        Debug.Log($"LOADOUT APPLIED: {RunLoadoutState.BuildSummary()}");
        Debug.Log($"CHARACTER APPLIED: {characterSummary}");
        Debug.Log($"PASSIVE APPLIED: {passiveSummary}");
    }

    private static void ApplyCharacter(GameObject player, PlayerStats stats, PlayerHealth health, out string characterSummary)
    {
        PlayableCharacterChoice choice = RunLoadoutState.CharacterChoice;
        FactionMember.Ensure(player, FactionType.Human);

        FactionVisualIdentity factionBadge = player.GetComponent<FactionVisualIdentity>();
        if (factionBadge != null)
            factionBadge.enabled = false;

        Transform existingFactionBadge = player.transform.Find("FactionBadge");
        if (existingFactionBadge != null)
            existingFactionBadge.gameObject.SetActive(false);

        PlayerCharacterVisualIdentity visualIdentity = player.GetComponent<PlayerCharacterVisualIdentity>();
        if (visualIdentity == null)
            visualIdentity = player.AddComponent<PlayerCharacterVisualIdentity>();

        visualIdentity.Apply(choice);

        PlayerMeleeAttack meleeAttack = player.GetComponent<PlayerMeleeAttack>();
        if (meleeAttack == null)
            meleeAttack = player.AddComponent<PlayerMeleeAttack>();

        meleeAttack.ConfigureForCharacter(choice);
        int maxHpBonus = RunLoadoutState.GetCharacterMaxHpBonus(choice);
        if (health != null && maxHpBonus > 0)
            health.IncreaseMaxHP(maxHpBonus, true);

        if (stats != null)
        {
            stats.AddMoveSpeedPercent(RunLoadoutState.GetCharacterMoveSpeedBonus(choice));
            stats.AddFireRatePercent(RunLoadoutState.GetCharacterFireRateBonus(choice));
            stats.AddDamagePercent(RunLoadoutState.GetCharacterDamageBonus(choice));
            stats.AddPickupRadius(RunLoadoutState.GetCharacterPickupRadiusBonus(choice));
        }

        characterSummary = $"{RunLoadoutState.GetCharacterName(choice)} ({RunLoadoutState.GetCharacterStatsSummary(choice)})";
    }

    private static void ApplyPrimaryCombat(GameObject player, PlayerShooting shooting)
    {
        PlayableCharacterChoice choice = RunLoadoutState.CharacterChoice;

        if (shooting != null)
        {
            shooting.enabled = choice == PlayableCharacterChoice.HumanRanger;
            if (!shooting.enabled)
                shooting.ResetCooldown();
        }

        PlayerMagicAttack magicAttack = player.GetComponent<PlayerMagicAttack>();
        if (magicAttack == null)
            magicAttack = player.AddComponent<PlayerMagicAttack>();

        magicAttack.ConfigureForCharacter(choice, RunLoadoutState.WeaponChoice);
        magicAttack.enabled = choice == PlayableCharacterChoice.HumanArcanist;

        PlayerMeleeAttack meleeAttack = player.GetComponent<PlayerMeleeAttack>();
        if (meleeAttack != null)
            meleeAttack.enabled = choice == PlayableCharacterChoice.HumanVanguard;
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
