using UnityEngine;

public class WhirlwindVisual : MonoBehaviour
{
    private Transform _follow;
    private float _duration;
    private float _elapsed;
    private float _radius;
    private SpriteRenderer[] _bladeRenderers;
    private Transform _spinRoot;

    public static WhirlwindVisual Spawn(Transform follow, float radius, float duration)
    {
        GameObject go = new GameObject("VanguardWhirlwindVisual");
        WhirlwindVisual visual = go.AddComponent<WhirlwindVisual>();
        visual.Initialize(follow, radius, duration);
        return visual;
    }

    private void Initialize(Transform follow, float radius, float duration)
    {
        _follow = follow;
        _radius = radius;
        _duration = duration;

        if (follow != null)
            transform.position = follow.position;

        GameObject spinObject = new GameObject("Spin");
        _spinRoot = spinObject.transform;
        _spinRoot.SetParent(transform, false);

        _bladeRenderers = new SpriteRenderer[3];
        for (int i = 0; i < _bladeRenderers.Length; i++)
        {
            GameObject blade = new GameObject($"Blade{i}");
            blade.transform.SetParent(_spinRoot, false);
            blade.transform.localPosition = Vector3.zero;
            blade.transform.localRotation = Quaternion.Euler(0f, 0f, i * (360f / _bladeRenderers.Length));

            SpriteRenderer renderer = blade.AddComponent<SpriteRenderer>();
            renderer.sprite = BuildBladeSprite();
            renderer.color = new Color(0.78f, 0.92f, 1f, 0.85f);
            renderer.sortingLayerName = "Actors";
            renderer.sortingOrder = 13;
            blade.transform.localScale = new Vector3(_radius * 1.05f, 0.32f, 1f);
            _bladeRenderers[i] = renderer;
        }
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        if (_follow != null)
            transform.position = _follow.position;

        if (_spinRoot != null)
            _spinRoot.Rotate(0f, 0f, 1080f * Time.deltaTime, Space.Self);

        float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, _duration));
        float fade = t < 0.85f ? Mathf.Lerp(0f, 0.85f, Mathf.Clamp01(t / 0.15f))
                               : Mathf.Lerp(0.85f, 0f, (t - 0.85f) / 0.15f);

        if (_bladeRenderers != null)
        {
            for (int i = 0; i < _bladeRenderers.Length; i++)
            {
                if (_bladeRenderers[i] == null) continue;
                Color c = _bladeRenderers[i].color;
                c.a = fade;
                _bladeRenderers[i].color = c;
            }
        }

        if (t >= 1f)
            Destroy(gameObject);
    }

    private static Sprite _bladeSprite;

    private static Sprite BuildBladeSprite()
    {
        if (_bladeSprite != null)
            return _bladeSprite;

        const int width = 64;
        const int height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] pixels = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            float lengthFalloff = Mathf.SmoothStep(0f, 1f, x / (float)(width - 1));
            for (int y = 0; y < height; y++)
            {
                float dy = (y - (height - 1) * 0.5f) / ((height - 1) * 0.5f);
                float widthFalloff = Mathf.Clamp01(1f - Mathf.Abs(dy));
                widthFalloff = Mathf.SmoothStep(0f, 1f, widthFalloff);
                pixels[y * width + x] = new Color(1f, 1f, 1f, lengthFalloff * widthFalloff);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply(false, true);

        _bladeSprite = Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(0f, 0.5f), 64f);
        return _bladeSprite;
    }
}
