using UnityEngine;

[System.Flags]
public enum EnemySpawnRegion
{
    None = 0,
    North = 1 << 0,
    East = 1 << 1,
    South = 1 << 2,
    West = 1 << 3,
    Center = 1 << 4,
    Any = North | East | South | West | Center
}

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Grouping")]
    [SerializeField] private bool autoAssignFromPosition = true;
    [SerializeField] private EnemySpawnRegion spawnRegions = EnemySpawnRegion.None;
    [SerializeField] private float centerThreshold = 3f;

    public Vector2 Position => transform.position;
    public EnemySpawnRegion SpawnRegions => GetEffectiveRegions();

    private EnemySpawnRegion GetEffectiveRegions()
    {
        if (autoAssignFromPosition || spawnRegions == EnemySpawnRegion.None)
            return CalculateRegionsFromPosition();

        return spawnRegions;
    }

    private EnemySpawnRegion CalculateRegionsFromPosition()
    {
        Vector3 pos = transform.position;
        EnemySpawnRegion regions = EnemySpawnRegion.None;
        float threshold = Mathf.Max(0f, centerThreshold);

        if (Mathf.Abs(pos.x) <= threshold && Mathf.Abs(pos.y) <= threshold)
            regions |= EnemySpawnRegion.Center;

        if (pos.y >= threshold)
            regions |= EnemySpawnRegion.North;
        else if (pos.y <= -threshold)
            regions |= EnemySpawnRegion.South;

        if (pos.x >= threshold)
            regions |= EnemySpawnRegion.East;
        else if (pos.x <= -threshold)
            regions |= EnemySpawnRegion.West;

        return regions == EnemySpawnRegion.None ? EnemySpawnRegion.Center : regions;
    }

    private Color GetGizmoColor()
    {
        EnemySpawnRegion regions = GetEffectiveRegions();

        if ((regions & EnemySpawnRegion.Center) != 0)
            return new Color(0.9f, 0.9f, 0.9f, 0.9f);

        bool north = (regions & EnemySpawnRegion.North) != 0;
        bool east = (regions & EnemySpawnRegion.East) != 0;
        bool south = (regions & EnemySpawnRegion.South) != 0;
        bool west = (regions & EnemySpawnRegion.West) != 0;

        if (north && east)
            return new Color(0.4f, 0.9f, 1f, 0.95f);
        if (north && west)
            return new Color(0.55f, 0.7f, 1f, 0.95f);
        if (south && east)
            return new Color(1f, 0.65f, 0.4f, 0.95f);
        if (south && west)
            return new Color(1f, 0.45f, 0.55f, 0.95f);
        if (north)
            return new Color(0.3f, 0.75f, 1f, 0.95f);
        if (east)
            return new Color(1f, 0.8f, 0.3f, 0.95f);
        if (south)
            return new Color(1f, 0.35f, 0.35f, 0.95f);
        if (west)
            return new Color(0.75f, 0.5f, 1f, 0.95f);

        return Color.white;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = GetGizmoColor();
        Gizmos.DrawWireSphere(transform.position, 0.35f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.8f);
    }
#endif
}
