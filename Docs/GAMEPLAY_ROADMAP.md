# Gameplay Direction — Roadmap

This is the working to-do for the next major direction: making the moment-to-moment loop more interesting, the runs more meaningful, and the faction-war premise actually load-bearing in play.

Tracks are ordered by recommended execution, not by size. Within each track, items are roughly sequential — earlier items unblock later ones.

**Current status: agreed to start with Track A.** Tracks B, C, D are queued behind it.

---

## Track A — Make talents transform behaviour, not just buff numbers

**Why first:** the talent refactor (4-tier, 9-node, mutex capstones) shipped the *structure* but most nodes are still flat stat bumps. This track delivers the payoff the player actually feels at level-up.

**Definition of done:** at least 4–6 capstones (and a handful of tier-3 nodes) change *how* the run plays, not just how big the numbers are. A "Splinter capstone" build feels mechanically different from a "Heavy capstone" build — same character, different verbs.

- [ ] **A1.** Pick 4–6 capstones to convert from stat → behaviour modifier. Candidates:
  - `atk_primary_overcharge` → on every Nth shot, fire a free shot that pierces all
  - `atk_primary_annihilator` → enemies below 30% HP take a guaranteed second hit
  - `atk_element_avatar` → element status effects spread between adjacent enemies on death
  - `atk_element_unleashed` → casting your skill consumes element stacks for a damage burst
  - `atk_bomb_chained` → bombs detonate twice in quick succession, second blast smaller
  - `def_field_phaseshift` → using your E skill teleports you a short distance toward cursor
- [ ] **A2.** Add new `PlayerUpgradeType` values to support these (e.g. `OnKillSpread`, `ExecuteThreshold`, `BombSecondaryBlast`, `SkillBlinkDistance`, `BurstEveryNthShot`).
- [ ] **A3.** Add handler logic where each new type takes effect — `PlayerCombat`, `PlayerProjectile`, `PlayerSecondaryActiveSkill`, `BombBehaviour` etc.
- [ ] **A4.** Extend `TalentCatalog.BuildSingleAppliedEffectText` to format the new types into readable strings.
- [ ] **A5.** Update the affected option builders in `TalentCatalog` to emit the new types instead of stat bumps.
- [ ] **A6.** Smoke-test each behaviour modifier in-run, confirm it survives between waves and stacks correctly with point count.
- [ ] **A7.** *(Stretch)* Convert 2–3 tier-3 nodes too, so deep investment in a branch reads as a behaviour shift, not just bigger T2 numbers.

---

## Track B — Faction warfare as visible backdrop

**Why second:** this is the project's identity differentiator. The "drop into an ongoing battle" premise becomes concrete instead of conceptual. Reusing Track A's behaviour-modifier infrastructure means we can also add faction-themed talents (e.g. an Angel-aligned capstone that buffs allied Angels you helped earlier in the run).

**Definition of done:** in any map zone, the player can see 3–6 mixed-faction skirmishes already in progress. Helping one side has consequences.

- [ ] **B1.** Author a `FactionSkirmish` data type — two faction archetypes, head counts, anchor position, optional "guarded structure" target.
- [ ] **B2.** Spawn skirmishes from a `SkirmishDirector` component analogous to `EnemyWaveDirector`. Don't double-spawn enemies that other directors already cap.
- [ ] **B3.** Tune skirmish AI so the two factions actually fight each other when player isn't engaged (`FactionTargeting` already supports it; verify priorities and aggro radii).
- [ ] **B4.** Detect player intervention: who took the killing blow, who did the most damage to which side.
- [ ] **B5.** Reward intervention — temporary allied unit from the supported faction joins the player for the rest of the run, or a stat buff scoped to that faction's archetype.
- [ ] **B6.** Penalty for ignoring — if a faction wins skirmishes uncontested, future waves of *that* faction get more aggressive (more elites, faster spawns).
- [ ] **B7.** *(Stretch)* Add 1–2 faction-themed capstones that key off skirmish history (e.g. "for every Demon skirmish you helped end, +5% damage").

---

## Track C — Run as a journey (multi-map + quartermasters)

**Why third:** structural piece for "more meaningful runs," but only feels different from "longer single map" if Tracks A and B have made each minute mechanically varied.

**Definition of done:** a single run is now Map 1 → reward → Map 2 → quartermaster shop → Map 3 → boss. Build, currency, and HP carry between maps; per-map enemy pools reset.

- [ ] **C1.** Persistent run state — extract a `RunSession` ScriptableObject or static holder (currency, current map index, HP fraction, talent state, run-currency).
- [ ] **C2.** Inter-map transition flow — post-map reward popup → loader screen → next map scene with state preserved.
- [ ] **C3.** Authored map definitions for at least 3 districts with distinct enemy mix and tone (Track D supplies hazards/decor).
- [ ] **C4.** Quartermaster NPC prefab + interaction (E to open, pauses or slows combat).
- [ ] **C5.** Shop UI: 3–5 randomized offers per visit, costs in run-currency. Offers can be HP heals, weapon-family upgrades, faction-themed buffs.
- [ ] **C6.** Per-district difficulty escalation curve.
- [ ] **C7.** Final-district boss handoff — the existing dragon boss becomes the run climax instead of a mid-survival event.

---

## Track D — Map hazards, pickups, environmental gameplay

**Why fourth (or "as you go"):** less transformative on its own, but slots opportunistically into Track C maps. Each item below can ship in any order.

- [ ] **D1.** Destructibles (statues, barrels) that block movement until broken; some drop pickups.
- [ ] **D2.** Hazard zones (lava, spikes, poison clouds) with damage-over-time on contact, faction-neutral.
- [ ] **D3.** Breakable LOS pillars / cover for tactical positioning.
- [ ] **D4.** Treasure chests with re-roll choice — pick from 3 random rare upgrades, locked behind a clearing event or mini-boss.
- [ ] **D5.** Non-XP pickups: food (heal), gold (run-currency for Track C shops), magnet pulse (vacuum nearby gems).
- [ ] **D6.** *(Stretch)* Time-of-day or weather variants per map — cosmetic but reads as a place, not a tile.

---

## Cross-cutting prerequisites

These aren't a track — they're shared dependencies several tracks need.

- [ ] **X1.** Map-definition data format (Track C and D need a way to specify "what's in this map" — see `MAP_AUTHORING_WORKFLOW.md` once we settle on it).
- [ ] **X2.** Difficulty profile ScriptableObject (enemy HP / spawn rate / elite frequency / hazard density multipliers). Picked on loadout screen, decoupled from map layout. Used by Tracks B, C, D.
- [ ] **X3.** Run-currency system — fronted by D5 (gold pickup) and consumed by C5 (quartermaster shops).
