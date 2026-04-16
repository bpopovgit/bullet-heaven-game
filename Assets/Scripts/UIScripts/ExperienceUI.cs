using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExperienceUI : MonoBehaviour
{
    [SerializeField] private PlayerExperience playerExperience;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text experienceText;
    [SerializeField] private Slider experienceSlider;

    private void OnEnable()
    {
        FindPlayerExperienceIfNeeded();

        if (playerExperience != null)
            playerExperience.ExperienceChanged += HandleExperienceChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerExperience != null)
            playerExperience.ExperienceChanged -= HandleExperienceChanged;
    }

    private void Start()
    {
        Refresh();
    }

    private void FindPlayerExperienceIfNeeded()
    {
        if (playerExperience != null)
            return;

        playerExperience = FindObjectOfType<PlayerExperience>();
    }

    private void Refresh()
    {
        if (playerExperience == null)
            return;

        HandleExperienceChanged(
            playerExperience.CurrentLevel,
            playerExperience.CurrentExperience,
            playerExperience.ExperienceToNextLevel);
    }

    private void HandleExperienceChanged(int level, int currentXp, int xpToNext)
    {
        if (levelText != null)
            levelText.text = $"Level {level}";

        if (experienceText != null)
            experienceText.text = $"XP {currentXp}/{xpToNext}";

        if (experienceSlider != null)
        {
            experienceSlider.minValue = 0f;
            experienceSlider.maxValue = Mathf.Max(1, xpToNext);
            experienceSlider.value = currentXp;
        }
    }
}
