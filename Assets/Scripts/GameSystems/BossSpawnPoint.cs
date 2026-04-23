using UnityEngine;

public class BossSpawnPoint : MonoBehaviour
{
    [SerializeField] private int priority = 0;

    public Vector3 Position => transform.position;
    public int Priority => priority;

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 1.25f);
    }
}
