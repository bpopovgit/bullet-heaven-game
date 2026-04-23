using UnityEngine;

public class BombPickup : PlayerPickup
{
    [Header("Bomb")]
    [SerializeField] private int damage = 50;
    [SerializeField] private float radius = 6f;

    public void Configure(int damageAmount, float blastRadius)
    {
        damage = Mathf.Max(1, damageAmount);
        radius = Mathf.Max(0.1f, blastRadius);
    }

    protected override void OnCollected(GameObject player)
    {
        Vector2 center = player.transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        int hitCount = 0;

        DamagePacket packet = new DamagePacket(
            damage,
            DamageElement.Physical,
            splashRadius: radius,
            sourcePos: center);

        packet.Clamp();

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.TryGetComponent<EnemyHealth>(out var enemy))
            {
                enemy.TakeDamage(packet);
                hitCount++;
            }
        }

        Debug.Log($"BOMB PICKUP: Hit {hitCount} enemies for {damage} damage.");
    }

    protected override GameSfxId GetPickupSound()
    {
        return GameSfxId.BombPickup;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    public static BombPickup SpawnDefault(Vector3 position, int damageAmount, float blastRadius)
    {
        GameObject go = new GameObject("Bomb Pickup");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.55f;

        PickupSpriteFactory.AddDefaultRenderer(go, new Color(1f, 0.68f, 0.08f), sortingOrder: 6);
        PickupSpriteFactory.AddDefaultPhysics(go, radius: 0.5f);

        BombPickup pickup = go.AddComponent<BombPickup>();
        pickup.Configure(damageAmount, blastRadius);

        return pickup;
    }
}
