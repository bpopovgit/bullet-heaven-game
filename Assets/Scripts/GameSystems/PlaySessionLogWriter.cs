using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaySessionLogWriter : MonoBehaviour
{
    private const string LogFolderName = "SessionLogs";

    private static PlaySessionLogWriter _instance;

    private StreamWriter _writer;
    private string _sessionLogPath;
    private bool _isSubscribed;

    public static string CurrentLogPath => _instance != null ? _instance._sessionLogPath : string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("PlaySessionLogWriter");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<PlaySessionLogWriter>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        OpenLogFile();
        Subscribe();
        WriteLine("=== SESSION START ===");
        WriteLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        WriteLine($"Unity Version: {Application.unityVersion}");
        WriteLine($"Persistent Data Path: {Application.persistentDataPath}");
        WriteLine($"Scene: {SceneManager.GetActiveScene().name}");
        WriteLine($"Log Path: {_sessionLogPath}");
        Debug.Log($"Play session log writer active: {_sessionLogPath}", this);
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        FlushWriter();
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;

        Unsubscribe();
        WriteLine("=== SESSION END ===");
        CloseWriter();
    }

    private void OnApplicationQuit()
    {
        WriteLine("=== APPLICATION QUIT ===");
        CloseWriter();
    }

    private void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (_writer == null)
            return;

        string time = DateTime.Now.ToString("HH:mm:ss.fff");
        _writer.WriteLine($"[{time}] [{type}] {condition}");

        if (!string.IsNullOrWhiteSpace(stackTrace) &&
            (type == LogType.Error || type == LogType.Assert || type == LogType.Exception))
        {
            _writer.WriteLine(stackTrace);
        }

        _writer.Flush();
    }

    private void OpenLogFile()
    {
        string logDirectory = Path.Combine(Application.persistentDataPath, LogFolderName);
        Directory.CreateDirectory(logDirectory);

        string fileName = $"play_session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        _sessionLogPath = Path.Combine(logDirectory, fileName);
        _writer = new StreamWriter(_sessionLogPath, append: false);
        _writer.AutoFlush = true;
    }

    private void Subscribe()
    {
        if (_isSubscribed)
            return;

        Application.logMessageReceived += HandleLog;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        _isSubscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_isSubscribed)
            return;

        Application.logMessageReceived -= HandleLog;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        _isSubscribed = false;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WriteLine($"Scene Loaded: {scene.name} ({mode})");
    }

    private void WriteLine(string message)
    {
        if (_writer == null)
            return;

        _writer.WriteLine(message);
        _writer.Flush();
    }

    private void FlushWriter()
    {
        if (_writer == null)
            return;

        _writer.Flush();
    }

    private void CloseWriter()
    {
        if (_writer == null)
            return;

        _writer.Flush();
        _writer.Dispose();
        _writer = null;
    }
}
