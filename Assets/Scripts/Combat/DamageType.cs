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

// Optional status flags you can expand later
public enum StatusEffect
{
    None,
    Burn,
    Shock,
    Slow,
    Poison
}

[Serializable]
public struct DamagePacket
{
    [Header("Base Damage")]
    public int amount;
    public DamageElement element;

    [Header("Optional Physics")]
    public float splashRadius;        // 0 = no splash
    public Vector2 sourcePos;         // for knockback direction (optional)

    [Header("Status Effect")]
    public StatusEffect status;        // None, Slow, Burn, etc.
    public float statusDuration;       // seconds
    public float statusStrength;       // 0–1 (e.g. 0.35 = 35%)

    // --------------------------------------------------
    // CONSTRUCTORS
    // --------------------------------------------------

    /// <summary>
    /// Convenience constructor for pure damage (no status).
    /// Keeps old code working: new DamagePacket(amount, element)
    /// </summary>
    public DamagePacket(
        int amount,
        DamageElement element,
        float splashRadius = 0f,
        Vector2 sourcePos = default)
    {
        this.amount = amount;
        this.element = element;
        this.splashRadius = splashRadius;
        this.sourcePos = sourcePos;

        // Status defaults
        this.status = StatusEffect.None;
        this.statusDuration = 0f;
        this.statusStrength = 0f;
    }

    /// <summary>
    /// Full constructor when you want damage + status in one line.
    /// </summary>
    public DamagePacket(
        int amount,
        DamageElement element,
        StatusEffect status,
        float statusDuration,
        float statusStrength,
        float splashRadius = 0f,
        Vector2 sourcePos = default)
    {
        this.amount = amount;
        this.element = element;
        this.splashRadius = splashRadius;
        this.sourcePos = sourcePos;

        this.status = status;
        this.statusDuration = statusDuration;
        this.statusStrength = statusStrength;
    }

    // --------------------------------------------------
    // VALIDATION / SAFETY
    // --------------------------------------------------

    /// <summary>
    /// Clamps values to safe ranges so bad data can’t break gameplay.
    /// Call once before applying damage.
    /// </summary>
    public void Clamp()
    {
        if (amount < 0)
            amount = 0;

        if (splashRadius < 0f)
            splashRadius = 0f;

        if (statusDuration < 0f)
            statusDuration = 0f;

        statusStrength = Mathf.Clamp01(statusStrength);
    }

    /// <summary>
    /// Convenience check.
    /// </summary>
    public bool HasStatus =>
        status != StatusEffect.None && statusDuration > 0f && statusStrength > 0f;
}
