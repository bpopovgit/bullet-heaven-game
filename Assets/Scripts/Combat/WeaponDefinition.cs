using UnityEngine;

[CreateAssetMenu(menuName = "Game/Weapon Definition")]
public class WeaponDefinition : ScriptableObject
{
    public string displayName;
    public DamageElement element = DamageElement.Physical;
    public StatusEffect onHitEffect = StatusEffect.None;
    [Range(0f, 1f)] public float effectChance = 0.3f; // 30% by default
    public int baseDamage = 10;
    public float shotsPerSecond = 6f;
    public float bulletSpeed = 20f;
    [Tooltip("0 = single target. >0 = splash radius in world units")]
    public float splashRadius = 0f;
    [Tooltip("How many targets the projectile can pass through before despawn")]
    public int pierce = 0;
    public GameObject bulletPrefab; // your bullet
}
