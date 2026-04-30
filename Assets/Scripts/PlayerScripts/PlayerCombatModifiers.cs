using UnityEngine;

public class PlayerCombatModifiers : MonoBehaviour
{
    private const int BurstShotIntervalBase = 12;
    private const int BurstShotIntervalStep = 4;
    private const int BurstShotIntervalMin = 4;

    public static PlayerCombatModifiers Instance { get; private set; }

    public float ExecuteHpThreshold { get; private set; }
    public float ExecuteBonusDamage { get; private set; }

    public int BurstShotFrequency { get; private set; }
    public int BurstShotProjectiles => BurstShotFrequency > 0 ? 2 + BurstShotFrequency : 0;
    public int BurstShotInterval => BurstShotFrequency > 0
        ? Mathf.Max(BurstShotIntervalMin, BurstShotIntervalBase - BurstShotIntervalStep * BurstShotFrequency)
        : 0;

    public float OnKillStatusSpreadRadius { get; private set; }
    public float OnKillStatusSpreadStrength { get; private set; }

    public float SkillElementBurstDamage { get; private set; }

    public float BombSecondaryBlastFraction { get; private set; }
    public float BombSecondaryBlastDelay { get; private set; } = 0.45f;

    public float SkillBlinkDistance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void AddExecuteThreshold(float perPoint)
    {
        ExecuteHpThreshold = Mathf.Clamp01(ExecuteHpThreshold + Mathf.Max(0f, perPoint));
    }

    public void AddExecuteBonusDamage(float perPoint)
    {
        ExecuteBonusDamage = Mathf.Max(0f, ExecuteBonusDamage + Mathf.Max(0f, perPoint));
    }

    public void AddBurstShotFrequency(int perPoint)
    {
        BurstShotFrequency = Mathf.Max(0, BurstShotFrequency + Mathf.Max(0, perPoint));
    }

    public void AddOnKillStatusSpreadRadius(float perPoint)
    {
        OnKillStatusSpreadRadius = Mathf.Max(0f, OnKillStatusSpreadRadius + Mathf.Max(0f, perPoint));
    }

    public void AddOnKillStatusSpreadStrength(float perPoint)
    {
        OnKillStatusSpreadStrength = Mathf.Max(0f, OnKillStatusSpreadStrength + Mathf.Max(0f, perPoint));
    }

    public void AddSkillElementBurstDamage(float perPoint)
    {
        SkillElementBurstDamage = Mathf.Max(0f, SkillElementBurstDamage + Mathf.Max(0f, perPoint));
    }

    public void AddBombSecondaryBlastFraction(float perPoint)
    {
        BombSecondaryBlastFraction = Mathf.Clamp01(BombSecondaryBlastFraction + Mathf.Max(0f, perPoint));
    }

    public void AddSkillBlinkDistance(float perPoint)
    {
        SkillBlinkDistance = Mathf.Max(0f, SkillBlinkDistance + Mathf.Max(0f, perPoint));
    }

    public int ApplyExecuteIfApplicable(int incomingDamage, float targetHpFraction)
    {
        if (ExecuteHpThreshold <= 0f || ExecuteBonusDamage <= 0f)
            return incomingDamage;

        if (targetHpFraction > ExecuteHpThreshold)
            return incomingDamage;

        return Mathf.RoundToInt(incomingDamage * (1f + ExecuteBonusDamage));
    }

    public void TrySpreadStatusOnKill(Vector2 deathPosition, StatusEffect status, float duration, float strength)
    {
        if (OnKillStatusSpreadRadius <= 0f || status == StatusEffect.None)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(deathPosition, OnKillStatusSpreadRadius);
        float spreadDuration = Mathf.Max(duration, 1f) + OnKillStatusSpreadStrength;
        float spreadStrength = Mathf.Max(strength, 0.4f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null)
                continue;

            StatusReceiver receiver = hit.GetComponent<StatusReceiver>();
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (receiver == null || enemy == null || enemy.IsDead)
                continue;

            DamagePacket spreadPacket = new DamagePacket(0, ElementForStatus(status), status, spreadDuration, spreadStrength, 0f, deathPosition);
            receiver.ApplyStatus(spreadPacket);
        }
    }

    private static DamageElement ElementForStatus(StatusEffect status)
    {
        switch (status)
        {
            case StatusEffect.Burn: return DamageElement.Fire;
            case StatusEffect.Freeze:
            case StatusEffect.Slow: return DamageElement.Frost;
            case StatusEffect.Shock: return DamageElement.Lightning;
            case StatusEffect.Poison: return DamageElement.Poison;
            default: return DamageElement.Physical;
        }
    }
}
