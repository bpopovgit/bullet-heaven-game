using UnityEngine;

public class FactionSkirmishUnit : MonoBehaviour
{
    public string SkirmishId { get; private set; }
    public bool IsSideA { get; private set; }

    public void Bind(string skirmishId, bool isSideA)
    {
        SkirmishId = skirmishId;
        IsSideA = isSideA;
    }
}
