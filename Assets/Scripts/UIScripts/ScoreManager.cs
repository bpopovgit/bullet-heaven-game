using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] private int currentScore = 0;

    public event Action<int> ScoreChanged;

    public int CurrentScore => currentScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ScoreChanged?.Invoke(currentScore);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddScore(int amount)
    {
        if (amount <= 0)
            return;

        currentScore += amount;
        ScoreChanged?.Invoke(currentScore);

        Debug.Log($"SCORE: {currentScore}");
    }

    public void ResetScore()
    {
        currentScore = 0;
        ScoreChanged?.Invoke(currentScore);

        Debug.Log("SCORE RESET");
    }
}
