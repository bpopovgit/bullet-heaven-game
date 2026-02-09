using System.Collections;
using UnityEngine;

public class StatusReceiver : MonoBehaviour
{
    // Movement reads this.
    public float SpeedMultiplier { get; private set; } = 1f;

    private Coroutine _slowRoutine;

    public void ApplyStatus(DamagePacket packet)
    {
        if (packet.status == StatusEffect.None) return;

        switch (packet.status)
        {
            case StatusEffect.Slow:
                ApplySlow(packet.statusStrength, packet.statusDuration);
                break;
        }
    }

    private void ApplySlow(float strength, float duration)
    {
        Debug.Log($"SLOW APPLIED: strength={strength}, duration={duration}", this);
        // strength: 0.35 => multiplier becomes 0.65
        float s = Mathf.Clamp01(strength);
        float mult = Mathf.Clamp(1f - s, 0.1f, 1f);

        if (_slowRoutine != null)
            StopCoroutine(_slowRoutine);

        _slowRoutine = StartCoroutine(SlowRoutine(mult, duration));
    }

    private IEnumerator SlowRoutine(float mult, float duration)
    {
        SpeedMultiplier = mult;
        yield return new WaitForSeconds(duration);
        SpeedMultiplier = 1f;
        _slowRoutine = null;
    }
}
