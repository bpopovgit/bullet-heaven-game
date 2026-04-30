using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    private const float PanelWidth = 720f;
    private const float ChoiceCardWidth = 580f;
    private const float ChoiceCardHeight = 104f;
    private const float ChoiceCardSpacing = 14f;

    private static readonly Color DefaultPanelColor = new Color(0.02f, 0.18f, 0.08f, 0.96f);
    private static readonly Color DefaultPanelOutlineColor = new Color(0.86f, 0.68f, 0.26f, 0.95f);
    private static readonly Color ChoiceCardColor = new Color(0.04f, 0.08f, 0.06f, 0.98f);
    private static readonly Color ChoiceCardHighlightColor = new Color(0.08f, 0.23f, 0.11f, 1f);
    private static readonly Color ChoiceCardPressedColor = new Color(0.12f, 0.34f, 0.15f, 1f);
    private static readonly Color ChoiceTextColor = new Color(0.92f, 0.94f, 0.86f, 1f);
    private static readonly Color ChoiceMutedTextColor = new Color(0.70f, 0.80f, 0.72f, 1f);
    private static readonly Color ChoiceGoldTextColor = new Color(1f, 0.83f, 0.28f, 1f);
    private static readonly Color AttackAccentColor = new Color(1f, 0.58f, 0.16f, 1f);
    private static readonly Color DefenseAccentColor = new Color(0.28f, 0.9f, 0.55f, 1f);
    private static readonly Color RewardAccentColor = new Color(0.92f, 0.32f, 0.16f, 1f);

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

    private sealed class ChoiceCardView
    {
        public Image AccentBar;
        public TMP_Text Title;
        public TMP_Text Progress;
        public TMP_Text Description;
        public TMP_Text Requirement;
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

        if (!_isShowing)
            _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;

        _isShowing = true;
        Time.timeScale = 0f;

        panel.SetActive(true);
        ApplyPopupLayout(choices.Count, panelColor, titleColor);

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Choose an upgrade" : title;
            titleText.color = titleColor ?? ChoiceTextColor;
        }

        if (_panelImage != null)
            _panelImage.color = panelColor ?? DefaultPanelColor;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choiceIndex = i;
            bool hasChoice = choiceIndex < choices.Count;

            choiceButtons[i].gameObject.SetActive(hasChoice);
            choiceButtons[i].onClick.RemoveAllListeners();

            if (!hasChoice)
                continue;

            choiceButtons[i].onClick.AddListener(() => Choose(choiceIndex));

            TMP_Text label = GetChoiceLabel(choiceIndex);
            ConfigureChoiceButton(choiceButtons[i], label, choices[choiceIndex], choiceIndex, choices.Count);
        }

        return true;
    }

    private void ApplyPopupLayout(int choiceCount, Color? panelColor, Color? titleColor)
    {
        choiceCount = Mathf.Max(1, choiceCount);
        float panelHeight = Mathf.Max(430f, 164f + choiceCount * ChoiceCardHeight + (choiceCount - 1) * ChoiceCardSpacing);

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
            panelOutline.effectDistance = new Vector2(3f, -3f);
        }

        if (titleText != null)
        {
            titleText.fontSize = 34f;
            titleText.color = titleColor ?? _defaultTitleColor;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            SetRect(titleText.rectTransform, new Vector2(0f, panelHeight * 0.5f - 56f), new Vector2(PanelWidth - 64f, 54f));
        }
    }

    private void ConfigureChoiceButton(Button button, TMP_Text titleLabel, PlayerUpgradeOption choice, int index, int totalChoices)
    {
        if (button == null || choice == null)
            return;

        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null)
        {
            float blockHeight = totalChoices * ChoiceCardHeight + (totalChoices - 1) * ChoiceCardSpacing;
            float firstY = blockHeight * 0.5f - ChoiceCardHeight * 0.5f - 46f;

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

        outline.effectColor = new Color(0.7f, 0.55f, 0.18f, 0.75f);
        outline.effectDistance = new Vector2(2f, -2f);

        ChoiceCardView view = GetOrCreateChoiceCardView(button.transform, titleLabel);
        Color accent = GetChoiceAccent(choice);

        if (view.AccentBar != null)
            view.AccentBar.color = accent;

        string effectText;
        string requirementText;
        SplitChoiceDescription(choice.DisplayDescription, out effectText, out requirementText);

        ConfigureText(view.Title, choice.Title, 21f, ChoiceTextColor, TextAlignmentOptions.Left, TextOverflowModes.Ellipsis);
        ConfigureText(view.Progress, BuildProgressText(choice), 15f, accent, TextAlignmentOptions.Center, TextOverflowModes.Ellipsis);
        ConfigureText(view.Description, effectText, 16f, ChoiceMutedTextColor, TextAlignmentOptions.Center, TextOverflowModes.Ellipsis);
        ConfigureText(view.Requirement, requirementText, 14f, ChoiceGoldTextColor, TextAlignmentOptions.Center, TextOverflowModes.Ellipsis);

        SetRect(view.Title.rectTransform, new Vector2(-98f, 28f), new Vector2(330f, 28f));
        SetRect(view.Progress.rectTransform, new Vector2(210f, 28f), new Vector2(118f, 24f));
        SetRect(view.Description.rectTransform, new Vector2(14f, -6f), new Vector2(500f, 34f));
        SetRect(view.Requirement.rectTransform, new Vector2(14f, -36f), new Vector2(500f, 22f));
    }

    private ChoiceCardView GetOrCreateChoiceCardView(Transform parent, TMP_Text titleFallback)
    {
        ChoiceCardView view = new ChoiceCardView();
        view.AccentBar = GetOrCreateAccentBar(parent);
        view.Title = GetOrCreateText(parent, "ChoiceTitle", titleFallback);
        view.Progress = GetOrCreateText(parent, "ChoiceProgress", null);
        view.Description = GetOrCreateText(parent, "ChoiceDescription", null);
        view.Requirement = GetOrCreateText(parent, "ChoiceRequirement", null);
        return view;
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
