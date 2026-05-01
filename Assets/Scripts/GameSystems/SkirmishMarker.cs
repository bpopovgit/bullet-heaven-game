using System.Collections;
using UnityEngine;

public class SkirmishMarker : MonoBehaviour
{
    private const string SortingLayerName = "Actors";
    private const int OuterSortingOrder = -4;
    private const int RingSortingOrder = -3;

    private SpriteRenderer _outerGlow;
    private SpriteRenderer _ring;
    private Color _baseColor;
    private float _baseRadius;
    private bool _fading;

    public static SkirmishMarker Spawn(Vector2 anchor, FactionType sideA, FactionType sideB, float radius)
    {
        GameObject host = new GameObject("SkirmishMarker");
        host.transform.position = new Vector3(anchor.x, anchor.y, 0f);

        SkirmishMarker marker = host.AddComponent<SkirmishMarker>();
        marker.Build(sideA, sideB, radius);
        return marker;
    }

    private void Build(FactionType sideA, FactionType sideB, float radius)
    {
        _baseRadius = Mathf.Max(1.5f, radius);
        _baseColor = BlendFactionColors(sideA, sideB);

        _outerGlow = CreateLayer("OuterGlow", _baseRadius * 3.4f, OuterSortingOrder, 0.10f);
        _ring = CreateLayer("InnerRing", _baseRadius * 1.6f, RingSortingOrder, 0.32f);

        StartCoroutine(PulseRoutine());
    }

    public void FadeOut()
    {
        if (_fading) return;
        _fading = true;
        StartCoroutine(FadeRoutine());
    }

    private SpriteRenderer CreateLayer(string name, float radius, int sortingOrder, float alpha)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localScale = Vector3.one * (radius * 2f);

        SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
        sr.sprite = PickupSpriteFactory.CircleSprite;
        sr.sortingLayerName = SortingLayerName;
        sr.sortingOrder = sortingOrder;
        Color c = _baseColor;
        c.a = alpha;
        sr.color = c;
        return sr;
    }

    private IEnumerator PulseRoutine()
    {
        float elapsed = 0f;
        const float spawnPulseDuration = 1.6f;

        while (elapsed < spawnPulseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spawnPulseDuration);
            float kick = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
            ApplyPulse(1f + kick * 0.45f, 0.6f + kick * 0.4f, 1f + kick * 0.25f, 0.55f + kick * 0.4f);
            yield return null;
        }

        ApplyPulse(1f, 1f, 1f, 1f);

        while (!_fading)
        {
            float pulse = (Mathf.Sin(Time.time * 2.4f) + 1f) * 0.5f;
            ApplyPulse(1f + pulse * 0.04f, 0.85f + pulse * 0.30f, 1f + pulse * 0.03f, 0.85f + pulse * 0.30f);
            yield return null;
        }
    }

    private IEnumerator FadeRoutine()
    {
        float duration = 0.7f;
        float startTime = Time.time;
        float startOuterScale = _outerGlow != null ? _outerGlow.transform.localScale.x : 1f;
        float startRingScale = _ring != null ? _ring.transform.localScale.x : 1f;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float scaleBoost = 1f + t * 0.35f;
            float fade = 1f - t;
            ApplyFade(startOuterScale * scaleBoost, fade, startRingScale * scaleBoost, fade);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void ApplyPulse(float outerScaleMul, float outerAlphaMul, float ringScaleMul, float ringAlphaMul)
    {
        if (_outerGlow != null)
        {
            _outerGlow.transform.localScale = Vector3.one * (_baseRadius * 3.4f * 2f * outerScaleMul);
            Color c = _outerGlow.color; c.a = 0.10f * outerAlphaMul; _outerGlow.color = c;
        }
        if (_ring != null)
        {
            _ring.transform.localScale = Vector3.one * (_baseRadius * 1.6f * 2f * ringScaleMul);
            Color c = _ring.color; c.a = 0.32f * ringAlphaMul; _ring.color = c;
        }
    }

    private void ApplyFade(float outerScale, float outerAlpha, float ringScale, float ringAlpha)
    {
        if (_outerGlow != null)
        {
            _outerGlow.transform.localScale = Vector3.one * outerScale;
            Color c = _outerGlow.color; c.a = 0.10f * outerAlpha; _outerGlow.color = c;
        }
        if (_ring != null)
        {
            _ring.transform.localScale = Vector3.one * ringScale;
            Color c = _ring.color; c.a = 0.32f * ringAlpha; _ring.color = c;
        }
    }

    private static Color BlendFactionColors(FactionType a, FactionType b)
    {
        Color ca = GetFactionColor(a);
        Color cb = GetFactionColor(b);
        return new Color((ca.r + cb.r) * 0.5f, (ca.g + cb.g) * 0.5f, (ca.b + cb.b) * 0.5f, 1f);
    }

    public static Color GetFactionColor(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Human:  return new Color(0.10f, 0.72f, 1.00f, 1f);
            case FactionType.Angel:  return new Color(1.00f, 0.92f, 0.35f, 1f);
            case FactionType.Demon:  return new Color(0.92f, 0.10f, 0.12f, 1f);
            case FactionType.Zombie: return new Color(0.42f, 0.95f, 0.24f, 1f);
            default:                 return new Color(0.78f, 0.78f, 0.78f, 1f);
        }
    }
}
