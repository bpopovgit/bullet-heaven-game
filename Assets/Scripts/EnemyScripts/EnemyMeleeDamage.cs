using UnityEngine;

public class EnemyMeleeDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int contactDamage = 10;
    [SerializeField] private float damageInterval = 2f; // seconds between hits
    [Header("Element & Status")]
    [SerializeField] private DamageElement element = DamageElement.Physical;
    [SerializeField] private StatusEffect status = StatusEffect.None;
    [SerializeField] private float statusDuration = 0f;
    [Range(0f, 1f)]
    [SerializeField] private float statusStrength = 0f;


    private float _damageTimer;
    private GameObject _targetInContact;
    private FactionMember _faction;

    private void Awake()
    {
        _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);
    }

    private void Update()
    {
        if (_targetInContact == null) return;

        _damageTimer -= Time.deltaTime;

        if (_damageTimer <= 0f)
        {
            var packet = new DamagePacket
            {
                amount = contactDamage,
                element = element,
                splashRadius = 0f,
                sourcePos = transform.position,

                status = status,
                statusDuration = statusDuration,
                statusStrength = statusStrength
            };

            packet.Clamp();
            FactionCombat.TryApplyDamage(_targetInContact, packet, _faction, applyPlayerKnockback: true);
            _damageTimer = damageInterval;
        }
    }

    // --- COLLISION VERSION (recommended for melee enemies) ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CanDamage(collision.collider.gameObject))
        {
            _targetInContact = collision.collider.gameObject;
            _damageTimer = 0f; // deal damage immediately on contact
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (_targetInContact == collision.collider.gameObject)
            _targetInContact = null;
    }

    private bool CanDamage(GameObject target)
    {
        if (target == null)
            return false;

        if (target.GetComponentInParent<PlayerHealth>() == null && target.GetComponentInParent<EnemyHealth>() == null)
            return false;

        FactionMember targetFaction = target.GetComponentInParent<FactionMember>();
        return _faction == null || targetFaction == null || FactionTargeting.AreHostile(_faction, targetFaction);
    }

    // --- TRIGGER VERSION (ONLY if you switch to trigger colliders) ---
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var player))
        {
            _playerInContact = player;
            _damageTimer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerHealth>(out var player))
        {
            if (_playerInContact == player)
            {
                _playerInContact = null;
            }
        }
    }
    */
}
