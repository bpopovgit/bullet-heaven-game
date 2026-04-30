# Track A — Behaviour-Modifier Talents Summary

## Goal

The talent refactor delivered structural depth (4-tier trees, mutex capstones, point gates) but most nodes were still numerical buffs. Track A's promise was to make at least 4–6 capstones *change how the run feels*, not just *make the numbers bigger*. That's what shipped.

## What shipped

### Six capstones converted from stat → behaviour

| Capstone | Old effect (stat buff) | New effect (behaviour) |
|---|---|---|
| **`atk_primary_overcharge`** | +22% damage, +CD reduction | Every Nth shot fires a free burst of extra projectiles in a straight line |
| **`atk_primary_annihilator`** | +20% damage, +pierce/etc. | Enemies below an HP threshold take bonus damage from all player-sourced hits |
| **`atk_element_avatar`** | +20% damage, +pickup radius | When a statused enemy dies, their status spreads to nearby enemies |
| **`atk_element_unleashed`** | +14% fire rate / status / cd | Casting your E skill deals burst damage to nearby statused enemies and consumes their status |
| **`atk_bomb_chained`** | -0.55s bomb CD, +12% damage | Each bomb fires a smaller aftershock blast 0.45s after the primary blast |
| **`def_field_phaseshift`** | -0.55s skill CD, +pickup radius | Activating E teleports the player toward the cursor before the skill fires |

### Three tier-3 nodes given behaviour-modifier secondaries

These act as a *preview* of their tree's capstone — investing deep gives a taste before committing the row's 9-point gate.

| Tier-3 node | Behaviour secondary | Hints at capstone |
|---|---|---|
| `atk_element_master` | OnKillStatusSpreadRadius (small) | Avatar |
| `atk_bomb_rhythm_master` | BombSecondaryBlastFraction (small) | Chained |
| `def_field_magnet_master` | SkillBlinkDistance (small) | Phaseshift |

### New shared component: `PlayerCombatModifiers`

`Assets/Scripts/PlayerScripts/PlayerCombatModifiers.cs` is the single home for all the new behaviour state:

- `ExecuteHpThreshold` / `ExecuteBonusDamage` (Annihilator)
- `BurstShotFrequency` / `BurstShotInterval` / `BurstShotProjectiles` (Overcharge)
- `OnKillStatusSpreadRadius` / `OnKillStatusSpreadStrength` (Avatar)
- `SkillElementBurstDamage` (Unleashed)
- `BombSecondaryBlastFraction` / `BombSecondaryBlastDelay` (Chained)
- `SkillBlinkDistance` (Phaseshift)

Plus helpers (`ApplyExecuteIfApplicable`, `TrySpreadStatusOnKill`) so combat code can ask the modifier "should I do anything special?" instead of every system implementing its own check.

A static `Instance` reference is set on Awake. Player-side code reads via `PlayerCombatModifiers.Instance`. Decision: I picked a singleton because there's exactly one player and the alternative — passing `PlayerCombatModifiers` through every damage path — would have meant changing many signatures.

### Eight new `PlayerUpgradeType` values

`ExecuteThreshold`, `ExecuteBonusDamage`, `BurstShotFrequency`, `OnKillStatusSpreadRadius`, `OnKillStatusSpreadStrength`, `SkillElementBurstDamage`, `BombSecondaryBlastFraction`, `SkillBlinkDistance`. Each routes to a corresponding `PlayerCombatModifiers.AddXxx` method via the existing `PlayerUpgradeOption.ApplyUpgrade` switch. They all return `true` from `UpgradeTypeMatchesCharacter`, meaning every character can roll them.

### Combat hook points touched

