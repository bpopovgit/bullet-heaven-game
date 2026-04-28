using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FactionProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2.5f;

    private Rigidbody2D _rb;
    private Collider2D _collider;
    private FactionMember _ownerFaction;
    private DamagePacket _packet;
    private bool _configured;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;
    }

    public void Configure(
        Vector2 direction,
        FactionMember ownerFaction,
        int damage,
        float speed,
        DamageElement element = DamageElement.Physical,
        StatusEffect status = StatusEffect.None,
        float statusDuration = 0f,
        float statusStrength = 0f)
    {
        _ownerFaction = ownerFaction;
        _packet = new DamagePacket
        {
            amount = damage,
            element = element,
            splashRadius = 0f,
            sourcePos = transform.position,
            status = status,
            statusDuration = statusDuration,
            statusStrength = statusStrength
        };
        _packet.Clamp();
        _configured = true;

        Vector2 safeDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;
        _rb.linearVelocity = safeDirection * Mathf.Max(0.1f, speed);
        transform.right = safeDirection;
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_configured)
            return;

        if (FactionCombat.TryApplyDamage(other.gameObject, _packet, _ownerFaction, applyPlayerKnockback: false))
            Destroy(gameObject);
    }
}
