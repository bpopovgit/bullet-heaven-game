using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float DamageMultiplier { get; private set; } = 1f;
    public float FireRateMultiplier { get; private set; } = 1f;
    public float MoveSpeedMultiplier { get; private set; } = 1f;
    public float PickupRadiusBonus { get; private set; } = 0f;
    public int BonusProjectiles { get; private set; } = 0;
    public int BonusPierce { get; private set; } = 0;
    public float SplashRadiusBonus { get; private set; } = 0f;

    public void AddDamagePercent(float percent)
    {
        DamageMultiplier += Mathf.Max(0f, percent);
    }

    public void AddFireRatePercent(float percent)
    {
        FireRateMultiplier += Mathf.Max(0f, percent);
    }

    public void AddMoveSpeedPercent(float percent)
    {
        MoveSpeedMultiplier += Mathf.Max(0f, percent);
    }

    public void AddPickupRadius(float amount)
    {
        PickupRadiusBonus += Mathf.Max(0f, amount);
    }

    public void AddProjectiles(int amount)
    {
        BonusProjectiles += Mathf.Max(0, amount);
    }

    public void AddPierce(int amount)
    {
        BonusPierce += Mathf.Max(0, amount);
    }

    public void AddSplashRadius(float amount)
    {
        SplashRadiusBonus += Mathf.Max(0f, amount);
    }
}
