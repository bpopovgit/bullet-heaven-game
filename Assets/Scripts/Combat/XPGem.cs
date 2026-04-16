using UnityEngine;

public class XPGem : PlayerPickup
{
    [SerializeField] private int experienceValue = 1;

    public void SetExperienceValue(int value)
    {
        experienceValue = Mathf.Max(1, value);
    }

    protected override bool CanCollect(GameObject player)
    {
        return player.GetComponent<PlayerExperience>() != null;
    }

    protected override void OnCollected(GameObject player)
    {
        PlayerExperience playerExperience = player.GetComponent<PlayerExperience>();
        playerExperience.AddExperience(Mathf.Max(1, experienceValue));
    }

    public static XPGem SpawnDefault(Vector3 position, int value)
    {
        GameObject go = new GameObject("XP Gem");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.35f;

        PickupSpriteFactory.AddDefaultRenderer(go, new Color(0.2f, 1f, 0.35f), sortingOrder: 5);
        PickupSpriteFactory.AddDefaultPhysics(go, radius: 0.5f);

        XPGem gem = go.AddComponent<XPGem>();
        gem.SetExperienceValue(value);

        return gem;
    }
}
