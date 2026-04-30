using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct RunTalentPointSnapshot
{
    public string talentId;
    public int points;
}

public class RunTalentState : MonoBehaviour
{
    [SerializeField] private List<RunTalentPointSnapshot> pointSnapshots = new List<RunTalentPointSnapshot>();

    private readonly Dictionary<string, int> _pointsById = new Dictionary<string, int>();
    private bool _initialized;

    public int GetPoints(string talentId)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(talentId))
            return 0;

        return _pointsById.TryGetValue(talentId, out int points) ? points : 0;
    }

    public bool HasPoints(string talentId)
    {
        return GetPoints(talentId) > 0;
    }

    public void AddPoint(string talentId, int maxPoints)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(talentId))
            return;

        int current = GetPoints(talentId);
        int next = maxPoints > 0 ? Mathf.Min(maxPoints, current + 1) : current + 1;
        _pointsById[talentId] = next;
        SyncSnapshots();

        Debug.Log($"RUN TALENT POINT: {talentId} {next}/{Mathf.Max(1, maxPoints)}");
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        _pointsById.Clear();
        for (int i = 0; i < pointSnapshots.Count; i++)
        {
            RunTalentPointSnapshot snapshot = pointSnapshots[i];
            if (!string.IsNullOrWhiteSpace(snapshot.talentId))
                _pointsById[snapshot.talentId] = Mathf.Max(0, snapshot.points);
        }

        _initialized = true;
    }

    private void SyncSnapshots()
    {
        pointSnapshots.Clear();
        foreach (KeyValuePair<string, int> pair in _pointsById)
        {
            pointSnapshots.Add(new RunTalentPointSnapshot
            {
                talentId = pair.Key,
                points = pair.Value
            });
        }
    }
}
