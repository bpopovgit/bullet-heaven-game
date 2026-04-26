using UnityEngine;

public class SecondarySkillVisual : MonoBehaviour
{
    private SpriteRenderer _innerRenderer;
    private SpriteRenderer _outerRenderer;
    private Transform _followTarget;
    private Vector3 _followOffset;
    private float _duration;
    private float _elapsed;
    private float _startScale;
    private float _endScale;

    public static void SpawnPulse(Vector3 position, Color innerColor, Color outerColor, float radius, float duration)
    {
        GameObject go = new GameObject("Secondary Skill Pulse");
        go.transform.position = position;

        SecondarySkillVisual visual = go.AddComponent<SecondarySkillVisual>();
        visual.Initialize(position, null, Vector3.zero, innerColor, outerColor, Mathf.Max(0.5f, radius * 0.35f), Mathf.Max(1.1f, radius * 1.55f), Mathf.Max(0.2f, duration));
    }

    public static void SpawnAura(Transform target, Color innerColor, Color outerColor, float radius, float duration)
    {
        if (target == null)
            return;

        GameObject go = new GameObject("Secondary Skill Aura");
        go.transform.position = target.position;

        SecondarySkillVisual visual = go.AddComponent<SecondarySkillVisual>();
        visual.Initialize(target.position, target, Vector3.zero, innerColor, outerColor, Mathf.Max(0.8f, radius * 0.8f), Mathf.Max(1f, radius), Mathf.Max(0.2f, duration));
    }

    private void Initialize(
        Vector3 position,
        Transform followTarget,
        Vector3 followOffset,
        Color innerColor,
        Color outerColor,
        float startScale,
        float endScale,
        float duration)
    {
        _followTarget = followTarget;
        _followOffset = followOffset;
        _duration = duration;
        _startScale = startScale;
        _endScale = endScale;

        GameObject outerObject = new GameObject("Outer");
        outerObject.transform.SetParent(transform, false);
        _outerRenderer = PickupSpriteFactory.AddDefaultRenderer(outerObject, outerColor, 18);
        outerObject.transform.localScale = Vector3.one * _startScale;

        GameObject innerObject = new GameObject("Inner");
        innerObject.transform.SetParent(transform, false);
        _innerRenderer = PickupSpriteFactory.AddDefaultRenderer(innerObject, innerColor, 19);
        innerObject.transform.localScale = Vector3.one * (_startScale * 0.55f);

        transform.position = position;
    }

    private void Update()
    {
        if (_followTarget != null)
            transform.position = _followTarget.position + _followOffset;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / Mathf.Max(0.01f, _duration));
        float currentScale = Mathf.Lerp(_startScale, _endScale, t);

        if (_outerRenderer != null)
        {
            _outerRenderer.transform.localScale = Vector3.one * currentScale;
            Color outerColor = _outerRenderer.color;
            outerColor.a = Mathf.Lerp(0.78f, 0f, t);
            _outerRenderer.color = outerColor;
        }

        if (_innerRenderer != null)
        {
            _innerRenderer.transform.localScale = Vector3.one * Mathf.Lerp(_startScale * 0.55f, _endScale * 0.72f, t);
            Color innerColor = _innerRenderer.color;
            innerColor.a = Mathf.Lerp(0.92f, 0f, t);
            _innerRenderer.color = innerColor;
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}
