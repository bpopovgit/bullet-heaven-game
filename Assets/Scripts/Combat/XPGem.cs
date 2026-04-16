using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class XPGem : MonoBehaviour
{
    [SerializeField] private int experienceValue = 1;
    [SerializeField] private float attractSpeed = 8f;
    [SerializeField] private float collectDistance = 0.25f;

    private static Sprite _defaultSprite;

    private Transform _target;
    private Rigidbody2D _rb;
    private bool _collected;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void FixedUpdate()
    {
        if (_target == null || _collected)
            return;

        Vector2 nextPosition = Vector2.MoveTowards(
            transform.position,
            _target.position,
            attractSpeed * Time.fixedDeltaTime);

        if (_rb != null)
            _rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;

        if (Vector2.Distance(transform.position, _target.position) <= collectDistance)
            Collect(_target.GetComponent<PlayerExperience>());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerExperience>(out var playerExperience))
            Collect(playerExperience);
    }

    public void SetExperienceValue(int value)
    {
        experienceValue = Mathf.Max(1, value);
    }

    public void AttractTo(Transform target)
    {
        if (target == null || _collected)
            return;

        _target = target;
    }

    private void Collect(PlayerExperience playerExperience)
    {
        if (_collected || playerExperience == null)
            return;

        _collected = true;
        playerExperience.AddExperience(Mathf.Max(1, experienceValue));
        Destroy(gameObject);
    }

    public static XPGem SpawnDefault(Vector3 position, int value)
    {
        GameObject go = new GameObject("XP Gem");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.35f;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = GetDefaultSprite();
        renderer.color = new Color(0.2f, 1f, 0.35f);
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 5;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        XPGem gem = go.AddComponent<XPGem>();
        gem.SetExperienceValue(value);

        return gem;
    }

    private static Sprite GetDefaultSprite()
    {
        if (_defaultSprite != null)
            return _defaultSprite;

        const int size = 16;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
        texture.filterMode = FilterMode.Point;

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.45f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = distance <= radius ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();

        _defaultSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit: 16f);

        return _defaultSprite;
    }
}
