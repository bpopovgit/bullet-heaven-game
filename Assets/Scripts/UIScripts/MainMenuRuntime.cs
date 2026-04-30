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

    private sealed class TalentCardView
    {
        public GameObject Root;
        public Image Background;
        public Outline Outline;
        public TextMeshProUGUI UnlockText;
        public TextMeshProUGUI TitleText;
        public TextMeshProUGUI RequirementText;
        public TextMeshProUGUI EffectText;
    }

    private sealed class RunChainView
    {
        public GameObject Root;
        public TextMeshProUGUI ChainNameText;
        public TalentCardView[] Nodes;
        public TextMeshProUGUI[] Arrows;
    }

    private sealed class RunTalentTreeView
    {
        public GameObject Root;
        public TextMeshProUGUI HeaderText;
        public TalentCardView RootNode;
        public TalentCardView LeftNode;
        public TalentCardView RightNode;
        public TalentCardView CapstoneNode;
    }

    private bool _isLoading;
    private RectTransform _root;
    private RectTransform _modeSelectionPanel;
    private RectTransform _factionSelectionPanel;
    private RectTransform _characterSelectionPanel;
    private RectTransform _singlePlayerPanel;
    private RectTransform _multiplayerPanel;
    private RectTransform _loadoutPanel;
    private RectTransform _talentPanel;
    private RectTransform _accountTalentPanel;

    private TextMeshProUGUI _factionChoiceText;
    private TextMeshProUGUI _factionDescriptionText;
    private TextMeshProUGUI _factionStatusText;
    private TextMeshProUGUI _singlePlayerCharacterSummaryText;
    private TextMeshProUGUI _singlePlayerLoadoutSummaryText;
    private TextMeshProUGUI _characterChoiceText;
    private TextMeshProUGUI _characterRoleText;
    private TextMeshProUGUI _characterPrimaryText;
    private TextMeshProUGUI _characterDescriptionText;
    private TextMeshProUGUI _characterStatsText;
    private TextMeshProUGUI _characterAlliesText;
    private TextMeshProUGUI _loadoutHeaderSummaryText;
    private TextMeshProUGUI _weaponChoiceText;
    private TextMeshProUGUI _weaponDescriptionText;
    private TextMeshProUGUI _weaponPreviewText;
    private TextMeshProUGUI _bombChoiceText;
    private TextMeshProUGUI _bombDescriptionText;
    private TextMeshProUGUI _skillChoiceText;
    private TextMeshProUGUI _skillDescriptionText;
    private TextMeshProUGUI _passiveChoiceText;
    private TextMeshProUGUI _passiveDescriptionText;
    private TextMeshProUGUI _talentKitSummaryText;
    private TextMeshProUGUI _talentTagSummaryText;
    private TalentCardView[] _accountTalentCards;
    private RunTalentTreeView[] _runTalentTreeViews;

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
        if (existingCanvas != null && existingCanvas.name == "MainMenuCanvas")
            Destroy(existingCanvas.gameObject);

        Canvas canvas = CreateCanvas();
        _root = canvas.GetComponent<RectTransform>();

        CreateBackground(_root);
        CreateTitle(_root);
        CreateSubtitle(_root);

        _modeSelectionPanel = CreatePanel("ModeSelectionPanel", _root, new Vector2(0f, -34f), new Vector2(900f, 540f));
        BuildModeSelectionPanel(_modeSelectionPanel);

        _factionSelectionPanel = CreatePanel("FactionSelectionPanel", _root, new Vector2(0f, -30f), new Vector2(980f, 590f));
        BuildFactionSelectionPanel(_factionSelectionPanel);

        _characterSelectionPanel = CreatePanel("CharacterSelectionPanel", _root, new Vector2(0f, -30f), new Vector2(980f, 590f));
        BuildCharacterSelectionPanel(_characterSelectionPanel);

        _singlePlayerPanel = CreatePanel("SinglePlayerPanel", _root, new Vector2(0f, -32f), new Vector2(920f, 560f));
        BuildSinglePlayerPanel(_singlePlayerPanel);

        _multiplayerPanel = CreatePanel("MultiplayerPanel", _root, new Vector2(0f, -30f), new Vector2(880f, 460f));
        BuildMultiplayerPanel(_multiplayerPanel);

        _loadoutPanel = CreatePanel("LoadoutPanel", _root, new Vector2(0f, -28f), new Vector2(1000f, 600f));
        BuildLoadoutPanel(_loadoutPanel);

        _talentPanel = CreatePanel("TalentPanel", _root, new Vector2(0f, -30f), new Vector2(1120f, 650f));
        BuildTalentPanel(_talentPanel);

        _accountTalentPanel = CreatePanel("AccountTalentPanel", _root, new Vector2(0f, -30f), new Vector2(1080f, 650f));
        BuildAccountTalentPanel(_accountTalentPanel);

        CreateFooter(_root);
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
        text.text = "Choose your mode, shape a loadout, and enter the faction war.";
        text.fontSize = 18f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = BodyColor;

        RectTransform rect = subtitleObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, 254f);
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
        CreatePanelTitle(panel, "Choose Mode", new Vector2(0f, 188f));
        CreatePanelBody(panel, "Account talents live here before you enter a mode. After that: faction, character, starting weapon, then the run.", new Vector2(0f, 148f), 720f);
        CreateDivider(panel, new Vector2(0f, 102f), 640f);

        CreateButton(panel, "Single Player", new Vector2(0f, 32f), new Vector2(440f, 64f), AccentColor, true, ShowFactionSelection, string.Empty);
        CreateButton(panel, "Multiplayer", new Vector2(0f, -48f), new Vector2(440f, 56f), PlaceholderArcane, true, ShowMultiplayerSetup, "Soon");

        CreateSectionLabel(panel, "Camp Desk", new Vector2(0f, -110f));
        CreateButton(panel, "Settings", new Vector2(-240f, -160f), new Vector2(190f, 48f), UtilityButtonColor, false, null, string.Empty);
        CreateButton(panel, "Sound", new Vector2(0f, -160f), new Vector2(190f, 48f), PlaceholderWar, false, null, string.Empty);
        CreateButton(panel, "Account Talents", new Vector2(240f, -160f), new Vector2(220f, 48f), PlaceholderRanger, true, ShowAccountTalentBrowser, string.Empty);
        CreateButton(panel, "Quit", new Vector2(0f, -230f), new Vector2(240f, 50f), SecondaryButtonColor, true, QuitGame, string.Empty);

        CreateHintLabel(panel, "Account talents are global progression. Run talents are shown later with the selected character and kit.", new Vector2(0f, -292f), 760f);
    }

    private void BuildFactionSelectionPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Choose Faction", new Vector2(0f, 202f));
        CreatePanelBody(panel, "Start with the side you represent. Zombies remain enemy-only; Angels and Demons are prepared as future player factions.", new Vector2(0f, 160f), 820f);
        CreateDivider(panel, new Vector2(0f, 116f), 760f);

        CreateButton(panel, "Humans", new Vector2(-300f, 36f), new Vector2(220f, 64f), AccentColor, true, SelectHumanFaction, "Ready");
        CreateButton(panel, "Angels", new Vector2(0f, 36f), new Vector2(220f, 64f), new Color(0.3f, 0.55f, 0.66f, 1f), true, SelectAngelFaction, "Later");
        CreateButton(panel, "Demons", new Vector2(300f, 36f), new Vector2(220f, 64f), new Color(0.72f, 0.22f, 0.14f, 1f), true, SelectDemonFaction, "Later");

        _factionChoiceText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(0f, -48f), 760f, 28f, TitleColor);
        _factionDescriptionText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(0f, -102f), 780f, 16f, BodyColor);
        _factionStatusText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(0f, -158f), 760f, 15f, HintColor);

        CreateButton(panel, "Continue", new Vector2(0f, -234f), new Vector2(300f, 58f), AccentColor, true, ContinueFromFactionSelection, string.Empty);
        CreateButton(panel, "Back", new Vector2(0f, -306f), new Vector2(220f, 48f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "Faction choice will later drive available characters, weapons, allies, and aggro politics.", new Vector2(0f, -358f), 800f);
    }

    private void BuildCharacterSelectionPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Choose Character", new Vector2(0f, 202f));
        CreatePanelBody(panel, "Characters belong to the selected faction. Pick the role, then choose the starting weapon that defines the opening run.", new Vector2(0f, 160f), 800f);
        CreateDivider(panel, new Vector2(0f, 116f), 720f);

        CreateButton(panel, "<", new Vector2(-350f, 28f), new Vector2(66f, 54f), UtilityButtonColor, true, CycleCharacterBackward, string.Empty);
        CreateButton(panel, ">", new Vector2(10f, 28f), new Vector2(66f, 54f), UtilityButtonColor, true, CycleCharacterForward, string.Empty);

        _characterChoiceText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(-170f, 70f), 420f, 30f, TitleColor);
        _characterRoleText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(-170f, 34f), 380f, 17f, HintColor);
        _characterPrimaryText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(-170f, -12f), 480f, 16f, TitleColor);
        _characterDescriptionText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(260f, 50f), 430f, 16f, BodyColor);
        _characterStatsText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(260f, -34f), 430f, 15f, HintColor);
        _characterAlliesText = CreateLargeCenterLabel(panel, string.Empty, new Vector2(260f, -100f), 430f, 15f, BodyColor);

        CreateButton(panel, "Continue", new Vector2(0f, -226f), new Vector2(300f, 58f), AccentColor, true, ShowSinglePlayerSetup, string.Empty);
        CreateButton(panel, "Back", new Vector2(0f, -298f), new Vector2(220f, 48f), SecondaryButtonColor, true, ShowFactionSelection, string.Empty);
        CreateHintLabel(panel, "The side panel is reserved for character flavor, stats, and ally expectations. This will matter more as factions grow.", new Vector2(0f, -350f), 820f);
    }

    private void BuildSinglePlayerPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Single Player Setup", new Vector2(0f, 184f));
        CreatePanelBody(panel, "Review the chosen faction and character, then tune the starting weapon and preview the run talent paths.", new Vector2(0f, 142f), 760f);
        CreateDivider(panel, new Vector2(0f, 98f), 680f);

        CreateSectionLabel(panel, "Run Setup", new Vector2(0f, 48f));
        CreateButton(panel, "Faction", new Vector2(-360f, -4f), new Vector2(160f, 52f), UtilityButtonColor, true, ShowFactionSelection, string.Empty);
        CreateButton(panel, "Character", new Vector2(-180f, -4f), new Vector2(160f, 52f), UtilityButtonColor, true, ShowCharacterSelection, string.Empty);
        CreateButton(panel, "Starting Kit", new Vector2(0f, -4f), new Vector2(160f, 52f), PlaceholderArcane, true, ShowLoadoutSetup, string.Empty);
        CreateButton(panel, "Run Talents", new Vector2(180f, -4f), new Vector2(160f, 52f), AccentColor, true, ShowTalentBrowser, string.Empty);
        CreateButton(panel, "Map Select", new Vector2(360f, -4f), new Vector2(160f, 52f), UtilityButtonColor, false, null, "Soon");

        _singlePlayerCharacterSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, -64f), 760f);
        _singlePlayerLoadoutSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, -112f), 760f);
        CreateButton(panel, "Start Run", new Vector2(0f, -178f), new Vector2(320f, 60f), AccentColor, true, LoadGameplayScene, string.Empty);
        CreateButton(panel, "Back", new Vector2(0f, -252f), new Vector2(220f, 48f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "Current flow: Faction -> Character -> Starting Kit -> Run Talents -> Start Run.", new Vector2(0f, -314f), 720f);
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
        CreatePanelTitle(panel, "Starting Loadout", new Vector2(0f, 204f));
        CreatePanelBody(panel, "Choose the character's starting weapon first, then the bomb, active skill, and passive that support it.", new Vector2(0f, 164f), 780f);
        CreateDivider(panel, new Vector2(0f, 120f), 720f);
        _loadoutHeaderSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, 94f), 760f);

        _weaponChoiceText = CreateChoiceBlock(panel, "Primary", new Vector2(0f, 18f), CycleWeaponBackward, CycleWeaponForward, out _weaponDescriptionText);
        _weaponPreviewText = CreateWeaponPreviewBox(panel, new Vector2(390f, 18f));
        _bombChoiceText = CreateChoiceBlock(panel, "Bomb Skill", new Vector2(0f, -82f), CycleBombBackward, CycleBombForward, out _bombDescriptionText);
        _skillChoiceText = CreateChoiceBlock(panel, "Active Skill", new Vector2(0f, -182f), CycleSkillBackward, CycleSkillForward, out _skillDescriptionText);
        _passiveChoiceText = CreateChoiceBlock(panel, "Passive", new Vector2(0f, -282f), CyclePassiveBackward, CyclePassiveForward, out _passiveDescriptionText);

        CreateButton(panel, "Back to Setup", new Vector2(0f, -374f), new Vector2(260f, 50f), SecondaryButtonColor, true, ShowSinglePlayerSetup, string.Empty);
        CreateHintLabel(panel, "These choices stay active until you change them here again. More weapon, bomb, and skill families can plug into this screen later.", new Vector2(0f, -436f), 760f);
    }

    private void BuildTalentPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Run Talents", new Vector2(0f, 236f));
        CreatePanelBody(panel, "Preview the in-run talent pools. Root talents appear first; taking one unlocks its connected follow-ups. Every talent can hold up to 5 points.", new Vector2(0f, 192f), 920f);
        CreateDivider(panel, new Vector2(0f, 144f), 840f);

        _talentKitSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, 114f), 900f);
        _talentTagSummaryText = CreateHintLabel(panel, string.Empty, new Vector2(0f, 84f), 900f);

        RectTransform talentContent = CreateTalentScrollContent(panel, new Vector2(0f, -94f), new Vector2(1060f, 430f), new Vector2(1040f, 1120f));
        CreateTalentCardText(talentContent, "AttackHeader", new Vector2(-270f, 520f), new Vector2(420f, 28f), 18f, HintColor).text = "Attack";
        CreateTalentCardText(talentContent, "DefenseHeader", new Vector2(270f, 520f), new Vector2(420f, 28f), 18f, HintColor).text = "Defense";

        _runTalentTreeViews = new RunTalentTreeView[6];
        _runTalentTreeViews[0] = CreateRunTalentTreeView(talentContent, new Vector2(-270f, 338f));
        _runTalentTreeViews[1] = CreateRunTalentTreeView(talentContent, new Vector2(-270f, 0f));
        _runTalentTreeViews[2] = CreateRunTalentTreeView(talentContent, new Vector2(-270f, -338f));
        _runTalentTreeViews[3] = CreateRunTalentTreeView(talentContent, new Vector2(270f, 338f));
        _runTalentTreeViews[4] = CreateRunTalentTreeView(talentContent, new Vector2(270f, 0f));
        _runTalentTreeViews[5] = CreateRunTalentTreeView(talentContent, new Vector2(270f, -338f));

        CreateButton(panel, "Change Loadout", new Vector2(-150f, -326f), new Vector2(250f, 48f), AccentColor, true, ShowLoadoutSetup, string.Empty);
        CreateButton(panel, "Back to Setup", new Vector2(150f, -326f), new Vector2(250f, 48f), SecondaryButtonColor, true, ShowSinglePlayerSetup, string.Empty);
        CreateHintLabel(panel, "Scroll to inspect all rows. In-game level-ups now use this tree and hide locked or maxed talents.", new Vector2(0f, -388f), 900f);
    }

    private void BuildAccountTalentPanel(RectTransform panel)
    {
        CreatePanelTitle(panel, "Account Talents", new Vector2(0f, 236f));
        CreatePanelBody(panel, "Permanent progression belongs before mode selection. These talents unlock long-term options across runs instead of changing one run's level-up pool.", new Vector2(0f, 192f), 900f);
        CreateDivider(panel, new Vector2(0f, 144f), 820f);

        _accountTalentCards = new TalentCardView[6];
        Vector2 cardSize = new Vector2(490f, 96f);
        _accountTalentCards[0] = CreateTalentCard(panel, new Vector2(-260f, 72f), cardSize);
        _accountTalentCards[1] = CreateTalentCard(panel, new Vector2(260f, 72f), cardSize);
        _accountTalentCards[2] = CreateTalentCard(panel, new Vector2(-260f, -38f), cardSize);
        _accountTalentCards[3] = CreateTalentCard(panel, new Vector2(260f, -38f), cardSize);
        _accountTalentCards[4] = CreateTalentCard(panel, new Vector2(-260f, -148f), cardSize);
        _accountTalentCards[5] = CreateTalentCard(panel, new Vector2(260f, -148f), cardSize);

        CreateButton(panel, "Back", new Vector2(0f, -326f), new Vector2(240f, 50f), SecondaryButtonColor, true, ShowModeSelection, string.Empty);
        CreateHintLabel(panel, "This screen is global. Character-specific run talent trees live inside Single Player setup.", new Vector2(0f, -388f), 900f);
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

    private static TextMeshProUGUI CreateLargeCenterLabel(
        Transform parent,
        string label,
        Vector2 anchoredPosition,
        float width,
        float fontSize,
        Color color)
    {
        GameObject labelObject = new GameObject("CenterLabel");
        labelObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = labelObject.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.color = color;

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

    private static TextMeshProUGUI CreateWeaponPreviewBox(Transform parent, Vector2 anchoredPosition)
    {
        GameObject boxObject = new GameObject("WeaponPreview");
        boxObject.transform.SetParent(parent, false);

        Image image = boxObject.AddComponent<Image>();
        image.color = new Color(0.03f, 0.13f, 0.04f, 0.82f);

        Outline outline = boxObject.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        RectTransform rect = boxObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(150f, 72f);

        GameObject textObject = new GameObject("PreviewText");
        textObject.transform.SetParent(boxObject.transform, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = 11f;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.color = BodyColor;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        Stretch(textRect, 8f);

        return text;
    }

    private static TalentCardView CreateTalentCard(Transform parent, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject cardObject = new GameObject("TalentCard");
        cardObject.transform.SetParent(parent, false);

        Image background = cardObject.AddComponent<Image>();
        background.color = new Color(0.05f, 0.12f, 0.06f, 0.78f);

        Outline outline = cardObject.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        RectTransform rect = cardObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        bool compact = size.y < 96f;
        float titleFontSize = compact ? 13f : 18f;
        float requirementFontSize = compact ? 9f : 11f;
        float effectFontSize = compact ? 9f : 12f;
        float unlockFontSize = compact ? 8.5f : 11f;

        TalentCardView view = new TalentCardView
        {
            Root = cardObject,
            Background = background,
            Outline = outline,
            UnlockText = CreateTalentCardText(cardObject.transform, "Unlock", new Vector2(0f, size.y * 0.34f), new Vector2(size.x - 24f, size.y * 0.16f), unlockFontSize, HintColor),
            TitleText = CreateTalentCardText(cardObject.transform, "Title", new Vector2(0f, size.y * 0.13f), new Vector2(size.x - 24f, size.y * 0.24f), titleFontSize, TitleColor),
            RequirementText = CreateTalentCardText(cardObject.transform, "Requirement", new Vector2(0f, size.y * -0.08f), new Vector2(size.x - 24f, size.y * 0.16f), requirementFontSize, new Color(0.72f, 0.88f, 0.68f, 1f)),
            EffectText = CreateTalentCardText(cardObject.transform, "Effect", new Vector2(0f, size.y * -0.31f), new Vector2(size.x - 28f, size.y * 0.34f), effectFontSize, BodyColor)
        };

        return view;
    }

    private static GameObject CreateViewRoot(string name, Transform parent)
    {
        GameObject rootObject = new GameObject(name, typeof(RectTransform));
        rootObject.transform.SetParent(parent, false);

        RectTransform rect = rootObject.GetComponent<RectTransform>();
        Stretch(rect, 0f);

        return rootObject;
    }

    private static RectTransform CreateTalentScrollContent(Transform parent, Vector2 anchoredPosition, Vector2 viewportSize, Vector2 contentSize)
    {
        GameObject scrollObject = new GameObject("RunTalentScrollView");
        scrollObject.transform.SetParent(parent, false);

        RectTransform scrollRectTransform = scrollObject.AddComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRectTransform.pivot = new Vector2(0.5f, 0.5f);
        scrollRectTransform.anchoredPosition = anchoredPosition;
        scrollRectTransform.sizeDelta = viewportSize;

        Image scrollBackground = scrollObject.AddComponent<Image>();
        scrollBackground.color = new Color(0.02f, 0.08f, 0.03f, 0.35f);

        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 32f;

        GameObject viewportObject = new GameObject("Viewport");
        viewportObject.transform.SetParent(scrollObject.transform, false);
        RectTransform viewportRect = viewportObject.AddComponent<RectTransform>();
        Stretch(viewportRect, 8f);

        Image viewportImage = viewportObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.05f);
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content");
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 1f);
        contentRect.anchorMax = new Vector2(0.5f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = contentSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.verticalNormalizedPosition = 1f;

        return contentRect;
    }

    private static RunTalentTreeView CreateRunTalentTreeView(Transform parent, Vector2 anchoredPosition)
    {
        GameObject treeObject = new GameObject("RunTalentTree");
        treeObject.transform.SetParent(parent, false);

        RectTransform rect = treeObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(500f, 330f);

        RunTalentTreeView view = new RunTalentTreeView
        {
            Root = treeObject,
            HeaderText = CreateTalentCardText(treeObject.transform, "TreeHeader", new Vector2(0f, 154f), new Vector2(450f, 24f), 14f, HintColor),
            RootNode = CreateTalentCard(treeObject.transform, new Vector2(0f, 86f), new Vector2(230f, 86f)),
            LeftNode = CreateTalentCard(treeObject.transform, new Vector2(-128f, -34f), new Vector2(220f, 92f)),
            RightNode = CreateTalentCard(treeObject.transform, new Vector2(128f, -34f), new Vector2(220f, 92f)),
            CapstoneNode = CreateTalentCard(treeObject.transform, new Vector2(0f, -160f), new Vector2(250f, 92f))
        };

        CreateTalentCardText(treeObject.transform, "SplitConnectorLeft", new Vector2(-64f, 28f), new Vector2(42f, 28f), 18f, HintColor).text = "/";
        CreateTalentCardText(treeObject.transform, "SplitConnectorRight", new Vector2(64f, 28f), new Vector2(42f, 28f), 18f, HintColor).text = "\\";
        CreateTalentCardText(treeObject.transform, "MergeConnectorLeft", new Vector2(-64f, -96f), new Vector2(42f, 28f), 18f, HintColor).text = "\\";
        CreateTalentCardText(treeObject.transform, "MergeConnectorRight", new Vector2(64f, -96f), new Vector2(42f, 28f), 18f, HintColor).text = "/";

        return view;
    }

    private static RunChainView CreateRunChainView(Transform parent, Vector2 anchoredPosition)
    {
        GameObject rowObject = new GameObject("RunUpgradeChain");
        rowObject.transform.SetParent(parent, false);

        RectTransform rect = rowObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(980f, 112f);

        RunChainView view = new RunChainView
        {
            Root = rowObject,
            ChainNameText = CreateTalentCardText(rowObject.transform, "ChainName", new Vector2(0f, 50f), new Vector2(920f, 20f), 13f, HintColor),
            Nodes = new TalentCardView[4],
            Arrows = new TextMeshProUGUI[3]
        };

        Vector2 nodeSize = new Vector2(208f, 78f);
        view.Nodes[0] = CreateTalentCard(rowObject.transform, new Vector2(-390f, 0f), nodeSize);
        view.Nodes[1] = CreateTalentCard(rowObject.transform, new Vector2(-130f, 0f), nodeSize);
        view.Nodes[2] = CreateTalentCard(rowObject.transform, new Vector2(130f, 0f), nodeSize);
        view.Nodes[3] = CreateTalentCard(rowObject.transform, new Vector2(390f, 0f), nodeSize);

        view.Arrows[0] = CreateTalentCardText(rowObject.transform, "Arrow", new Vector2(-260f, 0f), new Vector2(38f, 34f), 22f, HintColor);
        view.Arrows[1] = CreateTalentCardText(rowObject.transform, "Arrow", new Vector2(0f, 0f), new Vector2(38f, 34f), 22f, HintColor);
        view.Arrows[2] = CreateTalentCardText(rowObject.transform, "Arrow", new Vector2(260f, 0f), new Vector2(38f, 34f), 22f, HintColor);

        for (int i = 0; i < view.Arrows.Length; i++)
            view.Arrows[i].text = ">";

        return view;
    }

    private static TextMeshProUGUI CreateTalentCardText(
        Transform parent,
        string name,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.color = color;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
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
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
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

    private void CycleCharacterBackward()
    {
        RunLoadoutState.CycleCharacter(-1);
        RefreshLoadoutTexts();
    }

    private void CycleCharacterForward()
    {
        RunLoadoutState.CycleCharacter(1);
        RefreshLoadoutTexts();
    }

    private void SelectHumanFaction()
    {
        RunLoadoutState.SelectFaction(PlayableFactionChoice.Humans);
        RefreshLoadoutTexts();
    }

    private void SelectAngelFaction()
    {
        RunLoadoutState.SelectFaction(PlayableFactionChoice.Angels);
        RefreshLoadoutTexts();
    }

    private void SelectDemonFaction()
    {
        RunLoadoutState.SelectFaction(PlayableFactionChoice.Demons);
        RefreshLoadoutTexts();
    }

    private void ContinueFromFactionSelection()
    {
        if (!RunLoadoutState.IsFactionPlayable(RunLoadoutState.FactionChoice))
        {
            Debug.Log($"{RunLoadoutState.GetFactionName(RunLoadoutState.FactionChoice)} faction is not playable yet.");
            RefreshLoadoutTexts();
            return;
        }

        ShowCharacterSelection();
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
        string characterSummary = RunLoadoutState.BuildCharacterSummary();
        string kitSummary = RunLoadoutState.BuildKitSummary();

        if (_singlePlayerCharacterSummaryText != null)
            _singlePlayerCharacterSummaryText.text = characterSummary;

        if (_singlePlayerLoadoutSummaryText != null)
            _singlePlayerLoadoutSummaryText.text = kitSummary;

        if (_characterChoiceText != null)
            _characterChoiceText.text = RunLoadoutState.GetCharacterName(RunLoadoutState.CharacterChoice);

        if (_characterRoleText != null)
            _characterRoleText.text = RunLoadoutState.GetCharacterRole(RunLoadoutState.CharacterChoice);

        if (_characterPrimaryText != null)
            _characterPrimaryText.text = $"{RunLoadoutState.GetPrimaryAttackCategory(RunLoadoutState.CharacterChoice)} Primary: {RunLoadoutState.GetPrimaryAttackName(RunLoadoutState.CharacterChoice, RunLoadoutState.WeaponChoice)}";

        if (_characterDescriptionText != null)
            _characterDescriptionText.text = RunLoadoutState.GetCharacterDescription(RunLoadoutState.CharacterChoice);

        if (_characterStatsText != null)
            _characterStatsText.text = RunLoadoutState.GetCharacterStatsSummary(RunLoadoutState.CharacterChoice);

        if (_characterAlliesText != null)
            _characterAlliesText.text = RunLoadoutState.GetCharacterAllySummary(RunLoadoutState.CharacterChoice);

        if (_loadoutHeaderSummaryText != null)
            _loadoutHeaderSummaryText.text = kitSummary;

        if (_weaponChoiceText != null)
            _weaponChoiceText.text = RunLoadoutState.GetPrimaryAttackName(RunLoadoutState.CharacterChoice, RunLoadoutState.WeaponChoice);

        if (_weaponDescriptionText != null)
            _weaponDescriptionText.text = RunLoadoutState.GetPrimaryAttackDescription(RunLoadoutState.CharacterChoice, RunLoadoutState.WeaponChoice);

        if (_weaponPreviewText != null)
            _weaponPreviewText.text = RunLoadoutState.GetPrimaryAttackPreviewText(RunLoadoutState.CharacterChoice, RunLoadoutState.WeaponChoice);

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

        RefreshTalentTexts();
    }

    private void RefreshTalentTexts()
    {
        if (_factionChoiceText != null)
            _factionChoiceText.text = RunLoadoutState.GetFactionName(RunLoadoutState.FactionChoice);

        if (_factionDescriptionText != null)
            _factionDescriptionText.text = RunLoadoutState.GetFactionDescription(RunLoadoutState.FactionChoice);

        if (_factionStatusText != null)
        {
            string status = RunLoadoutState.GetFactionStatus(RunLoadoutState.FactionChoice);
            string nextStep = RunLoadoutState.IsFactionPlayable(RunLoadoutState.FactionChoice)
                ? "Continue to character selection."
                : "This faction is visible for planning but is not playable yet.";
            _factionStatusText.text = $"{status}  |  {nextStep}";
        }

        if (_talentKitSummaryText != null)
            _talentKitSummaryText.text = RunLoadoutState.BuildSummary();

        if (_talentTagSummaryText != null)
            _talentTagSummaryText.text = TalentCatalog.BuildCurrentTagSummary();

        TalentDisplayInfo[] cards = TalentCatalog.BuildCurrentDisplayCards();
        if (_accountTalentCards != null)
        {
            for (int i = 0; i < _accountTalentCards.Length; i++)
            {
                TalentCardView view = _accountTalentCards[i];
                if (view == null || view.Root == null)
                    continue;

                bool hasCard = i < cards.Length;
                view.Root.SetActive(hasCard);
                if (!hasCard)
                    continue;

                TalentDisplayInfo card = cards[i];
                ApplyTalentCardView(
                    view,
                    card.AccentColor,
                    card.IsUnlocked,
                    $"{card.TreeName}  |  {card.UnlockText}",
                    card.Title,
                    card.RequirementText,
                    card.EffectText);
            }
        }

        RefreshRunTalentTrees();
    }

    private void RefreshRunTalentTrees()
    {
        if (_runTalentTreeViews == null)
            return;

        RunUpgradeChainDisplayInfo[] trees = TalentCatalog.BuildCurrentRunTalentTrees();
        for (int i = 0; i < _runTalentTreeViews.Length; i++)
        {
            RunTalentTreeView view = _runTalentTreeViews[i];
            if (view == null || view.Root == null)
                continue;

            bool hasTree = i < trees.Length;
            view.Root.SetActive(hasTree);
            if (!hasTree)
                continue;

            RunUpgradeChainDisplayInfo tree = trees[i];
            if (view.HeaderText != null)
                view.HeaderText.text = tree.ChainName;

            ApplyTreeNode(view.RootNode, tree, 0);
            ApplyTreeNode(view.LeftNode, tree, 1);
            ApplyTreeNode(view.RightNode, tree, 2);
            ApplyTreeNode(view.CapstoneNode, tree, 3);
        }
    }

    private static void ApplyTreeNode(TalentCardView view, RunUpgradeChainDisplayInfo tree, int index)
    {
        if (view == null || view.Root == null)
            return;

        bool hasNode = tree.Nodes != null && index < tree.Nodes.Length;
        view.Root.SetActive(hasNode);
        if (!hasNode)
            return;

        RunUpgradeNodeDisplayInfo node = tree.Nodes[index];
        ApplyTalentCardView(
            view,
            node.AccentColor,
            true,
            node.StageText,
            node.Title,
            node.RequirementText,
            node.EffectText);
    }

    private static void ApplyTalentCardView(
        TalentCardView view,
        Color accent,
        bool bright,
        string unlockText,
        string title,
        string requirement,
        string effect)
    {
        if (view == null)
            return;

        if (view.Background != null)
            view.Background.color = new Color(accent.r * 0.45f, accent.g * 0.55f, accent.b * 0.45f, 0.78f);

        if (view.Outline != null)
            view.Outline.effectColor = new Color(accent.r, accent.g, accent.b, bright ? 0.45f : 0.2f);

        if (view.UnlockText != null)
            view.UnlockText.text = unlockText;

        if (view.TitleText != null)
            view.TitleText.text = title;

        if (view.RequirementText != null)
            view.RequirementText.text = requirement;

        if (view.EffectText != null)
        {
            view.EffectText.text = effect;
            view.EffectText.color = bright ? BodyColor : new Color(BodyColor.r, BodyColor.g, BodyColor.b, 0.78f);
        }
    }

    private void ShowModeSelection()
    {
        SetPanelState(modeSelection: true, factionSelection: false, characterSelection: false, singlePlayer: false, multiplayer: false, loadout: false);
    }

    private void ShowFactionSelection()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, factionSelection: true, characterSelection: false, singlePlayer: false, multiplayer: false, loadout: false);
    }

    private void ShowCharacterSelection()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, factionSelection: false, characterSelection: true, singlePlayer: false, multiplayer: false, loadout: false);
    }

    private void ShowSinglePlayerSetup()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, factionSelection: false, characterSelection: false, singlePlayer: true, multiplayer: false, loadout: false);
    }

    private void ShowMultiplayerSetup()
    {
        SetPanelState(modeSelection: false, factionSelection: false, characterSelection: false, singlePlayer: false, multiplayer: true, loadout: false);
    }

    private void ShowLoadoutSetup()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, factionSelection: false, characterSelection: false, singlePlayer: false, multiplayer: false, loadout: true);
    }

    private void ShowTalentBrowser()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, factionSelection: false, characterSelection: false, singlePlayer: false, multiplayer: false, loadout: false, talent: true);
    }

    private void ShowAccountTalentBrowser()
    {
        RefreshLoadoutTexts();
        SetPanelState(modeSelection: false, factionSelection: false, characterSelection: false, singlePlayer: false, multiplayer: false, loadout: false, accountTalent: true);
    }

    private void SetPanelState(bool modeSelection, bool factionSelection, bool characterSelection, bool singlePlayer, bool multiplayer, bool loadout, bool talent = false, bool accountTalent = false)
    {
        if (_modeSelectionPanel != null)
            _modeSelectionPanel.gameObject.SetActive(modeSelection);

        if (_factionSelectionPanel != null)
            _factionSelectionPanel.gameObject.SetActive(factionSelection);

        if (_characterSelectionPanel != null)
            _characterSelectionPanel.gameObject.SetActive(characterSelection);

        if (_singlePlayerPanel != null)
            _singlePlayerPanel.gameObject.SetActive(singlePlayer);

        if (_multiplayerPanel != null)
            _multiplayerPanel.gameObject.SetActive(multiplayer);

        if (_loadoutPanel != null)
            _loadoutPanel.gameObject.SetActive(loadout);

        if (_talentPanel != null)
            _talentPanel.gameObject.SetActive(talent);

        if (_accountTalentPanel != null)
            _accountTalentPanel.gameObject.SetActive(accountTalent);
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
