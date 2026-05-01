# Track C (Round 1) — Run as a Journey

## Goal

Stop the run from being a single timer on a single arena. Make it a sequence of distinct districts that escalate, with build/HP/currency carrying between them, and the dragon boss waiting only at the end.

This is **round 1** of Track C. Round 2 will add the Quartermaster NPC (C4) and shop UI (C5) — deferred deliberately because (a) the Quartermaster needs a key-binding decision (E is already the secondary skill) and (b) the shop depends on a currency-drop system that's queued under Track D5. Half-shipping those would create dead code; better to land them as a clean unit later.

## What shipped

### Four new files

| File | Role |
|---|---|
| `Assets/Scripts/GameSystems/MapDefinition.cs` | Plain C# data class describing one district: name, theme faction, background tint, duration, enemy HP/damage multipliers, isBossDistrict flag, flavor text. |
| `Assets/Scripts/GameSystems/MapCatalog.cs` | Hardcoded array of 3 districts: **Zombie Outskirts** (75s, ×1.0 HP), **Demon Foothills** (90s, ×1.30 HP, ×1.15 dmg), **Angel Citadel** (120s, ×1.65 HP, ×1.30 dmg, **boss district**). |
| `Assets/Scripts/GameSystems/RunSession.cs` | Static state: `CurrentDistrictIndex`, `Currency`, `DistrictsCompleted`, `SavedHpFraction`, `IsActive`, `IsFinalDistrict`, `IsRunComplete`. Methods: `StartNewRun()`, `AdvanceDistrict()`, `EndRun()`, `AddCurrency()`, `TrySpendCurrency()`. |
| `Assets/Scripts/GameSystems/MapTransitionDirector.cs` | The runtime engine. Auto-bootstraps in the Game scene; applies the current district on Awake; tracks elapsed time vs district duration; on completion runs `Announce → hold → fade-to-black → advance → clear-enemies → apply-next-district → fade-in`. |

### Existing files modified

- **`Assets/Scripts/Combat/EnemyHealth.cs`** — `Awake` now calls `ApplyDistrictDifficulty()`, which multiplies `maxHealth` by `RunSession.CurrentDistrict.EnemyHpMultiplier` for non-Human factions. Each spawned enemy bakes the district's difficulty into its own HP at spawn time.
- **`Assets/Scripts/GameSystems/BossSpawnDirector.cs`** — scene-load gate now also checks `RunSession.IsActive && !RunSession.IsFinalDistrict` and bails out if not on the final district. Boss only appears in *Angel Citadel*.
- **`Assets/Scripts/UIScripts/MainMenuRuntime.cs`** — `LoadGameplayScene()` now calls `RunSession.StartNewRun()` so the district counter resets every time the player starts a new run from the menu.

### How a run runs now

1. **Menu → Start Run.** `RunSession.StartNewRun()` resets `CurrentDistrictIndex = 0`, `Currency = 0`, etc.
2. **Game scene loads.** `MapTransitionDirector` auto-bootstraps. `Awake` applies the first district: sets `Camera.main.backgroundColor` to the district's tint, banners `"ENTERING ZOMBIE OUTSKIRTS\nThe shambling dead test your edge."` for 2.2s.
3. **Combat as normal** — wave director, allies, talents, etc. Enemies spawned during this district have `maxHealth × 1.0`.
4. **75 seconds elapse.** `MapTransitionDirector.Update` triggers `TransitionRoutine`:
   - Banner: `"ZOMBIE OUTSKIRTS CLEARED"` (1.6s).
   - Hold 1.6s (uses `WaitForSecondsRealtime` so it works even if you later add time-scale pauses).
   - Fade overlay alpha 0 → 1 over 0.55s (uses `Time.unscaledDeltaTime`).
   - `RunSession.AdvanceDistrict()` → index becomes 1.
   - `ClearEnemies()` finds every `EnemyHealth` in the scene and destroys non-Human ones (allies survive, player survives, fade overlay survives).
   - Apply the next district: new background tint, new intro banner.
   - Fade overlay alpha 1 → 0.
5. **District 2 (Demon Foothills).** Enemies spawn with `maxHealth × 1.30` because `EnemyHealth.Awake` reads the new district's multiplier.
6. **District 3 (Angel Citadel).** Enemies at `maxHealth × 1.65`. Boss enabled (if `GameTuning.bossEnabled = true`).
7. **Final district timer expires** OR **boss defeated** → `RunSession.AdvanceDistrict()` → `IsRunComplete` → `EndRun()` + `"RUN COMPLETE"` banner + screen stays at 85% black.

