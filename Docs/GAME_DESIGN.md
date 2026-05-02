# The War of Death Metal — Game Design Document

> **Genre:** Top-down bullet-heaven × roguelike with persistent faction warfare.
> **Engine-agnostic.** Currently implemented in Unity 2D; this document describes design intent so it can be rebuilt in Unreal 5 (or any other engine) without depending on Unity-specific terminology where avoidable.

---

## 1. Game identity

### One-sentence pitch

*The player is dropped into an ongoing four-faction war and must survive escalating enemy waves while shaping a build through tier-gated talents and pre-run kit choices, across three districts that culminate in a dragon boss fight.*

### Target experience

The player should feel like an **outsider stepping into a war that already exists**. The world doesn't revolve around them. Angels and Demons fight each other to the player's east; Zombies overrun a Demon outpost to the south; the player's choices about whom to help, whom to ignore, and whom to oppose shape the run alongside their character build.

This deliberately diverges from the standard bullet-heaven framing where the player is the centre of attention. Vampire Survivors gives you a lone protagonist swarmed by undifferentiated enemies; *The War of Death Metal* gives you a battlefield with sides.

### Genre fusion — what's borrowed and what's new

| Borrowed from | What |
|---|---|
| **Vampire Survivors** | Auto-attacking core loop, level-up upgrade selection during play, time-based escalation, XP-gem collection that pulls the player around the map. |
| **20 Minutes Till Dawn** | Pre-run loadout selection (weapon + bomb + skill + passive), pre-run talent-tree preview that previews the run's upgrade pool. |
| **Hades / Slay the Spire** | Multi-stage runs with district-specific tone and difficulty escalation; single-run progression that resets between attempts. |
| **Path of Exile / Diablo** | Talent tree with tiers, point-gate prerequisites, mutually-exclusive capstones. |

What's *new* compared to the genre:

- **Faction warfare backdrop** — skirmishes between the four factions happen autonomously around the player. The player's intervention shapes the run.
- **Districted runs** — the run is segmented into three thematically distinct maps (not a single endless arena).
- **Behaviour-modifier capstones** — high-tier talents alter *how* the build plays, not just stat numbers.

---

## 2. Setting & tone

### World

The world is a **dark-fantasy battlefield** where four factions wage perpetual war. Time is a constant — armies clash, banners fall, the dead rise. The player arrives mid-conflict and exits when they die or the dragon falls.

### Tone

- **Visual:** dark void backdrop, blood crimson accents, burnished gold ornament, aged-bone parchment text. Pixel-art (16×16 tiles) at character/world scale; clean geometric UI at interface scale.
- **Audio:** percussion-driven, metallic, with bursts of brass / horn / drone. The player's primary should sound *meaningful* (impact-heavy, not video-gamey ping).
- **Writing:** terse, severe, restrained. No quippy one-liners. Talent names lean toward *Killing Stroke*, *Iron Resolve*, *Eternal Vigil* — not *Bonus Damage III*.

The working title **"The War of Death Metal"** reflects this — heavy, theatrical, embracing metal-album-cover sensibility without parody.

### Factions

