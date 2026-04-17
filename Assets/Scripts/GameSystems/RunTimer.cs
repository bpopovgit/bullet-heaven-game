using System;
using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance { get; private set; }

    [Header("Timer")]
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private float elapsedSeconds = 0f;

    private int _lastWholeSecond = -1;

    public event Action<float, int> TimeChanged;
    public event Action<int> WholeSecondChanged;
    public event Action<int> MinuteChanged;
    public event Action<float> RunEnded;

    public bool IsRunning { get; private set; }
    public float ElapsedSeconds => elapsedSeconds;
    public int WholeSeconds => Mathf.FloorToInt(elapsedSeconds);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        PublishTime(force: true);

        if (startOnAwake)
            StartTimer();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!IsRunning)
            return;

        elapsedSeconds += Time.deltaTime;
        PublishTime(force: false);
    }

    public void StartTimer()
    {
        IsRunning = true;
        PublishTime(force: true);
    }

    public void StopTimer()
    {
        if (!IsRunning)
            return;

        IsRunning = false;
        PublishTime(force: true);
        RunEnded?.Invoke(elapsedSeconds);
    }

    public void ResetTimer(bool startAfterReset)
    {
        elapsedSeconds = 0f;
        _lastWholeSecond = -1;
        IsRunning = startAfterReset;
        PublishTime(force: true);
    }

    public static string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int secs = totalSeconds % 60;

        if (hours > 0)
            return $"{hours}:{minutes:00}:{secs:00}";

        return $"{minutes:00}:{secs:00}";
    }

    private void PublishTime(bool force)
    {
        int wholeSecond = WholeSeconds;

        if (!force && wholeSecond == _lastWholeSecond)
            return;

        bool minuteChanged = wholeSecond > 0 && wholeSecond % 60 == 0 && wholeSecond != _lastWholeSecond;
        _lastWholeSecond = wholeSecond;

        TimeChanged?.Invoke(elapsedSeconds, wholeSecond);
        WholeSecondChanged?.Invoke(wholeSecond);

        if (minuteChanged)
            MinuteChanged?.Invoke(wholeSecond / 60);
    }
}
