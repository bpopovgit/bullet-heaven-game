using UnityEngine;

public class ShatterPrimeGlow : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private float _expiresAt;
    private float _pulsePhase;
    private static Sprite _ringSprite;

    public void Refresh(float expiresAt)
    {
        _expiresAt = expiresAt;

        if (_renderer == null)
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = GetRingSprite();
            _renderer.color = new Color(0.62f, 0.92f, 1f, 0.85f);
            _renderer.sortingLayerName = "Actors";
            _renderer.sortingOrder = 11;
            transform.localScale = Vector3.one * 1.0f;
        }
    }

    private void Update()
    {
        if (Time.time >= _expiresAt)
        {
            Destroy(gameObject);
            return;
        }

        _pulsePhase += Time.deltaTime * 6f;
        float pulse = 0.85f + Mathf.Sin(_pulsePhase) * 0.15f;
        transform.localScale = Vector3.one * pulse;

        if (_renderer != null)
        {
            Color c = _renderer.color;
            c.a = 0.55f + Mathf.Sin(_pulsePhase * 1.2f) * 0.25f;
            _renderer.color = c;
        }
    }

    private static Sprite GetRingSprite()
    {
        if (_ringSprite != null)
            return _ringSprite;

        const int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float outerRadius = size * 0.5f;
        float innerRadius = size * 0.36f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 0f;
                if (distance <= outerRadius && distance >= innerRadius)
                {
                    float t = (distance - innerRadius) / (outerRadius - innerRadius);
                    alpha = Mathf.SmoothStep(1f, 0f, t);
                }
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply(false, true);

        _ringSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        return _ringSprite;
    }
}

public class ShatterBurstVisual : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private float _duration = 0.32f;
    private float _elapsed;

    public static void Spawn(Vector3 position)
    {
        GameObject go = new GameObject("ShatterBurst");
        go.transform.position = position;
        go.AddComponent<ShatterBurstVisual>();
    }

    private void Awake()
    {
        _renderer = gameObject.AddComponent<SpriteRenderer>();
        _renderer.sprite = ShatterPrimeGlow_GetRingSprite();
        _renderer.color = new Color(0.85f, 0.97f, 1f, 0.95f);
        _renderer.sortingLayerName = "Actors";
        _renderer.sortingOrder = 14;
        transform.localScale = Vector3.one * 0.6f;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);

        transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 1.6f, t);

        Color c = _renderer.color;
        c.a = Mathf.Lerp(0.95f, 0f, t);
        _renderer.color = c;

        if (t >= 1f)
            Destroy(gameObject);
    }

    private static Sprite ShatterPrimeGlow_GetRingSprite()
    {
        return ShatterPrimeGlowSpriteAccess.GetSprite();
    }
}

internal static class ShatterPrimeGlowSpriteAccess
{
    private static Sprite _cachedSprite;

    public static Sprite GetSprite()
    {
        if (_cachedSprite != null)
            return _cachedSprite;

        const int size = 48;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.5f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - d);
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply(false, true);

        _cachedSprite = Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
        return _cachedSprite;
    }
}
