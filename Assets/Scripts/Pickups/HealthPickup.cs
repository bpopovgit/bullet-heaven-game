using UnityEngine;

public class HealthPickup : PlayerPickup
{
    [Header("Health")]
    [SerializeField] private int healAmount = 20;
    [SerializeField] private bool collectAtFullHealth = false;

    public void SetHealAmount(int amount)
    {
        healAmount = Mathf.Max(1, amount);
    }

    protected override bool CanCollect(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        return health != null && (collectAtFullHealth || health.CurrentHP < health.MaxHP);
    }

    protected override void OnCollected(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.Heal(healAmount);
    }

    protected override GameSfxId GetPickupSound()
    {
        return GameSfxId.HealthPickup;
    }

    public static HealthPickup SpawnDefault(Vector3 position, int healAmount)
    {
        GameObject go = new GameObject("Health Pickup");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.45f;

        PickupSpriteFactory.AddDefaultRenderer(go, new Color(1f, 0.12f, 0.22f), sortingOrder: 6);
        PickupSpriteFactory.AddDefaultPhysics(go, radius: 0.5f);

        HealthPickup pickup = go.AddComponent<HealthPickup>();
        pickup.SetHealAmount(healAmount);

        return pickup;
    }
}
