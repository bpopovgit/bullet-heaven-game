using UnityEngine;

public class FactionVisualIdentity : MonoBehaviour
{
    private const string BadgeName = "FactionBadge";
    private const string BadgeLabelName = "FactionBadgeLabel";

    [SerializeField] private bool showBadge = true;
    [SerializeField] private Vector3 badgeOffset = new Vector3(0f, 0.78f, 0f);
    [SerializeField] private float badgeScale = 0.34f;

    private static Sprite _badgeSprite;

    private FactionMember _factionMember;
    private SpriteRenderer _badgeRenderer;
    private TextMesh _label;
    private FactionType _lastFaction;
    private bool _hasRefreshed;

    public static FactionVisualIdentity Ensure(GameObject target)
    {
        if (target == null)
            return null;

        FactionVisualIdentity visual = target.GetComponent<FactionVisualIdentity>();
        if (visual == null)
            visual = target.AddComponent<FactionVisualIdentity>();

        visual.Refresh();
        return visual;
    }

    private void Awake()
    {
        _factionMember = GetComponent<FactionMember>();
        Refresh();
    }

    private void LateUpdate()
    {
        if (_factionMember == null)
            _factionMember = GetComponent<FactionMember>();

        if (!_hasRefreshed || (_factionMember != null && _factionMember.Faction != _lastFaction))
            Refresh();
    }

    public void Refresh()
    {
        if (_factionMember == null)
            _factionMember = GetComponent<FactionMember>();

        if (_factionMember == null)
            return;

        if (!showBadge)
        {
            Transform existingBadge = transform.Find(BadgeName);
            if (existingBadge != null)
                existingBadge.gameObject.SetActive(false);

            return;
        }

        EnsureBadgeObjects();

        FactionType faction = _factionMember.Faction;
        _badgeRenderer.color = GetFactionColor(faction);
        _label.text = GetFactionSymbol(faction);
        _label.color = GetLabelColor(faction);

        _lastFaction = faction;
        _hasRefreshed = true;
    }

    private void EnsureBadgeObjects()
    {
        Transform badgeTransform = transform.Find(BadgeName);
        GameObject badgeObject;

        if (badgeTransform == null)
        {
            badgeObject = new GameObject(BadgeName);
            badgeObject.transform.SetParent(transform, false);
        }
        else
        {
            badgeObject = badgeTransform.gameObject;
            badgeObject.SetActive(true);
        }

        badgeObject.transform.localPosition = badgeOffset;
        badgeObject.transform.localScale = Vector3.one * badgeScale;

        _badgeRenderer = badgeObject.GetComponent<SpriteRenderer>();
        if (_badgeRenderer == null)
            _badgeRenderer = badgeObject.AddComponent<SpriteRenderer>();

        _badgeRenderer.sprite = BadgeSprite;
        ApplySorting(_badgeRenderer, offset: 10);

        Transform labelTransform = badgeObject.transform.Find(BadgeLabelName);
        GameObject labelObject;

        if (labelTransform == null)
        {
            labelObject = new GameObject(BadgeLabelName);
            labelObject.transform.SetParent(badgeObject.transform, false);
        }
        else
        {
            labelObject = labelTransform.gameObject;
        }

        labelObject.transform.localPosition = new Vector3(0f, -0.03f, -0.01f);
        labelObject.transform.localScale = Vector3.one;

        _label = labelObject.GetComponent<TextMesh>();
        if (_label == null)
            _label = labelObject.AddComponent<TextMesh>();

        _label.anchor = TextAnchor.MiddleCenter;
        _label.alignment = TextAlignment.Center;
        _label.fontSize = 48;
        _label.characterSize = 0.08f;

        MeshRenderer meshRenderer = labelObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            ApplySorting(meshRenderer, offset: 11);
    }

    private void ApplySorting(Renderer renderer, int offset)
    {
        SpriteRenderer baseRenderer = GetComponent<SpriteRenderer>();

        if (baseRenderer == null)
            baseRenderer = GetComponentInChildren<SpriteRenderer>();

        if (baseRenderer != null && baseRenderer != renderer)
        {
            renderer.sortingLayerID = baseRenderer.sortingLayerID;
            renderer.sortingOrder = baseRenderer.sortingOrder + offset;
            return;
        }

        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = offset;
    }

    private static Sprite BadgeSprite
    {
        get
        {
            if (_badgeSprite == null)
                _badgeSprite = CreateBadgeSprite();

            return _badgeSprite;
        }
    }

    private static Sprite CreateBadgeSprite()
    {
        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
        texture.filterMode = FilterMode.Point;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                texture.SetPixel(x, y, distance <= 14f ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit: 32f);
    }

    private static Color GetFactionColor(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Human:
                return new Color(0.1f, 0.72f, 1f, 0.95f);
            case FactionType.Angel:
                return new Color(1f, 0.92f, 0.35f, 0.95f);
            case FactionType.Demon:
                return new Color(0.92f, 0.1f, 0.12f, 0.95f);
            case FactionType.Zombie:
                return new Color(0.42f, 0.95f, 0.24f, 0.95f);
            default:
                return new Color(0.85f, 0.85f, 0.85f, 0.95f);
        }
    }

    private static Color GetLabelColor(FactionType faction)
    {
        return faction == FactionType.Angel ? new Color(0.08f, 0.08f, 0.08f, 1f) : Color.white;
    }

    private static string GetFactionSymbol(FactionType faction)
    {
        switch (faction)
        {
            case FactionType.Human:
                return "H";
            case FactionType.Angel:
                return "A";
            case FactionType.Demon:
                return "D";
            case FactionType.Zombie:
                return "Z";
            default:
                return "?";
        }
    }
}
