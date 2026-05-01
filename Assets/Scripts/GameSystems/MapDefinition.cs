using UnityEngine;

public sealed class MapDefinition
{
    public readonly string Id;
    public readonly string DisplayName;
    public readonly FactionType ThemeFaction;
    public readonly Color BackgroundTint;
    public readonly float DurationSeconds;
    public readonly float EnemyHpMultiplier;
    public readonly float EnemyDamageMultiplier;
    public readonly bool IsBossDistrict;
    public readonly string Flavor;

    public MapDefinition(
        string id,
        string displayName,
        FactionType themeFaction,
        Color backgroundTint,
        float durationSeconds,
        float enemyHpMultiplier,
        float enemyDamageMultiplier,
        bool isBossDistrict,
        string flavor)
    {
        Id = id;
        DisplayName = displayName;
        ThemeFaction = themeFaction;
        BackgroundTint = backgroundTint;
        DurationSeconds = Mathf.Max(15f, durationSeconds);
        EnemyHpMultiplier = Mathf.Max(0.1f, enemyHpMultiplier);
        EnemyDamageMultiplier = Mathf.Max(0.1f, enemyDamageMultiplier);
        IsBossDistrict = isBossDistrict;
        Flavor = flavor ?? string.Empty;
    }
}
