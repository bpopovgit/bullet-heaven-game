using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoldCounterUI : MonoBehaviour
{
    private const string GameSceneName = "Game";

    private static readonly Color FrameColor = new Color(0.06f, 0.05f, 0.07f, 0.86f);
    private static readonly Color OutlineColor = new Color(0.86f, 0.68f, 0.30f, 0.62f);
    private static readonly Color TitleColor = new Color(0.96f, 0.91f, 0.78f, 1f);
    private static readonly Color GoldAccent = new Color(0.96f, 0.78f, 0.22f, 1f);

    private TextMeshProUGUI _amountText;
    private int _lastDisplayed = -1;
    private float _pulseUntil;

    private static bool _sceneHookRegistered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_sceneHookRegistered)
            return;

        SceneManager.sceneLoaded += HandleSceneLoaded;
        _sceneHookRegistered = true;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.IsValid() || scene.name != GameSceneName)
            return;

        if (FindObjectOfType<GoldCounterUI>() != null)
            return;

        GameObject host = new GameObject("GoldCounterUI");
        host.AddComponent<GoldCounterUI>();
    }

    private void Awake()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        GameObject canvasGO = new GameObject("GoldCounterCanvas");
        canvasGO.transform.SetParent(transform, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject root = new GameObject("Root");
        root.transform.SetParent(canvasGO.transform, false);

        RectTransform rootRect = root.AddComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(1f, 1f);
        rootRect.anchorMax = new Vector2(1f, 1f);
        rootRect.pivot = new Vector2(1f, 1f);
        rootRect.anchoredPosition = new Vector2(-18f, -156f);
        rootRect.sizeDelta = new Vector2(220f, 56f);

        Image bg = root.AddComponent<Image>();
        bg.color = FrameColor;
        bg.raycastTarget = false;

        Outline outline = root.AddComponent<Outline>();
        outline.effectColor = OutlineColor;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject coin = new GameObject("Coin");
        coin.transform.SetParent(root.transform, false);
        Image coinImg = coin.AddComponent<Image>();
        coinImg.sprite = PickupSpriteFactory.CircleSprite;
        coinImg.color = GoldAccent;
        coinImg.raycastTarget = false;
        RectTransform coinRect = coin.GetComponent<RectTransform>();
        coinRect.anchorMin = new Vector2(0f, 0.5f);
        coinRect.anchorMax = new Vector2(0f, 0.5f);
        coinRect.pivot = new Vector2(0f, 0.5f);
        coinRect.anchoredPosition = new Vector2(14f, 0f);
        coinRect.sizeDelta = new Vector2(28f, 28f);

        GameObject label = new GameObject("Label");
        label.transform.SetParent(root.transform, false);
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "GOLD";
        labelText.fontSize = 13f;
        labelText.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        labelText.characterSpacing = 3f;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = OutlineColor;
        labelText.raycastTarget = false;
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(52f, 11f);
        labelRect.sizeDelta = new Vector2(80f, 18f);

        GameObject amount = new GameObject("Amount");
        amount.transform.SetParent(root.transform, false);
        _amountText = amount.AddComponent<TextMeshProUGUI>();
        _amountText.text = "0";
        _amountText.fontSize = 26f;
        _amountText.fontStyle = FontStyles.Bold;
        _amountText.alignment = TextAlignmentOptions.Left;
        _amountText.color = TitleColor;
        _amountText.raycastTarget = false;
        RectTransform amountRect = amount.GetComponent<RectTransform>();
        amountRect.anchorMin = new Vector2(0f, 0.5f);
        amountRect.anchorMax = new Vector2(0f, 0.5f);
        amountRect.pivot = new Vector2(0f, 0.5f);
        amountRect.anchoredPosition = new Vector2(52f, -8f);
        amountRect.sizeDelta = new Vector2(150f, 30f);
    }

    private void Update()
    {
        if (_amountText == null)
            return;

        int current = RunSession.Currency;
        if (current != _lastDisplayed)
        {
            _amountText.text = current.ToString();
            if (current > _lastDisplayed && _lastDisplayed >= 0)
                _pulseUntil = Time.time + 0.35f;
            _lastDisplayed = current;
        }

        bool pulsing = Time.time < _pulseUntil;
        _amountText.color = pulsing ? GoldAccent : TitleColor;
    }
}
