using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BombCooldownUI : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static BombCooldownUI _instance;
    private static Sprite _bombIconSprite;

    private RectTransform _root;
    private Image _iconImage;
    private Image _cooldownOverlayImage;
    private TMP_Text _cooldownText;
    private TMP_Text _keyText;
    private TMP_Text _nameText;
    private PlayerActiveBomb _trackedBomb;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("BombCooldownUI");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<BombCooldownUI>();
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
        _trackedBomb = null;

        if (!scene.IsValid() || scene.name != GameplaySceneName)
        {
            if (_root != null)
                _root.gameObject.SetActive(false);

            return;
        }

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

        if (_trackedBomb == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _trackedBomb = player.GetComponent<PlayerActiveBomb>();
        }

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
        GameObject rootObject = new GameObject("BombCooldownRoot");
        rootObject.transform.SetParent(canvasTransform, false);

        _root = rootObject.AddComponent<RectTransform>();
        _root.anchorMin = new Vector2(0f, 0f);
        _root.anchorMax = new Vector2(0f, 0f);
        _root.pivot = new Vector2(0f, 0f);
        _root.anchoredPosition = new Vector2(18f, 18f);
        _root.sizeDelta = new Vector2(124f, 124f);

        GameObject frameObject = new GameObject("Frame");
        frameObject.transform.SetParent(_root, false);

        Image frameImage = frameObject.AddComponent<Image>();
        frameImage.color = new Color(0.07f, 0.04f, 0.06f, 0.95f);

        Outline frameOutline = frameObject.AddComponent<Outline>();
        frameOutline.effectColor = new Color(0.86f, 0.68f, 0.30f, 0.85f);
        frameOutline.effectDistance = new Vector2(2f, -2f);

        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        Stretch(frameRect, 0f);

        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(_root, false);

        _iconImage = iconObject.AddComponent<Image>();
        _iconImage.sprite = BombIconSprite;
        _iconImage.color = new Color(0.96f, 0.45f, 0.20f, 1f);
        _iconImage.preserveAspect = true;

        RectTransform iconRect = iconObject.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 10f);
        iconRect.sizeDelta = new Vector2(70f, 70f);

        GameObject cooldownOverlayObject = new GameObject("CooldownOverlay");
        cooldownOverlayObject.transform.SetParent(iconObject.transform, false);

        _cooldownOverlayImage = cooldownOverlayObject.AddComponent<Image>();
        _cooldownOverlayImage.sprite = BombIconSprite;
        _cooldownOverlayImage.type = Image.Type.Filled;
        _cooldownOverlayImage.fillMethod = Image.FillMethod.Radial360;
        _cooldownOverlayImage.fillOrigin = (int)Image.Origin360.Top;
        _cooldownOverlayImage.fillClockwise = true;
        _cooldownOverlayImage.fillAmount = 0f;
        _cooldownOverlayImage.color = new Color(0f, 0f, 0f, 0.72f);
        _cooldownOverlayImage.preserveAspect = true;

        RectTransform overlayRect = cooldownOverlayObject.GetComponent<RectTransform>();
        Stretch(overlayRect, 0f);

        GameObject cooldownTextObject = new GameObject("CooldownText");
        cooldownTextObject.transform.SetParent(_root, false);

        _cooldownText = cooldownTextObject.AddComponent<TextMeshProUGUI>();
        _cooldownText.alignment = TextAlignmentOptions.Center;
        _cooldownText.fontSize = 32f;
        _cooldownText.color = new Color(0.96f, 0.91f, 0.78f, 1f);
        _cooldownText.fontStyle = FontStyles.Bold;
        _cooldownText.outlineWidth = 0.24f;
        _cooldownText.outlineColor = new Color(0f, 0f, 0f, 1f);

        RectTransform cooldownTextRect = cooldownTextObject.GetComponent<RectTransform>();
        cooldownTextRect.anchorMin = new Vector2(0.5f, 0.5f);
        cooldownTextRect.anchorMax = new Vector2(0.5f, 0.5f);
        cooldownTextRect.pivot = new Vector2(0.5f, 0.5f);
        cooldownTextRect.anchoredPosition = new Vector2(0f, 12f);
        cooldownTextRect.sizeDelta = new Vector2(74f, 40f);

        GameObject keyObject = new GameObject("KeyText");
        keyObject.transform.SetParent(_root, false);

        _keyText = keyObject.AddComponent<TextMeshProUGUI>();
        _keyText.text = "Q";
        _keyText.alignment = TextAlignmentOptions.Center;
        _keyText.fontSize = 22f;
        _keyText.fontStyle = FontStyles.Bold;
        _keyText.color = new Color(0.92f, 0.74f, 0.36f, 1f);

        RectTransform keyRect = keyObject.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0f);
        keyRect.anchorMax = new Vector2(0.5f, 0f);
        keyRect.pivot = new Vector2(0.5f, 0f);
        keyRect.anchoredPosition = new Vector2(0f, 24f);
        keyRect.sizeDelta = new Vector2(40f, 24f);

        GameObject nameObject = new GameObject("NameText");
        nameObject.transform.SetParent(_root, false);

        _nameText = nameObject.AddComponent<TextMeshProUGUI>();
        _nameText.text = "Bomb";
        _nameText.alignment = TextAlignmentOptions.Center;
        _nameText.fontSize = 16f;
        _nameText.enableWordWrapping = false;
        _nameText.color = new Color(0.96f, 0.91f, 0.78f, 1f);

        RectTransform nameRect = nameObject.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1f);
        nameRect.anchorMax = new Vector2(0.5f, 1f);
        nameRect.pivot = new Vector2(0.5f, 1f);
        nameRect.anchoredPosition = new Vector2(0f, -6f);
        nameRect.sizeDelta = new Vector2(110f, 22f);
    }

    private void Refresh()
    {
        if (_root == null)
            return;

        if (_trackedBomb == null || !_trackedBomb.IsConfigured)
        {
            _iconImage.color = new Color(0.45f, 0.48f, 0.56f, 1f);
            _cooldownOverlayImage.fillAmount = 0f;
            _cooldownText.text = string.Empty;
            _nameText.text = "Bomb";
            return;
        }

        _iconImage.color = _trackedBomb.BombIconColor;
        _nameText.text = _trackedBomb.BombDisplayName;

        float normalized = _trackedBomb.CooldownNormalized;
        _cooldownOverlayImage.fillAmount = normalized;
        _cooldownOverlayImage.enabled = normalized > 0.001f;

        if (_trackedBomb.CooldownRemaining > 0.05f)
        {
            _cooldownText.text = Mathf.CeilToInt(_trackedBomb.CooldownRemaining).ToString();
            _keyText.color = new Color(1f, 0.74f, 0.3f, 1f);
        }
        else
        {
            _cooldownText.text = string.Empty;
            _keyText.color = new Color(0.62f, 1f, 0.72f, 1f);
        }
    }

    private static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static Sprite BombIconSprite
    {
        get
        {
            if (_bombIconSprite == null)
                _bombIconSprite = CreateBombIconSprite();

            return _bombIconSprite;
        }
    }

    private static Sprite CreateBombIconSprite()
    {
        const int size = 48;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(20f, 24f);
        float radius = 13f;
        Vector2 fuseAnchor = new Vector2(29f, 34f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                Color pixel = Color.clear;

                if (Vector2.Distance(point, center) <= radius)
                {
                    pixel = Color.white;
                }
                else if (x >= 26 && x <= 33 && y >= 31 && y <= 36)
                {
                    pixel = Color.white;
                }
                else if (DistanceToSegment(point, fuseAnchor, new Vector2(39f, 43f)) <= 1.4f)
                {
                    pixel = Color.white;
                }
                else if (Vector2.Distance(point, new Vector2(41f, 44f)) <= 2f)
                {
                    pixel = Color.white;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float denominator = Vector2.Dot(ab, ab);
        if (denominator <= Mathf.Epsilon)
            return Vector2.Distance(point, a);

        float t = Mathf.Clamp01(Vector2.Dot(point - a, ab) / denominator);
        Vector2 projection = a + ab * t;
        return Vector2.Distance(point, projection);
    }
}
