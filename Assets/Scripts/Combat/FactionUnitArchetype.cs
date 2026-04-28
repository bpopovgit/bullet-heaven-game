using UnityEngine;

public enum FactionUnitArchetypeType
{
    HumanSupport,
    AngelMarksman,
    DemonRaider,
    ZombieGrunt,
    HumanMeleeAlly,
    HumanRangedAlly,
    AngelMelee,
    AngelRanged,
    DemonMelee,
    DemonRanged,
    ZombieMelee,
    ZombieRanged
}

public class FactionUnitArchetype : MonoBehaviour
{
    [SerializeField] private FactionUnitArchetypeType archetype = FactionUnitArchetypeType.ZombieGrunt;
    [SerializeField] private bool applyOnAwake = true;

    public FactionUnitArchetypeType Archetype => archetype;

    public static FactionUnitArchetype ApplyTo(
        GameObject target,
        FactionUnitArchetypeType archetype,
        bool? rewardsEnabled = null)
    {
        if (target == null)
            return null;

        FactionUnitArchetype unitArchetype = target.GetComponent<FactionUnitArchetype>();
        if (unitArchetype == null)
            unitArchetype = target.AddComponent<FactionUnitArchetype>();

        unitArchetype.Configure(archetype, rewardsEnabled);
        return unitArchetype;
    }

    public void Configure(FactionUnitArchetypeType newArchetype, bool? rewardsEnabled = null)
    {
        archetype = newArchetype;
        Apply(rewardsEnabled);
    }

    private void Awake()
    {
        if (applyOnAwake)
            Apply();
    }

