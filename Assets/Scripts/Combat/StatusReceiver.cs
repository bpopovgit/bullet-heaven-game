using System.Collections;
using UnityEngine;

public class StatusReceiver : MonoBehaviour
{
    public float SpeedMultiplier { get; private set; } = 1f;
    public bool IsStunned => _shockActive || _freezeActive;

    public StatusEffect MostRecentStatus { get; private set; } = StatusEffect.None;
    public float MostRecentStatusDuration { get; private set; }
    public float MostRecentStatusStrength { get; private set; }
    public float MostRecentStatusExpiresAt { get; private set; }
    public bool HasActiveStatus =>
        MostRecentStatus != StatusEffect.None && Time.time < MostRecentStatusExpiresAt;

    [Header("Status VFX")]
    [SerializeField] private ParticleSystem burnVFX;
    [SerializeField] private ParticleSystem frostVFX;
    [SerializeField] private ParticleSystem poisonVFX;
    [SerializeField] private ParticleSystem shockVFX;

    [Header("Freeze Visuals")]
    [SerializeField] private Color freezeTintColor = new Color(0.58f, 0.9f, 1f, 1f);
    [SerializeField, Range(0f, 1f)] private float freezeTintStrength = 0.7f;
    [SerializeField] private Color freezeRingColor = new Color(0.72f, 0.94f, 1f, 0.9f);
    [SerializeField] private Color freezeSparkleColor = new Color(0.9f, 0.98f, 1f, 0.92f);
    [SerializeField, Min(0.1f)] private float freezeVisualScale = 1.2f;

    private Coroutine _slowRoutine;
    private Coroutine _freezeRoutine;
    private Coroutine _burnRoutine;
    private Coroutine _poisonRoutine;
    private Coroutine _shockRoutine;

