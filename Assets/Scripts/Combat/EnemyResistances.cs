using System;
using UnityEngine;

[Serializable]
public struct ElementMultiplier
{
    public DamageElement element;
    [Range(0f, 3f)] public float multiplier; // 1.0 = normal, 0.75 = resist, 1.25 = weak
}

public class EnemyResistances : MonoBehaviour
{
    [Tooltip("Leave empty for all 1.0x. Only add entries you want to customize.")]
    public ElementMultiplier[] overrides;

    public float GetMultiplier(DamageElement el)
    {
        for (int i = 0; i < overrides.Length; i++)
        {
            if (overrides[i].element == el) return Mathf.Max(0f, overrides[i].multiplier);
        }
        return 1f;
    }
}
