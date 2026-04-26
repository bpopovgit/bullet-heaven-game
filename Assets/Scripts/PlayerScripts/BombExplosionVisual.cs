using UnityEngine;

public class BombExplosionVisual : MonoBehaviour
{
    private SpriteRenderer _coreRenderer;
    private SpriteRenderer _ringRenderer;
    private float _duration;
    private float _elapsed;
    private Vector3 _startCoreScale;
    private Vector3 _endCoreScale;
    private Vector3 _startRingScale;
    private Vector3 _endRingScale;
    private Color _coreColor;
    private Color _ringColor;

    public static void Spawn(Vector3 position, Color primaryColor, Color secondaryColor, float radius)
    {
        GameObject root = new GameObject("Bomb Explosion VFX");
        root.transform.position = position;

        BombExplosionVisual visual = root.AddComponent<BombExplosionVisual>();
        visual.Initialize(primaryColor, secondaryColor, radius);
    }

    private void Initialize(Color primaryColor, Color secondaryColor, float radius)
    {
        _duration = 0.32f;
        _coreColor = primaryColor;
        _ringColor = secondaryColor;

        _coreRenderer = CreateChildRenderer("Core", primaryColor, 18);
        _ringRenderer = CreateChildRenderer("Ring", secondaryColor, 19);

        float clampedRadius = Mathf.Max(0.5f, radius);
        _startCoreScale = Vector3.one * clampedRadius * 0.35f;
        _endCoreScale = Vector3.one * clampedRadius * 1.2f;
        _startRingScale = Vector3.one * clampedRadius * 0.45f;
        _endRingScale = Vector3.one * clampedRadius * 2.3f;

        _coreRenderer.transform.localScale = _startCoreScale;
        _ringRenderer.transform.localScale = _startRingScale;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        if (_coreRenderer != null)
        {
            _coreRenderer.transform.localScale = Vector3.Lerp(_startCoreScale, _endCoreScale, t);
            _coreRenderer.color = Color.Lerp(_coreColor, new Color(_coreColor.r, _coreColor.g, _coreColor.b, 0f), t);
        }

        if (_ringRenderer != null)
        {
            _ringRenderer.transform.localScale = Vector3.Lerp(_startRingScale, _endRingScale, t);
            _ringRenderer.color = Color.Lerp(_ringColor, new Color(_ringColor.r, _ringColor.g, _ringColor.b, 0f), t);
        }

        if (t >= 1f)
            Destroy(gameObject);
    }

    private SpriteRenderer CreateChildRenderer(string childName, Color color, int sortingOrder)
    {
        GameObject child = new GameObject(childName);
        child.transform.SetParent(transform, false);

        SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = PickupSpriteFactory.CircleSprite;
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = sortingOrder;
        renderer.color = color;

        return renderer;
    }
}
