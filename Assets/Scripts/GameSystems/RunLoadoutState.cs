using UnityEngine;

public enum StartingWeaponChoice
{
    EmberRepeater,
    FrostLance,
    VenomCaster,
    StormNeedler
}

public enum StartingBombChoice
{
    FragBomb,
    FrostBomb,
    FireBomb,
    ShockBomb
}

public enum StartingSkillChoice
{
    MagneticPulse,
    ArcaneShield,
    FrostNova
}

public enum StartingPassiveChoice
{
    Vitality,
    Swiftness,
    Magnetism,
    Overclock
}

public enum PlayableCharacterChoice
{
    HumanVanguard,
    HumanRanger,
    HumanArcanist
}

public static class RunLoadoutState
{
    public static PlayableCharacterChoice CharacterChoice { get; private set; } = PlayableCharacterChoice.HumanVanguard;
    public static StartingWeaponChoice WeaponChoice { get; private set; } = StartingWeaponChoice.EmberRepeater;
    public static StartingBombChoice BombChoice { get; private set; } = StartingBombChoice.FragBomb;
    public static StartingSkillChoice SkillChoice { get; private set; } = StartingSkillChoice.MagneticPulse;
    public static StartingPassiveChoice PassiveChoice { get; private set; } = StartingPassiveChoice.Vitality;

    public static void CycleCharacter(int delta)
    {
        CharacterChoice = CycleEnum(CharacterChoice, delta);
    }

    public static void CycleWeapon(int delta)
    {
        WeaponChoice = CycleEnum(WeaponChoice, delta);
    }

    public static void CycleBomb(int delta)
    {
        BombChoice = CycleEnum(BombChoice, delta);
    }

    public static void CycleSkill(int delta)
    {
        SkillChoice = CycleEnum(SkillChoice, delta);
    }

    public static void CyclePassive(int delta)
    {
        PassiveChoice = CycleEnum(PassiveChoice, delta);
    }

