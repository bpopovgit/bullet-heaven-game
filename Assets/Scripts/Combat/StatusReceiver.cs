using System.Collections;
using UnityEngine;

public class StatusReceiver : MonoBehaviour
{
    public float SpeedMultiplier { get; private set; } = 1f;
    public bool IsStunned { get; private set; } = false;

    private Coroutine _slowRoutine;
    private Coroutine _burnRoutine;
    private Coroutine _poisonRoutine;
    private Coroutine _shockRoutine;

    private PlayerHealth _playerHealth;

    // Keep track of the slow separately so Shock doesn't permanently overwrite it
    private float _slowMultiplier = 1f;

    private void Awake()
    {
        _playerHealth = GetComponent<PlayerHealth>();
    }

    public void ApplyStatus(DamagePacket packet)
    {
        if (packet.status == StatusEffect.None)
            return;

        switch (packet.status)
        {
            case StatusEffect.Slow:
                ApplySlow(packet.statusStrength, packet.statusDuration);
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

    // ---------------- SLOW ----------------

    private void ApplySlow(float strength, float duration)
    {
        Debug.Log($"SLOW APPLIED: strength={strength}, duration={duration}", this);

        float s = Mathf.Clamp01(strength);
        float mult = Mathf.Clamp(1f - s, 0.1f, 1f);

        if (_slowRoutine != null)
            StopCoroutine(_slowRoutine);

        _slowRoutine = StartCoroutine(SlowRoutine(mult, duration));
    }

    private IEnumerator SlowRoutine(float mult, float duration)
    {
        _slowMultiplier = mult;
        RefreshSpeedMultiplier();

        yield return new WaitForSeconds(duration);

        _slowMultiplier = 1f;
        RefreshSpeedMultiplier();

        _slowRoutine = null;
    }

    // ---------------- BURN ----------------

    private void ApplyBurn(float strength, float duration)
    {
        if (_burnRoutine != null)
            StopCoroutine(_burnRoutine);

        int damagePerTick = StrengthToDot(strength, 6f, 2f);
        _burnRoutine = StartCoroutine(BurnRoutine(duration, damagePerTick));
    }

    private IEnumerator BurnRoutine(float duration, int damagePerTick)
    {
        yield return StartCoroutine(DotRoutine(2f, duration, damagePerTick));
        _burnRoutine = null;
    }

    // ---------------- POISON ----------------

    private void ApplyPoison(float strength, float duration)
    {
        if (_poisonRoutine != null)
            StopCoroutine(_poisonRoutine);

        int damagePerTick = StrengthToDot(strength, 3f, 2f);
        _poisonRoutine = StartCoroutine(PoisonRoutine(duration, damagePerTick));
    }

    private IEnumerator PoisonRoutine(float duration, int damagePerTick)
    {
        yield return StartCoroutine(DotRoutine(2f, duration, damagePerTick));
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

    // ---------------- SHOCK ----------------

    private void ApplyShock(float strength, float duration)
    {
        if (_shockRoutine != null)
            StopCoroutine(_shockRoutine);

        _shockRoutine = StartCoroutine(ShockRoutine(duration));
    }

    private IEnumerator ShockRoutine(float duration)
    {
        IsStunned = true;
        RefreshSpeedMultiplier();

        yield return new WaitForSeconds(duration);

        IsStunned = false;
        RefreshSpeedMultiplier();

        _shockRoutine = null;
    }

    // ---------------- HELPERS ----------------

    private void RefreshSpeedMultiplier()
    {
        SpeedMultiplier = IsStunned ? 0f : _slowMultiplier;
    }
}