using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    public Vector2 Position => transform.position;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
    }
#endif
}
