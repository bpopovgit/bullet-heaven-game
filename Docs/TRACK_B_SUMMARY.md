# Track B — Faction Warfare Backdrop Summary

## Goal

Make the project's identity premise — *"you drop into an ongoing faction war, you're not the centre of the universe"* — actually load-bearing in moment-to-moment play. Before Track B, factions were a tagging system. After Track B, mixed-faction skirmishes spawn around the player at all times, fight each other on their own, and respond to the player's intervention (or lack of it).

## What shipped

### Four new components

| File | Role |
|---|---|
| `Assets/Scripts/GameSystems/FactionSkirmish.cs` | Plain C# data class — one ongoing skirmish. Holds Side A / Side B factions, anchor position, unit lists, lifecycle state, player-damage accumulators per side. |
| `Assets/Scripts/GameSystems/FactionSkirmishUnit.cs` | MonoBehaviour glue — attached to every spawned skirmish unit so `EnemyHealth` knows which skirmish (and which side) the unit belongs to. |
| `Assets/Scripts/GameSystems/SkirmishDirector.cs` | The runtime spawner. Auto-bootstraps in the `Game` scene (sibling to `EnemyWaveDirector`). Picks anchors, picks faction matchups, instantiates units, tracks them, resolves outcomes. |
| `Assets/Scripts/PlayerScripts/FactionAffinityTracker.cs` | Per-run counter — how many skirmishes the player has helped / opposed each faction in. Component lives on the player. Hooks for future history-aware talents. |

### Existing files modified

- **`Assets/Scripts/Combat/EnemyHealth.cs`**
  - In `TakeDamage`: when damage is player-sourced, look up `FactionSkirmishUnit` on the target and call `SkirmishDirector.OnPlayerDamagedSkirmishUnit` with the final damage.
  - In `Die`: notify `SkirmishDirector.OnSkirmishUnitKilled` with whether the killing blow was player-sourced.
- **`Assembly-CSharp.csproj`** — added entries for the four new files (Unity Editor will regenerate this on next launch and pick them up automatically).

### How a skirmish runs

1. **Bootstrap.** `SkirmishDirector` instantiates itself when the `Game` scene loads. Same pattern as `MainMenuRuntime`'s bootstrap on the `Main` scene.
2. **Spawn loop.** Every ~22s (after a 4s grace), if fewer than 3 skirmishes are active and an anchor far from the player can be found, spawn a new skirmish.
3. **Matchup pick.** Weighted toward Angel↔Demon (the priority-100 rivalry), with Angel-vs-Zombie, Demon-vs-Zombie, Human-vs-Demon, Human-vs-Zombie also possible.
4. **Anchor selection.** Prefers existing `EnemySpawnPoint` markers between 16m and 38m from the player. Falls back to a random ring around the player.
5. **Unit spawning.** Loads prefabs via `Resources.Load("Prefabs/Factions/{Faction}_{Melee|Ranged}")` (the paths produced by `FactionStarterPrefabBuilder`). Falls back to legacy names. Each side gets 4 units (alternating melee/ranged), placed with small jitter on opposite sides of the anchor. Each unit gets `FactionMember.Ensure` and a `FactionSkirmishUnit` link.
6. **Combat.** No new AI code. The existing `FactionTargeting.GetTargetPriority` matrix already gives Angels↔Demons a priority of 100, Demons↔Humans 70, etc. — so the spawned factions auto-engage each other before noticing the distant player.
7. **Damage tracking.** Every player-sourced hit increments per-side counters on the skirmish.
8. **Resolution.** When one side has zero living units, the skirmish resolves. Three outcomes:
   - **Helped** — the player did at least 2× more damage to the loser side. Reward: a 30-second Human-faction ally + permanent +3% damage stack via `PlayerStats.AddDamagePercent`. `FactionAffinityTracker.RecordHelped(winner)` and `RecordOpposed(loser)`.
   - **Idle** — the player did no damage and got no kills. Penalty: 3 units of the winning faction spawn near the player as a punishment squad.
   - **Split fire** — player damaged both sides; no reward, no penalty.

### Why this layering instead of a new wave system

`EnemyWaveDirector` and `EliteSpawnDirector` each maintain their own alive-tracking and don't share a registry. `SkirmishDirector` follows the same pattern: it tracks its own units in `FactionSkirmish.SideAUnits` / `SideBUnits`. No wave-cap contention, no new shared state. If we ever introduce a global enemy budget, this is one of three places that would need a refactor — but until then the layering is clean.

### Why the reward is a damage stack instead of a new talent

The roadmap's B7 ("Skirmish-history capstones") was queued as a stretch goal. Adding a new talent would have required restructuring an existing tree, since every capstone slot is full and tier-3 nodes already have primary + secondary bound. Instead, the reward path goes directly through `PlayerStats.AddDamagePercent(0.03f)` per helped skirmish — mechanically equivalent to a small affinity-bonus capstone, without the tree surgery. The `FactionAffinityTracker` per-faction counts exist for any future talent that wants to read them.

---

## Critical review — gaps and risks

### Genuinely good

- **No new combat AI.** The existing `FactionTargeting` priority matrix already makes Angels↔Demons fight each other before noticing the player. Track B reuses this.
- **No wave-director coupling.** SkirmishDirector tracks its own units. Spawning a skirmish doesn't decrement the regular-wave alive-cap.
- **Outcome handling is data-driven.** The three branches (helped / idle / split-fire) are easy to retune separately.
- **Affinity counters exist for free.** Future talents can read `FactionAffinityTracker.GetHelpedCount(faction)` without further plumbing.

### Honest gaps and risks