### What carries between districts

- **HP** — naturally, because the player GameObject isn't destroyed. The `RunSession.SavedHpFraction` field is wired but unused for now (left in place for a future "checkpoint heal" feature).
- **Talents** — `RunTalentState` lives on the player; not destroyed.
- **Loadout / build** — character/weapon/bomb/skill/passive choices live on the player and on `RunLoadoutState`; not destroyed.
- **Currency** — `RunSession.Currency` is a static int; persists for the run. No drops yet (waits on Track D5).

---

## Critical review — what works and what's risky

### Works well

1. **Single-scene swap, not multi-scene.** I considered making each district its own `.unity` file, but that needs Editor-side authoring and adds load complexity. Single-scene rebuild via `ClearEnemies()` + new background + new multiplier is dramatically simpler and reuses everything that's already running (wave director, ally squad, talent state, HUD).
2. **Difficulty bakes at spawn, not as a global modifier.** Each enemy's `maxHealth` is multiplied once in `Awake`. No need for runtime modifier accounting or per-frame rescaling. Cleared-and-respawned enemies in district 2 read the new multiplier; district 1 enemies were already destroyed.
3. **Boss gate is composable.** `BossSpawnDirector` now has *two* gates: `GameTuning.bossEnabled` (your dev-mode silence toggle) AND `RunSession.IsFinalDistrict`. Both must pass. So even with boss enabled in tuning, the dragon stays dormant until the player reaches Angel Citadel.
4. **No new prefabs required.** Everything is data + runtime construction, matching the project's existing style. You can run the game today and see all three districts cycle.
5. **Reuses `RunAnnouncementUI` and `Camera.main.backgroundColor`** — both surfaces that already exist.

### Honest gaps and risks

1. **Untested in Unity.** Same caveat as every prior track. The most fragile bits:
   - **`Camera.main.backgroundColor` only works if the camera's `clearFlags` is `SolidColor`.** If it's `Skybox`, the tint won't show. Easy fix in the Camera Inspector.
   - **`ClearEnemies` uses `FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None)`** — modern API. If you're on an older Unity version that doesn't have it, swap to `FindObjectsOfType<EnemyHealth>()`.
   - **Coroutine timing assumes `Time.unscaledDeltaTime`** for fades and `WaitForSecondsRealtime` for the hold. Should survive any time-scale pauses you add later (e.g. for a future Quartermaster shop).
2. **Damage multiplier isn't actually wired yet.** `MapDefinition.EnemyDamageMultiplier` exists and is logged at district start, but no code reads it for enemy attacks. To wire it, `EnemyMeleeDamage` and `FactionRangedAttacker` would each need a one-line `Awake` multiplier — same pattern as `EnemyHealth`. I deferred this because the HP multiplier is the bigger lever for "this district feels harder," and damage multiplier wants playtesting before tuning.
3. **District duration is purely time-based.** The current trigger is "the timer hit `DurationSeconds`." There's no kill-count or score-based completion. For a roguelike, time-based is fine — it's predictable and matches the bullet-heaven feel — but worth noting if you want a different rhythm.
4. **No "exit door" between districts.** A more game-y design would have the player walk to a portal at the end of each district, choose which next district to enter (Slay-the-Spire-style branching), and then transition. That's a separate feature; the current implementation is linear.
5. **No reward popup between districts.** The plan said "post-map reward popup" — currently the player just gets `"DISTRICT CLEARED"` and a fade. Adding a popup with a free upgrade choice (similar to `LevelUpManager`) is a natural Round 2 piece. Or it can wait for the Quartermaster to be the reward surface.
6. **No HP heal between districts.** `RunSession.SavedHpFraction` is stored but unused. Could be used to either heal a percentage (forgiving) or carry HP exactly (punishing). Currently nothing happens — HP carries because the player object persists.
7. **Wave director keeps running through transitions.** `EnemyWaveDirector` is unaware of districts. Stage progression in `EnemyWaveDirector` is tied to `RunTimer.WholeSecondChanged`, and `RunTimer` isn't reset between districts. If you've trimmed it to Stage 0 (which you have), this is fine. If you re-enable later stages, district 2 might already be at stage 3.
   - **Quick fix when you re-enable waves:** call `RunTimer.Instance.ResetTimer(true)` from `MapTransitionDirector.ApplyCurrentDistrict` (initial: false). I left this out for now to avoid breaking your "Stage 0 only" tuning.
