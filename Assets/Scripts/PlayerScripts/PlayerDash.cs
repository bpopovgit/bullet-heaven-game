using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerDash : MonoBehaviour
{
    private const float DashDistance = 3.6f;
    private const float DashDuration = 0.18f;
    private const float DashCooldown = 1.2f;
    private const float InvulnDuration = 0.22f;

    private Rigidbody2D _rigidbody;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private PlayerHealth _health;
    private Camera _mainCamera;

    private float _cooldownRemaining;
    private float _dashTimeRemaining;
    private Vector2 _dashVelocity;
    private float _previousLinearDamping;

    public float CooldownRemaining => Mathf.Max(0f, _cooldownRemaining);
    public float CooldownDuration => DashCooldown;
    public float CooldownNormalized => Mathf.Clamp01(_cooldownRemaining / DashCooldown);
    public bool IsDashing => _dashTimeRemaining > 0f;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerInput = GetComponent<PlayerInput>();
        _health = GetComponent<PlayerHealth>();
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["Move"];
    }

    private void Update()
    {
        if (_cooldownRemaining > 0f)
            _cooldownRemaining -= Time.deltaTime;

        if (Time.timeScale <= 0f)
            return;

        if (_dashTimeRemaining > 0f || _cooldownRemaining > 0f)
            return;

        if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame)
            return;

        TryDash();
    }

    private void FixedUpdate()
    {
        if (_dashTimeRemaining <= 0f)
            return;

        _rigidbody.linearVelocity = _dashVelocity;
        _dashTimeRemaining -= Time.fixedDeltaTime;

        if (_dashTimeRemaining <= 0f)
        {
            _dashTimeRemaining = 0f;
            _rigidbody.linearDamping = _previousLinearDamping;
            _rigidbody.linearVelocity = Vector2.zero;
        }
    }

    private void TryDash()
    {
        Vector2 direction = ResolveDashDirection();
        if (direction.sqrMagnitude <= 0.0001f)
            return;

        direction = direction.normalized;
        float speed = DashDistance / DashDuration;
        _dashVelocity = direction * speed;
        _dashTimeRemaining = DashDuration;
        _cooldownRemaining = DashCooldown;
        _previousLinearDamping = _rigidbody.linearDamping;
        _rigidbody.linearDamping = 0f;

        if (_health != null)
            _health.GrantTemporaryInvulnerability(InvulnDuration);

        SpawnTrail(transform.position, direction);
        Debug.Log($"DASH: {DashDistance:0.##}m toward ({direction.x:0.##}, {direction.y:0.##}).");
    }

    private Vector2 ResolveDashDirection()
    {
        Vector2 input = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        if (input.sqrMagnitude > 0.04f)
            return input;

        if (_mainCamera == null)
            _mainCamera = Camera.main;
        if (_mainCamera == null || Mouse.current == null)
            return (Vector2)transform.right;

        Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 toCursor = (Vector2)(mouseWorld - transform.position);
        return toCursor.sqrMagnitude > 0.0001f ? toCursor : (Vector2)transform.right;
    }

    private static void SpawnTrail(Vector3 origin, Vector2 direction)
    {
        GameObject visual = new GameObject("PlayerDashTrail");
        visual.transform.position = origin;
        visual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = BuildStreakSprite();
        renderer.color = new Color(0.78f, 0.92f, 1f, 0.7f);
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 8;

        visual.transform.localScale = new Vector3(DashDistance, 0.55f, 1f);
        visual.AddComponent<DashTrailFader>().Begin(0.22f);
    }

    private static Sprite _streakSprite;

    private static Sprite BuildStreakSprite()
    {
        if (_streakSprite != null)
            return _streakSprite;

        const int width = 64;
        const int height = 16;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        Color[] pixels = new Color[width * height];
        for (int x = 0; x < width; x++)
        {
            float falloffX = Mathf.SmoothStep(0f, 1f, 1f - (x / (float)(width - 1)));
            for (int y = 0; y < height; y++)
            {
                float dy = (y - (height - 1) * 0.5f) / ((height - 1) * 0.5f);
                float falloffY = Mathf.Clamp01(1f - Mathf.Abs(dy));
                pixels[y * width + x] = new Color(1f, 1f, 1f, falloffX * falloffY);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply(false, true);

        _streakSprite = Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(1f, 0.5f), 64f);
        return _streakSprite;
    }
}

internal class DashTrailFader : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private float _duration;
    private float _elapsed;

    public void Begin(float duration)
    {
        _renderer = GetComponent<SpriteRenderer>();
        _duration = Mathf.Max(0.01f, duration);
    }

    private void Update()
    {
        if (_renderer == null)
            return;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / _duration);
        Color color = _renderer.color;
        color.a = Mathf.Lerp(0.7f, 0f, t);
        _renderer.color = color;

        if (t >= 1f)
            Destroy(gameObject);
    }
}
