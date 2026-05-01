using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UnlockTooltip : MonoBehaviour
{
    private static readonly Color FrameColor = new Color(0.04f, 0.03f, 0.04f, 0.97f);
    private static readonly Color OutlineColor = new Color(0.86f, 0.68f, 0.30f, 0.95f);
    private static readonly Color TitleColor = new Color(0.96f, 0.91f, 0.78f, 1f);
    private static readonly Color BodyColor = new Color(0.86f, 0.84f, 0.80f, 1f);
    private static readonly Vector2 PointerOffset = new Vector2(18f, -12f);

    public static UnlockTooltip Instance { get; private set; }

    private RectTransform _rect;
    private Image _frame;
    private Image _accent;
    private TextMeshProUGUI _titleText;
    private TextMeshProUGUI _bodyText;

    public static UnlockTooltip Ensure(Transform contextTransform)
    {
        if (Instance != null) return Instance;
        if (contextTransform == null) return null;

        // Tooltip lives on the ROOT canvas so its position math is in canvas-local
        // coordinates and it can render above any nested panels without clipping.
        Canvas canvas = contextTransform.GetComponentInParent<Canvas>();
        if (canvas == null) return null;
        Canvas rootCanvas = canvas.rootCanvas;
        if (rootCanvas == null) rootCanvas = canvas;

        GameObject host = new GameObject("UnlockTooltip");
        host.transform.SetParent(rootCanvas.transform, false);
        host.transform.SetAsLastSibling();

        UnlockTooltip tooltip = host.AddComponent<UnlockTooltip>();
        tooltip.Build();
        tooltip.gameObject.SetActive(false);
        Instance = tooltip;
        return tooltip;
    }

    private void Build()
    {
        _rect = gameObject.AddComponent<RectTransform>();
        // Anchor to canvas centre so anchoredPosition matches the centred-origin
        // coordinates returned by ScreenPointToLocalPointInRectangle. Pivot is top-left
        // so we can measure clip distances by subtracting size from y for the bottom.
        _rect.anchorMin = new Vector2(0.5f, 0.5f);
        _rect.anchorMax = new Vector2(0.5f, 0.5f);
        _rect.pivot = new Vector2(0f, 1f);
        _rect.sizeDelta = new Vector2(280f, 96f);

        _frame = gameObject.AddComponent<Image>();
        _frame.color = FrameColor;
        _frame.raycastTarget = false;

        Outline outline = gameObject.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(2f, -2f);

        // accent stripe on left edge
        GameObject accentGO = new GameObject("Accent");
        accentGO.transform.SetParent(transform, false);
        _accent = accentGO.AddComponent<Image>();
        _accent.color = OutlineColor;
        _accent.raycastTarget = false;
        RectTransform accentRect = _accent.rectTransform;
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(6f, 0f);

        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(transform, false);
        _titleText = titleGO.AddComponent<TextMeshProUGUI>();
        _titleText.fontSize = 18f;
        _titleText.fontStyle = FontStyles.Bold;
        _titleText.color = TitleColor;
        _titleText.alignment = TextAlignmentOptions.Left;
        _titleText.raycastTarget = false;
        _titleText.enableWordWrapping = false;
        _titleText.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -10f);
        titleRect.offsetMin = new Vector2(18f, 0f);
        titleRect.offsetMax = new Vector2(-12f, 0f);
        titleRect.sizeDelta = new Vector2(titleRect.sizeDelta.x, 24f);

        GameObject bodyGO = new GameObject("Body");
        bodyGO.transform.SetParent(transform, false);
        _bodyText = bodyGO.AddComponent<TextMeshProUGUI>();
        _bodyText.fontSize = 13f;
        _bodyText.fontStyle = FontStyles.Normal;
        _bodyText.color = BodyColor;
        _bodyText.alignment = TextAlignmentOptions.TopLeft;
        _bodyText.raycastTarget = false;
        _bodyText.enableWordWrapping = true;
        _bodyText.overflowMode = TextOverflowModes.Ellipsis;
        RectTransform bodyRect = bodyGO.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0f);
        bodyRect.anchorMax = new Vector2(1f, 1f);
        bodyRect.pivot = new Vector2(0.5f, 0.5f);
        bodyRect.offsetMin = new Vector2(18f, 12f);
        bodyRect.offsetMax = new Vector2(-12f, -38f);
    }

    public void Show(string title, string body, Color accent)
    {
        if (_titleText == null) return;
        _titleText.text = title ?? string.Empty;
        _bodyText.text = body ?? string.Empty;
        if (_accent != null)
            _accent.color = accent;
        gameObject.SetActive(true);
        FollowMouse();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        FollowMouse();
    }

    private void FollowMouse()
    {
        if (_rect == null) return;
        if (Mouse.current == null) return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        RectTransform canvasRect = canvas.transform as RectTransform;
        if (canvasRect == null) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 anchored;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out anchored);

        Vector2 size = _rect.sizeDelta;
        // Default position: tooltip's top-left (its pivot) sits to the lower-right of the cursor.
        Vector2 desired = anchored + PointerOffset;

        Vector2 canvasSize = canvasRect.rect.size;
        float halfW = canvasSize.x * 0.5f;
        float halfH = canvasSize.y * 0.5f;

        // Right-edge clamp — flip to LEFT of the cursor.
        if (desired.x + size.x > halfW)
            desired.x = anchored.x - Mathf.Abs(PointerOffset.x) - size.x;

        // Bottom-edge clamp — flip ABOVE the cursor (with cursor below tooltip's bottom edge).
        // Tooltip's bottom y = desired.y - size.y, since pivot is (0, 1).
        if (desired.y - size.y < -halfH)
            desired.y = anchored.y + size.y + Mathf.Abs(PointerOffset.y);

        // Top-edge clamp (cursor near top of screen, tooltip pushed down).
        if (desired.y > halfH)
            desired.y = halfH;

        // Left-edge clamp (rare — tooltip flipped left went off the left side).
        if (desired.x < -halfW)
            desired.x = -halfW;

        _rect.anchoredPosition = desired;
    }
}