    private PlayerHealth _playerHealth;
    private float _slowMultiplier = 1f;
    private bool _shockActive;
    private bool _freezeActive;
    private SpriteRenderer[] _spriteRenderers;
    private Color[] _freezeOriginalColors;
    private GameObject _freezeVisualRoot;
    private LineRenderer _freezeRing;
    private ParticleSystem _freezeSparkles;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        CacheSpriteRenderers();
    }

    public void ApplyStatus(DamagePacket packet)
    {
        if (packet.status == StatusEffect.None)
            return;

        MostRecentStatus = packet.status;
        MostRecentStatusDuration = packet.statusDuration;
        MostRecentStatusStrength = packet.statusStrength;
        MostRecentStatusExpiresAt = Time.time + Mathf.Max(0.1f, packet.statusDuration);

        switch (packet.status)
        {
            case StatusEffect.Slow:
                ApplySlow(packet.statusStrength, packet.statusDuration);
                break;

            case StatusEffect.Freeze:
                ApplyFreeze(packet.statusDuration);
                break;

            case StatusEffect.Burn:
                ApplyBurn(packet.statusStrength, packet.statusDuration);
                break;

            case StatusEffect.Poison:
                ApplyPoison(packet.statusStrength, packet.statusDuration);
                break;

            case StatusEffect.Shock:
                ApplyShock(packet.statusStrength, packet.statusDuration);
                break;
        }
    }

    public void ClearMostRecentStatus()
    {
        MostRecentStatus = StatusEffect.None;
        MostRecentStatusDuration = 0f;
        MostRecentStatusStrength = 0f;
        MostRecentStatusExpiresAt = 0f;
    }

    private void ApplySlow(float strength, float duration)
    {
        Debug.Log($"SLOW APPLIED: strength={strength}, duration={duration}", this);

        float s = Mathf.Clamp01(strength);
        float mult = Mathf.Clamp(1f - s, 0.1f, 1f);

        if (_slowRoutine != null)
            StopCoroutine(_slowRoutine);

        PlayVFX(frostVFX);
        _slowRoutine = StartCoroutine(SlowRoutine(mult, duration));
    }

    private IEnumerator SlowRoutine(float mult, float duration)
    {
        _slowMultiplier = mult;
        RefreshSpeedMultiplier();

        yield return new WaitForSeconds(duration);

        _slowMultiplier = 1f;
        RefreshSpeedMultiplier();

        if (!_freezeActive)
            StopVFX(frostVFX);

        _slowRoutine = null;
    }

    private void ApplyFreeze(float duration)
    {
        if (duration <= 0f)
            return;

        if (_freezeRoutine != null)
            StopCoroutine(_freezeRoutine);

        if (!_freezeActive)
            ApplyFreezeTint();

        PlayVFX(frostVFX);
        _freezeRoutine = StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        _freezeActive = true;
        RefreshSpeedMultiplier();

        yield return new WaitForSeconds(duration);

        _freezeActive = false;
        RefreshSpeedMultiplier();
        RestoreFreezeTint();

        if (_slowRoutine == null)
            StopVFX(frostVFX);

        _freezeRoutine = null;
    }

    private void ApplyBurn(float strength, float duration)
    {
        if (_burnRoutine != null)
            StopCoroutine(_burnRoutine);

        int damagePerTick = StrengthToDot(strength, 6f, 2f);
        PlayVFX(burnVFX);
        _burnRoutine = StartCoroutine(BurnRoutine(duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(float duration, int damagePerTick)
    {
        yield return StartCoroutine(DotRoutine(2f, duration, damagePerTick));
        StopVFX(burnVFX);
        _burnRoutine = null;
    }

    private void ApplyPoison(float strength, float duration)
    {
        if (_poisonRoutine != null)
            StopCoroutine(_poisonRoutine);

        int damagePerTick = StrengthToDot(strength, 3f, 2f);
        PlayVFX(poisonVFX);
        _poisonRoutine = StartCoroutine(PoisonRoutine(duration, damagePerTick));
    }

    private IEnumerator PoisonRoutine(float duration, int damagePerTick)
    {
        yield return StartCoroutine(DotRoutine(2f, duration, damagePerTick));
        StopVFX(poisonVFX);
        _poisonRoutine = null;
    }

    private IEnumerator DotRoutine(float ticksPerSecond, float duration, int damagePerTick)
    {
        if (_playerHealth == null || damagePerTick <= 0 || duration <= 0f)
            yield break;

        float interval = 1f / ticksPerSecond;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            _playerHealth.TakeDamageDirect(damagePerTick);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    private int StrengthToDot(float strength, float basePerSecond, float ticksPerSecond)
    {
        float s = Mathf.Clamp01(strength);
        float damagePerSecond = basePerSecond * Mathf.Lerp(0.5f, 1.5f, s);
        float damagePerTick = damagePerSecond / ticksPerSecond;

        return Mathf.Max(1, Mathf.RoundToInt(damagePerTick));
    }

    private void ApplyShock(float strength, float duration)
    {
        if (_shockRoutine != null)
            StopCoroutine(_shockRoutine);

        PlayVFX(shockVFX);
        _shockRoutine = StartCoroutine(ShockRoutine(duration));
    }

    private IEnumerator ShockRoutine(float duration)
    {
        _shockActive = true;
        RefreshSpeedMultiplier();

        yield return new WaitForSeconds(duration);

        _shockActive = false;
        RefreshSpeedMultiplier();

        StopVFX(shockVFX);
        _shockRoutine = null;
    }

    private void RefreshSpeedMultiplier()
    {
        SpeedMultiplier = IsStunned ? 0f : _slowMultiplier;
    }

    private void PlayVFX(ParticleSystem vfx)
    {
        if (vfx != null && !vfx.isPlaying)
            vfx.Play();
    }

    private void StopVFX(ParticleSystem vfx)
    {
        if (vfx != null && vfx.isPlaying)
            vfx.Stop();
    }

    private void CacheSpriteRenderers()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void ApplyFreezeTint()
    {
        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            CacheSpriteRenderers();

        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
            return;

        _freezeOriginalColors = new Color[_spriteRenderers.Length];
        float tintStrength = Mathf.Clamp01(freezeTintStrength);

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            SpriteRenderer spriteRenderer = _spriteRenderers[i];
            if (spriteRenderer == null)
                continue;

            _freezeOriginalColors[i] = spriteRenderer.color;
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, freezeTintColor, tintStrength);
        }

        ShowFreezeVisuals();
    }

    private void RestoreFreezeTint()
    {
        if (_spriteRenderers == null || _freezeOriginalColors == null)
            return;

        int count = Mathf.Min(_spriteRenderers.Length, _freezeOriginalColors.Length);
        for (int i = 0; i < count; i++)
        {
            SpriteRenderer spriteRenderer = _spriteRenderers[i];
            if (spriteRenderer == null)
                continue;

            spriteRenderer.color = _freezeOriginalColors[i];
        }

        _freezeOriginalColors = null;
        HideFreezeVisuals();
    }

    private void EnsureFreezeVisuals()
    {
        if (_freezeVisualRoot != null)
            return;

        _freezeVisualRoot = new GameObject("FreezeVisual");
        _freezeVisualRoot.transform.SetParent(transform, false);
        _freezeVisualRoot.transform.localPosition = Vector3.zero;
        _freezeVisualRoot.SetActive(false);

        CreateFreezeRing();
        CreateFreezeSparkles();
    }

    private void CreateFreezeRing()
    {
        GameObject ringObject = new GameObject("FreezeRing");
        ringObject.transform.SetParent(_freezeVisualRoot.transform, false);
        ringObject.transform.localPosition = new Vector3(0f, -0.05f, 0f);

        _freezeRing = ringObject.AddComponent<LineRenderer>();
        _freezeRing.loop = true;
        _freezeRing.useWorldSpace = false;
        _freezeRing.alignment = LineAlignment.View;
        _freezeRing.positionCount = 28;
        _freezeRing.startWidth = 0.07f;
        _freezeRing.endWidth = 0.07f;
        _freezeRing.numCapVertices = 4;
        _freezeRing.numCornerVertices = 4;
        _freezeRing.material = new Material(Shader.Find("Sprites/Default"));
        _freezeRing.startColor = freezeRingColor;
        _freezeRing.endColor = freezeRingColor;
        _freezeRing.sortingOrder = 20;

        float radius = Mathf.Max(0.45f, GetVisualRadius() * freezeVisualScale);
        for (int i = 0; i < _freezeRing.positionCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / _freezeRing.positionCount;
            _freezeRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
        }
    }

    private void CreateFreezeSparkles()
    {
        GameObject sparkleObject = new GameObject("FreezeSparkles");
        sparkleObject.transform.SetParent(_freezeVisualRoot.transform, false);
        sparkleObject.transform.localPosition = Vector3.zero;

        _freezeSparkles = sparkleObject.AddComponent<ParticleSystem>();

        var main = _freezeSparkles.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.85f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
        main.startColor = freezeSparkleColor;
        main.maxParticles = 20;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = _freezeSparkles.emission;
        emission.rateOverTime = 12f;

        var shape = _freezeSparkles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = Mathf.Max(0.35f, GetVisualRadius() * freezeVisualScale * 0.9f);
        shape.arc = 360f;

        var velocityOverLifetime = _freezeSparkles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
        velocityOverLifetime.orbitalZ = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);

        var colorOverLifetime = _freezeSparkles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient sparkleGradient = new Gradient();
        sparkleGradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.82f, 0.95f, 1f), 0f),
                new GradientColorKey(new Color(0.58f, 0.86f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.9f, 0.18f),
                new GradientAlphaKey(0.75f, 0.82f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(sparkleGradient);

        var renderer = _freezeSparkles.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.sortingOrder = 21;
    }

    private void ShowFreezeVisuals()
    {
        EnsureFreezeVisuals();

        if (_freezeVisualRoot == null)
            return;

        _freezeVisualRoot.SetActive(true);

        if (_freezeSparkles != null && !_freezeSparkles.isPlaying)
            _freezeSparkles.Play();
    }

    private void HideFreezeVisuals()
    {
        if (_freezeSparkles != null && _freezeSparkles.isPlaying)
            _freezeSparkles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_freezeVisualRoot != null)
            _freezeVisualRoot.SetActive(false);
    }

    private float GetVisualRadius()
    {
        float radius = 0.5f;

        if (_spriteRenderers != null)
        {
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                SpriteRenderer spriteRenderer = _spriteRenderers[i];
                if (spriteRenderer == null)
                    continue;

                Bounds bounds = spriteRenderer.bounds;
                radius = Mathf.Max(radius, Mathf.Max(bounds.extents.x, bounds.extents.y));
            }
        }

        return radius;
    }
}
