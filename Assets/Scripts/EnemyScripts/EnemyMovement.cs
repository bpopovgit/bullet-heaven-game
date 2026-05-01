using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float targetRefreshInterval = 0.25f;
    [SerializeField] private float maxTargetRange = 0f;

    private Transform _target;
    private StatusReceiver _status;
    private FactionMember _faction;
    private Rigidbody2D _rb;
    private float _nextTargetRefreshTime;

    public void ConfigureMovement(float speed, float targetRefreshInterval = 0.25f, float maxTargetRange = 0f)
    {
        this.speed = Mathf.Max(0f, speed);
        this.targetRefreshInterval = Mathf.Max(0.05f, targetRefreshInterval);
        this.maxTargetRange = Mathf.Max(0f, maxTargetRange);
    }

    private void Awake()
    {
        _status = GetComponent<StatusReceiver>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
        if (_rb.bodyType == RigidbodyType2D.Static)
            _rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Start()
    {
        if (_status == null)
            _status = GetComponent<StatusReceiver>();

        RefreshTarget();
    }

    private void Update()
    {
        if (_target == null || Time.time >= _nextTargetRefreshTime)
            RefreshTarget();
    }

    private void FixedUpdate()
    {
        if (_target != null)
            MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        float speedMultiplier = _status != null ? _status.SpeedMultiplier : 1f;
        Vector2 currentPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            _target.position,
            speed * speedMultiplier * Time.fixedDeltaTime);

        if (_rb != null)
            _rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;
    }

    private void RefreshTarget()
    {
        _nextTargetRefreshTime = Time.time + Mathf.Max(0.05f, targetRefreshInterval);

        if (_faction == null)
            _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);

        FactionMember target = FactionTargeting.FindBestTarget(_faction, transform.position, maxTargetRange);
        _target = target != null ? target.transform : null;
    }
}
