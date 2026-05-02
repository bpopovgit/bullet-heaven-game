using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSecondaryMelee : MonoBehaviour
{
    private const float BashRange = 2.4f;
    private const float BashTravelDuration = 0.16f;
    private const float BashCooldown = 1.0f;
    private const float BashRadius = 1.55f;
    private const int BashBaseDamage = 30;
    private const float BashKnockbackForce = 14f;
    private const float StunDuration = 0.55f;

    private Camera _mainCam;
    private Rigidbody2D _rigidbody;
    private PlayerStats _stats;
    private FactionMember _faction;
    private PlayerHealth _health;

    private float _cooldownRemaining;
    private float _bashTimeRemaining;
    private Vector2 _bashVelocity;
    private bool _bashLandingResolved;
    private float _previousLinearDamping;

    public string DisplayName => "Shield Bash";
    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public float CooldownDuration => BashCooldown;
    public float CooldownNormalized => Mathf.Clamp01(_cooldownRemaining / BashCooldown);
    public bool IsBashing => _bashTimeRemaining > 0f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _stats = GetComponent<PlayerStats>();
        _health = GetComponent<PlayerHealth>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Human);
        _mainCam = Camera.main;
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;

        if (Time.timeScale <= 0f)
            return;

        if (_bashTimeRemaining > 0f || _cooldownRemaining > 0f)
            return;

        if (Mouse.current == null || !Mouse.current.rightButton.wasPressedThisFrame)
            return;

        TryBash();
    }

    private void FixedUpdate()
    {
        if (_bashTimeRemaining <= 0f)
            return;

        _rigidbody.linearVelocity = _bashVelocity;
        _bashTimeRemaining -= Time.fixedDeltaTime;

        if (_bashTimeRemaining <= 0f && !_bashLandingResolved)
        {
            ResolveLanding();
            _bashLandingResolved = true;
            _rigidbody.linearVelocity = Vector2.zero;
            _rigidbody.linearDamping = _previousLinearDamping;
        }
    }

    private void TryBash()
    {
        Vector2 direction = ResolveBashDirection();
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        direction = direction.normalized;
        _bashVelocity = direction * (BashRange / BashTravelDuration);
        _bashTimeRemaining = BashTravelDuration;
        _cooldownRemaining = BashCooldown;
        _bashLandingResolved = false;
        _previousLinearDamping = _rigidbody.linearDamping;
        _rigidbody.linearDamping = 0f;

        if (_health != null)
            _health.GrantTemporaryInvulnerability(BashTravelDuration + 0.05f);

        SpawnLungeStreak(transform.position, direction);
        Debug.Log($"SHIELD BASH: lunging {BashRange:0.##}m toward ({direction.x:0.##}, {direction.y:0.##}).");
    }

    private void ResolveLanding()
    {
        Vector2 origin = transform.position;
        int damage = Mathf.Max(1, Mathf.RoundToInt(BashBaseDamage * (_stats != null ? _stats.DamageMultiplier : 1f)));

        DamagePacket packet = new DamagePacket(
            damage,
            DamageElement.Physical,
            StatusEffect.Shock,
            StunDuration,
            0.6f,
            0f,
            origin);
        packet.Clamp();

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, BashRadius);
        int hitCount = 0;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                continue;

            FactionCombat.TryApplyDamage(hit.gameObject, packet, _faction, applyPlayerKnockback: false);

            Rigidbody2D body = hit.attachedRigidbody;
            if (body != null)
            {
                Vector2 outward = ((Vector2)hit.transform.position - origin);
                if (outward.sqrMagnitude < 0.001f)
                    outward = Random.insideUnitCircle.normalized;
                body.AddForce(outward.normalized * BashKnockbackForce, ForceMode2D.Impulse);
            }
            hitCount++;
        }

        SpawnLandingShock(origin);
        GameAudio.PlayPlayerHit();
        Debug.Log($"SHIELD BASH: landed and hit {hitCount} enemies for {damage} damage with stun.");
    }

    private Vector2 ResolveBashDirection()
    {
        if (_mainCam == null)
            _mainCam = Camera.main;
        if (_mainCam == null || Mouse.current == null)
            return (Vector2)transform.right;

        Vector3 mouseWorld = _mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 toCursor = (Vector2)(mouseWorld - transform.position);
        return toCursor.sqrMagnitude > 0.0001f ? toCursor : (Vector2)transform.right;
    }

    private static void SpawnLungeStreak(Vector3 origin, Vector2 direction)
    {
        GameObject visual = new GameObject("ShieldBashStreak");
        visual.transform.position = origin;
        visual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = ShatterPrimeGlowSpriteAccess.GetSprite();
        renderer.color = new Color(1f, 0.85f, 0.4f, 0.8f);
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 9;
        visual.transform.localScale = new Vector3(BashRange * 1.05f, 0.6f, 1f);

        DashTrailFader fader = visual.AddComponent<DashTrailFader>();
        fader.Begin(0.22f);
    }

    private static void SpawnLandingShock(Vector3 position)
    {
        GameObject visual = new GameObject("ShieldBashLanding");
        visual.transform.position = position;

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = ShatterPrimeGlowSpriteAccess.GetSprite();
        renderer.color = new Color(1f, 0.78f, 0.32f, 0.9f);
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 14;
        visual.transform.localScale = Vector3.one * (BashRadius * 1.6f);

        ShieldBashLandingFader fader = visual.AddComponent<ShieldBashLandingFader>();
        fader.Begin(0.32f);
    }
}

internal class ShieldBashLandingFader : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private float _duration;
    private float _elapsed;
    private float _startScale;

    public void Begin(float duration)
    {
        _renderer = GetComponent<SpriteRenderer>();
        _duration = Mathf.Max(0.01f, duration);
        _startScale = transform.localScale.x;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);
        transform.localScale = Vector3.one * Mathf.Lerp(_startScale, _startScale * 1.45f, t);

        if (_renderer != null)
        {
            Color c = _renderer.color;
            c.a = Mathf.Lerp(0.9f, 0f, t);
            _renderer.color = c;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}
