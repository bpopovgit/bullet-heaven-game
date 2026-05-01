using UnityEngine;

public static class MapCatalog
{
    private static readonly MapDefinition[] Districts =
    {
        new MapDefinition(
            id: "zombie_outskirts",
            displayName: "Zombie Outskirts",
            themeFaction: FactionType.Zombie,
            backgroundTint: new Color(0.06f, 0.08f, 0.05f, 1f),
            durationSeconds: 75f,
            enemyHpMultiplier: 1.0f,
            enemyDamageMultiplier: 1.0f,
            isBossDistrict: false,
            flavor: "The shambling dead test your edge."),

        new MapDefinition(
            id: "demon_foothills",
            displayName: "Demon Foothills",
            themeFaction: FactionType.Demon,
            backgroundTint: new Color(0.10f, 0.04f, 0.04f, 1f),
            durationSeconds: 90f,
            enemyHpMultiplier: 1.30f,
            enemyDamageMultiplier: 1.15f,
            isBossDistrict: false,
            flavor: "The hills smell of brimstone and old blood."),

        new MapDefinition(
            id: "angel_citadel",
            displayName: "Angel Citadel",
            themeFaction: FactionType.Angel,
            backgroundTint: new Color(0.08f, 0.07f, 0.10f, 1f),
            durationSeconds: 120f,
            enemyHpMultiplier: 1.65f,
            enemyDamageMultiplier: 1.30f,
            isBossDistrict: true,
            flavor: "Gilded ramparts. The dragon waits within.")
    };

    public static int DistrictCount => Districts.Length;

    public static MapDefinition Get(int index)
    {
        if (index < 0 || index >= Districts.Length)
            return null;
        return Districts[index];
    }

    public static MapDefinition First => Districts.Length > 0 ? Districts[0] : null;
    public static MapDefinition Final => Districts.Length > 0 ? Districts[Districts.Length - 1] : null;
}