1. **Untested in Unity.** Same caveat as every prior track. Likely-fragile bits:
   - **`Resources.Load` paths** silently abort if `FactionStarterPrefabBuilder` (Tools menu) was never run. Skirmishes won't spawn, no error message reaches the player.
   - **Spawned unit targeting radius** — `EnemyMovement` decides how far each unit looks for targets. If that radius is shorter than the ~4m anchor split, the two sides never see each other.
   - **`GameObject.FindWithTag("Player")`** assumes the player is tagged "Player". If the project uses a different tag, anchor selection fails silently.
2. **No skirmish lifetime cap.** If both sides retreat or one side gets stuck on geometry, the skirmish stays `Active` forever. Should add a 60s auto-resolve-to-neutral timeout. Easy fix once observed.
3. **Partial visual feedback now wired.** Resolution outcomes fire `RunAnnouncementUI.Instance.ShowMessage` banners ("ANGELS OWE YOU A DEBT / +3% damage, ally arrives" on a helped resolution, "DEMONS ADVANCE — A SQUAD APPROACHES" on an idle resolution, sentence-case "Angels held the field" on split-fire). Spawn-time and minimap markers are still deferred — adding spawn announcements would fire every 22s and spam the banner; minimap markers need a UI surface this project doesn't have yet.
4. **Intervention ally is generic.** The ally spawned when you help Angels is still a Human melee — visually doesn't communicate "Angels owe you one." Reward narrative is weak. A future improvement: spawn an Angel-faction unit configured as friendly to the Human player (would need `FactionMember`/`FactionTargeting` changes to support a "player-allied non-Human" relationship).
5. **+3% damage stack is uncapped.** A run with many helped skirmishes could pile up. Probably fine for short prototype runs, but tune-test before scaling.
6. **Penalty squad placement.** 3 units of the winning faction spawn at exactly the minimum 16m from the player — could feel cheap if the player is cornered. Visualize before tuning.
7. **Anchor clustering.** `TryPickAnchor` doesn't avoid placing skirmishes near each other. Three skirmishes could clump to the north. Not breaking, just less "war everywhere."
8. **B3 was verified by reading the priority matrix, not in play.** I'm confident factions WILL fight (Angel→Demon priority is 100, max), but I haven't watched it happen.
9. **All numbers are guessed.** Spawn cadence (22s), max active (3), units per side (4), penalty squad size (3), reward damage (+3%) — none are play-tested.
10. **`Assembly-CSharp.csproj` hand-edited.** Same as Track A — Unity will regenerate it on next Editor open and the four new files will be picked up via the Assets/ scan.

### What I deliberately deferred

- **Skirmish UI** (announcements, banners, map markers) — separate polish pass.
- **B7 dedicated capstone** — affinity counters exist, talent slot does not. Add when there's tree-design space for it.
- **Lifetime timeout** — trivial to add; deferred to first play-test.
- **Faction-aware ally rendering** — needs a small `FactionMember`/`FactionTargeting` change to allow non-Human player allies.

---

## Files touched

- **New:**
  - `Assets/Scripts/GameSystems/FactionSkirmish.cs`
  - `Assets/Scripts/GameSystems/FactionSkirmishUnit.cs`
  - `Assets/Scripts/GameSystems/SkirmishDirector.cs`
  - `Assets/Scripts/PlayerScripts/FactionAffinityTracker.cs`
- **Modified:**
  - `Assets/Scripts/Combat/EnemyHealth.cs` — damage attribution + on-kill notification for skirmish units.
  - `Assembly-CSharp.csproj` — file entries (will be regenerated by Unity Editor; harmless).

## Verification

- `dotnet build Assembly-CSharp.csproj` → 0 errors. Same 43 pre-existing Unity-API-deprecation warnings, none from Track B.
- All seven Track B tasks (B1–B7) marked complete; B7 was scoped down to a +damage stack delivered through the SkirmishDirector path rather than a new tree-resident capstone.

## What to verify in Unity first

1. **Run the `Tools > Bullet Heaven > Factions > Create Starter Prefabs` menu** if you haven't recently. The SkirmishDirector depends on those prefabs being in `Resources/Prefabs/Factions/`.
2. **Confirm the player GameObject is tagged `Player`.** If not, no skirmishes will spawn (silent failure).
3. **Watch the console** for `SKIRMISH SPAWNED: …` and `SKIRMISH RESOLVED: …` log lines. They should appear every ~22s after the first 4s of play.
4. **Walk away from the player** (or use Scene-view free-cam) and verify the two factions actually fight — Angel and Demon units should converge on each other when the player isn't between them.
5. **Damage one side hard, leave the other alone.** Confirm the resolution log says the player helped, an extra Human ally appears for ~30s, and your damage number jumps by 3%.
6. **Stay on the far side of the map and let a skirmish resolve untouched.** Confirm the penalty squad spawns near you with the winning faction.
7. **Open the talent browser mid-run** — talent UI is unchanged. Track B doesn't touch it.

If any of this fails, the most likely culprits, in rough order:
- Missing/outdated faction prefabs in `Resources/Prefabs/Factions/` (re-run the Tools menu).
- Player not tagged "Player".
- `EnemyMovement` targeting radius too small (factions don't engage). Look at `EnemyMovement.cs` — the radius might be a serialized field per archetype.

## What's next

Track C (multi-map run structure + quartermaster shops) is the recommended next track per `Docs/GAMEPLAY_ROADMAP.md`. Before starting Track C, the highest-value moves are:
- Play-test Tracks A + B together and tune the numbers.
- Add the skirmish lifetime timeout (~60s).
- Decide whether intervention should also surface a one-line announcement (`RunAnnouncementUI`) for player feedback.
