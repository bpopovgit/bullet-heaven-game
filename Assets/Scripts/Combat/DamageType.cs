using System;
using UnityEngine;

// Element type of the damage
public enum DamageElement
{
    Physical,
    Fire,
    Lightning,
    Frost,
    Poison
}

[Serializable]
public struct DamagePacket
{
    public int amount;
    public DamageElement element;
    public float splashRadius;   // 0 = no splash
    public Vector2 sourcePos;    // for knockback direction (optional)

    // Convenience constructor so we can write: new DamagePacket(amount, element)
    public DamagePacket(int amount, DamageElement element, float splashRadius = 0f, Vector2 sourcePos = default)
    {
        this.amount = amount;
        this.element = element;
        this.splashRadius = splashRadius;
        this.sourcePos = sourcePos;
    }
}

// Optional status flags you can expand later
public enum StatusEffect { None, Burn, Shock, Slow, Poison }