- **`PlayerShooting.ShootOnce`** — increments a shot counter, fires Overcharge burst at the right interval.
- **`EnemyHealth.TakeDamage`** — checks if the attacker is player-sourced, applies execute multiplier when target is below the HP threshold.
- **`EnemyHealth.Die`** — calls `PlayerCombatModifiers.TrySpreadStatusOnKill` if the dying enemy still has an active status.
- **`StatusReceiver`** — now tracks `MostRecentStatus`/`Duration`/`Strength`/`ExpiresAt` so on-kill spread knows what to copy. Added `ClearMostRecentStatus()` so Unleashed can consume status.
- **`PlayerSecondaryActiveSkill.TryActivate`** — wraps the existing `ActivateCurrentSkill()` with `TryPhaseShiftBlink()` (before) and `TryConsumeStatusForBurst()` (after).
- **`PlayerBombProjectile.Explode`** — refactored to call a shared `DetonateAt(position, damageFraction, isSecondary)` so the primary blast and the aftershock can use the same path; the aftershock is scheduled via a coroutine.

### Effect text builder extended

`TalentCatalog.BuildSingleAppliedEffectText` now formats all 8 new types into player-readable strings (e.g. "+15% execute HP threshold", "adds a 30% aftershock blast", "+4 E skill blink toward cursor"). The level-up popup and the pre-run preview both read this.

### Build status

