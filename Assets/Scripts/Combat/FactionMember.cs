using UnityEngine;

public class FactionMember : MonoBehaviour
{
    [SerializeField] private FactionType faction = FactionType.Zombie;
    [SerializeField] private bool targetable = true;

    public FactionType Faction => faction;
    public bool Targetable => targetable;

    public void Configure(FactionType newFaction)
    {
        faction = newFaction;

        FactionVisualIdentity visualIdentity = GetComponent<FactionVisualIdentity>();
        if (visualIdentity != null)
            visualIdentity.Refresh();
    }

    public static FactionMember Ensure(GameObject target, FactionType defaultFaction)
    {
        if (target == null)
            return null;

        FactionMember member = target.GetComponent<FactionMember>();
        if (member != null)
            return member;

        member = target.AddComponent<FactionMember>();
        member.Configure(defaultFaction);
        return member;
    }
}
