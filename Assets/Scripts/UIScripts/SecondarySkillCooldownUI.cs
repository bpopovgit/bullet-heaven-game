using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SecondarySkillCooldownUI : MonoBehaviour
{
    private const string GameplaySceneName = "Game";

    private static SecondarySkillCooldownUI _instance;
    private static Sprite _magneticPulseSprite;
    private static Sprite _arcaneShieldSprite;
    private static Sprite _frostNovaSprite;

    private RectTransform _root;
    private Image _iconImage;
    private Image _cooldownOverlayImage;
    private TMP_Text _cooldownText;
    private TMP_Text _keyText;
    private TMP_Text _nameText;
    private PlayerSecondaryActiveSkill _trackedSkill;
    private StartingSkillChoice _lastChoice = (StartingSkillChoice)(-1);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject go = new GameObject("SecondarySkillCooldownUI");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<SecondarySkillCooldownUI>();
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
        _trackedSkill = null;
        _lastChoice = (StartingSkillChoice)(-1);

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

        if (_trackedSkill == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _trackedSkill = player.GetComponent<PlayerSecondaryActiveSkill>();
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
        GameObject rootObject = new GameObject("SecondarySkillCooldownRoot");
        rootObject.transform.SetParent(canvasTransform, false);

        _root = rootObject.AddComponent<RectTransform>();
        _root.anchorMin = new Vector2(0f, 0f);
        _root.anchorMax = new Vector2(0f, 0f);
        _root.pivot = new Vector2(0f, 0f);
        _root.anchoredPosition = new Vector2(152f, 18f);
        _root.sizeDelta = new Vector2(124f, 124f);

        GameObject frameObject = new GameObject("Frame");
        frameObject.transform.SetParent(_root, false);

        Image frameImage = frameObject.AddComponent<Image>();
        frameImage.color = new Color(0.04f, 0.06f, 0.1f, 0.92f);

        Outline frameOutline = frameObject.AddComponent<Outline>();
        frameOutline.effectColor = new Color(0.82f, 0.7f, 0.22f, 0.8f);
        frameOutline.effectDistance = new Vector2(2f, -2f);

        RectTransform frameRect = frameObject.GetComponent<RectTransform>();
        Stretch(frameRect, 0f);

        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(_root, false);

        _iconImage = iconObject.AddComponent<Image>();
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
        _cooldownText.fontSize = 30f;
        _cooldownText.color = Color.white;
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
        _keyText.text = "E";
        _keyText.alignment = TextAlignmentOptions.Center;
        _keyText.fontSize = 20f;
        _keyText.color = new Color(1f, 0.89f, 0.38f, 1f);

        RectTransform keyRect = keyObject.GetComponent<RectTransform>();
        keyRect.anchorMin = new Vector2(0.5f, 0f);
        keyRect.anchorMax = new Vector2(0.5f, 0f);
        keyRect.pivot = new Vector2(0.5f, 0f);
        keyRect.anchoredPosition = new Vector2(0f, 24f);
        keyRect.sizeDelta = new Vector2(40f, 24f);

        GameObject nameObject = new GameObject("NameText");
        nameObject.transform.SetParent(_root, false);

        _nameText = nameObject.AddComponent<TextMeshProUGUI>();
        _nameText.text = "Skill";
        _nameText.alignment = TextAlignmentOptions.Center;
        _nameText.fontSize = 15f;
        _nameText.enableWordWrapping = false;
        _nameText.color = new Color(0.78f, 0.93f, 1f, 1f);

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

        if (_trackedSkill == null || !_trackedSkill.IsConfigured)
        {
            _iconImage.color = new Color(0.45f, 0.48f, 0.56f, 1f);
            _cooldownOverlayImage.fillAmount = 0f;
            _cooldownText.text = string.Empty;
            _nameText.text = "Skill";
            return;
        }

        if (_lastChoice != _trackedSkill.SkillChoice)
        {
            _lastChoice = _trackedSkill.SkillChoice;
            _iconImage.sprite = GetIconSprite(_lastChoice);
            _cooldownOverlayImage.sprite = _iconImage.sprite;
        }

        _iconImage.color = _trackedSkill.SkillPrimaryColor;
        _nameText.text = _trackedSkill.SkillDisplayName;

        float normalized = _trackedSkill.CooldownNormalized;
        _cooldownOverlayImage.fillAmount = normalized;
        _cooldownOverlayImage.enabled = normalized > 0.001f;

        if (_trackedSkill.CooldownRemaining > 0.05f)
        {
            _cooldownText.text = Mathf.CeilToInt(_trackedSkill.CooldownRemaining).ToString();
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

    private static Sprite GetIconSprite(StartingSkillChoice choice)
    {
        switch (choice)
        {
            case StartingSkillChoice.ArcaneShield:
                if (_arcaneShieldSprite == null)
                    _arcaneShieldSprite = CreateArcaneShieldSprite();
                return _arcaneShieldSprite;

            case StartingSkillChoice.FrostNova:
                if (_frostNovaSprite == null)
                    _frostNovaSprite = CreateFrostNovaSprite();
                return _frostNovaSprite;

            default:
                if (_magneticPulseSprite == null)
                    _magneticPulseSprite = CreateMagneticPulseSprite();
                return _magneticPulseSprite;
        }
    }

    private static Sprite CreateMagneticPulseSprite()
    {
        const int size = 48;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(24f, 24f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                float distance = Vector2.Distance(point, center);
                Color pixel = Color.clear;

                if (Mathf.Abs(distance - 16f) <= 1.8f || Mathf.Abs(distance - 8f) <= 1.8f)
                    pixel = Color.white;
                else if ((Mathf.Abs(x - 24f) <= 1f && y > 8 && y < 40) || (Mathf.Abs(y - 24f) <= 1f && x > 8 && x < 40))
                    pixel = Color.white;

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
    }

    private static Sprite CreateArcaneShieldSprite()
    {
        const int size = 48;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Color pixel = Color.clear;
                bool upperBody = y >= 18 && y <= 36 && x >= 12 && x <= 36;
                bool taperedLower = y >= 10 && y < 18 && x >= 16 + (18 - y) / 2 && x <= 32 - (18 - y) / 2;
                bool outline = false;

                if (upperBody || taperedLower)
                {
                    outline =
                        x <= 13 || x >= 35 ||
                        y <= 11 || y >= 35 ||
                        (y < 18 && (x <= 18 + (18 - y) / 2 || x >= 30 - (18 - y) / 2));
                }

                if (outline)
                    pixel = Color.white;

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
    }

    private static Sprite CreateFrostNovaSprite()
    {
        const int size = 48;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(24f, 24f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y);
                Vector2 delta = point - center;
                Color pixel = Color.clear;

                if ((Mathf.Abs(delta.x) <= 1f && Mathf.Abs(delta.y) <= 16f) ||
                    (Mathf.Abs(delta.y) <= 1f && Mathf.Abs(delta.x) <= 16f) ||
                    Mathf.Abs(delta.x - delta.y) <= 1.2f && delta.magnitude <= 18f ||
                    Mathf.Abs(delta.x + delta.y) <= 1.2f && delta.magnitude <= 18f)
                {
                    pixel = Color.white;
                }
                else if (delta.magnitude <= 3f)
                {
                    pixel = Color.white;
                }

                texture.SetPixel(x, y, pixel);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
    }
}