    public void Apply(bool? rewardsEnabled = null)
    {
        switch (archetype)
        {
            case FactionUnitArchetypeType.HumanSupport:
            case FactionUnitArchetypeType.HumanRangedAlly:
                ApplyHumanRangedAlly(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.HumanMeleeAlly:
                ApplyHumanMeleeAlly(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.AngelMarksman:
            case FactionUnitArchetypeType.AngelRanged:
                ApplyAngelRanged(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.AngelMelee:
                ApplyAngelMelee(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.DemonRaider:
            case FactionUnitArchetypeType.DemonMelee:
                ApplyDemonMelee(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.DemonRanged:
                ApplyDemonRanged(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.ZombieGrunt:
            case FactionUnitArchetypeType.ZombieMelee:
            default:
                ApplyZombieMelee(rewardsEnabled);
                break;
            case FactionUnitArchetypeType.ZombieRanged:
                ApplyZombieRanged(rewardsEnabled);
                break;
        }
    }

    public static FactionType GetFaction(FactionUnitArchetypeType archetype)
    {
        switch (archetype)
        {
            case FactionUnitArchetypeType.HumanSupport:
            case FactionUnitArchetypeType.HumanMeleeAlly:
            case FactionUnitArchetypeType.HumanRangedAlly:
                return FactionType.Human;
            case FactionUnitArchetypeType.AngelMarksman:
            case FactionUnitArchetypeType.AngelMelee:
            case FactionUnitArchetypeType.AngelRanged:
                return FactionType.Angel;
            case FactionUnitArchetypeType.DemonRaider:
            case FactionUnitArchetypeType.DemonMelee:
            case FactionUnitArchetypeType.DemonRanged:
                return FactionType.Demon;
            case FactionUnitArchetypeType.ZombieGrunt:
            case FactionUnitArchetypeType.ZombieMelee:
            case FactionUnitArchetypeType.ZombieRanged:
            default:
                return FactionType.Zombie;
        }
    }

    private void ApplyHumanRangedAlly(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Human, health: 55, scale: 0.76f, colliderRadius: 0.42f, rewardsEnabled ?? false);

        SetEnabled<EnemyMovement>(false);
        SetEnabled<EnemyMeleeDamage>(false);
        SetEnabled<FactionRangedAttacker>(false);
        SetEnabled<RangedShooter>(false);

        FriendlyAlly ally = Ensure<FriendlyAlly>();
        ally.enabled = true;
        ally.ConfigureCombat(
            moveSpeed: 3.65f,
            attackRange: 7.75f,
            fireCooldown: 0.78f,
            projectileDamage: 6,
            projectileSpeed: 13f,
            projectileColor: new Color(1f, 0.86f, 0.28f, 1f));
    }

    private void ApplyHumanMeleeAlly(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Human, health: 64, scale: 0.8f, colliderRadius: 0.45f, rewardsEnabled ?? false);

        SetEnabled<FriendlyAlly>(false);
        SetEnabled<FactionRangedAttacker>(false);
        SetEnabled<RangedShooter>(false);

        EnemyMovement movement = Ensure<EnemyMovement>();
        movement.enabled = true;
        movement.ConfigureMovement(speed: 2.3f, targetRefreshInterval: 0.2f, maxTargetRange: 0f);

        EnemyMeleeDamage melee = Ensure<EnemyMeleeDamage>();
        melee.enabled = true;
        melee.ConfigureDamage(
            contactDamage: 8,
            damageInterval: 1.25f,
            element: DamageElement.Physical,
            status: StatusEffect.None,
            statusDuration: 0f,
            statusStrength: 0f);
    }

    private void ApplyAngelRanged(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Angel, health: 32, scale: 0.74f, colliderRadius: 0.42f, rewardsEnabled ?? false);

        SetEnabled<EnemyMovement>(false);
        SetEnabled<EnemyMeleeDamage>(false);
        SetEnabled<FriendlyAlly>(false);
        SetEnabled<RangedShooter>(false);

        FactionRangedAttacker ranged = Ensure<FactionRangedAttacker>();
        ranged.enabled = true;
        ranged.ConfigureCombat(
            newPreferredRange: 5.75f,
            newAttackRange: 8.75f,
            newMoveSpeed: 2.35f,
            newFireCooldown: 1.05f,
            newProjectileDamage: 6,
            newProjectileSpeed: 12f,
            newProjectileElement: DamageElement.Lightning,
            newProjectileStatus: StatusEffect.None,
            newStatusDuration: 0f,
            newStatusStrength: 0f,
            newProjectileColor: new Color(1f, 0.92f, 0.28f, 1f));
    }

    private void ApplyAngelMelee(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Angel, health: 42, scale: 0.78f, colliderRadius: 0.44f, rewardsEnabled ?? false);

        SetEnabled<FriendlyAlly>(false);
        SetEnabled<FactionRangedAttacker>(false);
        SetEnabled<RangedShooter>(false);

        EnemyMovement movement = Ensure<EnemyMovement>();
        movement.enabled = true;
        movement.ConfigureMovement(speed: 2.15f, targetRefreshInterval: 0.2f, maxTargetRange: 0f);

        EnemyMeleeDamage melee = Ensure<EnemyMeleeDamage>();
        melee.enabled = true;
        melee.ConfigureDamage(
            contactDamage: 9,
            damageInterval: 1.4f,
            element: DamageElement.Lightning,
            status: StatusEffect.Shock,
            statusDuration: 0.75f,
            statusStrength: 0.25f);
    }

    private void ApplyDemonMelee(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Demon, health: 48, scale: 0.84f, colliderRadius: 0.46f, rewardsEnabled ?? false);

        SetEnabled<FriendlyAlly>(false);
        SetEnabled<FactionRangedAttacker>(false);
        SetEnabled<RangedShooter>(false);

        EnemyMovement movement = Ensure<EnemyMovement>();
        movement.enabled = true;
        movement.ConfigureMovement(speed: 2.85f, targetRefreshInterval: 0.18f, maxTargetRange: 0f);

        EnemyMeleeDamage melee = Ensure<EnemyMeleeDamage>();
        melee.enabled = true;
        melee.ConfigureDamage(
            contactDamage: 12,
            damageInterval: 1.35f,
            element: DamageElement.Fire,
            status: StatusEffect.Burn,
            statusDuration: 2f,
            statusStrength: 0.35f);
    }

    private void ApplyDemonRanged(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Demon, health: 38, scale: 0.78f, colliderRadius: 0.42f, rewardsEnabled ?? false);

        SetEnabled<EnemyMovement>(false);
        SetEnabled<EnemyMeleeDamage>(false);
        SetEnabled<FriendlyAlly>(false);
        SetEnabled<RangedShooter>(false);

        FactionRangedAttacker ranged = Ensure<FactionRangedAttacker>();
        ranged.enabled = true;
        ranged.ConfigureCombat(
            newPreferredRange: 4.85f,
            newAttackRange: 8f,
            newMoveSpeed: 2.55f,
            newFireCooldown: 1.25f,
            newProjectileDamage: 8,
            newProjectileSpeed: 10.75f,
            newProjectileElement: DamageElement.Fire,
            newProjectileStatus: StatusEffect.Burn,
            newStatusDuration: 2f,
            newStatusStrength: 0.3f,
            newProjectileColor: new Color(1f, 0.32f, 0.08f, 1f));
    }

