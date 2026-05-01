using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SkirmishCompassIndicator : MonoBehaviour
{
    private const string GameSceneName = "Game";
    private const float EdgeMargin = 86f;
    private const float OnScreenInset = 60f;
    private const float SpawnFadeDuration = 0.6f;

    private sealed class IndicatorView
    {
        public GameObject Root;
        public RectTransform Rect;
        public Image Background;
        public TextMeshProUGUI ArrowText;
        public TextMeshProUGUI DistanceText;
        public CanvasGroup Group;
        public Color SideColor;
        public float SpawnedAt;
    }

    private Camera _camera;
    private Canvas _canvas;
    private RectTransform _canvasRect;
    private readonly Dictionary<string, IndicatorView> _views = new Dictionary<string, IndicatorView>();
    private readonly List<string> _staleIds = new List<string>();

    private static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_sceneHookRegistered)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        _sceneHookRegistered = true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid() || scene.name != GameSceneName)
            return;

        if (FindObjectOfType<SkirmishCompassIndicator>() != null)
            return;

        GameObject host = new GameObject("SkirmishCompassIndicator");
        host.AddComponent<SkirmishCompassIndicator>();
    }

    private void Awake()
    {
        EnsureCanvas();
        _camera = Camera.main;
    }

    private void EnsureCanvas()
    {
        GameObject canvasGO = new GameObject("SkirmishCompassCanvas");
        canvasGO.transform.SetParent(transform, false);

        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        _canvasRect = canvasGO.GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (SkirmishDirector.Instance == null || _canvasRect == null)
            return;

        if (_camera == null)
            _camera = Camera.main;
        if (_camera == null)
            return;

        Transform player = GameObject.FindWithTag("Player")?.transform;

        IReadOnlyCollection<FactionSkirmish> skirmishes = SkirmishDirector.Instance.Skirmishes;

        _staleIds.Clear();
        foreach (string id in _views.Keys)
            _staleIds.Add(id);

        foreach (FactionSkirmish skirmish in skirmishes)
        {
            if (skirmish.State != SkirmishState.Active)
                continue;

            _staleIds.Remove(skirmish.Id);
            UpdateOrCreateView(skirmish, player);
        }

        for (int i = 0; i < _staleIds.Count; i++)
        {
            if (_views.TryGetValue(_staleIds[i], out IndicatorView v) && v.Root != null)
                Destroy(v.Root);
            _views.Remove(_staleIds[i]);
        }
    }

    private void UpdateOrCreateView(FactionSkirmish skirmish, Transform player)
    {
        if (!_views.TryGetValue(skirmish.Id, out IndicatorView view))
        {
            view = CreateView(skirmish);
            _views[skirmish.Id] = view;
        }

        Vector3 anchorWorld = new Vector3(skirmish.Anchor.x, skirmish.Anchor.y, 0f);
        Vector3 screenPos = _camera.WorldToScreenPoint(anchorWorld);
        bool behind = screenPos.z < 0f;

        Vector2 canvasSize = _canvasRect.rect.size;
        float refW = Screen.width;
        float refH = Screen.height;

        Vector2 normalized = new Vector2(screenPos.x / refW, screenPos.y / refH);
        Vector2 canvasPos = new Vector2(
            (normalized.x - 0.5f) * canvasSize.x,
            (normalized.y - 0.5f) * canvasSize.y);

        bool onScreen = !behind
            && screenPos.x > OnScreenInset && screenPos.x < refW - OnScreenInset
            && screenPos.y > OnScreenInset && screenPos.y < refH - OnScreenInset;

        if (onScreen)
        {
            view.Group.alpha = 0f;
        }
        else
        {
            float halfW = canvasSize.x * 0.5f - EdgeMargin;
            float halfH = canvasSize.y * 0.5f - EdgeMargin;
            Vector2 dir = behind ? -canvasPos : canvasPos;
            if (dir.sqrMagnitude < 0.001f) dir = Vector2.up;

            float scale = Mathf.Min(halfW / Mathf.Max(0.001f, Mathf.Abs(dir.x)), halfH / Mathf.Max(0.001f, Mathf.Abs(dir.y)));
            Vector2 clamped = dir.normalized * (dir.magnitude * scale);
            clamped.x = Mathf.Clamp(clamped.x, -halfW, halfW);
            clamped.y = Mathf.Clamp(clamped.y, -halfH, halfH);

            view.Rect.anchoredPosition = clamped;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            view.ArrowText.rectTransform.localEulerAngles = new Vector3(0f, 0f, angle);

            float age = Time.time - view.SpawnedAt;
            float spawnFade = Mathf.Clamp01(age / SpawnFadeDuration);
            view.Group.alpha = spawnFade;
        }

        if (player != null)
        {
            float distance = Vector2.Distance(player.position, skirmish.Anchor);
            view.DistanceText.text = $"{Mathf.RoundToInt(distance)}m";
        }
        else
        {
            view.DistanceText.text = "—";
        }
    }

    private IndicatorView CreateView(FactionSkirmish skirmish)
    {
        Color sideColor = SkirmishMarker.GetFactionColor(skirmish.SideAFaction);
        Color blendColor = new Color(
            (SkirmishMarker.GetFactionColor(skirmish.SideAFaction).r + SkirmishMarker.GetFactionColor(skirmish.SideBFaction).r) * 0.5f,
            (SkirmishMarker.GetFactionColor(skirmish.SideAFaction).g + SkirmishMarker.GetFactionColor(skirmish.SideBFaction).g) * 0.5f,
            (SkirmishMarker.GetFactionColor(skirmish.SideAFaction).b + SkirmishMarker.GetFactionColor(skirmish.SideBFaction).b) * 0.5f,
            1f);

        GameObject root = new GameObject($"SkirmishIndicator_{skirmish.Id}");
        root.transform.SetParent(_canvasRect, false);

        RectTransform rect = root.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(96f, 96f);

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(root.transform, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.04f, 0.06f, 0.78f);
        bgImage.raycastTarget = false;
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.sizeDelta = new Vector2(78f, 78f);

        Outline outline = bg.AddComponent<Outline>();
        outline.effectColor = new Color(blendColor.r, blendColor.g, blendColor.b, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(root.transform, false);
        TextMeshProUGUI arrowText = arrow.AddComponent<TextMeshProUGUI>();
        arrowText.text = "▲";
        arrowText.fontSize = 44f;
        arrowText.fontStyle = FontStyles.Bold;
        arrowText.alignment = TextAlignmentOptions.Center;
        arrowText.color = blendColor;
        arrowText.raycastTarget = false;
        RectTransform arrowRect = arrow.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(0.5f, 0.5f);
        arrowRect.anchorMax = new Vector2(0.5f, 0.5f);
        arrowRect.pivot = new Vector2(0.5f, 0.5f);
        arrowRect.anchoredPosition = new Vector2(0f, 8f);
        arrowRect.sizeDelta = new Vector2(60f, 60f);

        GameObject dist = new GameObject("Distance");
        dist.transform.SetParent(root.transform, false);
        TextMeshProUGUI distText = dist.AddComponent<TextMeshProUGUI>();
        distText.text = "0m";
        distText.fontSize = 18f;
        distText.fontStyle = FontStyles.Bold;
        distText.alignment = TextAlignmentOptions.Center;
        distText.color = new Color(0.96f, 0.91f, 0.78f, 1f);
        distText.raycastTarget = false;
        RectTransform distRect = dist.GetComponent<RectTransform>();
        distRect.anchorMin = new Vector2(0.5f, 0.5f);
        distRect.anchorMax = new Vector2(0.5f, 0.5f);
        distRect.pivot = new Vector2(0.5f, 0.5f);
        distRect.anchoredPosition = new Vector2(0f, -22f);
        distRect.sizeDelta = new Vector2(80f, 22f);

        return new IndicatorView
        {
            Root = root,
            Rect = rect,
            Background = bgImage,
            ArrowText = arrowText,
            DistanceText = distText,
            Group = group,
            SideColor = blendColor,
            SpawnedAt = Time.time
        };
    }
}
