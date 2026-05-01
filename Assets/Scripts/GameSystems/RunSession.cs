using UnityEngine;

public static class RunSession
{
    public static int CurrentDistrictIndex { get; private set; }
    public static int Currency { get; private set; }
    public static int DistrictsCompleted { get; private set; }
    public static float SavedHpFraction { get; set; } = 1f;
    public static bool IsActive { get; private set; }

    public static MapDefinition CurrentDistrict => MapCatalog.Get(CurrentDistrictIndex);
    public static int TotalDistricts => MapCatalog.DistrictCount;
    public static bool IsFinalDistrict => CurrentDistrictIndex == TotalDistricts - 1;
    public static bool IsRunComplete => CurrentDistrictIndex >= TotalDistricts;

    public static void StartNewRun()
    {
        CurrentDistrictIndex = 0;
        Currency = 0;
        DistrictsCompleted = 0;
        SavedHpFraction = 1f;
        IsActive = true;
        Debug.Log("RUN SESSION: started new run.");
    }

    public static void AdvanceDistrict()
    {
        DistrictsCompleted++;
        CurrentDistrictIndex++;
        Debug.Log($"RUN SESSION: advanced to district index {CurrentDistrictIndex} ({CurrentDistrict?.DisplayName ?? "RUN COMPLETE"}).");
    }

    public static void EndRun()
    {
        IsActive = false;
        Debug.Log($"RUN SESSION: ended after {DistrictsCompleted} district(s).");
    }

    public static void AddCurrency(int amount)
    {
        if (amount > 0)
            Currency += amount;
    }

    public static bool TrySpendCurrency(int amount)
    {
        if (amount <= 0 || Currency < amount)
            return false;
        Currency -= amount;
        return true;
    }
}