8. **`RunSession` is a static class** — survives scene loads, doesn't survive app restart. That's correct for a single-run-per-session roguelike but means there's no save-resume for the run. If you ever want to resume, `RunSession` becomes a serialized blob.
9. **Camera background tint is the only visual difference between districts.** No backdrop sprite, no terrain swap, no decoration. The flavor lives in the banner text. Track D (hazards/decor) supplies the missing variety.
10. **`RunSession.StartNewRun()` is called from `MainMenuRuntime.LoadGameplayScene`.** If the player enters the Game scene some other way (e.g. you add a debug "load Game directly" path), `RunSession` won't be reset and `MapTransitionDirector.Bootstrap` will start a new run with whatever state was leftover. The bootstrap calls `StartNewRun()` itself when `IsActive == false`, but if you previously completed a run and re-entered without going through the menu, it'll think district 0 is district `TotalDistricts` and bail. Belt-and-suspenders fix would be to always reset on bootstrap, but I went with the menu-call to avoid surprising any "continue run" feature that might come later.

### What I deliberately deferred to Round 2

- **C4 — Quartermaster NPC.** Needs: a prefab, an interaction key (the user's E is the secondary skill — Quartermaster wants its own key, possibly contextual when standing near the NPC), and a "pause combat" mechanism.
- **C5 — Shop UI.** Needs C4 + currency drops. Currency drops are in Track D5. Without those, the shop is unbuyable.
- **Inter-district reward popup.** A free upgrade choice between districts (mini-`LevelUpManager`-style screen) would feel rewarding. Easy add once we decide whether it lives separately or under the Quartermaster.
- **Damage multiplier wiring.** Per gap #2 above.
- **Wave director district reset.** Per gap #7 above. Easy when you re-enable waves.

---

## Files touched

- **New:** `MapDefinition.cs`, `MapCatalog.cs`, `RunSession.cs`, `MapTransitionDirector.cs`
- **Modified:** `EnemyHealth.cs`, `BossSpawnDirector.cs`, `MainMenuRuntime.cs`

## Verification

`dotnet build Assembly-CSharp.csproj` → 0 errors. Same 43 pre-existing Unity-API-deprecation warnings, none from Track C.

## What to verify in Unity first

1. **Camera clear-flags.** In the Game scene, select the Main Camera in the Hierarchy, confirm `Clear Flags = Solid Color` in the Camera component. If it's `Skybox`, the per-district background tint won't show.
2. **Run a fresh game.** From the menu, hit Start Run. Watch for:
   - Console line `RUN SESSION: started new run.`
   - Console line `DISTRICT START: Zombie Outskirts (idx 0, hp×1, dmg×1, boss=False)`.
   - Banner: `ENTERING ZOMBIE OUTSKIRTS\nThe shambling dead test your edge.`
   - Camera background a faint dark green.
3. **Wait 75 seconds.** You should see:
   - Banner: `ZOMBIE OUTSKIRTS CLEARED`
   - Screen fades to black.
   - Console: `DISTRICT CLEAR: removed N non-Human entities for the next district.`
   - Console: `RUN SESSION: advanced to district index 1 (Demon Foothills).`
   - Banner: `ADVANCING TO DEMON FOOTHILLS\nThe hills smell of brimstone and old blood.`
   - Screen fades back in with a darker red tint.
4. **Check enemy HP scaling.** In Demon Foothills, enemies should take noticeably more hits to kill than in Zombie Outskirts. Console will print the multiplier on district start.
5. **Reach Angel Citadel and play through to its end** (75 + 90 + 120 = 285 seconds total). Confirm:
   - If `GameTuning.bossEnabled = true`, the dragon spawns *only here*.
   - At 120s in this district: banner `RUN COMPLETE`, screen settles at 85% black, gameplay halts.

If anything breaks: most likely culprit is the camera clear-flags (#1), then the `FindObjectsByType` API version mismatch.

## What's next

Round 2 of Track C (C4 Quartermaster + C5 Shop) is the natural follow-on, but only after:
- Track D5 (currency drops) is at least stubbed in, OR
- We agree on a decoupled placeholder ("you start each run with 100 gold") so the shop has something to spend.

Alternatively: jump to Track D for hazards/decor, which Track C maps will benefit from immediately. Distinct backgrounds + destructible barrels + spike hazards would make the three current districts feel meaningfully different in play, not just in tooltip text.
