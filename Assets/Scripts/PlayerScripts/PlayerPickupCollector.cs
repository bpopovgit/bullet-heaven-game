using UnityEngine;

[RequireComponent(typeof(PlayerExperience))]
public class PlayerPickupCollector : MonoBehaviour
{
    [SerializeField] private float basePickupRadius = 2f;
    [SerializeField] private LayerMask pickupMask = ~0;

    private PlayerStats _stats;
    private readonly Collider2D[] _hits = new Collider2D[64];

    public float PickupRadius => basePickupRadius + (_stats != null ? _stats.PickupRadiusBonus : 0f);

    private void Awake()
    {
        _stats = GetComponent<PlayerStats>();
    }

    private void FixedUpdate()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, PickupRadius, _hits, pickupMask);

        for (int i = 0; i < count; i++)
        {
            if (_hits[i] != null && _hits[i].TryGetComponent<PlayerPickup>(out var pickup))
                pickup.AttractTo(transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, PickupRadius);
    }
}
