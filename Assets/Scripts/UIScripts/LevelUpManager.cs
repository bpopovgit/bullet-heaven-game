using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    private const float PanelWidth = 920f;
    private const float ChoiceCardWidth = 820f;
    private const float ChoiceCardHeight = 142f;
    private const float ChoiceCardSpacing = 16f;
    private const float UnlockPanelWidth = 240f;
    private const float UnlockEntryHeight = 26f;
    private const int MaxUnlockEntriesShown = 4;
    private const float UnlockIconTileSize = 60f;
    private const float UnlockIconTileSpacing = 10f;
    private const float ChooseButtonHeight = 64f;

    // The War of Death Metal palette — match MainMenuRuntime tones.
    // Wrapper window: deep cold black with very restrained crimson edge — no warm/gold border that reads as brown.
    private static readonly Color DefaultPanelColor = new Color(0.025f, 0.025f, 0.04f, 0.98f);
    private static readonly Color DefaultPanelOutlineColor = new Color(0.55f, 0.10f, 0.12f, 0.55f);
    private static readonly Color ChoiceCardColor = new Color(0.05f, 0.04f, 0.06f, 0.98f);
    private static readonly Color ChoiceCardHighlightColor = new Color(0.18f, 0.06f, 0.08f, 1f);
    private static readonly Color ChoiceCardPressedColor = new Color(0.32f, 0.10f, 0.12f, 1f);
    private static readonly Color ChoiceTextColor = new Color(0.96f, 0.91f, 0.78f, 1f);
    private static readonly Color ChoiceMutedTextColor = new Color(0.84f, 0.80f, 0.74f, 1f);
    private static readonly Color ChoiceGoldTextColor = new Color(0.94f, 0.78f, 0.36f, 1f);
    private static readonly Color AttackAccentColor = new Color(0.85f, 0.20f, 0.18f, 1f);
    private static readonly Color DefenseAccentColor = new Color(0.55f, 0.86f, 0.78f, 1f);
    private static readonly Color RewardAccentColor = new Color(0.96f, 0.72f, 0.28f, 1f);

    // Choose-button palette: dark blackened iron plate with crimson edge accent when active.
    private static readonly Color ChooseButtonActiveBody = new Color(0.16f, 0.09f, 0.11f, 0.98f);
    private static readonly Color ChooseButtonIdleBody = new Color(0.12f, 0.10f, 0.13f, 0.88f);
    private static readonly Color ChooseButtonActiveOutline = new Color(0.82f, 0.18f, 0.18f, 0.95f);
    private static readonly Color ChooseButtonIdleOutline = new Color(0.32f, 0.28f, 0.32f, 0.40f);
    private static readonly Color ChooseButtonActiveText = new Color(0.98f, 0.94f, 0.82f, 1f);
    private static readonly Color ChooseButtonIdleText = new Color(0.58f, 0.54f, 0.50f, 0.85f);

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceTexts;

    private List<PlayerUpgradeOption> _activeChoices;
    private Action<PlayerUpgradeOption> _activeSelectionHandler;
    private float _previousTimeScale = 1f;
    private bool _isShowing;
    private Image _panelImage;
    private Color _defaultPanelColor = Color.white;
    private Color _defaultTitleColor = Color.white;
    private int _selectedIndex = -1;
    private Button _chooseButton;
    private TMP_Text _chooseButtonLabel;
    private Image _chooseButtonImage;
    private Outline _chooseButtonOutline;

    private struct CardSelectionState
    {
        public Outline Outline;
        public Image AccentBar;
        public Image Background;
        public Color BaseAccent;
        public Color BaseBackgroundColor;
    }

    private readonly List<CardSelectionState> _cardStates = new List<CardSelectionState>();

    private sealed class ChoiceCardView
    {
        public Image AccentBar;
        public TMP_Text Title;
        public TMP_Text Progress;
        public TMP_Text Description;
        public TMP_Text Requirement;
        public Image UnlocksPanel;
        public TMP_Text UnlocksHeader;
        public UnlockEntryView[] UnlockEntries;
        public TMP_Text UnlocksEmptyText;
    }

    private sealed class UnlockEntryView
    {
        public GameObject Root;
        public Image Frame;
        public Image Tile;
        public Image Highlight;
        public UnlockTooltipHover Hover;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _panelImage = panel != null ? panel.GetComponent<Image>() : null;
        if (_panelImage != null)
            _defaultPanelColor = _panelImage.color;

        if (titleText != null)
            _defaultTitleColor = titleText.color;

        if (panel != null)
            panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool ShowChoices(PlayerExperience experience, List<PlayerUpgradeOption> choices)
    {
        if (experience == null)
            return false;

        return ShowCustomChoices(
            choices,
            chosen => experience.SelectUpgrade(chosen),
            "Choose an upgrade");
    }

    public bool ShowCustomChoices(
        List<PlayerUpgradeOption> choices,
        Action<PlayerUpgradeOption> onChosen,
        string title,
        Color? titleColor = null,
        Color? panelColor = null)
    {
        if (choices == null || choices.Count == 0)
            return false;

        if (panel == null || choiceButtons == null || choiceButtons.Length == 0)
            return false;

        _activeChoices = choices;
        _activeSelectionHandler = onChosen;
        _selectedIndex = -1;

        if (!_isShowing)
            _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;

        _isShowing = true;
        Time.timeScale = 0f;

        panel.SetActive(true);
        ApplyPopupLayout(choices.Count, panelColor, titleColor);
        EnsureChooseButton();

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Choose an upgrade" : title;
            titleText.color = titleColor ?? ChoiceTextColor;
        }

        if (_panelImage != null)
            _panelImage.color = panelColor ?? DefaultPanelColor;

        _cardStates.Clear();
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            bool hasChoice = choiceIndex < choices.Count;

            choiceButtons[i].gameObject.SetActive(hasChoice);
            choiceButtons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
                continue;

            choiceButtons[i].onClick.AddListener(() => SelectIndex(choiceIndex));

            TMP_Text label = GetChoiceLabel(choiceIndex);
            ChoiceCardView cardView = ConfigureChoiceButton(choiceButtons[i], label, choices[choiceIndex], choiceIndex, choices.Count);

            CardSelectionState state = new CardSelectionState
            {
                Outline = choiceButtons[i].GetComponent<Outline>(),
                AccentBar = cardView != null ? cardView.AccentBar : null,
                Background = choiceButtons[i].GetComponent<Image>(),
                BaseAccent = GetChoiceAccent(choices[choiceIndex]),
                BaseBackgroundColor = ChoiceCardColor
            };
            _cardStates.Add(state);
        }

        UpdateSelectionVisuals();
        return true;
    }

    private void SelectIndex(int index)
    {
        if (_activeChoices == null || index < 0 || index >= _activeChoices.Count)
            return;

        _selectedIndex = index;
        UpdateSelectionVisuals();
        GameAudio.PlayUISelect();
    }

    private void UpdateSelectionVisuals()
    {
        for (int i = 0; i < _cardStates.Count; i++)
        {
            CardSelectionState state = _cardStates[i];
            bool selected = i == _selectedIndex;

            if (state.Outline != null)
            {
                // Big contrast: unselected outline is barely there, selected is loud bright gold with thick shadow.
                state.Outline.effectColor = selected
                    ? new Color(1.00f, 0.86f, 0.36f, 1.00f)
                    : new Color(0.86f, 0.68f, 0.30f, 0.22f);
                state.Outline.effectDistance = selected
                    ? new Vector2(4f, -4f)
                    : new Vector2(2f, -2f);
            }

            if (state.AccentBar != null)
            {
                // Brighten the accent stripe on the selected card.
                Color baseAccent = state.BaseAccent;
                state.AccentBar.color = selected
                    ? new Color(
                        Mathf.Clamp01(baseAccent.r * 1.30f + 0.10f),
                        Mathf.Clamp01(baseAccent.g * 1.30f + 0.10f),
                        Mathf.Clamp01(baseAccent.b * 1.30f + 0.10f),
                        1f)
                    : new Color(baseAccent.r * 0.75f, baseAccent.g * 0.75f, baseAccent.b * 0.75f, 1f);

                RectTransform barRect = state.AccentBar.rectTransform;
                barRect.sizeDelta = new Vector2(selected ? 16f : 12f, 0f);
            }

            if (state.Background != null)
            {
                // Subtly tint selected card with the accent colour for a "lit up" feel.
                Color baseBg = state.BaseBackgroundColor;
                if (selected)
                {
                    Color tint = state.BaseAccent;
                    state.Background.color = new Color(
                        baseBg.r + tint.r * 0.10f,
                        baseBg.g + tint.g * 0.10f,
                        baseBg.b + tint.b * 0.10f,
                        baseBg.a);
                }
                else
                {
                    state.Background.color = baseBg;
                }
            }
        }

        if (_chooseButton != null)
        {
            bool ready = _selectedIndex >= 0;
            _chooseButton.interactable = ready;
            if (_chooseButtonImage != null)
                _chooseButtonImage.color = ready ? ChooseButtonActiveBody : ChooseButtonIdleBody;
            if (_chooseButtonLabel != null)
            {
                _chooseButtonLabel.color = ready ? ChooseButtonActiveText : ChooseButtonIdleText;
                _chooseButtonLabel.text = ready ? "CHOOSE" : "SELECT AN UPGRADE";
            }
            if (_chooseButtonOutline != null)
                _chooseButtonOutline.effectColor = ready ? ChooseButtonActiveOutline : ChooseButtonIdleOutline;
        }
    }

    private void EnsureChooseButton()
    {
        if (panel == null) return;
        if (_chooseButton != null) return;

        GameObject buttonGO = new GameObject("ChooseButton");
        buttonGO.transform.SetParent(panel.transform, false);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 24f);
        rect.sizeDelta = new Vector2(360f, ChooseButtonHeight);

        _chooseButtonImage = buttonGO.AddComponent<Image>();
        _chooseButtonImage.color = ChooseButtonIdleBody;

        _chooseButtonOutline = buttonGO.AddComponent<Outline>();
        _chooseButtonOutline.effectColor = ChooseButtonIdleOutline;
        _chooseButtonOutline.effectDistance = new Vector2(2f, -2f);

        _chooseButton = buttonGO.AddComponent<Button>();
        _chooseButton.targetGraphic = _chooseButtonImage;
        ColorBlock cb = _chooseButton.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.05f, 0.95f, 1f);
        cb.pressedColor = new Color(0.78f, 0.72f, 0.62f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        cb.fadeDuration = 0.06f;
        _chooseButton.colors = cb;
        _chooseButton.onClick.AddListener(ConfirmSelection);

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(buttonGO.transform, false);
        _chooseButtonLabel = labelGO.AddComponent<TextMeshProUGUI>();
        _chooseButtonLabel.text = "SELECT AN UPGRADE";
        _chooseButtonLabel.fontSize = 22f;
        _chooseButtonLabel.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        _chooseButtonLabel.characterSpacing = 4f;
        _chooseButtonLabel.alignment = TextAlignmentOptions.Center;
        _chooseButtonLabel.color = ChooseButtonIdleText;
        _chooseButtonLabel.raycastTarget = false;
        RectTransform labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private void ConfirmSelection()
    {
        if (_selectedIndex < 0)
            return;
        Choose(_selectedIndex);
    }

    private void ApplyPopupLayout(int choiceCount, Color? panelColor, Color? titleColor)
    {
        choiceCount = Mathf.Max(1, choiceCount);
        // Panel height = title zone + cards block + bottom Choose-button zone
        float cardsBlock = choiceCount * ChoiceCardHeight + (choiceCount - 1) * ChoiceCardSpacing;
        float panelHeight = Mathf.Max(560f, 184f + cardsBlock + ChooseButtonHeight + 48f);

        RectTransform panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(PanelWidth, panelHeight);
        }

        if (_panelImage != null)
            _panelImage.color = panelColor ?? DefaultPanelColor;

        Outline panelOutline = panel != null ? panel.GetComponent<Outline>() : null;
        if (panel != null && panelOutline == null)
            panelOutline = panel.AddComponent<Outline>();

        if (panelOutline != null)
        {
            panelOutline.effectColor = DefaultPanelOutlineColor;
            panelOutline.effectDistance = new Vector2(2f, -2f);
        }

        if (titleText != null)
        {
            titleText.fontSize = 38f;
            titleText.color = titleColor ?? _defaultTitleColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            titleText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            titleText.characterSpacing = 4f;
            SetRect(titleText.rectTransform, new Vector2(0f, panelHeight * 0.5f - 60f), new Vector2(PanelWidth - 64f, 56f));
        }
    }

    private ChoiceCardView ConfigureChoiceButton(Button button, TMP_Text titleLabel, PlayerUpgradeOption choice, int index, int totalChoices)
    {
        if (button == null || choice == null)
            return null;

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            float blockHeight = totalChoices * ChoiceCardHeight + (totalChoices - 1) * ChoiceCardSpacing;
            // Cards centred in the zone between title and Choose button — vertically symmetric.
            float firstY = blockHeight * 0.5f - ChoiceCardHeight * 0.5f;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, firstY - index * (ChoiceCardHeight + ChoiceCardSpacing));
            rect.sizeDelta = new Vector2(ChoiceCardWidth, ChoiceCardHeight);
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
            image.color = ChoiceCardColor;

        ColorBlock colors = button.colors;
        colors.normalColor = ChoiceCardColor;
        colors.highlightedColor = ChoiceCardHighlightColor;
        colors.pressedColor = ChoiceCardPressedColor;
        colors.selectedColor = ChoiceCardHighlightColor;
        colors.disabledColor = new Color(0.06f, 0.06f, 0.06f, 0.9f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
            outline = button.gameObject.AddComponent<Outline>();

        outline.effectColor = new Color(0.86f, 0.68f, 0.30f, 0.65f);
        outline.effectDistance = new Vector2(2f, -2f);

        ChoiceCardView view = GetOrCreateChoiceCardView(button.transform, titleLabel);
        Color accent = GetChoiceAccent(choice);

        if (view.AccentBar != null)
        {
            view.AccentBar.color = accent;
            RectTransform barRect = view.AccentBar.rectTransform;
            barRect.sizeDelta = new Vector2(12f, 0f);
        }

        string effectText;
        string requirementText;
        SplitChoiceDescription(choice.DisplayDescription, out effectText, out requirementText);

        // Layout zones (card spans -410 to +410):
        //   Left content:  -395 to +145  (width 540, center -125) — title + progress + description + requirement
        //   Right unlocks: +156 to +396  (width 240, center +276) — UnlocksPanel + entries
        // Progress sits at the right edge of the left zone and must NOT enter the unlocks panel.
        float leftCenterX = -125f;
        float leftAreaWidth = 540f;

        ConfigureText(view.Title, choice.Title, 25f, ChoiceTextColor, TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);
        ConfigureText(view.Progress, BuildProgressText(choice), 15f, accent, TextAlignmentOptions.Right, TextOverflowModes.Ellipsis);
        ConfigureText(view.Description, effectText, 18f, ChoiceMutedTextColor, TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);
        ConfigureText(view.Requirement, requirementText, 13f, ChoiceGoldTextColor, TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);

        SetRect(view.Title.rectTransform, new Vector2(-220f, 42f), new Vector2(330f, 30f));
        SetRect(view.Progress.rectTransform, new Vector2(60f, 42f), new Vector2(120f, 24f));
        SetRect(view.Description.rectTransform, new Vector2(leftCenterX, 4f), new Vector2(leftAreaWidth - 40f, 36f));
        SetRect(view.Requirement.rectTransform, new Vector2(leftCenterX, -36f), new Vector2(leftAreaWidth - 40f, 22f));

        UnlockPreviewEntry[] unlocks = TalentCatalog.BuildUnlockPreview(choice);
        ConfigureUnlocksPanel(view, accent, unlocks);
        return view;
    }

    private void ConfigureUnlocksPanel(ChoiceCardView view, Color accent, UnlockPreviewEntry[] unlocks)
    {
        if (view.UnlocksPanel == null)
            return;

        // Tooltip lives at the top-level panel so it can render above the cards.
        UnlockTooltip.Ensure(panel.transform);

        Color frameColor = new Color(0.04f, 0.03f, 0.04f, 0.85f);
        view.UnlocksPanel.color = frameColor;

        float panelX = (ChoiceCardWidth * 0.5f) - (UnlockPanelWidth * 0.5f) - 14f;
        SetRect(view.UnlocksPanel.rectTransform, new Vector2(panelX, 0f), new Vector2(UnlockPanelWidth, ChoiceCardHeight - 18f));

        Outline outline = view.UnlocksPanel.GetComponent<Outline>();
        if (outline == null)
            outline = view.UnlocksPanel.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.65f);
        outline.effectDistance = new Vector2(2f, -2f);

        if (view.UnlocksHeader != null)
        {
            ConfigureText(view.UnlocksHeader, "UNLOCKS NEXT", 11f, accent, TextAlignmentOptions.Center, TextOverflowModes.Ellipsis);
            view.UnlocksHeader.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            view.UnlocksHeader.characterSpacing = 4f;
            SetRect(view.UnlocksHeader.rectTransform, new Vector2(panelX, ChoiceCardHeight * 0.5f - 22f), new Vector2(UnlockPanelWidth - 32f, 16f));
        }

        bool hasUnlocks = unlocks != null && unlocks.Length > 0;
        int shownCount = hasUnlocks ? Mathf.Min(unlocks.Length, MaxUnlockEntriesShown) : 0;

        // Icon tiles laid out horizontally in a single row, centred within the panel.
        float totalRowWidth = shownCount * UnlockIconTileSize + Mathf.Max(0, shownCount - 1) * UnlockIconTileSpacing;
        float startLocalX = -totalRowWidth * 0.5f + UnlockIconTileSize * 0.5f;

        for (int i = 0; i < view.UnlockEntries.Length; i++)
        {
            UnlockEntryView entry = view.UnlockEntries[i];
            if (entry == null || entry.Root == null) continue;

            bool show = hasUnlocks && i < shownCount;
            entry.Root.SetActive(show);
            if (!show) continue;

            UnlockPreviewEntry data = unlocks[i];

            float xLocal = startLocalX + i * (UnlockIconTileSize + UnlockIconTileSpacing);
            // The Frame Image lives on entry.Root (same RectTransform). Position the entry root once.
            // Do NOT call SetRect on entry.Frame.rectTransform — it's the same object and would clobber this.
            SetRect(entry.Root.GetComponent<RectTransform>(), new Vector2(panelX + xLocal, -6f), new Vector2(UnlockIconTileSize, UnlockIconTileSize));

            if (entry.Frame != null)
            {
                // Frame: deep accent backdrop, the dark "ring" of the coin.
                entry.Frame.color = new Color(data.AccentColor.r * 0.30f, data.AccentColor.g * 0.30f, data.AccentColor.b * 0.30f, 0.98f);

                Outline frameOutline = entry.Frame.GetComponent<Outline>();
                if (frameOutline == null)
                    frameOutline = entry.Frame.gameObject.AddComponent<Outline>();
                frameOutline.effectColor = new Color(data.AccentColor.r, data.AccentColor.g, data.AccentColor.b, 0.95f);
                frameOutline.effectDistance = new Vector2(2f, -2f);
            }

            if (entry.Tile != null)
            {
                // Inner tile: bright saturated accent — the coin face.
                entry.Tile.color = new Color(data.AccentColor.r * 0.78f, data.AccentColor.g * 0.78f, data.AccentColor.b * 0.78f, 0.98f);
                SetRect(entry.Tile.rectTransform, Vector2.zero, new Vector2(UnlockIconTileSize - 14f, UnlockIconTileSize - 14f));
            }

            if (entry.Highlight != null)
            {
                // Tiny bright sheen offset toward upper-left, lifting the tile from "flat circle" to "gem".
                Color sheen = new Color(
                    Mathf.Clamp01(data.AccentColor.r * 1.6f + 0.20f),
                    Mathf.Clamp01(data.AccentColor.g * 1.6f + 0.20f),
                    Mathf.Clamp01(data.AccentColor.b * 1.6f + 0.20f),
                    0.55f);
                entry.Highlight.color = sheen;
                float highlightDiameter = UnlockIconTileSize * 0.32f;
                float highlightOffset = UnlockIconTileSize * 0.18f;
                SetRect(entry.Highlight.rectTransform, new Vector2(-highlightOffset, highlightOffset), new Vector2(highlightDiameter, highlightDiameter));
            }

            if (entry.Hover != null)
                entry.Hover.Configure(data.Title, data.ShortEffect, data.AccentColor);
        }

        if (view.UnlocksEmptyText != null)
        {
            view.UnlocksEmptyText.gameObject.SetActive(!hasUnlocks);
            if (!hasUnlocks)
            {
                ConfigureText(view.UnlocksEmptyText, "Direct boost\nno follow-ups", 11f, ChoiceMutedTextColor, TextAlignmentOptions.Center, TextOverflowModes.Ellipsis);
                view.UnlocksEmptyText.fontStyle = FontStyles.Italic;
                SetRect(view.UnlocksEmptyText.rectTransform, new Vector2(panelX, -4f), new Vector2(UnlockPanelWidth - 28f, 36f));
            }
        }
    }

    private ChoiceCardView GetOrCreateChoiceCardView(Transform parent, TMP_Text titleFallback)
    {
        ChoiceCardView view = new ChoiceCardView();
        view.AccentBar = GetOrCreateAccentBar(parent);
        view.Title = GetOrCreateText(parent, "ChoiceTitle", titleFallback);
        view.Progress = GetOrCreateText(parent, "ChoiceProgress", null);
        view.Description = GetOrCreateText(parent, "ChoiceDescription", null);
        view.Requirement = GetOrCreateText(parent, "ChoiceRequirement", null);
        view.UnlocksPanel = GetOrCreateUnlocksPanel(parent);
        view.UnlocksHeader = GetOrCreateText(parent, "UnlocksHeader", null);
        view.UnlocksEmptyText = GetOrCreateText(parent, "UnlocksEmpty", null);
        view.UnlockEntries = GetOrCreateUnlockEntries(parent);
        return view;
    }

    private Image GetOrCreateUnlocksPanel(Transform parent)
    {
        Transform existing = parent.Find("UnlocksPanel");
        GameObject panelObject = existing != null ? existing.gameObject : new GameObject("UnlocksPanel");
        panelObject.transform.SetParent(parent, false);
        panelObject.transform.SetAsFirstSibling();

        Image image = panelObject.GetComponent<Image>();
        if (image == null)
            image = panelObject.AddComponent<Image>();
        image.raycastTarget = false;

        return image;
    }

    private UnlockEntryView[] GetOrCreateUnlockEntries(Transform parent)
    {
        UnlockEntryView[] entries = new UnlockEntryView[MaxUnlockEntriesShown];
        for (int i = 0; i < MaxUnlockEntriesShown; i++)
        {
            string entryName = $"UnlockEntry_{i}";
            Transform existing = parent.Find(entryName);
            GameObject entryRoot = existing != null ? existing.gameObject : new GameObject(entryName);
            entryRoot.transform.SetParent(parent, false);

            RectTransform rt = entryRoot.GetComponent<RectTransform>();
            if (rt == null)
                rt = entryRoot.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);

            // Outer dark frame (the tile background) — circular coin shape.
            Image frame = entryRoot.GetComponent<Image>();
            if (frame == null)
                frame = entryRoot.AddComponent<Image>();
            frame.sprite = PickupSpriteFactory.CircleSprite;
            frame.raycastTarget = true;
            frame.preserveAspect = true;

            // Inner tinted tile — smaller circle inside the frame ring.
            Transform tileChild = entryRoot.transform.Find("Tile");
            GameObject tileObject = tileChild != null ? tileChild.gameObject : new GameObject("Tile");
            tileObject.transform.SetParent(entryRoot.transform, false);
            Image tile = tileObject.GetComponent<Image>();
            if (tile == null)
                tile = tileObject.AddComponent<Image>();
            tile.sprite = PickupSpriteFactory.CircleSprite;
            tile.raycastTarget = false;
            tile.preserveAspect = true;

            // Inner highlight — a small bright dot offset toward upper-left for a "gem" sheen.
            Transform highlightChild = entryRoot.transform.Find("Highlight");
            GameObject highlightObject = highlightChild != null ? highlightChild.gameObject : new GameObject("Highlight");
            highlightObject.transform.SetParent(entryRoot.transform, false);
            Image highlight = highlightObject.GetComponent<Image>();
            if (highlight == null)
                highlight = highlightObject.AddComponent<Image>();
            highlight.sprite = PickupSpriteFactory.CircleSprite;
            highlight.raycastTarget = false;
            highlight.preserveAspect = true;

            // Remove any leftover glyph child from earlier code versions.
            Transform legacyGlyph = entryRoot.transform.Find("Glyph");
            if (legacyGlyph != null)
                Destroy(legacyGlyph.gameObject);

            // Hover handler — drives the shared tooltip.
            UnlockTooltipHover hover = entryRoot.GetComponent<UnlockTooltipHover>();
            if (hover == null)
                hover = entryRoot.AddComponent<UnlockTooltipHover>();

            entries[i] = new UnlockEntryView
            {
                Root = entryRoot,
                Frame = frame,
                Tile = tile,
                Highlight = highlight,
                Hover = hover
            };
        }
        return entries;
    }

    private Image GetOrCreateAccentBar(Transform parent)
    {
        Transform existing = parent.Find("AccentBar");
        GameObject barObject = existing != null ? existing.gameObject : new GameObject("AccentBar");
        barObject.transform.SetParent(parent, false);

        Image image = barObject.GetComponent<Image>();
        if (image == null)
            image = barObject.AddComponent<Image>();

        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(9f, 0f);

        return image;
    }

    private TMP_Text GetOrCreateText(Transform parent, string name, TMP_Text fallback)
    {
        if (fallback != null)
            return fallback;

        Transform existing = parent.Find(name);
        if (existing != null)
        {
            TMP_Text existingText = existing.GetComponent<TMP_Text>();
            if (existingText != null)
                return existingText;
        }

        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        return textObject.AddComponent<TextMeshProUGUI>();
    }

    private static void ConfigureText(
        TMP_Text text,
        string value,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        TextOverflowModes overflowMode)
    {
        if (text == null)
            return;

        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.overflowMode = overflowMode;
        text.enableAutoSizing = true;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Max(11f, fontSize - 5f);
        text.raycastTarget = false;
    }

    private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
    {
        if (rect == null)
            return;

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void SplitChoiceDescription(string description, out string effectText, out string requirementText)
    {
        effectText = string.IsNullOrWhiteSpace(description) ? "Gain a useful upgrade." : description.Trim();
        requirementText = string.Empty;

        string[] lines = effectText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
            return;

        string lastLine = lines[lines.Length - 1].Trim();
        if (lastLine.StartsWith("Root", StringComparison.OrdinalIgnoreCase) ||
            lastLine.StartsWith("Requires", StringComparison.OrdinalIgnoreCase) ||
            lastLine.StartsWith("Locked", StringComparison.OrdinalIgnoreCase) ||
            lastLine.StartsWith("Maxed", StringComparison.OrdinalIgnoreCase))
        {
            requirementText = lastLine;
            effectText = string.Join(" ", lines, 0, lines.Length - 1).Trim();
        }
    }

    private static string BuildProgressText(PlayerUpgradeOption choice)
    {
        if (choice == null)
            return string.Empty;

        if (choice.IsRunTalent && choice.RunTalentMaxPoints > 0)
            return $"Rank {choice.RunTalentPointsAfter}/{choice.RunTalentMaxPoints}";

        return "Upgrade";
    }

    private static Color GetChoiceAccent(PlayerUpgradeOption choice)
    {
        if (choice == null)
            return DefaultPanelOutlineColor;

        if (!string.IsNullOrWhiteSpace(choice.RunTalentId))
        {
            if (choice.RunTalentId.StartsWith("atk_", StringComparison.OrdinalIgnoreCase))
                return AttackAccentColor;

            if (choice.RunTalentId.StartsWith("def_", StringComparison.OrdinalIgnoreCase))
                return DefenseAccentColor;
        }

        return RewardAccentColor;
    }

    private TMP_Text GetChoiceLabel(int index)
    {
        if (choiceTexts != null && index < choiceTexts.Length && choiceTexts[index] != null)
            return choiceTexts[index];

        if (choiceButtons != null && index < choiceButtons.Length && choiceButtons[index] != null)
            return choiceButtons[index].GetComponentInChildren<TMP_Text>();

        return null;
    }

    private void Choose(int index)
    {
        if (_activeChoices == null || index >= _activeChoices.Count)
            return;

        PlayerUpgradeOption chosen = _activeChoices[index];
        Action<PlayerUpgradeOption> selectionHandler = _activeSelectionHandler;

        Close();
        GameAudio.PlayUISelect();
        selectionHandler?.Invoke(chosen);
    }

    private void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        if (titleText != null)
            titleText.color = _defaultTitleColor;

        if (_panelImage != null)
            _panelImage.color = _defaultPanelColor;

        Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
        _activeChoices = null;
        _activeSelectionHandler = null;
        _isShowing = false;
    }
}
