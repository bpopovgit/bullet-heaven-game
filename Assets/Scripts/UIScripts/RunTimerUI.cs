using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RunTimerUI : MonoBehaviour
{
    [SerializeField] private RunTimer runTimer;
    [SerializeField] private string prefix = "Time: ";

    private TMP_Text _timerText;
    private RunTimer _subscribedTimer;

    private void Awake()
    {
        _timerText = GetComponent<TMP_Text>();
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
        if (_subscribedTimer != GetActiveTimer())
        {
            Unsubscribe();
            TrySubscribe();
            Refresh();
        }
    }

    private void TrySubscribe()
    {
        RunTimer activeTimer = GetActiveTimer();

        if (activeTimer == null || _subscribedTimer == activeTimer)
            return;

        _subscribedTimer = activeTimer;
        _subscribedTimer.TimeChanged += HandleTimeChanged;
    }

    private void Unsubscribe()
    {
        if (_subscribedTimer == null)
            return;

        _subscribedTimer.TimeChanged -= HandleTimeChanged;
        _subscribedTimer = null;
    }

    private RunTimer GetActiveTimer()
    {
        if (runTimer != null)
            return runTimer;

        return RunTimer.Instance;
    }

    private void Refresh()
    {
        RunTimer activeTimer = GetActiveTimer();
        float seconds = activeTimer != null ? activeTimer.ElapsedSeconds : 0f;
        HandleTimeChanged(seconds, Mathf.FloorToInt(seconds));
    }

    private void HandleTimeChanged(float elapsedSeconds, int wholeSeconds)
    {
        _timerText.text = $"{prefix}{RunTimer.FormatTime(elapsedSeconds)}";
    }
}
