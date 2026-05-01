using System.Collections.Generic;
using UnityEngine;

public enum SkirmishState
{
    Pending,
    Active,
    Resolved
}

public sealed class FactionSkirmish
{
    public readonly string Id;
    public readonly FactionType SideAFaction;
    public readonly FactionType SideBFaction;
    public readonly Vector2 Anchor;
    public readonly float AnchorRadius;

    public readonly List<GameObject> SideAUnits = new List<GameObject>();
    public readonly List<GameObject> SideBUnits = new List<GameObject>();

    public SkirmishState State { get; private set; } = SkirmishState.Pending;
    public FactionType Winner { get; private set; } = FactionType.Neutral;

    public int PlayerDamageToA { get; private set; }
    public int PlayerDamageToB { get; private set; }
    public int PlayerKillsOnA { get; private set; }
    public int PlayerKillsOnB { get; private set; }

    public FactionSkirmish(string id, FactionType sideA, FactionType sideB, Vector2 anchor, float anchorRadius)
    {
        Id = id;
        SideAFaction = sideA;
        SideBFaction = sideB;
        Anchor = anchor;
        AnchorRadius = Mathf.Max(0.5f, anchorRadius);
    }

    public void MarkActive()
    {
        if (State == SkirmishState.Pending)
            State = SkirmishState.Active;
    }

    public bool TryResolve()
    {
        if (State == SkirmishState.Resolved)
            return false;

        PruneDeadUnits();

        bool aDead = SideAUnits.Count == 0;
        bool bDead = SideBUnits.Count == 0;

        if (!aDead && !bDead)
            return false;

        if (aDead && bDead)
            Winner = FactionType.Neutral;
        else if (aDead)
            Winner = SideBFaction;
        else
            Winner = SideAFaction;

        State = SkirmishState.Resolved;
        return true;
    }

    public void RecordPlayerDamageToA(int amount)
    {
        if (amount > 0)
            PlayerDamageToA += amount;
    }

    public void RecordPlayerDamageToB(int amount)
    {
        if (amount > 0)
            PlayerDamageToB += amount;
    }

    public void RecordPlayerKillOnA() => PlayerKillsOnA++;
    public void RecordPlayerKillOnB() => PlayerKillsOnB++;

    public bool PlayerHelped(FactionType side)
    {
        if (PlayerDamageToA == 0 && PlayerDamageToB == 0)
            return false;

        if (side == SideAFaction)
            return PlayerDamageToB > PlayerDamageToA * 2;
        if (side == SideBFaction)
            return PlayerDamageToA > PlayerDamageToB * 2;

        return false;
    }

    public bool PlayerWasIdle =>
        PlayerDamageToA + PlayerDamageToB == 0 &&
        PlayerKillsOnA + PlayerKillsOnB == 0;

    private void PruneDeadUnits()
    {
        for (int i = SideAUnits.Count - 1; i >= 0; i--)
        {
            if (SideAUnits[i] == null)
                SideAUnits.RemoveAt(i);
        }
        for (int i = SideBUnits.Count - 1; i >= 0; i--)
        {
            if (SideBUnits[i] == null)
                SideBUnits.RemoveAt(i);
        }
    }
}
