#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FactionStarterPrefabBuilder
{
    private const string PrefabFolder = "Assets/Resources/Prefabs/Factions";
    private const string SpriteFolder = "Assets/Art/Sprites/Factions";
    private const string MeleeSourceFolder = "Assets/Prefabs/Enemies/Melee";
    private const string RangedSourceFolder = "Assets/Prefabs/Enemies/Ranged";

    [MenuItem("Tools/Bullet Heaven/Factions/Create Starter Prefabs")]
    public static void CreateStarterPrefabs()
    {
        EnsureFolder("Assets", "Resources");
        EnsureFolder("Assets/Resources", "Prefabs");
        EnsureFolder("Assets/Resources/Prefabs", "Factions");
        EnsureFolder("Assets/Art", "Sprites");
        EnsureFolder("Assets/Art/Sprites", "Factions");

        Sprite humanSprite = CreateCircleSpriteAsset("HumanAlly_Marker", new Color32(65, 200, 255, 255));
        Sprite angelSprite = CreateCircleSpriteAsset("AngelTestUnit_Marker", new Color32(250, 245, 185, 255));
        Sprite demonSprite = CreateCircleSpriteAsset("DemonTestUnit_Marker", new Color32(220, 55, 65, 255));
        Sprite zombieSprite = CreateCircleSpriteAsset("ZombieTestUnit_Marker", new Color32(115, 210, 80, 255));

        CreateFactionPrefab("HumanAlly", FactionUnitArchetypeType.HumanRangedAlly, $"{RangedSourceFolder}/RangedEnemy_Base.prefab", humanSprite, rewardsEnabled: false);
        CreateFactionPrefab("HumanAlly_Melee", FactionUnitArchetypeType.HumanMeleeAlly, $"{MeleeSourceFolder}/MeleeEnemy_Base.prefab", humanSprite, rewardsEnabled: false);
        CreateFactionPrefab("HumanAlly_Ranged", FactionUnitArchetypeType.HumanRangedAlly, $"{RangedSourceFolder}/RangedEnemy_Base.prefab", humanSprite, rewardsEnabled: false);

        CreateFactionPrefab("AngelTestUnit", FactionUnitArchetypeType.AngelRanged, $"{RangedSourceFolder}/RangedEnemy_Lightning.prefab", angelSprite, rewardsEnabled: false);
        CreateFactionPrefab("Angel_Melee", FactionUnitArchetypeType.AngelMelee, $"{MeleeSourceFolder}/MeleeEnemy_Lightning.prefab", angelSprite, rewardsEnabled: false);
        CreateFactionPrefab("Angel_Ranged", FactionUnitArchetypeType.AngelRanged, $"{RangedSourceFolder}/RangedEnemy_Lightning.prefab", angelSprite, rewardsEnabled: false);

        CreateFactionPrefab("DemonTestUnit", FactionUnitArchetypeType.DemonMelee, $"{MeleeSourceFolder}/MeleeEnemy_Fire.prefab", demonSprite, rewardsEnabled: false);
        CreateFactionPrefab("Demon_Melee", FactionUnitArchetypeType.DemonMelee, $"{MeleeSourceFolder}/MeleeEnemy_Fire.prefab", demonSprite, rewardsEnabled: false);
        CreateFactionPrefab("Demon_Ranged", FactionUnitArchetypeType.DemonRanged, $"{RangedSourceFolder}/RangedEnemy_Fire.prefab", demonSprite, rewardsEnabled: false);

        CreateFactionPrefab("ZombieTestUnit", FactionUnitArchetypeType.ZombieMelee, $"{MeleeSourceFolder}/MeleeEnemy_Poison.prefab", zombieSprite, rewardsEnabled: true);
        CreateFactionPrefab("Zombie_Melee", FactionUnitArchetypeType.ZombieMelee, $"{MeleeSourceFolder}/MeleeEnemy_Poison.prefab", zombieSprite, rewardsEnabled: true);
        CreateFactionPrefab("Zombie_Ranged", FactionUnitArchetypeType.ZombieRanged, $"{RangedSourceFolder}/RangedEnemy_Poison.prefab", zombieSprite, rewardsEnabled: true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject humanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/HumanAlly.prefab");
        Selection.activeObject = humanPrefab;
        Debug.Log("Created starter faction prefabs in Assets/Resources/Prefabs/Factions.");
    }

    private static void CreateFactionPrefab(
        string name,
        FactionUnitArchetypeType archetype,
        string sourcePrefabPath,
        Sprite fallbackSprite,
        bool rewardsEnabled)
    {
        GameObject go = CreateActorFromSourceOrFallback(name, FactionUnitArchetype.GetFaction(archetype), sourcePrefabPath, fallbackSprite);
        FactionUnitArchetype.ApplyTo(go, archetype, rewardsEnabled);
        SavePrefab(go, name);
    }

    private static GameObject CreateActorFromSourceOrFallback(
        string name,
        FactionType faction,
        string sourcePrefabPath,
        Sprite fallbackSprite)
    {
        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePrefabPath);
        if (sourcePrefab == null)
        {
            Debug.LogWarning($"Could not find source prefab at '{sourcePrefabPath}'. Creating marker fallback for {name}.");
            return CreateBaseActor(name, faction, fallbackSprite);
        }

        GameObject go = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject;
        if (go == null)
            go = Object.Instantiate(sourcePrefab);

        if (PrefabUtility.IsPartOfPrefabInstance(go))
            PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        go.name = name;
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;

        EnsureFactionBasics(go, faction, fallbackSprite);
        return go;
    }

    private static GameObject CreateBaseActor(
        string name,
        FactionType faction,
        Sprite sprite)
    {
        GameObject go = new GameObject(name);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 2;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;
        collider.isTrigger = false;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        FactionMember factionMember = go.AddComponent<FactionMember>();
        factionMember.Configure(faction);

        go.AddComponent<FactionVisualIdentity>();
        return go;
    }

    private static void EnsureFactionBasics(GameObject go, FactionType faction, Sprite fallbackSprite)
    {
        if (go == null)
            return;

        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = fallbackSprite;
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 2;
        }

        FactionMember factionMember = go.GetComponent<FactionMember>();
        if (factionMember == null)
            factionMember = go.AddComponent<FactionMember>();

        factionMember.Configure(faction);

        if (go.GetComponent<FactionVisualIdentity>() == null)
            go.AddComponent<FactionVisualIdentity>();
    }

    private static void SavePrefab(GameObject go, string fileName)
    {
        string path = $"{PrefabFolder}/{fileName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
    }

    private static Sprite CreateCircleSpriteAsset(string name, Color32 color)
    {
        string texturePath = $"{SpriteFolder}/{name}.png";
        string fullPath = Path.GetFullPath(texturePath);

        if (!File.Exists(fullPath))
        {
            Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, mipChain: false);
            texture.filterMode = FilterMode.Point;

            Vector2 center = new Vector2(15.5f, 15.5f);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= 14f ? color : Color.clear);
                }
            }

            texture.Apply();
            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        AssetDatabase.ImportAsset(texturePath);
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
    }

    private static void EnsureFolder(string parent, string folderName)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{folderName}"))
            AssetDatabase.CreateFolder(parent, folderName);
    }
}
#endif
