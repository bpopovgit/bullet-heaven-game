#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FactionStarterPrefabBuilder
{
    private const string PrefabFolder = "Assets/Resources/Prefabs/Factions";
    private const string SpriteFolder = "Assets/Art/Sprites/Factions";

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

        CreateHumanAllyPrefab(humanSprite);
        CreateFactionPrefab("AngelTestUnit", FactionUnitArchetypeType.AngelMarksman, angelSprite, rewardsEnabled: false);
        CreateFactionPrefab("DemonTestUnit", FactionUnitArchetypeType.DemonRaider, demonSprite, rewardsEnabled: false);
        CreateFactionPrefab("ZombieTestUnit", FactionUnitArchetypeType.ZombieGrunt, zombieSprite, rewardsEnabled: true);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject humanPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/HumanAlly.prefab");
        Selection.activeObject = humanPrefab;
        Debug.Log("Created starter faction prefabs in Assets/Resources/Prefabs/Factions.");
    }

    private static void CreateHumanAllyPrefab(Sprite sprite)
    {
        GameObject go = CreateBaseActor("HumanAlly", FactionType.Human, sprite);
        FactionUnitArchetype.ApplyTo(go, FactionUnitArchetypeType.HumanSupport, rewardsEnabled: false);
        SavePrefab(go, "HumanAlly");
    }

    private static void CreateFactionPrefab(string name, FactionUnitArchetypeType archetype, Sprite sprite, bool rewardsEnabled)
    {
        GameObject go = CreateBaseActor(name, GetFactionForArchetype(archetype), sprite);
        FactionUnitArchetype.ApplyTo(go, archetype, rewardsEnabled);
        SavePrefab(go, name);
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

    private static FactionType GetFactionForArchetype(FactionUnitArchetypeType archetype)
    {
        switch (archetype)
        {
            case FactionUnitArchetypeType.HumanSupport:
                return FactionType.Human;
            case FactionUnitArchetypeType.AngelMarksman:
                return FactionType.Angel;
            case FactionUnitArchetypeType.DemonRaider:
                return FactionType.Demon;
            case FactionUnitArchetypeType.ZombieGrunt:
            default:
                return FactionType.Zombie;
        }
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
