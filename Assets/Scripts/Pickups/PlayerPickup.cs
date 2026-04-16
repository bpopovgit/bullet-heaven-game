using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public abstract class PlayerPickup : MonoBehaviour
{
    [Header("Pickup Movement")]
    [SerializeField] private float attractSpeed = 8f;
    [SerializeField] private float collectDistance = 0.25f;
    [SerializeField] private float lifetime = 0f;

    private Transform _target;
    private Rigidbody2D _rb;
    private bool _collected;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    protected virtual void Start()
    {
        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (_target == null || _collected)
            return;

        Vector2 nextPosition = Vector2.MoveTowards(
            transform.position,
            _target.position,
            attractSpeed * Time.fixedDeltaTime);

        if (_rb != null)
            _rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        if (Vector2.Distance(transform.position, _target.position) <= collectDistance)
            TryCollect(_target.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other.gameObject);
    }

    public void AttractTo(Transform target)
    {
        if (target == null || _collected)
            return;

        _target = target;
    }

    private void TryCollect(GameObject player)
    {
        if (_collected || player == null || !CanCollect(player))
            return;

        _collected = true;
        OnCollected(player);
        Destroy(gameObject);
    }

    protected virtual bool CanCollect(GameObject player)
    {
        return player.GetComponent<PlayerHealth>() != null || player.GetComponent<PlayerExperience>() != null;
    }

    protected abstract void OnCollected(GameObject player);
}
