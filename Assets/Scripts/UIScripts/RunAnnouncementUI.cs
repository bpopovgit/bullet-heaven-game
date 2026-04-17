using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class RunAnnouncementUI : MonoBehaviour
{
    public static RunAnnouncementUI Instance { get; private set; }

    [SerializeField] private float defaultDuration = 2f;
    [SerializeField] private bool clearOnAwake = true;

    private TMP_Text _messageText;
    private Coroutine _messageRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _messageText = GetComponent<TMP_Text>();
        _messageText.raycastTarget = false;

        if (clearOnAwake)
            _messageText.text = string.Empty;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowMessage(string message)
    {
        ShowMessage(message, defaultDuration);
    }

    public void ShowMessage(string message, float duration)
    {
        if (_messageRoutine != null)
            StopCoroutine(_messageRoutine);

        _messageRoutine = StartCoroutine(MessageRoutine(message, duration));
    }

    private IEnumerator MessageRoutine(string message, float duration)
    {
        _messageText.text = message;

        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, duration));

        _messageText.text = string.Empty;
        _messageRoutine = null;
    }
}
