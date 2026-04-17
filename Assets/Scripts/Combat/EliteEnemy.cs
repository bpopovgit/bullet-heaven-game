using UnityEngine;

public class EliteEnemy : MonoBehaviour
{
    private bool _configured;

    public void Configure(
        float healthMultiplier,
        float rewardMultiplier,
        float pickupDropChanceBonus,
        float scaleMultiplier,
        Color tintColor)
    {
        if (_configured)
            return;

        _configured = true;

        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
            health.ApplyEliteModifiers(healthMultiplier, rewardMultiplier, pickupDropChanceBonus);

        float safeScale = Mathf.Max(1f, scaleMultiplier);
        transform.localScale *= safeScale;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null)
                renderer.color = tintColor;
        }

        gameObject.name = $"Elite {gameObject.name}";
    }
}
