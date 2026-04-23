using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpManager : MonoBehaviour
{
    public static LevelUpManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button[] choiceButtons;
    [SerializeField] private TMP_Text[] choiceTexts;

    private PlayerExperience _activeExperience;
    private List<PlayerUpgradeOption> _activeChoices;
    private float _previousTimeScale = 1f;
    private bool _isShowing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

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
        if (experience == null || choices == null || choices.Count == 0)
            return false;

        if (panel == null || choiceButtons == null || choiceButtons.Length == 0)
            return false;

        _activeExperience = experience;
        _activeChoices = choices;

        if (!_isShowing)
            _previousTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;

        _isShowing = true;
        Time.timeScale = 0f;

        panel.SetActive(true);

        if (titleText != null)
            titleText.text = "Choose an upgrade";

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
        if (_activeExperience == null || _activeChoices == null || index >= _activeChoices.Count)
            return;

        PlayerExperience experience = _activeExperience;
        PlayerUpgradeOption chosen = _activeChoices[index];

        Close();
        GameAudio.PlayUISelect();
        experience.SelectUpgrade(chosen);
    }

    private void Close()
    {
        if (panel != null)
            panel.SetActive(false);

        Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
        _activeExperience = null;
        _activeChoices = null;
        _isShowing = false;
    }
}