`dotnet build Assembly-CSharp.csproj` → 0 errors. The same 43 pre-existing warnings (all about Unity API deprecations in code I didn't touch).

---

## Critical review

### What's genuinely good

1. **The capstones now read as different verbs in build talk.** "Annihilator build" and "Avatar build" describe meaningfully different ways of playing. The talent system delivers on the premise we set up two refactors ago.
2. **Architecture is clean.** A single component holds the new state. Combat code reads via a static reference. No new coupling between damage sources.
3. **Existing systems unchanged at the API level.** `PlayerUpgradeOption` consumers (LevelUpManager, the talent catalog) continue to work. The level-up popup shows the new effects through the existing description format.
4. **Tier-3 → capstone chain reads correctly.** Picking deep into a branch builds toward its capstone instead of just being a stepping stone.

### Honest gaps and risks

1. **Nothing has been play-tested.** Every behaviour was implemented blind; I have no way to verify in Unity. Bugs are likely. The most fragile bits: the bomb aftershock coroutine, the blink Rigidbody2D.position write, the on-kill OverlapCircle.
2. **Description vs. behaviour mismatch on Overcharge.** I called it a "piercing burst" in the design notes but didn't actually grant extra pierce — the burst projectiles use the player's normal pierce stat. Either the description should change ("a free volley") or the burst should temporarily bump pierce. Worth deciding before the next play-test.
3. **Annihilator's execute applies to all Human-faction damage**, not strictly the player's bullets. Human ally squad bullets benefit too. I judged this thematic ("crossfire drills + annihilator = Human army executes weakened enemies") but it's worth confirming play-tests don't reveal weird edge cases like ally bullets accidentally one-shotting bosses below the threshold.
4. **Avatar of [Element] is mis-named for some builds.** The runtime spread reads the dying enemy's *current* status and spreads that. So a Frost Lance player's Avatar spreads slow/freeze, an Ember Repeater player spreads burn — the talent name "Avatar of the Element" works for any element, but the per-talent flavour text we wrote earlier still leans on "fire spreads burn" thinking. Worth sweeping the descriptions when we polish text.
5. **Element Unleashed damage is flat 14/pt.** It does not scale with `PlayerStats.DamageMultiplier`. Late-run scaling will likely feel weak. Easy fix: multiply by `stats.DamageMultiplier` in `TryConsumeStatusForBurst`. Deferred to avoid premature tuning.
6. **All numbers are guessed.** 15% per-point execute threshold, 0.30 aftershock fraction, 4m blink, 14-damage element burst — none are tuned. Expect to revisit these after first play-test.
7. **No VFX for the new behaviours yet.** Burst shots look identical to normal bullets. Aftershocks reuse `BombExplosionVisual`. Blink has no flash, element burst has no popup. None of this is blocking but it'll be visible in play.
8. **Phaseshift blink uses `Rigidbody2D.position` directly.** No collision check. Currently fine because the arena has no walls, but **this will teleport through obstacles once Track D adds them** — flag for Track D.
9. **Tier-3 hints might not be obvious to players.** A 1m on-kill spread radius from `atk_element_master` is small enough you might not notice. The point of the hint is to teach the build path; if it's invisible we're not communicating well. Consider bumping the per-point amounts on the tier-3 secondaries after first play-test.
10. **The catalog's per-row effect-text lambdas (e.g. `context => "+0.7 pickup radius and +6% movement speed per point."`) for the changed tier-3 nodes are now stale.** They're dead code in practice (`BuildOption` always returns non-null), but it's misleading. Cleanup task — not blocking.
11. **`Assembly-CSharp.csproj` was edited manually** to add the new `PlayerCombatModifiers.cs` so my CLI compile-check would pass. **Unity will regenerate this file when the Editor opens** — the regenerated version will include the new file automatically (Unity scans `Assets/` for `.cs`), so my edit is throwaway. Mentioned only so it doesn't confuse you when you see git diffs on the csproj.

### What I deliberately did *not* do

- **Did not add new VFX/audio.** Visual polish is its own pass.
- **Did not touch `PlayerStats` for these behaviours.** Behaviour-modifier state lives on `PlayerCombatModifiers` to keep `PlayerStats` focused on numerical multipliers.
- **Did not add tertiary-upgrade support to `PlayerUpgradeOption`.** The existing primary + one secondary slot is sufficient for everything we converted.
- **Did not migrate the catalog to ScriptableObjects.** Same reasoning as the prior refactor — out of scope.
- **Did not respec / undo logic.** PlayerCombatModifiers fields only grow. If we add a respec system later, it'll need a Reset method.

---

## Files touched

- **New:** `Assets/Scripts/PlayerScripts/PlayerCombatModifiers.cs`
- `Assets/Scripts/PlayerScripts/PlayerUpgradeOption.cs` — 8 new enum values, ApplyUpgrade fans out to `PlayerCombatModifiers`
- `Assets/Scripts/PlayerScripts/PlayerShooting.cs` — Overcharge burst hook
- `Assets/Scripts/Combat/EnemyHealth.cs` — Annihilator execute check, Avatar on-kill spread call
- `Assets/Scripts/Combat/StatusReceiver.cs` — `MostRecentStatus*` tracking, `ClearMostRecentStatus()`
- `Assets/Scripts/PlayerScripts/PlayerSecondaryActiveSkill.cs` — Phaseshift blink + Unleashed burst
- `Assets/Scripts/PlayerScripts/PlayerBombProjectile.cs` — refactored `Explode` → `DetonateAt`, Chained aftershock coroutine
- `Assets/Scripts/GameSystems/TalentCatalog.cs` — 6 capstone option builders rewritten, 3 tier-3 builders extended, `BuildSingleAppliedEffectText` extended
- `Assembly-CSharp.csproj` — added the new `.cs` file (will be overwritten by Unity Editor on next launch; harmless)
- **Bonus fix unrelated to Track A:** `Assets/Scripts/GameSystems/GameAudio.cs` had a stray `2` glued to the first `using` line (parsed as `2using System;` → "; expected"). Removed.

## What I'd verify in Unity first

1. **Annihilator** — pick a 2-pt build, walk into a low-HP enemy with the primary, confirm the kill happens earlier than without the talent.
2. **Overcharge** — fire continuously, count to 8, watch for the bonus burst on the 8th shot.
3. **Avatar** — set yourself up with a status-applying weapon (Frost Lance, Ember Repeater), kill a statused enemy, confirm nearby enemies pick up the same status.
4. **Unleashed** — apply status to a clump of enemies, fire your E, confirm the clump takes the burst damage and loses status.
5. **Chained** — drop a bomb, watch for the smaller aftershock ~0.45s later.
6. **Phaseshift** — aim cursor away from the player, hit E, confirm you teleport toward the cursor before the AoE fires.
7. **Tier-3 hints** — pick `atk_element_master` and confirm a faint on-kill spread fires before reaching the Avatar capstone.

If any of these don't work or feel wrong, the most likely culprits — in rough order — are the bomb coroutine (#5), the blink Rigidbody position write (#6), or the on-kill spread overlap (#3).

## What's next

Track B (faction-warfare backdrop) is the recommended next track. Before starting it, we should:
- Play-test Track A and adjust the numbers / fix any bugs.
- Decide on the Overcharge "piercing" misalignment (description vs. behaviour).
- Consider whether to scale Element Unleashed damage by `PlayerStats.DamageMultiplier` for late-game viability.
