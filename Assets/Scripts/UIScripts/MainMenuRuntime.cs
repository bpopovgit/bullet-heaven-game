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

    private static readonly Color BackgroundColor = new Color(0.04f, 0.05f, 0.08f, 1f);
    private static readonly Color AccentColor = new Color(0.24f, 0.56f, 0.21f, 1f);
    private static readonly Color AccentHighlightColor = new Color(0.31f, 0.68f, 0.28f, 1f);
    private static readonly Color AccentPressedColor = new Color(0.18f, 0.42f, 0.16f, 1f);
    private static readonly Color SecondaryButtonColor = new Color(0.73f, 0.36f, 0.14f, 1f);
    private static readonly Color UtilityButtonColor = new Color(0.18f, 0.34f, 0.18f, 1f);
    private static readonly Color PlaceholderArcane = new Color(0.22f, 0.42f, 0.24f, 1f);
    private static readonly Color PlaceholderWar = new Color(0.65f, 0.31f, 0.12f, 1f);
    private static readonly Color PlaceholderRanger = new Color(0.17f, 0.29f, 0.2f, 1f);
    private static readonly Color OutlineColor = new Color(0.92f, 0.71f, 0.34f, 0.24f);
    private static readonly Color TitleColor = new Color(0.96f, 0.9f, 0.74f, 1f);
    private static readonly Color BodyColor = new Color(0.83f, 0.88f, 0.82f, 1f);
    private static readonly Color HintColor = new Color(0.93f, 0.79f, 0.35f, 1f);

    private bool _isLoading;
    private RectTransform _root;
    private RectTransform _modeSelectionPanel;
    private RectTransform _singlePlayerPanel;
    private RectTransform _multiplayerPanel;
    private RectTransform _loadoutPanel;

    private TextMeshProUGUI _singlePlayerLoadoutSummaryText;
    private TextMeshProUGUI _loadoutHeaderSummaryText;
    private TextMeshProUGUI _weaponChoiceText;
    private TextMeshProUGUI _weaponDescriptionText;
    private TextMeshProUGUI _bombChoiceText;
    private TextMeshProUGUI _bombDescriptionText;
    private TextMeshProUGUI _skillChoiceText;
    private TextMeshProUGUI _skillDescriptionText;
    private TextMeshProUGUI _passiveChoiceText;
    private TextMeshProUGUI _passiveDescriptionText;

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
        RefreshLoadoutTexts();
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

        _modeSelectionPanel = CreatePanel("ModeSelectionPanel", _root, new Vector2(0f, -12f), new Vector2(860f, 500f));
        BuildModeSelectionPanel(_modeSelectionPanel);

        _singlePlayerPanel = CreatePanel("SinglePlayerPanel", _root, new Vector2(0f, -16f), new Vector2(900f, 540f));
        BuildSinglePlayerPanel(_singlePlayerPanel);

        _multiplayerPanel = CreatePanel("MultiplayerPanel", _root, new Vector2(0f, -18f), new Vector2(860f, 430f));
        BuildMultiplayerPanel(_multiplayerPanel);

        _loadoutPanel = CreatePanel("LoadoutPanel", _root, new Vector2(0f, -24f), new Vector2(980f, 560f));
        BuildLoadoutPanel(_loadoutPanel);
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
        text.fontSize = 42f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TitleColor;

        RectTransform rect = titleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 310f);
        rect.sizeDelta = new Vector2(1200f, 72f);
    }

    private static void CreateSubtitle(Transform parent)
    {
        GameObject subtitleObject = new GameObject("Subtitle");
        subtitleObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = subtitleObject.AddComponent<TextMeshProUGUI>();
        text.text = "Survive the swarm. Defeat the dragon. Build-breaking loadouts are on the way.";
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = BodyColor;

        RectTransform rect = subtitleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 248f);
        rect.sizeDelta = new Vector2(900f, 62f);
    }

    private static void CreateFooter(Transform parent)
    {
        GameObject footerObject = new GameObject("Footer");
        footerObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = footerObject.AddComponent<TextMeshProUGUI>();
        text.text = "Dragon trouble above. Bigger loadouts below. More systems are on deck.";
        text.fontSize = 12f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.72f, 0.68f, 0.55f, 0.92f);

        RectTransform rect = footerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 26f);
        rect.sizeDelta = new Vector2(780f, 28f);
    }

    private static RectTransform CreatePanel(string name, Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject panelObject = new GameObject(name, typeof(RectTransform));
        panelObject.transform.SetParent(parent, false);

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
        CreatePanelTitle(panel, "Choose Mode", new Vector2(0f, 170f));
        CreatePanelBody(panel, "Step into a solo run now, or leave room for party adventures later. Clean, readable choices first. Extra systems can grow around them.", new Vector2(0f, 130f), 700f);
        CreateDivider(panel, new Vector2(0f, 88f), 640f);

        CreateButton(panel, "Single Player", new Vector2(0f, 26f), new Vector2(420f, 64f), AccentColor, true, ShowSinglePlayerSetup, string.Empty);
        CreateButton(panel, "Multiplayer", new Vector2(0f, -56f), new Vector2(420f, 56f), PlaceholderArcane, true, ShowMultiplayerSetup, "Soon");

        CreateSectionLabel(panel, "Adventurer's Desk", new Vector2(0f, -120f));
        CreateButton(panel, "Settings", new Vector2(-220f, -170f), new Vector2(180f, 46f), UtilityButtonColor, false, null, string.Empty);
        CreateButton(panel, "Sound", new Vector2(0f, -170f), new Vector2(180f, 46f), PlaceholderWar, false, null, string.Empty);
        CreateButton(panel, "Profile", new Vector2(220f, -170f), new Vector2(180f, 46f), PlaceholderRanger, false, null, string.Empty);
        CreateButton(panel, "Quit", new Vector2(0f, -242f), new Vector2(220f, 48f), SecondaryButtonColor, true, QuitGame, string.Empty);

        CreateHintLabel(panel, "Loadout comes after mode selection, so Single Player and Multiplayer can each grow into their own flavor of adventure.", new Vector2(0f, -300f), 720f);
    }

    private void BuildSinglePlayerPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Single Player Setup", new Vector2(0f, 176f));
        CreatePanelBody(panel, "Shape the run before the first enemy shows up. Your opening kit, route, and pace should feel chosen rather than accidental.", new Vector2(0f, 136f), 720f);
        CreateDivider(panel, new Vector2(0f, 94f), 680f);

        CreateSectionLabel(panel, "Run Setup", new Vector2(0f, 48f));
        CreateButton(panel, "Loadout", new Vector2(-240f, -6f), new Vector2(220f, 52f), PlaceholderArcane, true, ShowLoadoutSetup, string.Empty);
        CreateButton(panel, "Map Select", new Vector2(0f, -6f), new Vector2(220f, 52f), UtilityButtonColor, false, null, "Soon");
        CreateButton(panel, "Difficulty", new Vector2(240f, -6f), new Vector2(220f, 52f), PlaceholderWar, false, null, "Soon");

        _singlePlayerLoadoutSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, -72f), 720f);
        CreateButton(panel, "Start Run", new Vector2(0f, -150f), new Vector2(320f, 60f), AccentColor, true, LoadGameplayScene, string.Empty);
        CreateButton(panel, "Back", new Vector2(0f, -226f), new Vector2(220f, 48f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "Your selected loadout carries straight into the run. Press Q in combat to unleash the starting bomb you've prepared here.", new Vector2(0f, -290f), 720f);
    }

    private void BuildMultiplayerPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Multiplayer", new Vector2(0f, 134f));
        CreatePanelBody(panel, "Party play deserves its own table, not a copy of single-player with extra chairs. It can grow into shared loadouts and host rules later.", new Vector2(0f, 90f), 700f);
        CreateDivider(panel, new Vector2(0f, 52f), 620f);

        CreateButton(panel, "Party Setup", new Vector2(-160f, -8f), new Vector2(240f, 52f), PlaceholderArcane, false, null, "Soon");
        CreateButton(panel, "Shared Loadout", new Vector2(160f, -8f), new Vector2(240f, 52f), PlaceholderRanger, false, null, "Later");
        CreateButton(panel, "Back", new Vector2(0f, -104f), new Vector2(220f, 48f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "For now, Multiplayer is a future branch. Later it can decide whether kits are personal, shared, or host-shaped.", new Vector2(0f, -176f), 700f);
    }

    private void BuildLoadoutPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Starting Loadout", new Vector2(0f, 188f));
        CreatePanelBody(panel, "Choose the weapon, bomb skill, and passive that define your first steps into danger. This should feel like preparing a kit, not flipping random toggles.", new Vector2(0f, 148f), 760f);
        CreateDivider(panel, new Vector2(0f, 108f), 720f);
        _loadoutHeaderSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, 84f), 760f);

        _weaponChoiceText = CreateChoiceBlock(panel, "Weapon", new Vector2(0f, 8f), CycleWeaponBackward, CycleWeaponForward, out _weaponDescriptionText);
        _bombChoiceText = CreateChoiceBlock(panel, "Bomb Skill", new Vector2(0f, -94f), CycleBombBackward, CycleBombForward, out _bombDescriptionText);
        _skillChoiceText = CreateChoiceBlock(panel, "Active Skill", new Vector2(0f, -196f), CycleSkillBackward, CycleSkillForward, out _skillDescriptionText);
        _passiveChoiceText = CreateChoiceBlock(panel, "Passive", new Vector2(0f, -298f), CyclePassiveBackward, CyclePassiveForward, out _passiveDescriptionText);

        CreateButton(panel, "Back to Setup", new Vector2(0f, -390f), new Vector2(260f, 50f), SecondaryButtonColor, true, ShowSinglePlayerSetup, string.Empty);
        CreateHintLabel(panel, "These choices stay active until you change them here again. The next step is expanding the weapon, bomb, and skill families so every loadout feels more distinct.", new Vector2(0f, -458f), 760f);
    }

    private static void CreatePanelTitle(Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject labelObject = new GameObject($"{label}Title");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 28f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = TitleColor;

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(620f, 40f);
    }

    private static void CreatePanelBody(Transform parent, string body, Vector2 anchoredPosition, float width)
    {
        GameObject bodyObject = new GameObject("Body");
        bodyObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = bodyObject.AddComponent<TextMeshProUGUI>();
        text.text = body;
        text.fontSize = 16f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = BodyColor;

        RectTransform rect = bodyObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(width, 64f);
    }

    private static void CreateSectionLabel(Transform parent, string label, Vector2 anchoredPosition)
    {
        GameObject labelObject = new GameObject($"{label}Label");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = HintColor;

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(360f, 30f);
    }

    private static TextMeshProUGUI CreateHintLabel(Transform parent, string label, Vector2 anchoredPosition, float width)
    {
        GameObject labelObject = new GameObject("Hint");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 13f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = HintColor;

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(width, 48f);

        return text;
    }

    private static void CreateDivider(Transform parent, Vector2 anchoredPosition, float width)
    {
        GameObject dividerObject = new GameObject("Divider");
        dividerObject.transform.SetParent(parent, false);

        Image image = dividerObject.AddComponent<Image>();
        image.color = new Color(0.85f, 0.69f, 0.35f, 0.18f);

        RectTransform rect = dividerObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(width, 2f);
    }

    private TextMeshProUGUI CreateChoiceBlock(
        Transform parent,
        string label,
        Vector2 centerPosition,
        UnityEngine.Events.UnityAction onPrevious,
        UnityEngine.Events.UnityAction onNext,
        out TextMeshProUGUI descriptionText)
    {
        CreateSectionLabel(parent, label, new Vector2(centerPosition.x, centerPosition.y + 30f));
        CreateButton(parent, "<", new Vector2(centerPosition.x - 320f, centerPosition.y), new Vector2(60f, 48f), UtilityButtonColor, true, onPrevious, string.Empty);
        CreateButton(parent, ">", new Vector2(centerPosition.x + 320f, centerPosition.y), new Vector2(60f, 48f), UtilityButtonColor, true, onNext, string.Empty);

        GameObject choiceObject = new GameObject($"{label}Choice");
        choiceObject.transform.SetParent(parent, false);

        TextMeshProUGUI choiceText = choiceObject.AddComponent<TextMeshProUGUI>();
        choiceText.fontSize = 24f;
        choiceText.alignment = TextAlignmentOptions.Center;
        choiceText.color = TitleColor;

        RectTransform choiceRect = choiceObject.GetComponent<RectTransform>();
        choiceRect.anchorMin = new Vector2(0.5f, 0.5f);
        choiceRect.anchorMax = new Vector2(0.5f, 0.5f);
        choiceRect.pivot = new Vector2(0.5f, 0.5f);
        choiceRect.anchoredPosition = new Vector2(centerPosition.x, centerPosition.y);
        choiceRect.sizeDelta = new Vector2(520f, 36f);

        GameObject descriptionObject = new GameObject($"{label}Description");
        descriptionObject.transform.SetParent(parent, false);

        descriptionText = descriptionObject.AddComponent<TextMeshProUGUI>();
        descriptionText.fontSize = 14f;
        descriptionText.alignment = TextAlignmentOptions.Center;
        descriptionText.enableWordWrapping = true;
        descriptionText.color = BodyColor;

        RectTransform descriptionRect = descriptionObject.GetComponent<RectTransform>();
        descriptionRect.anchorMin = new Vector2(0.5f, 0.5f);
        descriptionRect.anchorMax = new Vector2(0.5f, 0.5f);
        descriptionRect.pivot = new Vector2(0.5f, 0.5f);
        descriptionRect.anchoredPosition = new Vector2(centerPosition.x, centerPosition.y - 34f);
        descriptionRect.sizeDelta = new Vector2(640f, 40f);

        return choiceText;
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
        outline.effectDistance = new Vector2(2f, -2f);

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
        text.fontSize = size.y >= 60f ? 26f : (size.y >= 54f ? 22f : 18f);
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
        image.color = new Color(0f, 0f, 0f, 0.26f);

        RectTransform rect = badgeObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-8f, -8f);
        rect.sizeDelta = new Vector2(82f, 22f);

        GameObject textObject = new GameObject("BadgeLabel");
        textObject.transform.SetParent(badgeObject.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 10f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.91f, 0.62f, 1f);

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

    private void CycleWeaponBackward()
    {
        RunLoadoutState.CycleWeapon(-1);
        RefreshLoadoutTexts();
    }

    private void CycleWeaponForward()
    {
        RunLoadoutState.CycleWeapon(1);
        RefreshLoadoutTexts();
    }

    private void CycleBombBackward()
    {
        RunLoadoutState.CycleBomb(-1);
        RefreshLoadoutTexts();
    }

    private void CycleBombForward()
    {
        RunLoadoutState.CycleBomb(1);
        RefreshLoadoutTexts();
    }

    private void CycleSkillBackward()
    {
        RunLoadoutState.CycleSkill(-1);
        RefreshLoadoutTexts();
    }

    private void CycleSkillForward()
    {
        RunLoadoutState.CycleSkill(1);
        RefreshLoadoutTexts();
    }

    private void CyclePassiveBackward()
    {
        RunLoadoutState.CyclePassive(-1);
        RefreshLoadoutTexts();
    }

    private void CyclePassiveForward()
    {
        RunLoadoutState.CyclePassive(1);
        RefreshLoadoutTexts();
    }

    private void RefreshLoadoutTexts()
    {
        string summary = RunLoadoutState.BuildSummary();

        if (_singlePlayerLoadoutSummaryText != null)
            _singlePlayerLoadoutSummaryText.text = summary;

        if (_loadoutHeaderSummaryText != null)
            _loadoutHeaderSummaryText.text = summary;

        if (_weaponChoiceText != null)
            _weaponChoiceText.text = RunLoadoutState.GetWeaponName(RunLoadoutState.WeaponChoice);

        if (_weaponDescriptionText != null)
            _weaponDescriptionText.text = RunLoadoutState.GetWeaponDescription(RunLoadoutState.WeaponChoice);

        if (_bombChoiceText != null)
            _bombChoiceText.text = RunLoadoutState.GetBombName(RunLoadoutState.BombChoice);

        if (_bombDescriptionText != null)
            _bombDescriptionText.text = RunLoadoutState.GetBombDescription(RunLoadoutState.BombChoice);

        if (_skillChoiceText != null)
            _skillChoiceText.text = RunLoadoutState.GetSkillName(RunLoadoutState.SkillChoice);

        if (_skillDescriptionText != null)
            _skillDescriptionText.text = RunLoadoutState.GetSkillDescription(RunLoadoutState.SkillChoice);

        if (_passiveChoiceText != null)
            _passiveChoiceText.text = RunLoadoutState.GetPassiveName(RunLoadoutState.PassiveChoice);

        if (_passiveDescriptionText != null)
            _passiveDescriptionText.text = RunLoadoutState.GetPassiveDescription(RunLoadoutState.PassiveChoice);
    }

    private void ShowModeSelection()
    {
        SetPanelState(modeSelection: true, singlePlayer: false, multiplayer: false, loadout: false);
    }

    private void ShowSinglePlayerSetup()
    {
        SetPanelState(modeSelection: false, singlePlayer: true, multiplayer: false, loadout: false);
    }

    private void ShowMultiplayerSetup()
    {
        SetPanelState(modeSelection: false, singlePlayer: false, multiplayer: true, loadout: false);
    }

    private void ShowLoadoutSetup()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, singlePlayer: false, multiplayer: false, loadout: true);
    }

    private void SetPanelState(bool modeSelection, bool singlePlayer, bool multiplayer, bool loadout)
    {
        if (_modeSelectionPanel != null)
            _modeSelectionPanel.gameObject.SetActive(modeSelection);

        if (_singlePlayerPanel != null)
            _singlePlayerPanel.gameObject.SetActive(singlePlayer);

        if (_multiplayerPanel != null)
            _multiplayerPanel.gameObject.SetActive(multiplayer);

        if (_loadoutPanel != null)
            _loadoutPanel.gameObject.SetActive(loadout);
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
