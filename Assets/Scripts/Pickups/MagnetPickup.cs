using UnityEngine;

public class MagnetPickup : PlayerPickup
{
    protected override void OnCollected(GameObject player)
    {
        XPGem[] gems = FindObjectsOfType<XPGem>();

        foreach (XPGem gem in gems)
        {
            if (gem != null)
                gem.AttractTo(player.transform);
        }

        Debug.Log($"MAGNET PICKUP: Attracting {gems.Length} XP gems.");
    }

    public static MagnetPickup SpawnDefault(Vector3 position)
    {
        GameObject go = new GameObject("Magnet Pickup");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.5f;

        PickupSpriteFactory.AddDefaultRenderer(go, new Color(0.15f, 0.8f, 1f), sortingOrder: 6);
        PickupSpriteFactory.AddDefaultPhysics(go, radius: 0.5f);

        return go.AddComponent<MagnetPickup>();
    }
}
