using System.Collections.Generic;
using UnityEngine;

public class FactionAffinityTracker : MonoBehaviour
{
    public static FactionAffinityTracker Instance { get; private set; }

    private readonly Dictionary<FactionType, int> _helped = new Dictionary<FactionType, int>();
    private readonly Dictionary<FactionType, int> _opposed = new Dictionary<FactionType, int>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public int GetHelpedCount(FactionType faction)
    {
        return _helped.TryGetValue(faction, out int count) ? count : 0;
    }

    public int GetOpposedCount(FactionType faction)
    {
        return _opposed.TryGetValue(faction, out int count) ? count : 0;
    }

    public void RecordHelped(FactionType faction)
    {
        if (faction == FactionType.Neutral)
            return;

        _helped[faction] = GetHelpedCount(faction) + 1;
        Debug.Log($"FACTION AFFINITY: helped {faction} ({_helped[faction]} total)");
    }

    public void RecordOpposed(FactionType faction)
    {
        if (faction == FactionType.Neutral)
            return;

        _opposed[faction] = GetOpposedCount(faction) + 1;
        Debug.Log($"FACTION AFFINITY: opposed {faction} ({_opposed[faction]} total)");
    }
}
