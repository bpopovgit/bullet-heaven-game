using UnityEngine;

public class BossWorldHealthBar : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float width = 3.6f;
    [SerializeField] private float height = 0.3f;

    private Transform _fillTransform;
    private SpriteRenderer _fillRenderer;
    private SpriteRenderer _backgroundRenderer;

    public static BossWorldHealthBar Create(Transform target, EnemyHealth enemyHealth, Vector3 worldOffset, Color fillColor)
    {
        if (target == null || enemyHealth == null)
            return null;

        GameObject root = new GameObject("BossHealthBar");
        BossWorldHealthBar bar = root.AddComponent<BossWorldHealthBar>();
        bar.target = target;
        bar.enemyHealth = enemyHealth;
        bar.worldOffset = worldOffset;
        bar.Build(fillColor);
        bar.RefreshVisual();
        return bar;
    }

    public void SetFillColor(Color color)
    {
        if (_fillRenderer != null)
            _fillRenderer.color = color;
    }

    private void LateUpdate()
    {
        if (target == null || enemyHealth == null || enemyHealth.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = target.position + worldOffset;
        RefreshVisual();
    }

    private void Build(Color fillColor)
    {
        Sprite sprite = PickupSpriteFactory.CircleSprite;

        GameObject background = new GameObject("Background");
        background.transform.SetParent(transform, false);
        _backgroundRenderer = background.AddComponent<SpriteRenderer>();
        _backgroundRenderer.sprite = sprite;
        _backgroundRenderer.color = new Color(0.08f, 0.05f, 0.05f, 0.9f);
        _backgroundRenderer.sortingLayerName = "Actors";
        _backgroundRenderer.sortingOrder = 150;
        background.transform.localScale = new Vector3(width, height, 1f);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(transform, false);
        _fillTransform = fill.transform;
        _fillRenderer = fill.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite = sprite;
        _fillRenderer.color = fillColor;
        _fillRenderer.sortingLayerName = "Actors";
        _fillRenderer.sortingOrder = 151;
    }

    private void RefreshVisual()
    {
        if (_fillTransform == null || enemyHealth == null)
            return;

        int maxHealth = Mathf.Max(1, enemyHealth.MaxHealth);
        float ratio = Mathf.Clamp01(enemyHealth.CurrentHealth / (float)maxHealth);
        float fillWidth = Mathf.Max(0.001f, width * ratio);

        _fillTransform.localScale = new Vector3(fillWidth, height * 0.72f, 1f);
        _fillTransform.localPosition = new Vector3(-(width - fillWidth) * 0.5f, 0f, -0.01f);
    }
}
