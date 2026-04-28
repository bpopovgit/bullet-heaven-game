using UnityEngine;

public static class FactionTargeting
{
    public static bool AreHostile(FactionMember attacker, FactionMember target)
    {
        if (attacker == null || target == null)
            return false;

        return GetTargetPriority(attacker.Faction, target.Faction) > 0;
    }

    public static bool AreHostile(FactionType attacker, FactionType target)
    {
        return GetTargetPriority(attacker, target) > 0;
    }

    public static int GetTargetPriority(FactionType attacker, FactionType target)
    {
        if (attacker == FactionType.Neutral || target == FactionType.Neutral || attacker == target)
            return 0;

        switch (attacker)
        {
            case FactionType.Demon:
                if (target == FactionType.Angel) return 100;
                if (target == FactionType.Human) return 70;
                if (target == FactionType.Zombie) return 35;
                break;

            case FactionType.Angel:
                if (target == FactionType.Demon) return 100;
                if (target == FactionType.Human) return 70;
                if (target == FactionType.Zombie) return 35;
                break;

            case FactionType.Human:
                if (target == FactionType.Zombie) return 100;
                if (target == FactionType.Demon || target == FactionType.Angel) return 55;
                break;

            case FactionType.Zombie:
                return 60;
        }

        return 0;
    }

    public static FactionMember FindBestTarget(FactionMember seeker, Vector3 origin, float maxRange = 0f)
    {
        if (seeker == null)
            return null;

        FactionMember[] candidates = Object.FindObjectsOfType<FactionMember>();
        FactionMember bestTarget = null;
        int bestPriority = 0;
        float bestDistance = float.MaxValue;
        float maxRangeSqr = maxRange > 0f ? maxRange * maxRange : float.MaxValue;

        for (int i = 0; i < candidates.Length; i++)
        {
            FactionMember candidate = candidates[i];
            if (!IsValidTarget(seeker, candidate))
                continue;

            Vector3 offset = candidate.transform.position - origin;
            float distanceSqr = offset.sqrMagnitude;
            if (distanceSqr > maxRangeSqr)
                continue;

            int priority = GetTargetPriority(seeker.Faction, candidate.Faction);
            if (priority <= 0)
                continue;

            float distance = Mathf.Sqrt(distanceSqr);
            if (bestTarget == null || priority > bestPriority || (priority == bestPriority && distance < bestDistance))
            {
                bestTarget = candidate;
                bestPriority = priority;
                bestDistance = distance;
            }
        }

        return bestTarget;
    }

    private static bool IsValidTarget(FactionMember seeker, FactionMember candidate)
    {
        if (candidate == null || candidate == seeker || !candidate.Targetable)
            return false;

        if (candidate.gameObject == seeker.gameObject || !candidate.isActiveAndEnabled)
            return false;

        EnemyHealth enemyHealth = candidate.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null && enemyHealth.IsDead)
            return false;

        PlayerHealth playerHealth = candidate.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && playerHealth.CurrentHP <= 0)
            return false;

        return true;
    }
}
