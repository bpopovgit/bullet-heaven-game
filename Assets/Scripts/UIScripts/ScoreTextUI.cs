using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class ScoreTextUI : MonoBehaviour
{
    [SerializeField] private string prefix = "Score: ";

    private TMP_Text _scoreText;
    private ScoreManager _subscribedManager;

    private void Awake()
    {
        _scoreText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        TrySubscribe();
        Refresh();
    }

    private void Update()
    {
        if (_subscribedManager != ScoreManager.Instance)
        {
            Unsubscribe();
            TrySubscribe();
            Refresh();
        }
    }

    private void TrySubscribe()
    {
        if (ScoreManager.Instance == null || _subscribedManager == ScoreManager.Instance)
            return;

        _subscribedManager = ScoreManager.Instance;
        _subscribedManager.ScoreChanged += HandleScoreChanged;
    }

    private void Unsubscribe()
    {
        if (_subscribedManager == null)
            return;

        _subscribedManager.ScoreChanged -= HandleScoreChanged;
        _subscribedManager = null;
    }

    private void Refresh()
    {
        int score = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        HandleScoreChanged(score);
    }

    private void HandleScoreChanged(int score)
    {
        _scoreText.text = $"{prefix}{score}";
    }
}
