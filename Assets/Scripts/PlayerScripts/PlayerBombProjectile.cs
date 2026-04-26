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
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, _config.radius);
        int hitCount = 0;

        DamagePacket packet = new DamagePacket(
            _config.damage,
            _config.element,
            _config.status,
            _config.statusDuration,
            _config.statusStrength,
            splashRadius: _config.radius,
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

        BombExplosionVisual.Spawn(transform.position, _config.explosionPrimaryColor, _config.explosionSecondaryColor, _config.radius);
        GameAudio.PlayBombImpact();
        Debug.Log($"ACTIVE BOMB USED: {_config.displayName} hit {hitCount} enemies at {transform.position}.");
        Destroy(gameObject);
    }
}
