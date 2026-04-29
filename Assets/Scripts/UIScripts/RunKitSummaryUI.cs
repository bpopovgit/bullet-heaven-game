using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RunKitSummaryUI : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static RunKitSummaryUI _instance;

    private RectTransform _root;
    private TMP_Text _characterText;
    private TMP_Text _primaryText;
    private TMP_Text _abilitiesText;
    private Image _accentImage;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("RunKitSummaryUI");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<RunKitSummaryUI>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_root != null)
            _root.gameObject.SetActive(false);
    }

    private void Update()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid() || activeScene.name != GameplaySceneName)
            return;

        if (!EnsureUI())
            return;

        Refresh();
    }

    private bool EnsureUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return false;

        if (_root != null && _root.transform.parent == canvas.transform)
        {
            if (!_root.gameObject.activeSelf)
                _root.gameObject.SetActive(true);

            return true;
        }

        if (_root != null)
            Destroy(_root.gameObject);

        BuildUI(canvas.transform);
        return true;
    }

    private void BuildUI(Transform canvasTransform)
    {
        GameObject rootObject = new GameObject("RunKitSummaryRoot");
        rootObject.transform.SetParent(canvasTransform, false);

        _root = rootObject.AddComponent<RectTransform>();
        _root.anchorMin = new Vector2(1f, 1f);
        _root.anchorMax = new Vector2(1f, 1f);
        _root.pivot = new Vector2(1f, 1f);
        _root.anchoredPosition = new Vector2(-18f, -60f);
        _root.sizeDelta = new Vector2(430f, 86f);

        Image background = rootObject.AddComponent<Image>();
        background.color = new Color(0.03f, 0.05f, 0.07f, 0.78f);

        Outline outline = rootObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.85f, 0.72f, 0.32f, 0.45f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject accentObject = new GameObject("CharacterAccent");
        accentObject.transform.SetParent(_root, false);

        _accentImage = accentObject.AddComponent<Image>();

        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(8f, 0f);

        _characterText = CreateText(_root, "CharacterText", new Vector2(18f, -8f), new Vector2(392f, 24f), 18f, TextAlignmentOptions.TopLeft, new Color(0.96f, 0.9f, 0.74f, 1f));
        _primaryText = CreateText(_root, "PrimaryText", new Vector2(18f, -33f), new Vector2(392f, 22f), 15f, TextAlignmentOptions.TopLeft, new Color(0.82f, 0.92f, 0.86f, 1f));
        _abilitiesText = CreateText(_root, "AbilitiesText", new Vector2(18f, -57f), new Vector2(392f, 22f), 13f, TextAlignmentOptions.TopLeft, new Color(0.93f, 0.79f, 0.35f, 1f));

        Refresh();
    }

    private static TMP_Text CreateText(
        Transform parent,
        string name,
        Vector2 anchoredPosition,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.color = color;

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        return text;
    }

    private void Refresh()
    {
        PlayableCharacterChoice character = RunLoadoutState.CharacterChoice;
        StartingWeaponChoice weapon = RunLoadoutState.WeaponChoice;

        if (_accentImage != null)
            _accentImage.color = RunLoadoutState.GetCharacterTint(character);

        if (_characterText != null)
            _characterText.text = $"{RunLoadoutState.GetCharacterName(character)} - {RunLoadoutState.GetCharacterRole(character)}";

        if (_primaryText != null)
            _primaryText.text = $"{RunLoadoutState.GetPrimaryAttackCategory(character)}: {RunLoadoutState.GetPrimaryAttackName(character, weapon)}";

        if (_abilitiesText != null)
            _abilitiesText.text = $"Q: {RunLoadoutState.GetBombName(RunLoadoutState.BombChoice)}  |  E: {RunLoadoutState.GetSkillName(RunLoadoutState.SkillChoice)}";
    }
}
