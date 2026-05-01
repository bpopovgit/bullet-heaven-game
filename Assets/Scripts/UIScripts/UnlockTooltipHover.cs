using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnlockTooltipHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private const float HoverScale = 1.18f;
    private const float ScaleLerpSpeed = 14f;

    public string Title;
    public string Body;
    public Color Accent = Color.white;

    private Vector3 _baseScale = Vector3.one;
    private Vector3 _targetScale = Vector3.one;
    private bool _hasCachedBase;

    public void Configure(string title, string body, Color accent)
    {
        Title = title;
        Body = body;
        Accent = accent;
    }

    private void OnEnable()
    {
        if (!_hasCachedBase)
        {
            _baseScale = transform.localScale;
            _hasCachedBase = true;
        }
        _targetScale = _baseScale;
        transform.localScale = _baseScale;
    }

    private void Update()
    {
        if (!_hasCachedBase) return;
        Vector3 current = transform.localScale;
        if ((current - _targetScale).sqrMagnitude < 0.0001f)
        {
            transform.localScale = _targetScale;
            return;
        }
        // Use unscaled time so hover responds while the level-up popup pauses gameplay.
        transform.localScale = Vector3.Lerp(current, _targetScale, ScaleLerpSpeed * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetScale = _baseScale * HoverScale;
        if (UnlockTooltip.Instance != null)
            UnlockTooltip.Instance.Show(Title, Body, Accent);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetScale = _baseScale;
        if (UnlockTooltip.Instance != null)
            UnlockTooltip.Instance.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UnlockTooltip.Instance != null)
            UnlockTooltip.Instance.Hide();

        // The icon overlays the parent choice button; forward the click so selecting the
        // upgrade still works when the player clicks an icon instead of the card body.
        Button parentButton = GetComponentInParent<Button>();
        if (parentButton != null)
            parentButton.onClick.Invoke();
    }
}
