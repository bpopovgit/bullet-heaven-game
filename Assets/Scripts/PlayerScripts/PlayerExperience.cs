using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerExperience : MonoBehaviour
{
    [Header("Leveling")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int baseExperienceToNextLevel = 10;
    [SerializeField] private float levelExperienceGrowth = 1.25f;
    [SerializeField] private int choicesPerLevel = 3;

    [Header("Upgrade Pool")]
    [SerializeField] private List<PlayerUpgradeOption> upgradePool = new List<PlayerUpgradeOption>();

    private int _pendingLevelUps;
    private bool _waitingForChoice;

    public event Action<int, int, int> ExperienceChanged;
    public event Action<int> LevelChanged;

    public int CurrentLevel => currentLevel;
    public int CurrentExperience => currentExperience;
    public int ExperienceToNextLevel => GetExperienceForLevel(currentLevel);

    private void Awake()
    {
        EnsureDefaultUpgradePool();
    }

    private void Start()
    {
        PublishProgress();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        currentExperience += amount;

        while (currentExperience >= ExperienceToNextLevel)
        {
            currentExperience -= ExperienceToNextLevel;
            currentLevel++;
            _pendingLevelUps++;
            LevelChanged?.Invoke(currentLevel);
            GameAudio.PlayLevelUp();
            Debug.Log($"LEVEL UP: {currentLevel}");
        }

        PublishProgress();

        if (_pendingLevelUps > 0 && !_waitingForChoice)
            RequestNextUpgrade();
    }

    public void SelectUpgrade(PlayerUpgradeOption upgrade)
    {
        if (upgrade != null)
            upgrade.Apply(gameObject);

        _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);
        _waitingForChoice = false;

        if (_pendingLevelUps > 0)
            RequestNextUpgrade();
    }

    private void RequestNextUpgrade()
    {
        _waitingForChoice = true;
        List<PlayerUpgradeOption> choices = PickUpgradeChoices();

        if (choices.Count == 0)
        {
            _waitingForChoice = false;
            _pendingLevelUps = Mathf.Max(0, _pendingLevelUps - 1);
            return;
        }

        if (LevelUpManager.Instance != null && LevelUpManager.Instance.ShowChoices(this, choices))
            return;

        PlayerUpgradeOption fallback = choices[0];
        Debug.Log($"No LevelUpManager found. Auto-picking upgrade: {fallback.Title}");
        SelectUpgrade(fallback);
    }

    private List<PlayerUpgradeOption> PickUpgradeChoices()
    {
        EnsureDefaultUpgradePool();

        PlayableCharacterChoice character = RunLoadoutState.CharacterChoice;
        List<PlayerUpgradeOption> available = BuildAvailableUpgradePool(character);
        List<PlayerUpgradeOption> choices = new List<PlayerUpgradeOption>();

        if (available.Count == 0)
            return choices;

        int count = Mathf.Clamp(choicesPerLevel, 1, available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(0, available.Count);
            choices.Add(available[index]);
            available.RemoveAt(index);
        }

        return choices;
    }

    private List<PlayerUpgradeOption> BuildAvailableUpgradePool(PlayableCharacterChoice character)
    {
        List<PlayerUpgradeOption> available = new List<PlayerUpgradeOption>();

        for (int i = 0; i < upgradePool.Count; i++)
        {
            PlayerUpgradeOption option = upgradePool[i];
            if (option != null && option.IsAvailableFor(character))
                AddIfMissingTitle(available, option);
        }

        PlayerUpgradeOption[] defaults = PlayerUpgradeOption.CreateDefaultPool();
        for (int i = 0; i < defaults.Length; i++)
        {
            PlayerUpgradeOption option = defaults[i];
            if (option != null && option.IsAvailableFor(character))
                AddIfMissingTitle(available, option);
        }

        return available;
    }

    private static void AddIfMissingTitle(List<PlayerUpgradeOption> options, PlayerUpgradeOption option)
    {
        for (int i = 0; i < options.Count; i++)
        {
            if (options[i] != null && options[i].Title == option.Title)
                return;
        }

        options.Add(option);
    }

    private int GetExperienceForLevel(int level)
    {
        int baseXp = Mathf.Max(1, baseExperienceToNextLevel);
        float growth = Mathf.Max(1f, levelExperienceGrowth);
        return Mathf.Max(1, Mathf.RoundToInt(baseXp * Mathf.Pow(growth, Mathf.Max(0, level - 1))));
    }

    private void EnsureDefaultUpgradePool()
    {
        if (upgradePool != null && upgradePool.Count > 0)
            return;

        upgradePool = new List<PlayerUpgradeOption>(PlayerUpgradeOption.CreateDefaultPool());
    }

    private void PublishProgress()
    {
        ExperienceChanged?.Invoke(currentLevel, currentExperience, ExperienceToNextLevel);
    }
}
