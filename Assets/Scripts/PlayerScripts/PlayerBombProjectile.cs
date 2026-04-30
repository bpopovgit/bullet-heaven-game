using System.Collections;
using UnityEngine;

public class PlayerBombProjectile : MonoBehaviour
{
    private BombAbilityDefinition _config;
    private Vector3 _targetPosition;
    private SpriteRenderer _renderer;

    public static void Spawn(Vector3 startPosition, Vector3 targetPosition, BombAbilityDefinition config)
    {
        if (config == null)
            return;

        GameObject projectile = new GameObject($"{config.displayName} Projectile");
        projectile.transform.position = startPosition;

        PlayerBombProjectile bombProjectile = projectile.AddComponent<PlayerBombProjectile>();
        bombProjectile.Initialize(targetPosition, config);
    }

    private void Initialize(Vector3 targetPosition, BombAbilityDefinition config)
    {
        _config = config;
        _targetPosition = targetPosition;

        _renderer = gameObject.AddComponent<SpriteRenderer>();
        _renderer.sprite = PickupSpriteFactory.CircleSprite;
        _renderer.color = config.projectileColor;
        _renderer.sortingLayerName = "Actors";
        _renderer.sortingOrder = 20;

        transform.localScale = Vector3.one * Mathf.Max(0.35f, config.projectileScale);
    }

    private void Update()
    {
        if (_config == null)
        {
            Destroy(gameObject);
            return;
        }

        float step = Mathf.Max(1f, _config.projectileSpeed) * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);
        transform.Rotate(0f, 0f, 240f * Time.deltaTime);

        float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.08f;
        transform.localScale = Vector3.one * Mathf.Max(0.35f, _config.projectileScale) * pulse;

        if (Vector3.Distance(transform.position, _targetPosition) <= 0.02f)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (_config == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 center = transform.position;
        DetonateAt(center, 1f, isSecondary: false);

        PlayerCombatModifiers modifiers = PlayerCombatModifiers.Instance;
        if (modifiers != null && modifiers.BombSecondaryBlastFraction > 0f)
        {
            if (_renderer != null)
                _renderer.enabled = false;
            StartCoroutine(SecondaryBlastRoutine(center, modifiers.BombSecondaryBlastFraction, modifiers.BombSecondaryBlastDelay));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator SecondaryBlastRoutine(Vector2 center, float fraction, float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, delay));
        DetonateAt(center, Mathf.Clamp01(fraction), isSecondary: true);
        Destroy(gameObject);
    }

    private void DetonateAt(Vector2 center, float damageFraction, bool isSecondary)
    {
        if (_config == null)
            return;

        float radius = isSecondary ? _config.radius * 0.85f : _config.radius;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        int hitCount = 0;

        int scaledDamage = Mathf.Max(1, Mathf.RoundToInt(_config.damage * damageFraction));
        DamagePacket packet = new DamagePacket(
            scaledDamage,
            _config.element,
            _config.status,
            _config.statusDuration,
            _config.statusStrength,
            splashRadius: radius,
            sourcePos: center);

        packet.Clamp();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || !hit.TryGetComponent<EnemyHealth>(out EnemyHealth enemy))
                continue;

            enemy.TakeDamage(packet);
            hitCount++;
        }

        BombExplosionVisual.Spawn(center, _config.explosionPrimaryColor, _config.explosionSecondaryColor, radius);
        GameAudio.PlayBombImpact();
        Debug.Log($"ACTIVE BOMB {(isSecondary ? "AFTERSHOCK" : "USED")}: {_config.displayName} hit {hitCount} enemies at {center}.");
    }
}
