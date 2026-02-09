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
    private PlayerHealth _playerInContact;

    private void Update()
    {
        if (_playerInContact == null) return;

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
            _playerInContact.TakeDamage(packet, applyKnockback: true);
            _damageTimer = damageInterval;
        }
    }

    // --- COLLISION VERSION (recommended for melee enemies) ---

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerHealth>(out var player))
        {
            _playerInContact = player;
            _damageTimer = 0f; // deal damage immediately on contact
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent<PlayerHealth>(out var player))
        {
            if (_playerInContact == player)
            {
                _playerInContact = null;
            }
        }
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
