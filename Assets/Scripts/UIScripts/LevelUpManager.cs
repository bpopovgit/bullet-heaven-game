using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

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

        if (titleText != null)
        {
            titleText.text = string.IsNullOrWhiteSpace(title) ? "Choose an upgrade" : title;
            titleText.color = titleColor ?? _defaultTitleColor;
        }

        if (_panelImage != null)
            _panelImage.color = panelColor ?? _defaultPanelColor;

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
            if (label != null)
            {
                PlayerUpgradeOption choice = choices[choiceIndex];
                label.text = $"{choice.Title}\n{choice.Description}";
            }
        }

        return true;
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
