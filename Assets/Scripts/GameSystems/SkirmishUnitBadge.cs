using UnityEngine;

public static class SkirmishUnitBadge
{
    private const string SortingLayerName = "Actors";
    private const int BadgeSortingOrder = 30;
    private const float BadgeYOffset = 0.7f;
    private const float BadgeScale = 0.32f;
    private const float HaloScale = 0.55f;

    public static void Attach(GameObject unit, FactionType sideFaction)
    {
        if (unit == null) return;

        GameObject badgeRoot = new GameObject("SkirmishBadge");
        badgeRoot.transform.SetParent(unit.transform, false);
        badgeRoot.transform.localPosition = new Vector3(0f, BadgeYOffset, 0f);

        Color sideColor = SkirmishMarker.GetFactionColor(sideFaction);

        GameObject halo = new GameObject("Halo");
        halo.transform.SetParent(badgeRoot.transform, false);
        halo.transform.localScale = Vector3.one * HaloScale;
        SpriteRenderer haloRenderer = halo.AddComponent<SpriteRenderer>();
        haloRenderer.sprite = PickupSpriteFactory.CircleSprite;
        haloRenderer.sortingLayerName = SortingLayerName;
        haloRenderer.sortingOrder = BadgeSortingOrder;
        haloRenderer.color = new Color(sideColor.r, sideColor.g, sideColor.b, 0.32f);

        GameObject pip = new GameObject("Pip");
        pip.transform.SetParent(badgeRoot.transform, false);
        pip.transform.localScale = Vector3.one * BadgeScale;
        SpriteRenderer pipRenderer = pip.AddComponent<SpriteRenderer>();
        pipRenderer.sprite = PickupSpriteFactory.CircleSprite;
        pipRenderer.sortingLayerName = SortingLayerName;
        pipRenderer.sortingOrder = BadgeSortingOrder + 1;
        pipRenderer.color = new Color(sideColor.r, sideColor.g, sideColor.b, 0.95f);

        badgeRoot.AddComponent<SkirmishUnitBadgePulser>();
    }
}

public class SkirmishUnitBadgePulser : MonoBehaviour
{
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        float pulse = (Mathf.Sin(Time.time * 3.2f) + 1f) * 0.5f;
        transform.localScale = _baseScale * (1f + pulse * 0.10f);
    }
}