    private void ApplyZombieMelee(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Zombie, health: 24, scale: 0.68f, colliderRadius: 0.43f, rewardsEnabled ?? true);

        SetEnabled<FriendlyAlly>(false);
        SetEnabled<FactionRangedAttacker>(false);
        SetEnabled<RangedShooter>(false);

        EnemyMovement movement = Ensure<EnemyMovement>();
        movement.enabled = true;
        movement.ConfigureMovement(speed: 1.65f, targetRefreshInterval: 0.3f, maxTargetRange: 0f);

        EnemyMeleeDamage melee = Ensure<EnemyMeleeDamage>();
        melee.enabled = true;
        melee.ConfigureDamage(
            contactDamage: 6,
            damageInterval: 1.75f,
            element: DamageElement.Poison,
            status: StatusEffect.Poison,
            statusDuration: 2f,
            statusStrength: 0.22f);
    }

    private void ApplyZombieRanged(bool? rewardsEnabled)
    {
        ApplyCommon(FactionType.Zombie, health: 26, scale: 0.68f, colliderRadius: 0.42f, rewardsEnabled ?? true);

        SetEnabled<EnemyMovement>(false);
        SetEnabled<EnemyMeleeDamage>(false);
        SetEnabled<FriendlyAlly>(false);
        SetEnabled<RangedShooter>(false);

        FactionRangedAttacker ranged = Ensure<FactionRangedAttacker>();
        ranged.enabled = true;
        ranged.ConfigureCombat(
            newPreferredRange: 4.6f,
            newAttackRange: 7.25f,
            newMoveSpeed: 1.45f,
            newFireCooldown: 1.65f,
            newProjectileDamage: 5,
            newProjectileSpeed: 8.5f,
            newProjectileElement: DamageElement.Poison,
            newProjectileStatus: StatusEffect.Poison,
            newStatusDuration: 2.75f,
            newStatusStrength: 0.25f,
            newProjectileColor: new Color(0.5f, 1f, 0.22f, 1f));
    }

    private void ApplyCommon(FactionType faction, int health, float scale, float colliderRadius, bool rewardsEnabled)
    {
        transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);

        FactionMember member = FactionMember.Ensure(gameObject, faction);
        member.Configure(faction);
        FactionVisualIdentity.Ensure(gameObject);

        EnemyHealth enemyHealth = Ensure<EnemyHealth>();
        enemyHealth.ConfigureHealth(health);
        enemyHealth.SetRewardMode(GetRewardMode(faction, rewardsEnabled));

        Ensure<StatusReceiver>();

        Rigidbody2D rb = Ensure<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
            circle.radius = colliderRadius;
            circle.isTrigger = false;
        }
        else if (collider is CircleCollider2D circle)
        {
            circle.radius = colliderRadius;
            circle.isTrigger = false;
        }
    }

    private T Ensure<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();

        return component;
    }

    private EnemyRewardMode GetRewardMode(FactionType faction, bool rewardsEnabled)
    {
        if (rewardsEnabled)
            return EnemyRewardMode.Always;

        if (faction == FactionType.Human)
            return EnemyRewardMode.Disabled;

        return EnemyRewardMode.HumanOnly;
    }

    private void SetEnabled<T>(bool enabled) where T : Behaviour
    {
        T[] behaviours = GetComponents<T>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
                behaviours[i].enabled = enabled;
        }
    }
}
