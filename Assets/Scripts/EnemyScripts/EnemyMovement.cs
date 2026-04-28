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
    private float _nextTargetRefreshTime;

    private void Awake()
    {
        _status = GetComponent<StatusReceiver>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Zombie);
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

        if (_target != null)
            MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        float speedMultiplier = _status != null ? _status.SpeedMultiplier : 1f;
        transform.position = Vector2.MoveTowards(transform.position, _target.position, speed * speedMultiplier * Time.deltaTime);
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
