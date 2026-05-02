using System.Collections.Generic;
using UnityEngine;

public class MagneticVortexEffect : MonoBehaviour
{
    private const float PullDuration = 0.55f;
    private const float HoldDuration = 0.12f;
    private const float TotalDuration = 0.95f;
    private const float PullStrength = 18f;

    private float _radius;
    private int _detonationDamage;
    private float _pushForce;
    private Color _innerColor;
    private Color _outerColor;
    private float _elapsed;
    private bool _detonated;

    private SpriteRenderer _ringRenderer;
    private SpriteRenderer _coreRenderer;

    private readonly Collider2D[] _hits = new Collider2D[64];
    private readonly HashSet<EnemyHealth> _trackedEnemies = new HashSet<EnemyHealth>();

    public static MagneticVortexEffect Spawn(Vector3 position, float radius, int detonationDamage, float pushForce, Color innerColor, Color outerColor)
    {
        GameObject go = new GameObject("MagneticVortex");
        go.transform.position = position;

        MagneticVortexEffect vortex = go.AddComponent<MagneticVortexEffect>();
        vortex._radius = radius;
        vortex._detonationDamage = detonationDamage;
        vortex._pushForce = pushForce;
        vortex._innerColor = innerColor;
        vortex._outerColor = outerColor;
        vortex.BuildVisuals();
        return vortex;
    }

    private void BuildVisuals()
    {
        GameObject ring = new GameObject("Ring");
        ring.transform.SetParent(transform, false);
        _ringRenderer = ring.AddComponent<SpriteRenderer>();
        _ringRenderer.sprite = ShatterPrimeGlowSpriteAccess.GetSprite();
        _ringRenderer.color = _outerColor;
        _ringRenderer.sortingLayerName = "Actors";
        _ringRenderer.sortingOrder = 16;
        ring.transform.localScale = Vector3.one * (_radius * 1.6f);

        GameObject core = new GameObject("Core");
        core.transform.SetParent(transform, false);
        _coreRenderer = core.AddComponent<SpriteRenderer>();
        _coreRenderer.sprite = ShatterPrimeGlowSpriteAccess.GetSprite();
        _coreRenderer.color = _innerColor;
        _coreRenderer.sortingLayerName = "Actors";
        _coreRenderer.sortingOrder = 17;
        core.transform.localScale = Vector3.one * (_radius * 0.55f);
    }

    private void FixedUpdate()
    {
        if (_detonated)
            return;

        if (_elapsed >= PullDuration)
            return;

        Vector2 origin = transform.position;
        int count = Physics2D.OverlapCircleNonAlloc(origin, _radius * 1.1f, _hits);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _hits[i];
            if (hit == null || !hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                continue;

            Rigidbody2D body = hit.attachedRigidbody;
            if (body == null)
                continue;

            _trackedEnemies.Add(enemy);
            Vector2 toCenter = origin - (Vector2)hit.transform.position;
            float distance = toCenter.magnitude;
            if (distance < 0.2f)
                continue;

            Vector2 direction = toCenter / distance;
            float scale = Mathf.Clamp01(distance / _radius);
            body.AddForce(direction * (PullStrength * scale), ForceMode2D.Force);
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        // Spin + pulse the visuals.
        transform.Rotate(0f, 0f, 540f * Time.deltaTime, Space.Self);

        float pullT = Mathf.Clamp01(_elapsed / PullDuration);
        if (_ringRenderer != null)
        {
            float scale = Mathf.Lerp(_radius * 1.7f, _radius * 0.6f, pullT);
            _ringRenderer.transform.localScale = Vector3.one * scale;
            Color c = _ringRenderer.color;
            c.a = Mathf.Lerp(0.7f, 1f, pullT);
            _ringRenderer.color = c;
        }
        if (_coreRenderer != null)
        {
            float coreScale = Mathf.Lerp(_radius * 0.45f, _radius * 0.95f, pullT);
            _coreRenderer.transform.localScale = Vector3.one * coreScale;
        }

        if (!_detonated && _elapsed >= PullDuration + HoldDuration)
            Detonate();

        if (_elapsed >= TotalDuration)
            Destroy(gameObject);
    }

    private void Detonate()
    {
        _detonated = true;
        Vector2 origin = transform.position;

        DamagePacket packet = new DamagePacket(
            _detonationDamage,
            DamageElement.Lightning,
            StatusEffect.Shock,
            1.6f,
            0.35f,
            0f,
            origin);
        packet.Clamp();

        int count = Physics2D.OverlapCircleNonAlloc(origin, _radius * 1.2f, _hits);
        int hitCount = 0;
        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _hits[i];
            if (hit == null || !hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                continue;

            enemy.TakeDamage(packet);
            Rigidbody2D body = hit.attachedRigidbody;
            if (body != null)
            {
                Vector2 outward = ((Vector2)hit.transform.position - origin).normalized;
                if (outward.sqrMagnitude < 0.001f)
                    outward = Random.insideUnitCircle.normalized;
                body.AddForce(outward * _pushForce, ForceMode2D.Impulse);
            }
            hitCount++;
        }

        if (_ringRenderer != null)
        {
            _ringRenderer.transform.localScale = Vector3.one * (_radius * 2.2f);
            _ringRenderer.color = new Color(_outerColor.r, _outerColor.g, _outerColor.b, 0.9f);
        }
        if (_coreRenderer != null)
        {
            _coreRenderer.transform.localScale = Vector3.one * (_radius * 1.4f);
            _coreRenderer.color = new Color(1f, 1f, 0.9f, 0.95f);
        }

        Debug.Log($"MAGNETIC VORTEX: detonated, hit {hitCount} enemies for {_detonationDamage} shock damage.");
    }
}
