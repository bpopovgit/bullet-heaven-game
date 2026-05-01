using UnityEngine;

public class GoldPickup : PlayerPickup
{
    private static readonly Color GoldColor = new Color(1f, 0.78f, 0.18f, 1f);

    [Header("Gold")]
    [SerializeField] private int value = 1;

    public void SetValue(int amount)
    {
        value = Mathf.Max(1, amount);
    }

    protected override bool CanCollect(GameObject player)
    {
        // Only the player picks up gold — without this guard, enemies/bullets/allies passing
        // over the coin would "collect" it via OnTriggerEnter2D.
        return RunSession.IsActive && player.GetComponent<PlayerExperience>() != null;
    }

    protected override void OnCollected(GameObject player)
    {
        RunSession.AddCurrency(value);
        Debug.Log($"GOLD COLLECTED: +{value} (total {RunSession.Currency}).");
    }

    protected override GameSfxId GetPickupSound()
    {
        return GameSfxId.GoldPickup;
    }

    public static GoldPickup SpawnDefault(Vector3 position, int amount)
    {
        GameObject go = new GameObject("Gold Pickup");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.36f;

        PickupSpriteFactory.AddDefaultRenderer(go, GoldColor, sortingOrder: 6);
        PickupSpriteFactory.AddDefaultPhysics(go, radius: 0.42f);

        GoldPickup pickup = go.AddComponent<GoldPickup>();
        pickup.SetValue(amount);

        return pickup;
    }
}