    public static string GetCharacterName(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Waywatch Ranger";
            case PlayableCharacterChoice.HumanArcanist:
                return "Runebound Arcanist";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Iron Vanguard";
        }
    }

    public static string GetCharacterRole(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Human Skirmisher";
            case PlayableCharacterChoice.HumanArcanist:
                return "Human Spellfighter";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Human Frontliner";
        }
    }

    public static string GetCharacterDescription(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "A fast survivor who keeps the projectile weapons and opens with more ranged support.";
            case PlayableCharacterChoice.HumanArcanist:
                return "A fragile caster who channels the selected primary into a spell beam instead of bullets.";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "A durable melee-only leader who drops into the fight with a guard squad and a forward cleave.";
        }
    }

    public static string GetCharacterStatsSummary(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return "Projectile weapons  |  +12% move speed  |  +10% fire rate";
            case PlayableCharacterChoice.HumanArcanist:
                return "Arcane spell beam  |  +15% damage  |  +1 pickup radius";
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return "Melee only  |  +35 max HP  |  Vanguard Cleave";
        }
    }

    public static string GetCharacterAllySummary(PlayableCharacterChoice choice)
    {
        return $"Starts with {GetCharacterMeleeAllyCount(choice)} melee allies and {GetCharacterRangedAllyCount(choice)} ranged allies.";
    }

    public static int GetCharacterMaxHpBonus(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return 10;
            case PlayableCharacterChoice.HumanArcanist:
                return 0;
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return 35;
        }
    }

    public static float GetCharacterMoveSpeedBonus(PlayableCharacterChoice choice)
    {
        return choice == PlayableCharacterChoice.HumanRanger ? 0.12f : 0f;
    }

    public static float GetCharacterFireRateBonus(PlayableCharacterChoice choice)
    {
        return choice == PlayableCharacterChoice.HumanRanger ? 0.1f : 0f;
    }

    public static float GetCharacterDamageBonus(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanArcanist:
                return 0.15f;
            case PlayableCharacterChoice.HumanVanguard:
                return 0.08f;
            default:
                return 0f;
        }
    }

    public static float GetCharacterPickupRadiusBonus(PlayableCharacterChoice choice)
    {
        return choice == PlayableCharacterChoice.HumanArcanist ? 1f : 0f;
    }

    public static int GetCharacterMeleeAllyCount(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return 1;
            case PlayableCharacterChoice.HumanArcanist:
                return 0;
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return 2;
        }
    }

    public static int GetCharacterRangedAllyCount(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return 3;
            case PlayableCharacterChoice.HumanArcanist:
                return 2;
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return 2;
        }
    }

    public static float GetCharacterFormationRadius(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return 1.75f;
            case PlayableCharacterChoice.HumanArcanist:
                return 1.35f;
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return 1.55f;
        }
    }

    public static Color GetCharacterTint(PlayableCharacterChoice choice)
    {
        switch (choice)
        {
            case PlayableCharacterChoice.HumanRanger:
                return new Color(0.72f, 1f, 0.44f, 1f);
            case PlayableCharacterChoice.HumanArcanist:
                return new Color(0.58f, 0.86f, 1f, 1f);
            case PlayableCharacterChoice.HumanVanguard:
            default:
                return new Color(1f, 0.92f, 0.62f, 1f);
        }
    }

    public static string BuildCharacterSummary()
    {
        return $"{GetCharacterName(CharacterChoice)}  |  {GetCharacterStatsSummary(CharacterChoice)}";
    }

    public static string GetWeaponName(StartingWeaponChoice choice)
    {
        switch (choice)
        {
            case StartingWeaponChoice.EmberRepeater:
                return "Ember Repeater";
            case StartingWeaponChoice.FrostLance:
                return "Frost Lance";
            case StartingWeaponChoice.VenomCaster:
                return "Venom Caster";
            case StartingWeaponChoice.StormNeedler:
                return "Storm Needler";
            default:
                return choice.ToString();
        }
    }

    public static string GetWeaponDescription(StartingWeaponChoice choice)
    {
        switch (choice)
        {
            case StartingWeaponChoice.EmberRepeater:
                return "Fast fire, reliable burn chance, easy to scale with projectile upgrades.";
            case StartingWeaponChoice.FrostLance:
                return "Slower, heavier rounds with pierce and slowing hits for better crowd control.";
            case StartingWeaponChoice.VenomCaster:
                return "Poison rounds with splash pressure that soften clustered enemies.";
            case StartingWeaponChoice.StormNeedler:
                return "Sharp, fast lightning shots that lean into precision and burst damage.";
            default:
                return string.Empty;
        }
    }

    public static string GetBombName(StartingBombChoice choice)
    {
        switch (choice)
        {
            case StartingBombChoice.FragBomb:
                return "Frag Bomb";
            case StartingBombChoice.FrostBomb:
                return "Frost Bomb";
            case StartingBombChoice.FireBomb:
                return "Fire Bomb";
            case StartingBombChoice.ShockBomb:
                return "Shock Bomb";
            default:
                return choice.ToString();
        }
    }

    public static string GetBombDescription(StartingBombChoice choice)
    {
        switch (choice)
        {
            case StartingBombChoice.FragBomb:
                return "Big physical blast on Q. Straight panic button.";
            case StartingBombChoice.FrostBomb:
                return "Larger slow field on Q for breathing room when the screen gets crowded.";
            case StartingBombChoice.FireBomb:
                return "Explosive burst with a burning follow-up to finish weakened packs.";
            case StartingBombChoice.ShockBomb:
                return "Electric detonation with shock status and a slightly faster reuse pace.";
            default:
                return string.Empty;
        }
    }

    public static string GetSkillName(StartingSkillChoice choice)
    {
        switch (choice)
        {
            case StartingSkillChoice.MagneticPulse:
                return "Magnetic Pulse";
            case StartingSkillChoice.ArcaneShield:
                return "Arcane Shield";
            case StartingSkillChoice.FrostNova:
                return "Frost Nova";
            default:
                return choice.ToString();
        }
    }

    public static string GetSkillDescription(StartingSkillChoice choice)
    {
        switch (choice)
        {
            case StartingSkillChoice.MagneticPulse:
                return "Pushes nearby enemies back and tugs nearby pickups toward you on E.";
            case StartingSkillChoice.ArcaneShield:
                return "Grants a short burst of invulnerability on E when the screen gets dangerous.";
            case StartingSkillChoice.FrostNova:
                return "Unleashes a cold burst around you that freezes nearby enemies in place for a short window.";
            default:
                return string.Empty;
        }
    }

    public static string GetPassiveName(StartingPassiveChoice choice)
    {
        switch (choice)
        {
            case StartingPassiveChoice.Vitality:
                return "Vitality";
            case StartingPassiveChoice.Swiftness:
                return "Swiftness";
            case StartingPassiveChoice.Magnetism:
                return "Magnetism";
            case StartingPassiveChoice.Overclock:
                return "Overclock";
            default:
                return choice.ToString();
        }
    }

    public static string GetPassiveDescription(StartingPassiveChoice choice)
    {
        switch (choice)
        {
            case StartingPassiveChoice.Vitality:
                return "+25 max HP and a stronger opening safety buffer.";
            case StartingPassiveChoice.Swiftness:
                return "+18% move speed for dodging and repositioning.";
            case StartingPassiveChoice.Magnetism:
                return "+2 pickup radius so XP and drops come to you faster.";
            case StartingPassiveChoice.Overclock:
                return "+20% fire rate for a stronger early damage ramp.";
            default:
                return string.Empty;
        }
    }

    public static string BuildSummary()
    {
        return $"Character: {GetCharacterName(CharacterChoice)}  |  Weapon: {GetWeaponName(WeaponChoice)}  |  Bomb: {GetBombName(BombChoice)}  |  Skill: {GetSkillName(SkillChoice)}  |  Passive: {GetPassiveName(PassiveChoice)}";
    }

    public static string BuildKitSummary()
    {
        return $"Weapon: {GetWeaponName(WeaponChoice)}  |  Bomb: {GetBombName(BombChoice)}  |  Skill: {GetSkillName(SkillChoice)}  |  Passive: {GetPassiveName(PassiveChoice)}";
    }

    private static T CycleEnum<T>(T current, int delta) where T : struct
    {
        T[] values = (T[])System.Enum.GetValues(typeof(T));
        int currentIndex = System.Array.IndexOf(values, current);
        int nextIndex = (currentIndex + delta) % values.Length;

        if (nextIndex < 0)
            nextIndex += values.Length;

        return values[nextIndex];
    }
}
