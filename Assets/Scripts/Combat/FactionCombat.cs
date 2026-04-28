using UnityEngine;

public static class FactionCombat
{
    public static bool TryApplyDamage(GameObject target, DamagePacket packet, FactionMember attacker, bool applyPlayerKnockback)
    {
        if (target == null)
            return false;

        FactionMember targetFaction = target.GetComponentInParent<FactionMember>();
        if (attacker != null && targetFaction == attacker)
            return false;

        if (attacker != null && targetFaction != null && !FactionTargeting.AreHostile(attacker, targetFaction))
            return false;

        PlayerHealth playerHealth = target.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(packet, applyPlayerKnockback);
            return true;
        }

        EnemyHealth enemyHealth = target.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(packet, attacker);
            return true;
        }

        return false;
    }
}
