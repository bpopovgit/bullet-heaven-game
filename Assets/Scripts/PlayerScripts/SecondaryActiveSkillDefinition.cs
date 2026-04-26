using UnityEngine;

public enum SecondaryActiveSkillType
{
    MagneticPulse,
    ArcaneShield,
    FrostNova
}

public sealed class SecondaryActiveSkillDefinition
{
    public string displayName;
    public SecondaryActiveSkillType type;
    public float cooldown;
    public float radius;
    public float duration;
    public float force;
    public int damage;
    public float statusDuration;
    public float statusStrength;
    public Color iconPrimaryColor;
    public Color iconSecondaryColor;
}
