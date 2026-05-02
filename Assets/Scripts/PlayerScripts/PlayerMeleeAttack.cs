using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMeleeAttack : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private Camera mainCam;

    [Header("Cleave")]
    [SerializeField] private bool meleeEnabled;
    [SerializeField] private int baseDamage = 32;
    [SerializeField] private float radius = 1.75f;
    [SerializeField] private float arcAngle = 105f;
    [SerializeField] private float cooldown = 0.42f;
    [SerializeField] private float visualDuration = 0.12f;
    [SerializeField] private Color slashColor = new Color(1f, 0.78f, 0.24f, 0.44f);

    private const float WhirlwindDuration = 0.85f;
    private const float WhirlwindTickInterval = 0.17f;
    private const float WhirlwindTickDamageFraction = 0.6f;
    private const float WhirlwindRadiusBonus = 0.55f;
    private const float WhirlwindCooldown = 1.05f;

    private PlayerInput _playerInput;
    private InputAction _fire;
    private PlayerStats _stats;
    private FactionMember _faction;
    private float _cooldownRemaining;

    private bool _whirlwindUnlocked;
    private bool _whirlwindActive;
    private float _whirlwindTimeRemaining;
    private float _whirlwindNextTickAt;
    private WhirlwindVisual _activeWhirlwindVisual;

    public bool IsWhirlwindUnlocked => _whirlwindUnlocked;

    public void ConfigureForCharacter(PlayableCharacterChoice character)
    {
        meleeEnabled = character == PlayableCharacterChoice.HumanVanguard;

        if (meleeEnabled)
        {
            baseDamage = 32;
            radius = 1.85f;
            arcAngle = 112f;
            cooldown = 0.42f;
            slashColor = new Color(1f, 0.78f, 0.24f, 0.46f);
        }
    }

    public void AddRadius(float amount)
    {
        radius += Mathf.Max(0f, amount);
        Debug.Log($"VANGUARD CLEAVE RANGE: {radius:0.00}");
    }

    public void AddArcAngle(float degrees)
    {
        arcAngle = Mathf.Clamp(arcAngle + Mathf.Max(0f, degrees), 45f, 220f);
        Debug.Log($"VANGUARD CLEAVE ARC: {arcAngle:0} degrees");
    }

    public void ReduceCooldown(float seconds)
    {
        cooldown = Mathf.Max(0.14f, cooldown - Mathf.Max(0f, seconds));
        Debug.Log($"VANGUARD CLEAVE COOLDOWN: {cooldown:0.00}s");
    }

    public void UnlockWhirlwind()
    {
        if (_whirlwindUnlocked)
            return;

        _whirlwindUnlocked = true;
        Debug.Log("WHIRLWIND STANCE UNLOCKED: cleave is now a 360 degrees whirlwind.");

        if (RunAnnouncementUI.Instance != null)
            RunAnnouncementUI.Instance.ShowMessage("WHIRLWIND STANCE", 2.6f);
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
        if (!meleeEnabled)
            return;

        _cooldownRemaining -= Time.deltaTime;

        if (_whirlwindActive)
        {
            UpdateWhirlwind();
            return;
        }

        if (_cooldownRemaining > 0f || _fire == null || !_fire.IsPressed())
            return;

        Vector2 direction = GetAimDirection();

        if (_whirlwindUnlocked)
            StartWhirlwind();
        else
        {
            PerformCleave(direction);
            _cooldownRemaining = cooldown;
        }
    }

    private void StartWhirlwind()
    {
        _whirlwindActive = true;
        _whirlwindTimeRemaining = WhirlwindDuration;
        _whirlwindNextTickAt = 0f;
        _activeWhirlwindVisual = WhirlwindVisual.Spawn(transform, radius + WhirlwindRadiusBonus, WhirlwindDuration);
    }

    private void UpdateWhirlwind()
    {
        _whirlwindTimeRemaining -= Time.deltaTime;
        _whirlwindNextTickAt -= Time.deltaTime;

        if (_whirlwindNextTickAt <= 0f)
        {
            PerformWhirlwindTick();
            _whirlwindNextTickAt = WhirlwindTickInterval;
        }

        if (_whirlwindTimeRemaining <= 0f)
        {
            _whirlwindActive = false;
            _whirlwindTimeRemaining = 0f;
            _cooldownRemaining = WhirlwindCooldown;
            _activeWhirlwindVisual = null;
        }
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

    private void PerformCleave(Vector2 direction)
    {
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * (_stats != null ? _stats.DamageMultiplier : 1f)));
        DamagePacket packet = new DamagePacket
        {
            amount = damage,
            element = DamageElement.Physical,
            splashRadius = 0f,
            sourcePos = transform.position,
            status = StatusEffect.None,
            statusDuration = 0f,
            statusStrength = 0f
        };
        packet.Clamp();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.GetComponentInParent<EnemyHealth>() == null)
                continue;

            Vector2 toTarget = (Vector2)hit.transform.position - (Vector2)transform.position;
            if (toTarget.sqrMagnitude <= 0.001f)
                continue;

            float angle = Vector2.Angle(direction, toTarget.normalized);
            if (angle > arcAngle * 0.5f)
                continue;

            FactionCombat.TryApplyDamage(hit.gameObject, packet, _faction, applyPlayerKnockback: false);
        }

        SpawnSlashVisual(direction);
    }

    private void PerformWhirlwindTick()
    {
        int tickDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * WhirlwindTickDamageFraction * (_stats != null ? _stats.DamageMultiplier : 1f)));
        float effectiveRadius = radius + WhirlwindRadiusBonus;
        DamagePacket packet = new DamagePacket
        {
            amount = tickDamage,
            element = DamageElement.Physical,
            splashRadius = 0f,
            sourcePos = transform.position,
            status = StatusEffect.None,
            statusDuration = 0f,
            statusStrength = 0f
        };
        packet.Clamp();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, effectiveRadius);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit.GetComponentInParent<EnemyHealth>() == null)
                continue;

            FactionCombat.TryApplyDamage(hit.gameObject, packet, _faction, applyPlayerKnockback: false);
        }
    }

    private void SpawnSlashVisual(Vector2 direction)
    {
        GameObject visual = new GameObject("VanguardCleaveVisual");
        visual.transform.position = transform.position + (Vector3)(direction.normalized * 0.12f);
        visual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        MeshFilter filter = visual.AddComponent<MeshFilter>();
        MeshRenderer renderer = visual.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = slashColor;
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 12;

        filter.mesh = CreateArcMesh(radius, arcAngle, segments: 18);
        Destroy(visual, visualDuration);
    }

    private static Mesh CreateArcMesh(float radius, float arcAngle, int segments)
    {
        int safeSegments = Mathf.Max(3, segments);
        Vector3[] vertices = new Vector3[safeSegments + 2];
        int[] triangles = new int[safeSegments * 3];

        vertices[0] = Vector3.zero;
        float startAngle = -arcAngle * 0.5f;
        float step = arcAngle / safeSegments;

        for (int i = 0; i <= safeSegments; i++)
        {
            float angle = (startAngle + step * i) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }

        for (int i = 0; i < safeSegments; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        Mesh mesh = new Mesh
        {
            name = "VanguardCleaveArc",
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateBounds();
        return mesh;
    }
}
