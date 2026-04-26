using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuRuntime : MonoBehaviour
{
    private const string MenuSceneName = "Main";
    private const string GameplaySceneName = "Game";

    private static readonly Color BackgroundColor = new Color(0.05f, 0.06f, 0.11f, 1f);
    private static readonly Color PanelColor = new Color(0.09f, 0.12f, 0.19f, 0.94f);
    private static readonly Color AccentColor = new Color(0.04f, 0.72f, 0.42f, 1f);
    private static readonly Color AccentHighlightColor = new Color(0.1f, 0.85f, 0.52f, 1f);
    private static readonly Color AccentPressedColor = new Color(0.02f, 0.56f, 0.32f, 1f);
    private static readonly Color SecondaryButtonColor = new Color(0.12f, 0.42f, 0.85f, 1f);
    private static readonly Color PlaceholderColor = new Color(0.36f, 0.18f, 0.74f, 1f);
    private static readonly Color OutlineColor = new Color(0.6f, 0.84f, 1f, 0.18f);

    private bool _isLoading;
    private RectTransform _root;
    private RectTransform _modeSelectionPanel;
    private RectTransform _singlePlayerPanel;
    private RectTransform _multiplayerPanel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != MenuSceneName)
            return;

        if (FindObjectOfType<MainMenuRuntime>() != null)
            return;

        GameObject root = new GameObject("MainMenuRuntime");
        root.AddComponent<MainMenuRuntime>();
    }

    private void Awake()
    {
        BuildMenuIfNeeded();
        ShowModeSelection();
    }

    private void BuildMenuIfNeeded()
    {
        EnsureEventSystem();

        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null && existingCanvas.transform.childCount > 0)
            return;

        Canvas canvas = CreateCanvas();
        _root = canvas.GetComponent<RectTransform>();

        CreateBackground(_root);
        CreateTitle(_root);
        CreateSubtitle(_root);
        CreateFooter(_root);

        _modeSelectionPanel = CreatePanel("ModeSelectionPanel", _root, new Vector2(0f, -10f), new Vector2(760f, 340f));
        BuildModeSelectionPanel(_modeSelectionPanel);

        _singlePlayerPanel = CreatePanel("SinglePlayerPanel", _root, new Vector2(0f, -10f), new Vector2(760f, 430f));
        BuildSinglePlayerPanel(_singlePlayerPanel);

        _multiplayerPanel = CreatePanel("MultiplayerPanel", _root, new Vector2(0f, -10f), new Vector2(760f, 360f));
        BuildMultiplayerPanel(_multiplayerPanel);
    }

    private static void EnsureEventSystem()
    {
        EventSystem existing = FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule legacyModule = existing.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                    Destroy(legacyModule);

                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("MainMenuCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(parent, false);

        Image image = backgroundObject.AddComponent<Image>();
        image.color = BackgroundColor;

        RectTransform rect = backgroundObject.GetComponent<RectTransform>();
        Stretch(rect, 0f);
    }

    private static void CreateTitle(Transform parent)
    {
        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = titleObject.AddComponent<TextMeshProUGUI>();
        text.text = "Spas and Bobkata's Amazing Game";
        text.fontSize = 38f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.96f, 0.82f, 1f);

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 250f);
        rect.sizeDelta = new Vector2(980f, 64f);
    }

    private static void CreateSubtitle(Transform parent)
    {
        GameObject subtitleObject = new GameObject("Subtitle");
        subtitleObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = subtitleObject.AddComponent<TextMeshProUGUI>();
        text.text = "Survive the swarm. Defeat the dragon. Build-breaking loadouts are on the way.";
        text.fontSize = 16f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = new Color(0.76f, 0.93f, 1f, 1f);

        RectTransform rect = subtitleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 195f);
        rect.sizeDelta = new Vector2(860f, 58f);
    }

    private static void CreateFooter(Transform parent)
    {
        GameObject footerObject = new GameObject("Footer");
        footerObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = footerObject.AddComponent<TextMeshProUGUI>();
        text.text = "Dragon trouble above. Bigger loadouts below. More systems are on deck.";
        text.fontSize = 12f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.71f, 0.85f, 1f, 0.92f);

        RectTransform rect = footerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 26f);
        rect.sizeDelta = new Vector2(780f, 28f);
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);

        Image image = panelObject.AddComponent<Image>();
        image.color = PanelColor;

        Outline outline = panelObject.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private void BuildModeSelectionPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Choose Mode", new Vector2(0f, 128f));
        CreatePanelBody(panel, "Pick how you want to enter the game. Single Player is ready now, and Multiplayer has a place waiting for it.", new Vector2(0f, 85f), 620f);

        CreateButton(panel, "Single Player", new Vector2(0f, 20f), new Vector2(320f, 58f), AccentColor, true, ShowSinglePlayerSetup, string.Empty);
        CreateButton(panel, "Multiplayer", new Vector2(0f, -56f), new Vector2(320f, 54f), PlaceholderColor, true, ShowMultiplayerSetup, "Soon");
        CreateButton(panel, "Settings", new Vector2(-200f, -145f), new Vector2(180f, 46f), new Color(0.82f, 0.28f, 0.58f, 1f), false, null, string.Empty);
        CreateButton(panel, "Sound", new Vector2(0f, -145f), new Vector2(180f, 46f), new Color(0.91f, 0.46f, 0.18f, 1f), false, null, string.Empty);
        CreateButton(panel, "Profile", new Vector2(200f, -145f), new Vector2(180f, 46f), new Color(0.25f, 0.54f, 0.92f, 1f), false, null, string.Empty);
        CreateButton(panel, "Quit", new Vector2(0f, -220f), new Vector2(220f, 52f), SecondaryButtonColor, true, QuitGame, string.Empty);
        CreateHintLabel(panel, "Loadout makes the most sense after mode selection, so each mode can have its own rules later.", new Vector2(0f, -275f));
    }

    private void BuildSinglePlayerPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Single Player Setup", new Vector2(0f, 168f));
        CreatePanelBody(panel, "This is where run-specific setup should live. For now, you can either jump into the game or see the next systems taking shape.", new Vector2(0f, 122f), 640f);

        CreateSectionLabel(panel, "Run Setup", new Vector2(0f, 55f));
        CreateButton(panel, "Loadout", new Vector2(-190f, 0f), new Vector2(200f, 50f), new Color(0.78f, 0.24f, 0.66f, 1f), false, null, "Phase B");
        CreateButton(panel, "Map Select", new Vector2(0f, 0f), new Vector2(200f, 50f), new Color(0.95f, 0.54f, 0.16f, 1f), false, null, "Soon");
        CreateButton(panel, "Difficulty", new Vector2(190f, 0f), new Vector2(200f, 50f), new Color(0.24f, 0.56f, 0.92f, 1f), false, null, "Soon");

        CreateButton(panel, "Start Run", new Vector2(0f, -110f), new Vector2(280f, 58f), AccentColor, true, LoadGameplayScene, string.Empty);
        CreateButton(panel, "Back", new Vector2(0f, -192f), new Vector2(220f, 52f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "My vote: Single Player and Multiplayer should each lead to their own setup flow, with loadout options shared where it makes sense.", new Vector2(0f, -260f));
    }

    private void BuildMultiplayerPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Multiplayer", new Vector2(0f, 132f));
        CreatePanelBody(panel, "I agree with your instinct here too: Multiplayer should not inherit Single Player blindly. It deserves its own setup flow, even if some loadout rules are shared underneath.", new Vector2(0f, 82f), 640f);

        CreateButton(panel, "Party Setup", new Vector2(-140f, -10f), new Vector2(220f, 50f), new Color(0.82f, 0.28f, 0.58f, 1f), false, null, "Soon");
        CreateButton(panel, "Shared Loadout", new Vector2(140f, -10f), new Vector2(220f, 50f), new Color(0.28f, 0.62f, 0.96f, 1f), false, null, "Later");
        CreateButton(panel, "Back", new Vector2(0f, -110f), new Vector2(220f, 52f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "For now, Multiplayer is a branch placeholder. Later it can decide whether loadouts are personal, shared, or host-selected.", new Vector2(0f, -185f));
    }

    private static void CreatePanelTitle(Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject labelObject = new GameObject($"{label}Title");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 24f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.97f, 0.86f, 1f);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(540f, 34f);
    }

    private static void CreatePanelBody(Transform parent, string body, Vector2 anchoredPosition, float width)
    {
        GameObject bodyObject = new GameObject("Body");
        bodyObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = bodyObject.AddComponent<TextMeshProUGUI>();
        text.text = body;
        text.fontSize = 15f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = new Color(0.78f, 0.93f, 1f, 1f);

        RectTransform rect = bodyObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(width, 56f);
    }

    private static void CreateSectionLabel(Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject labelObject = new GameObject($"{label}Label");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.83f, 0.28f, 1f);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(320f, 30f);
    }

    private static void CreateHintLabel(Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject labelObject = new GameObject("Hint");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 13f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = new Color(1f, 0.86f, 0.35f, 1f);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(640f, 44f);
    }

    private static void CreateButton(
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        Vector2 size,
        Color backgroundColor,
        bool interactable,
        UnityEngine.Events.UnityAction onClick,
        string badge)
    {
        GameObject buttonObject = new GameObject($"{label}Button");
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = backgroundColor;

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = interactable ? AccentHighlightColor : image.color;
        colors.pressedColor = interactable ? AccentPressedColor : image.color;
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = backgroundColor;
        button.colors = colors;
        button.interactable = interactable;

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(1f, -1f);

        if (interactable && onClick != null)
        {
            button.onClick.AddListener(() =>
            {
                GameAudio.PlayUISelect();
                onClick.Invoke();
            });
        }

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = size.y >= 56f ? 24f : 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = interactable ? Color.white : new Color(0.97f, 0.97f, 0.99f, 1f);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        Stretch(labelRect, 0f);

        if (!string.IsNullOrWhiteSpace(badge))
            CreateBadge(buttonObject.transform, badge);
    }

    private static void CreateBadge(Transform parent, string label)
    {
        GameObject badgeObject = new GameObject("Badge");
        badgeObject.transform.SetParent(parent, false);

        Image image = badgeObject.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.18f);

        RectTransform rect = badgeObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-8f, -8f);
        rect.sizeDelta = new Vector2(76f, 20f);

        GameObject textObject = new GameObject("BadgeLabel");
        textObject.transform.SetParent(badgeObject.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 10f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.92f, 0.55f, 1f);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        Stretch(textRect, 0f);
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private void ShowModeSelection()
    {
        SetPanelState(modeSelection: true, singlePlayer: false, multiplayer: false);
    }

    private void ShowSinglePlayerSetup()
    {
        SetPanelState(modeSelection: false, singlePlayer: true, multiplayer: false);
    }

    private void ShowMultiplayerSetup()
    {
        SetPanelState(modeSelection: false, singlePlayer: false, multiplayer: true);
    }

    private void SetPanelState(bool modeSelection, bool singlePlayer, bool multiplayer)
    {
        if (_modeSelectionPanel != null)
            _modeSelectionPanel.gameObject.SetActive(modeSelection);

        if (_singlePlayerPanel != null)
            _singlePlayerPanel.gameObject.SetActive(singlePlayer);

        if (_multiplayerPanel != null)
            _multiplayerPanel.gameObject.SetActive(multiplayer);
    }

    private void LoadGameplayScene()
    {
        if (_isLoading)
            return;

        _isLoading = true;
        SceneManager.LoadScene(GameplaySceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
