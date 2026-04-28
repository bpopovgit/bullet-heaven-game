# Roadmap

This roadmap stays practical on purpose. It should track the next features that make the game feel more complete, not every possible dream system at once.

## Completed Foundations

- Top-down movement with the new Input System.
- Mouse-aimed primary shooting.
- Elemental damage packets.
- Enemy health, resistances, and status handling.
- Melee and ranged enemies.
- Score system.
- XP gems.
- Level-up upgrades and popup choice UI.
- Health, XP, score, and timer HUD support.
- Spawn-point based enemy respawning.
- Region-aware wave stages.
- Timed elite enemy spawns.
- Dragon boss with phase two.
- Boss reward popup.
- Runtime event announcements.
- Sound-effect system with random variation by folder.
- Main menu and pre-run single-player setup flow.
- Starting loadout selection.
- Bomb on `Q`.
- Secondary active skill on `E`.
- Bomb and secondary-skill cooldown UI.
- Faction identity foundation for Humans, Angels, Demons, Zombies, and Neutral actors.
- Faction-aware enemy targeting and damage routing.
- Faction visual badges for readable Human, Angel, Demon, and Zombie actors during prototyping.
- Starter faction unit archetypes for Human, Angel, Demon, and Zombie melee/ranged roles.
- Runtime Human ally squad that follows the player and shoots hostile targets.
- Editor helper that builds Human, Angel, Demon, and Zombie faction prefabs from the existing melee/ranged enemy prefabs.
- Starter Angel/Demon/Zombie skirmish that makes faction aggro visible in-game.
- Faction-aware regular wave spawning with per-archetype alive caps.
- Runtime spawn-point discovery so new authored spawn markers automatically contribute to regular waves.

## Highest-Value Next Features

### 1. Better Faction Art and Authored Prefabs

The first code-level archetypes exist, and the generator can clone from the old enemy prefab family. The next content pass should replace placeholder shapes with authored prefabs for:

- Human allies
- Angel melee/ranged units
- Demon melee/ranged units
- Zombie melee/ranged units

Each prefab should include:

- `FactionMember`
- `FactionVisualIdentity` while placeholder art is still in use
- `FactionUnitArchetype`
- faction-specific visuals
- faction-specific health/damage tuning
- optional faction-specific weapons or abilities

### 2. Author-Friendly Ally Prefabs

The current Human squad is runtime-generated placeholder content.

Next improvement:

- create one authored Human ally prefab
- expose health, weapon range, fire rate, and projectile damage in the Inspector
- replace runtime circles with prefab references when available

### 3. Character Selection

The menu/loadout scene should eventually choose a playable character before the run.

Useful first character data:

- faction
- starting HP
- move speed
- default weapon family
- starting active skill
- passive trait

### 4. Multi-Map Run Structure

The GDD direction describes one run as multiple maps/districts.

Good first version:

- Map 1 -> reward choice -> Map 2
- preserve run build between maps
- reset local enemies/pickups per map
- increase pressure per district

### 5. Talent Trees With Prerequisites

The current level-up system offers flat choices. The future system should move toward visible trees similar to the design reference.

Good first version:

- upgrade nodes have IDs
- upgrade nodes can require previous nodes
- selected nodes unlock deeper options
- the menu can preview the full tree before the run

### 6. Quartermaster / NPC Shops

NPCs on the map can become run shops.

Good first version:

- interactable NPC
- pauses or slows combat while shopping
- spends run currency
- offers health, weapon, or faction-themed upgrades

### 7. Mobility Skill on `Shift` or `Space`

The combat kit now has:

- primary weapon
- bomb on `Q`
- active skill on `E`

The next clean expansion is a mobility layer:

- dash
- blink
- short evasive burst

This would make positioning feel more deliberate and skillful.

### 8. Ultimate / Special on `R`

After movement utility, the next satisfying escalation is a longer-cooldown power:

- screen clear
- time slow
- dragon breath
- empowered storm burst

This should be rarer, stronger, and more dramatic than the `Q` and `E` slots.

### 9. More Distinct Weapon Families

Starting weapons exist, but they can become more mechanically different.

Strong next direction:

- tighter fire identity
- stronger projectile visuals
- unique hit feel
- clearer weapon-specific upgrade synergies

### 10. More Bomb Families

Current bombs:

- Frag
- Frost
- Fire
- Shock

Good next additions:

- Poison Bomb
- Black Hole Bomb
- delayed cluster bomb

### 11. Meta Progression / Profile System

The game now has a proper pre-run loadout flow, so the next structural layer is profile progression across runs.

Good first goals:

- profile level
- unlockable starting options
- milestone-based feature unlocks

### 12. Unlock Structure

Early profile progression should unlock gameplay options:

- new weapons
- new bombs
- new secondary skills
- new passives
- maybe new maps

After the important gameplay unlocks are in place, later levels can skew more cosmetic.

### 13. Cosmetics

Good cosmetic targets:

- player color variants
- projectile color variants
- freeze / burn / bomb VFX variants
- aura / trail
- boss-announcement style variants
- profile banners or titles

### 14. Map-Specific Spawn Profiles

The spawn-point system is now better than the old dynamic-only idea, which means we can lean into authored maps.

Best next map-side step:

- map-owned spawn groups
- boss spawn anchors
- enemy pool by map
- terrain-aware spawn rules
- per-map faction wave profiles

### 15. More Bosses and Boss Attacks

The dragon is a strong first boss foundation.

Next boss-side growth:

- second attack pattern for the dragon
- minion summon phase
- hazard zones
- second boss type

## Suggested Immediate Implementation Order

1. Generate the starter faction prefabs from the Unity `Tools` menu.
2. Replace placeholder faction sprites with real or better temporary art.
3. Tune or replace the generated starter faction prefabs.
4. Add character selection data to the pre-run flow.
5. Create the first visible talent-tree data structure.
6. Add a two-map run handoff prototype.
7. Add a basic quartermaster NPC.

## Design Rule of Thumb

Every few minutes of play should add at least one of these:

- a new choice
- a new threat
- a new reward
- a new pattern the player must learn
