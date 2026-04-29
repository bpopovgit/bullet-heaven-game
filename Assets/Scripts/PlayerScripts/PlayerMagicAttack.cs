using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMagicAttack : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera mainCam;

    [Header("Spell")]
    [SerializeField] private bool magicEnabled;
    [SerializeField] private int baseDamage = 30;
    [SerializeField] private float range = 7.5f;
    [SerializeField] private float beamWidth = 0.55f;
    [SerializeField] private float cooldown = 0.55f;
    [SerializeField] private float visualDuration = 0.16f;
    [SerializeField] private DamageElement element = DamageElement.Lightning;
    [SerializeField] private StatusEffect status = StatusEffect.Shock;
    [SerializeField] private float statusChance = 0.3f;
    [SerializeField] private float statusDuration = 1.6f;
    [SerializeField] private float statusStrength = 0.28f;
    [SerializeField] private Color spellColor = new Color(0.45f, 0.92f, 1f, 0.9f);

    private PlayerInput _playerInput;
    private InputAction _fire;
    private PlayerStats _stats;
    private FactionMember _faction;
    private float _cooldownRemaining;

    public void ConfigureForCharacter(PlayableCharacterChoice character, StartingWeaponChoice primaryChoice)
    {
        magicEnabled = character == PlayableCharacterChoice.HumanArcanist;

        if (!magicEnabled)
            return;

        switch (primaryChoice)
        {
            case StartingWeaponChoice.FrostLance:
                ConfigureSpell(DamageElement.Frost, StatusEffect.Slow, 32, 7.25f, 0.62f, 0.62f, 0.45f, 2.5f, 0.38f, new Color(0.55f, 0.9f, 1f, 0.9f));
                break;

            case StartingWeaponChoice.VenomCaster:
                ConfigureSpell(DamageElement.Poison, StatusEffect.Poison, 28, 6.75f, 0.76f, 0.72f, 0.5f, 3f, 0.32f, new Color(0.45f, 1f, 0.42f, 0.9f));
                break;

            case StartingWeaponChoice.StormNeedler:
                ConfigureSpell(DamageElement.Lightning, StatusEffect.Shock, 30, 8.5f, 0.48f, 0.46f, 0.36f, 1.8f, 0.34f, new Color(1f, 0.95f, 0.28f, 0.9f));
                break;

            case StartingWeaponChoice.EmberRepeater:
            default:
                ConfigureSpell(DamageElement.Fire, StatusEffect.Burn, 30, 7.5f, 0.58f, 0.55f, 0.36f, 2.4f, 0.3f, new Color(1f, 0.43f, 0.14f, 0.9f));
                break;
        }
    }

    public void AddRange(float amount)
    {
        range += Mathf.Max(0f, amount);
        Debug.Log($"ARCANIST SPELL RANGE: {range:0.00}");
    }

    public void AddBeamWidth(float amount)
    {
        beamWidth += Mathf.Max(0f, amount);
        Debug.Log($"ARCANIST SPELL WIDTH: {beamWidth:0.00}");
    }

    public void ReduceCooldown(float seconds)
    {
        cooldown = Mathf.Max(0.16f, cooldown - Mathf.Max(0f, seconds));
        Debug.Log($"ARCANIST SPELL COOLDOWN: {cooldown:0.00}s");
    }

    public void AddStatusChance(float amount)
    {
        statusChance = Mathf.Clamp01(statusChance + Mathf.Max(0f, amount));
        Debug.Log($"ARCANIST STATUS CHANCE: {statusChance:P0}");
    }

    private void ConfigureSpell(
        DamageElement newElement,
        StatusEffect newStatus,
        int newDamage,
        float newRange,
        float newBeamWidth,
        float newCooldown,
        float newStatusChance,
        float newStatusDuration,
        float newStatusStrength,
        Color newSpellColor)
    {
        element = newElement;
        status = newStatus;
        baseDamage = Mathf.Max(1, newDamage);
        range = Mathf.Max(1f, newRange);
        beamWidth = Mathf.Max(0.1f, newBeamWidth);
        cooldown = Mathf.Max(0.05f, newCooldown);
        statusChance = Mathf.Clamp01(newStatusChance);
        statusDuration = Mathf.Max(0f, newStatusDuration);
        statusStrength = Mathf.Clamp01(newStatusStrength);
        spellColor = newSpellColor;
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _stats = GetComponent<PlayerStats>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Human);

        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void OnEnable()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        _fire = _playerInput.actions["Fire"];
        _playerInput.actions.Enable();
    }

    private void Update()
    {
        if (!magicEnabled)
            return;

        _cooldownRemaining -= Time.deltaTime;

        if (_cooldownRemaining > 0f || _fire == null || !_fire.IsPressed())
            return;

        Vector2 direction = GetAimDirection();
        CastSpell(direction);
        _cooldownRemaining = cooldown;
    }

    private Vector2 GetAimDirection()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null || Mouse.current == null)
            return transform.right.sqrMagnitude > 0.01f ? (Vector2)transform.right : Vector2.right;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 direction = mouseWorld - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.right.sqrMagnitude > 0.01f ? (Vector2)transform.right : Vector2.right;

        return direction.normalized;
    }

    private void CastSpell(Vector2 direction)
    {
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * (_stats != null ? _stats.DamageMultiplier : 1f)));
        DamagePacket packet = new DamagePacket
        {
            amount = damage,
            element = element,
            splashRadius = 0f,
            sourcePos = transform.position,
            status = Random.value <= statusChance ? status : StatusEffect.None,
            statusDuration = statusDuration,
            statusStrength = statusStrength
        };
        packet.Clamp();

        Vector2 origin = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range + beamWidth);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.GetComponentInParent<EnemyHealth>() == null)
                continue;

            Vector2 toTarget = (Vector2)hit.transform.position - origin;
            float forwardDistance = Vector2.Dot(toTarget, direction);
            if (forwardDistance < 0f || forwardDistance > range)
                continue;

            Vector2 closestPoint = origin + direction * forwardDistance;
            float perpendicularDistance = Vector2.Distance(hit.transform.position, closestPoint);
            if (perpendicularDistance > beamWidth)
                continue;

            FactionCombat.TryApplyDamage(hit.gameObject, packet, _faction, applyPlayerKnockback: false);
        }

        SpawnSpellVisual(origin, direction);
    }

    private void SpawnSpellVisual(Vector2 origin, Vector2 direction)
    {
        GameObject visual = new GameObject("ArcanistSpellVisual");
        LineRenderer line = visual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction.normalized * range);
        line.startWidth = beamWidth * 0.42f;
        line.endWidth = beamWidth * 0.16f;
        line.numCapVertices = 8;
        line.sortingLayerName = "Actors";
        line.sortingOrder = 13;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = spellColor;
        line.endColor = new Color(spellColor.r, spellColor.g, spellColor.b, 0.08f);

        GameObject impact = new GameObject("ArcanistSpellImpact");
        impact.transform.SetParent(visual.transform, false);
        impact.transform.position = origin + direction.normalized * range;
        impact.transform.localScale = Vector3.one * (beamWidth * 1.35f);

        SpriteRenderer impactRenderer = impact.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = PickupSpriteFactory.CircleSprite;
        impactRenderer.color = new Color(spellColor.r, spellColor.g, spellColor.b, 0.38f);
        impactRenderer.sortingLayerName = "Actors";
        impactRenderer.sortingOrder = 14;

        Destroy(visual, visualDuration);
    }
}