| Faction | Identity | Role | Playable? |
|---|---|---|---|
| **Humans** | Disciplined, mortal, banner-and-steel infantry. The player's faction. | Player + ally squad | ✅ |
| **Angels** | Golden, holy, blade-and-light. Hate Demons above all. | Mid/late-game enemies + skirmish faction | ❌ (faction member, not player) |
| **Demons** | Crimson, brimstone, fire-and-claw. Hate Angels above all. | Mid/late-game enemies + skirmish faction | ❌ |
| **Zombies** | Decaying, viridian, mob-and-pus. Hate any non-Zombie. | Early-game cannon fodder + skirmish faction | ❌ |
| **Neutral** | Untargeted entities (props, allies who shouldn't draw aggro). | Decorative / utility | n/a |

**Aggro priority matrix** (higher = more wanted as target):

|             | vs Human | vs Angel | vs Demon | vs Zombie |
|-------------|---------:|---------:|---------:|----------:|
| **Human**   |        — |       55 |       55 |       100 |
| **Angel**   |       70 |        — |    **100** |       35 |
| **Demon**   |       70 |    **100** |        — |       35 |
| **Zombie**  |       60 |       60 |       60 |         — |

This data-driven matrix produces the world's behaviour: when an Angel sees both a Demon and a Human at equal distance, it always picks the Demon (priority 100 > 70). The player is *peripheral* to most factions until they intervene.

---

## 3. Run structure

### Out-of-run loop

```
Main Menu
  ↓
Mode Select (Single Player / Multiplayer-soon)
  ↓
Faction Select (currently Human only — Angels/Demons reserved for future)
  ↓
Character Select (Vanguard / Ranger / Arcanist)
  ↓
Loadout Setup (weapon + bomb + active skill + passive)
  ↓
Talent Preview (loadout-aware tree visualization, no commitments yet)
  ↓
[Start Run] → Game scene
```

Between runs, **persistent profile progression** is reserved for a later phase (it's a known feature on the roadmap, not yet implemented). All roguelike progression for now is per-run.

### In-run flow

A full run is **three districts** in sequence:

```
DISTRICT 1: Zombie Outskirts (75 sec)
  ↓ District-Cleared banner → fade-out → enemy clear
DISTRICT 2: Demon Foothills (90 sec, ×1.30 enemy HP, ×1.15 damage)
  ↓
DISTRICT 3: Angel Citadel (120 sec, ×1.65 enemy HP, ×1.30 damage)
  - Boss appears in this district only.
  ↓
RUN COMPLETE
```

Total run length: ~5 minutes (75 + 90 + 120 seconds). Build, HP, talents, and currency carry between districts; per-district enemies reset.

### Within a district

- Enemies spawn in waves driven by a `WaveDirector` that consults a per-district enemy mix and difficulty curve.
- Skirmishes (Track B) spawn periodically — mixed-faction battles around the player.
- The player kills enemies, collects XP gems and gold coins, levels up, picks talent or stat upgrades from a popup, and survives.
- At the end of the district's duration, the run advances.

### What carries between districts

- **HP** (the player isn't destroyed when transitioning).
- **Build** (talents, character, loadout, accumulated stat upgrades).
- **Currency** (gold for Quartermaster shops in a future phase).
- **Faction affinity** (per-faction "helped" / "opposed" counts from skirmishes).

### What resets between districts

- **Enemies** in the world (cleared at transition).
- **Per-district difficulty multiplier** (each new district has its own HP/damage scaling).

---

## 4. Player characters

Three Human-faction characters are currently playable. Each has a distinct primary attack, role, and talent affinity. **All share the same talent tree structure** but receive different effects from character-scoped nodes.

### Iron Vanguard (Melee Vanguard)

- **Primary:** *Vanguard Cleave* — short-range arc swing in front of the cursor direction.
- **Stats:** highest HP, slowest move speed.
- **Talent affinity:** Melee Radius, Arc Angle, Cooldown Reduction (melee-themed effects on character-scoped talents).
- **Role:** front-line bruiser. Kites and cleaves. Talents that grow the cleave arc reward aggressive positioning.

### Waywatch Ranger

- **Primary:** *Ember Repeater* (or chosen weapon family) — projectile-based, mouse-aimed, can multi-shot via Projectile Count upgrades.
- **Stats:** medium HP, fast move speed.
- **Talent affinity:** Projectile Count, Pierce, Splash Radius, Fire Rate.
- **Role:** mobile ranged DPS. Best at maintaining distance and pressuring crowds.

### Conclave Arcanist

- **Primary:** *Frost Lance / Storm Needler / Venom Caster* (chosen at loadout) — directed spell, beam/cone/cloud/chain shape per weapon family.
- **Stats:** lowest HP, medium move speed.
- **Talent affinity:** Magic Range, Beam Width, Status Chance, Magic Cooldown Reduction.
- **Role:** zone-control caster. Status effects (burn / freeze / shock / poison) drive their kit's value.

### Loadout slots (independent of character)

| Slot | Choices | Notes |
|---|---|---|
| **Weapon** | Ember Repeater / Frost Lance / Venom Caster / Storm Needler | Determines element + projectile pattern. Some interactions depend on character class. |
| **Bomb** (Q) | Frag / Frost / Fire / Shock | Periodic burst attack on cooldown. Carries element/status. |
| **Active Skill** (E) | Magnetic Pulse / Arcane Shield / Frost Nova | Tactical defensive / utility ability on cooldown. |
| **Passive** | Swiftness / Magnetism / Overclock / Vitality | Always-on stat trait. |

Future combat slots planned but not yet built: **Mobility (Shift / Space)** and **Ultimate (R)**.

---

## 5. Combat systems

### Primary attack

- **Trigger:** held mouse button (auto-fires) or input action equivalent.
- **Cadence:** weapon's `shotsPerSecond` × player's `FireRateMultiplier`.
- **Aim:** mouse cursor direction in world space.
- **Effects:** damage + element + optional status. Specifics vary by weapon family + character.

### Bomb (Q)

- **Trigger:** key press, on cooldown.
- **Behaviour:** projectile travels toward cursor, detonates on arrival as a circular AoE blast.
- **Configurable:** damage, radius, element, status, cooldown — modified by talents.
- **Feel:** screen-clear panic button. Used to bail out of being surrounded.

### Active Skill (E)

- **Trigger:** key press, on cooldown.
- **Three options** with distinct behaviours:
  - **Magnetic Pulse** — radial knockback + pickup vacuum. Best for crowd control + gem collection.
  - **Arcane Shield** — temporary invulnerability + clears nearby enemy projectiles. Best for emergency survival.
  - **Frost Nova** — radial freeze status applied to enemies. Best for setting up large damage windows.
- **Capstone-modifiable** by *Phase Shift* (adds short blink toward cursor before activation).

### Damage + status pipeline

Every damaging hit produces a `DamagePacket` with:
- `amount` (raw damage)
- `element` (Physical / Fire / Lightning / Frost / Poison)
- `status` (None / Burn / Shock / Slow / Freeze / Poison)
- `statusDuration`, `statusStrength`
- `splashRadius`, `sourcePos` (for AoE / knockback)

The packet routes through `EnemyHealth.TakeDamage` which consults:
1. Resistances (per-element multiplier).
2. Player-side modifiers (e.g. Annihilator execute bonus on low-HP targets).
3. Status receiver (applies burn/poison ticks, freeze immobilization, etc.).

Status effects are timed and can stack/refresh. Statuses also enable **on-kill faction-spread** behaviour from the *Avatar of the Element* capstone.

### Player-side combat modifiers (behaviour layer)

A central `PlayerCombatModifiers` component holds the run's accumulated *behavioural* upgrades — distinct from raw stats. Examples:

- `ExecuteHpThreshold` + `ExecuteBonusDamage` — the *Annihilator* capstone deals bonus damage to enemies below an HP fraction.
- `BurstShotInterval` + `BurstShotProjectiles` — *Overcharge* fires extra free shots periodically.
- `OnKillStatusSpreadRadius` — *Avatar* spreads the dying enemy's status to nearby enemies.
- `BombSecondaryBlastFraction` + `BombSecondaryBlastDelay` — *Chained Salvo* fires a second bomb blast after a delay.
- `SkillBlinkDistance` — *Phase Shift* teleports the player toward cursor before E activates.
- `SkillElementBurstDamage` — *Unleashed* consumes status stacks for extra burst damage on E.

These are read by the relevant systems (PlayerShooting, EnemyHealth, Bomb projectile, Active skill) at the right moment in the pipeline.

---

## 6. Talent system

### Structure

**Six trees**, each with **nine nodes** organised in **four tiers**:

```
                    TIER 1 (Root)
                    1 node, 5 max points
                         |
                  ┌──────┼──────┐
                  ↓      ↓      ↓
              TIER 2 (Branches)
              3 nodes, 5 max points each
              gate: 1 row point spent
                  |      |      |
                  ↓      ↓      ↓
             TIER 3 (Specializations)
             3 nodes, 3 max points each
             gate: 4 row points spent
                  |             |
                  ↓             ↓
              TIER 4 (Capstones — Mutually Exclusive)
              2 nodes, 2 max points each
              gate: 9 row points spent
              picking one locks the other
```

### The six trees

| Tree | Theme | Root | Capstones (mutex) |
|---|---|---|---|
| **Attack: Primary** | Boost the chosen weapon | Power Shot | Overcharge / Annihilator |
| **Attack: Element** | Boost the chosen element's effects | Elemental Spark | Avatar of the Element / Unleashed |
| **Attack: Bomb** | Boost the Q ability | Bomb Craft | Devastator / Chained Salvo |
| **Defense: Vital** | Survival, HP, recovery | Vital Core | Eternal Vigil / Indomitable |
| **Defense: Field** | Boost the E ability | Field Control | Field Dominion / Phase Shift |
| **Defense: Command** | Boost ally squad / faction synergy | Rallying Banner | War Banner / Phalanx |

### Two ways a node unlocks

A node is reachable if it satisfies BOTH:

1. **Parent dependency** — the node's parent (one tier up) has at least one point spent.
2. **Row-point gate** — the player has spent at least N points anywhere in this tree (N = 0 / 1 / 4 / 9 by tier).

Plus capstones additionally enforce **mutex** — picking one capstone locks all sibling capstones in the same tree.

### Behaviour vs stat nodes

- **Tiers 1-2:** mostly numerical stat boosts (+8% damage per point, +12 max HP per point, etc.).
- **Tier 3:** stat boosts with optional behaviour-modifier secondaries (each tier-3 hints at its tree's capstone).
- **Tier 4 (capstones):** **always** behaviour modifiers, never raw stats. This is the talent system's payoff — capstones change how the build plays.

### Per-character node scoping

Some nodes are character-scoped via `PlayerUpgradeScope` (Vanguard / Ranger / Arcanist / All). E.g., *Big Shot* tier-2 in Attack: Primary tree presents:
- **Vanguard:** +damage + melee radius
- **Ranger:** +damage + splash radius
- **Arcanist:** +damage + beam width

Each character only sees their flavoured version of that node when it appears in the level-up popup. The talent tree's *structure* is shared; the *effect* is character-tailored.

---

## 7. Faction warfare backdrop

### What it is

The world contains **autonomous faction skirmishes** that exist independently of the player. A `SkirmishDirector` periodically spawns mixed-faction battles within ~12-26 metres of the player. Each skirmish is:
- 4 units of Side A (random faction)
- 4 units of Side B (different random faction)
- Deployed on opposite sides of an anchor point
- Visible via an anchor marker (pulsing ring, faction-blended colour)
- Tracked via an off-screen UI compass arrow with distance label

**Up to 3 active skirmishes** at any time. New ones spawn ~22 seconds apart.

### Why this exists

The player should *see* the war, not just exist within it. Standing still on one part of the map should reveal Angels and Demons fighting each other in the distance. This is what justifies the faction system mechanically — without skirmishes, factions are just enemy categories.

### Player intervention and outcomes

The director tracks how much damage the player deals to each side of every skirmish. When one side is fully dead:

- **Helped** (player damaged the loser ≥ 2× more than the winner): rewards a 30-second Human ally + permanent +3% damage stack. Faction affinity for the winner increments.
- **Idle** (player did 0 damage to either side): penalty squad of 3 winning-faction units spawns near the player.
- **Split-fire** (player damaged both sides similarly): no reward, no penalty — just XP/gold from any kills.

Outcomes are announced via on-screen banner ("ANGELS OWE YOU A DEBT — +3% damage, ally arrives", "DEMONS ADVANCE — A SQUAD APPROACHES").

### `FactionAffinityTracker`

A per-run counter of how many skirmishes the player has helped each faction in. This data exists for future talent-tree capstones that key off skirmish history (e.g., "for every Angel skirmish helped, +X% damage to Demons") — currently only the +3% damage stack consumes affinity-related events.

---

## 8. Maps & districts

### Three districts

**1. Zombie Outskirts** — *teach the loop*. Forgiving open grass field with sparse obstacles. Cemetery (visual flavour), ruined cottage (first cover/pinch point), stone gateway (visual landmark), swamp boundary. Low enemy density. Goal: player learns to fight, dodge, collect.

**2. Demon Foothills** — *introduce tactical pressure*. Volcanic dirt with lava river running diagonally across the middle. **Single bridge** is the only safe crossing — a major pinch point. Rocky outcrops break up the perimeter. Difficulty multiplier ×1.30 HP / ×1.15 damage.

**3. Angel Citadel** — *prepare for the boss*. Marble-and-gold radial courtyard with central fountain (boss spawn point). 8 columns ring the inner arena providing dodge cover. Visually distinct (cream/gold) from the previous two districts. Difficulty ×1.65 / ×1.30. **Boss appears here** (only here).

### Per-district configuration (data-driven)

Each district has a `MapDefinition` carrying:
- Display name
- Theme faction (visual / spawn bias)
- Background tint
- Duration (seconds)
- Enemy HP multiplier
- Enemy damage multiplier
- "Is boss district" flag
- Flavour text (intro banner)

A `MapTransitionDirector` handles the timing, fade-to-black between districts, enemy clearing, and applying the next district's config.

### Map authoring (Unity-current, port-implications-noted)

In Unity, districts are painted using the **Tilemap workflow** with Kenney's Roguelike RPG Pack tiles. Each district is a prefab with:
- A `Ground` tilemap (no collider) — grass, dirt, paths, bridge planks
- A `Walls` tilemap (collider) — perimeter walls, water, lava, obstacles
- An optional `Decoration` tilemap (no collider) — props, gravestones, dead trees

For an **Unreal 5 port**, equivalent approaches:
- **Paper2D Tilemap** — direct equivalent of Unity Tilemap (less mature but works).
- **Hand-authored levels** — author each district as a `.umap` with placed actors.
- **Procedural generation** — use the PCG plugin or Houdini to generate districts at runtime.

The data-driven `MapDefinition` is engine-agnostic: a struct/asset holding name, durations, multipliers, theme. Engine-specific is only how the **visuals** are loaded (a tilemap prefab in Unity, a level asset or PCG configuration in Unreal).

### Map design principles

(See `Docs/MAP_DESIGN_GUIDE.md` for full version.)

A good arena follows the **3-2-1 rule**:
- 3 large open spaces (where waves can surround)
- 2 corridors / pinch points (tactical funneling)
- 1 distinctive landmark (visual anchor — a fountain, statue, ruin)

Spatial rhythm: alternate **tight → open → tight**. Avoid uniformly-sized rooms.

---

## 9. Enemies

### Enemy archetypes

Each faction has paired melee + ranged variants:
- **HumanMeleeAlly** + **HumanRangedAlly** — friendly squad
- **AngelMelee** + **AngelMarksman** + **AngelRanged**
- **DemonMelee** + **DemonRaider** + **DemonRanged**
- **ZombieMelee** + **ZombieGrunt** + **ZombieRanged**

Each archetype is configured (via `FactionUnitArchetype`) with HP, speed, damage, element, and status affinity (Angels lean Lightning, Demons lean Fire, Zombies lean Poison).

### Spawning

- **Wave director** — periodic baseline spawns at scene-defined spawn points, mixing archetypes per stage.
- **Elite director** — periodic harder variants of base archetypes.
- **Boss director** — single-shot spawn (the dragon) on the final district only.
- **Skirmish director** — autonomous mixed-faction battles (see §7).

### The Dragon Boss

The current boss is a dragon with a phase-2 transition. Spawns only in **Angel Citadel**. On defeat, drops a boss-reward popup with stronger upgrade options (the `CreateBossRewardPool`).

Future bosses planned: a second boss type (mid-run mini-boss), a third boss for an extended run.

### Status interactions

Statuses (Burn, Shock, Slow, Freeze, Poison) modify enemy behaviour:
- **Burn / Poison** — DoT damage over duration.
- **Slow** — reduces move speed.
- **Freeze** — fully stops the enemy.
- **Shock** — periodic stun ticks.

Status duration and strength are carried in the damage packet. A `StatusReceiver` on each enemy handles application and decay.

---

## 10. Resources & economy

### XP

- Drops from killed enemies as collectible gems on the ground.
- Vacuumed by the player's pickup radius (default ~2m, scales with talents).
- Drives **level-ups**, which present three upgrade choices (talent nodes, basic stat boosts, or boss reward variants).

### Gold

- Drops from a fraction (~20%) of killed enemies as coins on the ground.
- Same vacuum logic as XP.
- Value scales with district difficulty (Angel Citadel coins are worth more than Outskirts coins).
- Tracked by `RunSession.Currency`. **Future:** consumed by Quartermaster NPC shops between districts.

### Future currencies (not yet implemented)

- **Profile XP** — between-run progression (Track #11 in the older roadmap).
- **Unlockables** — weapons, bombs, skills, passives gated by profile level.

---

## 11. Level-up upgrade flow

When the player levels up:

1. Game pauses (time scale to 0).
2. **"CHOOSE AN UPGRADE"** popup appears.
3. Three upgrade cards are shown vertically. Each card has:
   - **Title** (e.g., *Power Shot*)
   - **Effect description** (e.g., *+8% player damage per point*)
   - **Progress indicator** (e.g., *Rank 1/5*)
   - **Requirement line** (e.g., *Tier 1 | Root talent*)
   - **"Unlocks Next" preview panel** with up to 4 small "gem" icon tiles, each colour-coded by tree.
4. Hovering an icon tile shows a **tooltip** with the unlocked talent's name + brief effect.
5. Clicking a card **selects** it (gold halo + brighter accent stripe + body tint).
6. Clicking the **"Choose"** button at the popup bottom confirms the selection.
7. Game resumes.

This mirrors 20 Min Till Midnight's unlock-preview mechanic: players can see what *future* upgrades become available by taking the current one. It's the talent tree's unlock structure surfaced at the moment of choice.

---

## 12. UI & aesthetic

### Palette ("War of Death Metal" theme)

| Role | Hex | RGB |
|---|---|---|
| Background void | `#0C0A0F` | dark almost-black with cool tint |
| Panel body | `#080612` | even darker, cooler |
| Title text | `#F4E8C7` | aged bone / parchment |
| Body text | `#DBD7CC` | warm off-white |
| Hint text | `#EBBC5C` | burnished gold |
| Primary accent (action / hover) | `#9E1A1A` | blood crimson |
| Bright accent (selection / highlight) | `#FF2D2A` | bright crimson |
| Danger / boss | `#EB4D33` | warning orange-red |
| Outline / brass | `#8E6F2C` | restrained metallic gold |

### Typography

- **Hero title** ("THE WAR OF DEATH METAL"): bold uppercase, ~60pt, +6 character spacing, with a thin crimson outline stroke.
- **Panel titles**: bold uppercase, ~32pt, with a subtle crimson stroke.
- **Section labels**: uppercase, ~22pt, +4 character spacing, gold colour.
- **Body**: regular, ~18pt.
- **Hints / footnotes**: regular, ~15pt, gold.

### Key screens

- **Main Menu** — runtime-built (no scene-bound prefab). The WAR OF DEATH METAL hero title dominates the upper third; navigation buttons centre.
- **Loadout / Talent Browser** — split panels for character info, weapon/bomb/skill/passive cycling, and a scrollable preview of all six talent trees with the current loadout's flavour applied.
- **In-game HUD** — score / HP / level / XP top-left; bomb (Q) + skill (E) cooldowns bottom-left with key letter; gold counter top-right (under loadout summary).
- **Level-up popup** — full-screen overlay with 3 cards and a Choose button (see §11).
- **Run-end "RUN COMPLETE"** banner — fade-out with banner text after final district.

### UX principles

1. **Information at a glance.** Show 3 upgrade choices at once, not a carousel. Bullet-heaven players make many decisions per run; minimise click friction.
2. **Visual differentiation through colour.** Each talent tree has its own accent colour; UI panels inherit that colour for instant recognition.
3. **Hover discloses, click commits.** Tooltip on hover gives detail; click confirms action. No tooltip should require a click to read.
4. **State must be obvious.** A selected card should be *visibly* different (gold halo, fattened accent stripe, body tint) — not subtle.
5. **No text where colour or shape will do.** Icon tiles communicate tree affiliation through colour without needing labels.

---

## 13. Roadmap & implementation phases

### Done (current state)

- ✅ Core combat loop (primary, bomb, skill, passive)
- ✅ Three playable Human characters with distinct primaries
- ✅ Faction system with priority-based aggro matrix
- ✅ Six talent trees × 9 nodes × 4 tiers, with mutex capstones
- ✅ Behaviour-modifier capstones (execute, burst, blink, on-kill spread, aftershock, status burst)
- ✅ Faction skirmish system with anchor markers, compass UI, and intervention rewards
- ✅ Multi-district runs (3 districts) with per-district HP scaling
- ✅ Boss handoff to final district (the dragon)
- ✅ Gold currency drops + HUD
- ✅ XP-gem collection loop
- ✅ Run loadout system (weapon / bomb / skill / passive)
- ✅ Level-up popup with Unlocks Next preview, tooltips, and Choose button
- ✅ "War of Death Metal" UI overhaul

### Next (planned, partial code present)

- 🚧 First district painted in Tilemap (Zombie Outskirts) — in progress.
- 🚧 Three-district playthrough fully wired (currently the data is there; the visual map prefabs need authoring).
- 🚧 Faction-affinity-aware capstones (hooks exist via `FactionAffinityTracker`).

### Round 2 — Track C completion

- ⏳ Quartermaster NPC + interaction (E key conflict to resolve — likely contextual).
- ⏳ Shop UI (consumes Gold currency for HP heals, weapon-family upgrades, faction-themed buffs).
- ⏳ Inter-district reward popup (mini level-up between districts).

### Track D — environmental gameplay

- ⏳ Destructibles (statues, barrels) with optional pickup drops.
- ⏳ Hazard zones (lava, spikes, poison clouds) with damage-on-contact.
- ⏳ Breakable LOS pillars / cover.
- ⏳ Treasure chests with re-roll mechanic.
- ⏳ Non-XP pickups (food / heal, magnet pulse).

### Future combat slots

- ⏳ Mobility skill on Shift / Space (dash / blink / evasive burst).
- ⏳ Ultimate on R (longer cooldown, dramatic effect — screen-clear, time slow, dragon breath).

### Future structural

- ⏳ More weapon families with distinct mechanical identities.
- ⏳ More bomb families (Poison, Black Hole, Cluster).
- ⏳ Profile / meta progression (between-run).
- ⏳ Unlock structure (weapons, bombs, skills, passives gated by profile level).
- ⏳ More bosses (second boss type, mid-run encounters).
- ⏳ Cosmetics (player tints, projectile variants, VFX variants).

---

## 14. Engine-agnostic implementation notes

Below are the **systems** the game needs, described in terms of responsibilities and data, not framework specifics. Every system has a Unity-current implementation; equivalents in Unreal 5 are noted where they differ meaningfully.

### Run state / session

- **Need:** a static or singleton holding current district index, currency, helped/opposed faction counts, total districts cleared.
- **Unity:** `RunSession.cs` static class.
- **Unreal:** a `UGameInstance` subclass (lives across level loads) or a `UGameInstanceSubsystem`.

### Map definition / catalog

- **Need:** a data table of district configs (name, theme, duration, multipliers, boss flag).
- **Unity:** hard-coded `MapDefinition[]` in `MapCatalog.cs`. Could be `ScriptableObject` for designer-edit.
- **Unreal:** `UDataAsset` per district + a `UDataTable` of districts.

### Map transition

- **Need:** a tick-based timer per district; on completion, fade-out → clear enemies → apply next district → fade-in.
- **Unity:** `MapTransitionDirector` MonoBehaviour with coroutines + a UI fade overlay.
- **Unreal:** an actor with `Tick()` + `UWidget` for the fade. Or use `UWorldSubsystem`.

### Talent tree

- **Need:** a hierarchical structure where each node has an ID, parent ID, tier, max points, mutex group, and produces an upgrade option when picked.
- **Unity:** plain C# classes (`RunTalentDefinition[]` in `TalentCatalog.cs`).
- **Unreal:** `UDataAsset` per talent, asset registry to query, or hard-coded as in Unity.

### Faction targeting

- **Need:** for any agent + position, find the highest-priority hostile target in range.
- **Unity:** `FactionTargeting.FindBestTarget` does a scene-wide `FindObjectsOfType<FactionMember>` + priority matrix lookup.
- **Unreal:** an `AISenseConfig_Sight` + `AIPerceptionComponent` setup, OR a custom subsystem-managed list of `IGenericTeamAgentInterface` actors. Unreal's perception system is heavier but more complete.

### Status / damage pipeline

- **Need:** damage packets carrying amount, element, status, source position. Per-enemy resistance multiplier. Per-enemy status receiver applying timed effects.
- **Unity:** `DamagePacket` struct + `EnemyHealth.TakeDamage(packet)` + `StatusReceiver.ApplyStatus(packet)`.
- **Unreal:** Unreal's `UGameplayAbilitySystem` is a natural fit (Gameplay Effects ≈ DamagePacket+Status). Or roll your own with a `UDamageType` subclass + a status component.

### Combat hooks

- **Need:** the player's primary attack, bomb, and skill systems each consult a `PlayerCombatModifiers` component for behaviour-altering values (execute threshold, burst frequency, blink distance, etc.).
- **Unity:** `PlayerCombatModifiers` MonoBehaviour with public fields, queried via `GetComponent<PlayerCombatModifiers>()`.
- **Unreal:** `UActorComponent` on the player pawn, queried via `GetOwner()->FindComponentByClass<UPlayerCombatModifiers>()`. Or use Gameplay Tags + Gameplay Attributes.

### Tilemap / world rendering

- **Need:** painted ground + walls + decoration layers per district.
- **Unity:** `Tilemap` + `Grid` + `TilemapCollider2D` + `CompositeCollider2D`.
- **Unreal:** Paper2D's `UPaperTileMap` for grid-based, OR hand-authored `.umap` levels with placed `AStaticMeshActor`s, OR PCG-driven generation.

### UI / HUD

- **Need:** runtime-built panels with consistent palette; level-up popup with Unlocks Next preview; tooltips that follow cursor and clamp to screen edges.
- **Unity:** `Canvas` + UGUI + TextMeshPro, all built procedurally in C#.
- **Unreal:** `UUserWidget` + UMG. Construct in C++ at runtime or design Blueprint widgets.

### Input

- **Need:** mouse aim, keyboard for Q/E/movement, click to confirm.
- **Unity:** new Input System package (`UnityEngine.InputSystem`).
- **Unreal:** Enhanced Input plugin (UE5's modern input).

---

## 15. References and inspirations

### Games to study (mechanics)

- **Vampire Survivors** — for the core auto-attack + collect + level-up loop and weapon evolution patterns.
- **20 Minutes Till Dawn** — for the loadout-aware pre-run upgrade tree visualization.
- **Halls of Torment** — for dungeon-style top-down arena design.
- **Brotato** — for asymmetric obstacle placement on a single screen.
- **Hades** — for compact tactical room design and boss encounters.
- **Slay the Spire** — for the "discrete journey of distinct stages" run structure and reward-popup design.

### Games to study (aesthetic)

- **Diablo II** — gothic dark fantasy with metal-album-cover sensibility.
- **Path of Exile** — talent tree visualization and information density.
- **Darkest Dungeon** — restrained, severe, high-contrast UI.
- **Dead Cells** — pixel-art clarity at small scale.

### Reading

- *The Art of Game Design* — Jesse Schell (chapters on space architecture and player decisions).
- *Game Maker's Toolkit / Boss Keys* on YouTube — Mark Brown's analyses of room-by-room dungeon design.
- *GDC talks* — search "GDC Vampire Survivors", "GDC roguelike level design", "GDC bullet hell".

---

## 16. Glossary

| Term | Meaning |
|---|---|
| **Run** | One playthrough from menu Start to RUN COMPLETE or death. |
| **District** | One of the 3 maps within a run. |
| **Stage** | A timed phase within `EnemyWaveDirector` driving spawn rules — distinct from "district". |
| **Loadout** | The pre-run choice of weapon + bomb + active skill + passive. |
| **Talent** | A node in the in-run upgrade tree. |
| **Capstone** | A tier-4 talent. Always behaviour-modifier; mutex with sibling capstones in the same tree. |
| **Skirmish** | An autonomous mixed-faction battle around the player. |
| **Affinity** | Per-run counter of how many skirmishes the player helped a given faction. |
| **Behaviour modifier** | A talent or upgrade that changes *how* the player's kit works (vs. raw stat numbers). |
| **Pickup radius** | Distance within which gems/coins are vacuumed toward the player. |

---

## 17. For the Unreal port specifically

If you're recreating this in Unreal 5, here's the priority order I'd suggest for a working prototype:

1. **Player pawn + primary attack + cursor aim.** Get the basic shooter loop running before anything else.
2. **Enemy pawn with simple AI (move toward player, contact damage).** Just to have something to shoot.
3. **Health + damage pipeline (Gameplay Ability System recommended).** Set up so additions are easy.
4. **One district level + camera follow.** Doesn't need to be tile-painted — a flat plane with some walls is enough.
5. **Wave spawning.** Periodic enemies appearing at scene markers.
6. **XP gems + level-up popup.** The bullet-heaven loop is now in place.
7. **Three character classes with distinct primaries.**
8. **Loadout selection in main menu.**
9. **Bomb + active skill systems.** Q and E inputs now have meaning.
10. **Six talent trees + popup integration.** This is where the game gains depth.
11. **Faction system (member component + targeting priority).** Initially: just enemies that target each other.
12. **Skirmish director.** Mixed-faction autonomous battles.
13. **Multi-district runs + transitions.**
14. **Boss + boss reward popup.**
15. **Gold currency + future shop.**
16. **UI polish and theming.**

You can use **Gameplay Ability System (GAS)** for most of the combat logic — it's the engine-supported version of what's been hand-rolled in Unity. Effects, cues, attributes, and tags map nearly 1:1 to what `PlayerCombatModifiers` and `DamagePacket` are doing manually.

For maps, **Paper2D** is fine if you want to stay 2D, but **3D top-down** would also work — many recent bullet-heaven games (Halls of Torment) are 3D rendered from a top-down angle. The aesthetic isn't "must be 2D pixel art"; it's "dark fantasy faction war".

---

## Appendix: cross-referenced docs

- `Docs/TRACK_A_SUMMARY.md` — behaviour-modifier talent system implementation details.
- `Docs/TRACK_B_SUMMARY.md` — faction skirmish system implementation details.
- `Docs/TRACK_C_SUMMARY.md` — multi-map run system implementation details.
- `Docs/GAMEPLAY_ROADMAP.md` — the four-track plan (A/B/C/D).
- `Docs/MAP_AUTHORING_GUIDE.md` — Tilemap workflow (Unity-specific).
- `Docs/MAP_DESIGN_GUIDE.md` — composition principles + 3 district blueprints (engine-agnostic).
- `Docs/PROJECT_OVERVIEW.md`, `Docs/GAMEPLAY_SYSTEMS.md`, `Docs/SYSTEM_GRAPH.md` — earlier architectural docs.
