# Map Design Guide — The War of Death Metal

This is the design-side companion to `MAP_AUTHORING_GUIDE.md`. The other doc is *how* to use Unity's tilemap workflow. This doc is *what* to paint and *why*. Read this before each pass on a district.

---

## Critical principles

Reading what makes a map good is half of getting better. The principles below are what separate "first-draft test scene" maps from maps the player describes to a friend.

### A map should *tell the player something*

Before any tile is painted, the map needs an answer to: **what does this map teach or escalate?** A map without a role is just decoration. The three districts in this run each have a distinct role:

- **Zombie Outskirts** — *teach the loop*. Forgiving open space, sparse obstacles, low enemy variety. The player learns to fight, dodge, collect XP/gold.
- **Demon Foothills** — *introduce tactical pressure*. Hazards (lava, cliffs), pinch points, faster enemies. The player learns positioning matters.
- **Angel Citadel** — *prepare for the boss*. Visually distinct, structured, leads the eye to a central arena where the dragon will appear.

If you can't write that role in one sentence, you're not ready to paint.

### Composition fundamentals

These transfer from any visual medium:

1. **Focal point.** The map needs *one* visual anchor — a fountain, statue, ruined tower, marble arch. Without it, the eye doesn't know where to go.
2. **Asymmetric balance.** Perfect symmetry reads as "test scene." Heavier weight on one side, balanced visually with smaller elements on the other.
3. **Leading lines.** Paths, rivers, and walls guide the eye (and the player's feet) toward objectives.
4. **Variety in scale.** Mix small intimate pockets with wide open arenas in the same map.
5. **Color zones.** Different tile palettes define different "rooms" without needing actual walls.

### Spatial rhythm

The single most-skipped principle in first-pass maps. **Good arenas alternate tight → open → tight in their layout.** Pacing is everything. A uniformly open arena bores the player. A uniformly tight arena suffocates them.

For a top-down bullet-heaven specifically, follow what I'll call **the 3-2-1 rule**:

- **3 large open spaces** — where enemy waves can surround the player and combat goes wide.
- **2 corridors / pinch points** — where the player can funnel enemies (great for bombs) or get cornered.
- **1 distinctive landmark** — the visual anchor described above.

If your map doesn't have all three, redesign the macro before painting anything.

### Negative space matters

Walls aren't just obstacles — they shape choices. Water shouldn't be there because water looks pretty; it should be there because the player has to *decide* whether to commit to crossing it.

A good test: can the player look at any 4×4 tile section and immediately tell what they should do? If yes, that section is doing its job. If they're confused or indifferent, that section is filler.

---

## Where to look for solid map designs

### Reference games — go steal from these

**Bullet-heaven specifically:**
- **Vampire Survivors** — Mad Forest, Inlaid Library, Dairy Plant, Cappella Magna. Five maps that nail this exact problem. Search Google Images for `"vampire survivors" map [name] top-down`.
- **20 Minutes Till Dawn** — simpler than VS but composition is sharp. Look at how they use sparse trees and cliffs.
- **Brotato** — single-screen arenas but the obstacle placement is masterclass. Asymmetric, intentional.
- **Halls of Torment** — more dungeon-like; great pinch-point design.

**Top-down arena design generally:**
- **Hades** rooms — masterclass in compact tactical design.
- **Enter the Gungeon** rooms — every room geometric and intentional.
- **Realm of the Mad God** dungeon rooms — for larger arena scale.
- **Zelda: Link to the Past** dungeon rooms — classic top-down composition.

### Search terms that work

- `"vampire survivors" map design`
- `top down 2d arena map design`
- `roguelike level layout`
- `pixel art top down map composition`

Set up a Pinterest board or local folder of 20-30 reference screenshots before painting your second pass. Composition is learned by collecting examples, not by inspiration.

### Books / talks

- **The Art of Game Design — Jesse Schell** — chapter on space and architecture.
- **GDC talks on YouTube** — search "GDC level design top down" or "GDC Vampire Survivors."
- **Boss Keys (Mark Brown, Game Maker's Toolkit)** on YouTube — Zelda dungeon analysis whose principles transfer directly.

### About AI image generation

Asked: can I generate map images? **No** — I don't have an image-generation tool. If you want to use AI yourself:

- **Midjourney** prompt style: `top-down 2d pixel art game arena map, closed area, ruined graveyard with stone walls, river running through, central monument, view from directly above, 16x16 pixel art style --ar 1:1`
- **DALL-E** in ChatGPT
- **Stable Diffusion** locally for CC0-style outputs

**Caveat:** AI-generated maps are visual *mood boards* only. They're not actually playable layouts. Use for color/mood reference, not as blueprints. **Strongly prefer real game screenshots** as references — they're proven playable.

---

## Industry-standard process for designing one map

Skip any of these steps at your peril.

### Step 1 — Define role (5 minutes, paper)

Write one sentence: *"This map teaches/escalates X by doing Y."* If you can't, stop here.

### Step 2 — Macro layout on paper (15 minutes, paper)

Sketch the map at the macro level — **just shapes, no detail**. Stick figures in the middle, blocks for walls, lines for paths. Answer:

- Where does the player **spawn**?
- Where's the **focal point**?
- What's the **shape rhythm** — tight corridor → open arena → tight corridor → final arena?
- Where are the **2-3 pinch points**?
- Where are the **2-3 open kill zones**?
- What's the **landmark** the player will describe to a friend?

A useful template:

```
Entry      Wide          Pinch       Wider          Landmark
spawn  →   grass field → ruined  →   open field  →  central
                         gateway                    monument
```

### Step 3 — Block-out in Unity (30 minutes, Tilemap)

Paint **only with two tile types**: one ground, one wall. No water, no paths, no decoration. Just the shape. The player should be able to play through this and feel the arena.

This is the most-skipped step and the one that separates first-draft maps from second-pass maps. **Paint the shape in 30 minutes, then play it.** If it feels good with just two tiles, more detail will only improve it. If it feels boring with two tiles, more detail won't save it.

### Step 4 — Terrain pass (60 minutes)

Now bring in variety: dirt paths inside grass areas, water as hazard/obstacle in the right places, rocky tiles as accents. **Each terrain change should mean something** — a path leads somewhere, water blocks somewhere. No purely-decorative terrain changes yet.

### Step 5 — Decoration pass (60 minutes)

Trees, gravestones, broken fences, rocks. These give the map character but should NEVER block movement unintentionally. Keep them visual-only on a non-collider Decoration tilemap layer.

### Step 6 — Polish pass (later)

Random tile variants, animated tiles (water shimmer), edge transitions. This is the "make it look professional" pass and should be last.

---

## District 1 — Zombie Outskirts

### Role

*Teach the loop.* The player learns to shoot, dodge, collect, level up. The space is forgiving — open central area, low obstacle density, no hazards. The map's secondary purpose is mood: "you're in a haunted countryside that's been overrun."

### Recommended size

**40 × 30 tiles** — large enough to wander, small enough to feel personal.

### Macro layout (ASCII abstraction at ~50% scale)

Each character represents roughly 2×2 actual tiles. The actual painted map should be 40 wide × 30 tall.

```
########################################
#~~~~. . . . , , , . . . . . . . .~~~~#
#~~~. . . . , G G , . . . . . . . .~~~#
#~. . . . . , G G , . . . . . . . . .~#
#. . . . . . , , , . . . . . . . . . .#
#. . . . . . . . . . . . . H H H . . .#
#. * * F F . . . . . . . . H . H . . .#
#. * . . . . . . . . . . . H H H . . .#
#. * * F . . . . . . . . . . . . . . .#
#. * . . . . . . P . . . . . . . . . .#
#. * F F . . . . . . . . . . . H H . .#
#. . . . . . . . . . . . . . . H . . .#
#. T . . . . . . . . . . . . . H H . .#
#. . . . , , , , , , , , , , . . . . .#
#~. . , , , , , , , , , , , , . . . .~#
#~~. . , , , , , , , , , , , , . . .~~#
#~~~. . . . . . . . . . . . . . . .~~~#
########################################
```

**Legend:**
- `#` = perimeter stone wall (Walls tilemap, collider)
- `~` = swamp / outer marsh boundary (Walls tilemap, collider — non-passable visual edge)
- `.` = grass (Ground tilemap, no collider)
- `,` = dirt path (Ground tilemap, no collider — same walkability as grass, just visual)
- `G` = stone gateway pillars (Walls tilemap, collider — leave a 2-tile gap between them)
- `H` = ruined cottage walls (Walls tilemap, collider — 3-4 tiles in U or L shape)
- `*` = gravestone (Decoration tilemap, no collider — just visual props)
- `F` = broken wooden fence (Walls tilemap, collider OR Decoration if you want them passable)
- `T` = dead tree (Decoration tilemap)
- `P` = player spawn (no tile painted; just a marker)

### Region-by-region walkthrough

**Spawn zone (centre, around `P`)** — 8-10 tile open grass radius. Nothing in the player's first 5-tile sphere on spawn. This is their breathing room.

**West cemetery (rows 6-12, columns 2-5)** — 5-6 gravestones spread loosely, 2-3 broken fence segments suggesting a once-fenced graveyard. Sparse dead grass underfoot (dirt mixed with grass tiles). This is your **visual flavour** zone — no tactical complexity, just atmosphere.

**East cottage ruin (rows 5-12, columns 26-32)** — Two ruined cottages. Each is 3-4 wall tiles in an L or partial-rectangle shape. The player can walk around them, hide behind one for cover from ranged enemies, or get cornered against them. This is your **first pinch-point training**.

**North gateway (rows 2-4, columns 16-18)** — Two stone gateway pillars (1-2 tiles each) framing a 2-tile-wide gap. Visually communicates "this is the exit toward the next district" — even if mechanically the transition is time-based, the gateway is the player's mental map of "I'm heading north into the foothills." This is your **landmark**.

**South dirt road (rows 16-22, columns 5-30)** — A wider dirt path running east-west across the southern part of the map. Suggests an old road. Visually breaks up the grass field.

**Outer swamp (boundary, `~`)** — 2-tile-thick water/marsh ring inside the perimeter. This serves the closed-arena bounds AND provides visual flavour ("the outskirts are surrounded by foul marsh"). Water tiles painted on Walls layer = collider = player can't escape.

### Tile suggestions from the Kenney Roguelike RPG Pack

- **Grass:** the standard medium-green tile. Add 2-3 variants (lighter, darker, with small flowers) painted randomly in the open field for variety.
- **Dirt:** the brown packed-earth tile. Use for the south road and as scattered patches inside grass.
- **Gravestones:** the small grey crosses or rounded headstones. There should be 3-4 variants.
- **Fences:** the wooden post tiles. Look for broken ones with missing planks for the abandoned-cemetery look.
- **Cottage walls:** stone-brick or wood-board tiles. Use the corner-piece tiles to make L-shaped walls look intentional rather than truncated.
- **Dead trees:** the bare-branch tile (no leaves). Don't overuse — 3-4 across the whole map is plenty.
- **Stone gateway:** the larger stone-pillar tiles. The pack usually has at least one column-shaped tile suitable for this.

### Player flow narrative

The player spawns in the open centre. Looking around, they see:
- Cemetery west (mood)
- Cottages east (cover)
- A monumental gateway north (destination)
- A dirt road south (alternate movement)

When enemies start spawning, the player has multiple tactical choices: stay in the centre (most open, hardest to be cornered), retreat into cottages (defensive but riskier), kite enemies along the dirt road (mobile play). All choices are valid; none are mandatory.

### What to avoid

- ❌ Don't make the cemetery a maze. Gravestones should be sparse decoration, not movement obstacles.
- ❌ Don't fully enclose the cottages. They should be U-shaped or L-shaped, not boxes the player can hide inside (cheese the AI).
- ❌ Don't paint water inside the play area for District 1. Save hazards for District 2.

---

## District 2 — Demon Foothills

### Role

*Introduce tactical pressure.* The player learns positioning matters. Lava rivers as hazards. Pinch points that force commitment. Tighter spaces. Faster enemy mix (Track C's difficulty multiplier already applies enemy-HP scaling; the map adds geometric pressure on top).

### Recommended size

**45 × 35 tiles** — slightly larger than Outskirts to fit the lava-river divide.

### Macro layout (ASCII abstraction)

```
#############################################
#R R R R . . . . . . . . . . . . . .R R R R#
#R R . . . . . . . . . . . . . . . . . .R R#
#R . . . . , , , , , . . . . . . . . . . .R#
#R . . . , , , , , , , , . . . . . . . . .R#
#R . . . , , , , , , , , , , . P . . . . .R#
#R . . . . , , , , , , , . . . . . . . . .R#
#R . . . . . . . , , , . . . . . . . . . .R#
#R~. . . . . . . . . . . . . . . . . . .~R#
#R~L L L L L . . . . . . . . . . . L L L~R#
#R~L L L L L L . . . . . . . . L L L L L~R#
#R . L L L L L L . . . B B . L L L L L . .R#
#R . . L L L L . . . . B B . . L L L . . .R#
#R . . . L L . . . . . . . . . . L L . . .R#
#R . . . . . . . . . . . . . . . . . . . .R#
#R . . . . . . . . . . . . . . . . . . . .R#
#R . . . . . . . . . . . . . . . . . . . .R#
#R R . . . . . . . . . . . . . . . . . .R R#
#R R R R . . . . . . . . . . . . . .R R R R#
#############################################
```

**Legend:**
- `#` = stone perimeter (Walls)
- `R` = jagged rock outcrop (Walls — irregular boundary, more "natural" than the smooth wall of Outskirts)
- `~` = cliff edge (Walls — visual height difference, also blocking)
- `.` = volcanic dirt (Ground — dark brown / charcoal tile)
- `,` = stone path (Ground — lighter cobble tile)
- `L` = lava (Walls layer, collider — orange/red recoloured water tile, OR if your pack lacks lava, paint reddish dirt and add particle effect later)
- `B` = wooden bridge planks (Ground layer — the only safe crossing of the lava river)
- `P` = player spawn (north end)

### Region-by-region walkthrough

**Northern arena (rows 1-7)** — Open volcanic-dirt area where the player spawns. Stone path tiles wind through it suggesting "this is the way south." Mid-sized open zone (about 12×6 tiles of clear space) for the first wave of combat.

**The lava river (rows 8-13)** — A diagonal lava river running east-to-west across the map's middle. Jagged shape, not straight — it looks natural, not planned. **The player MUST cross it to progress south**, but the only safe crossing is the wooden bridge in the centre (the `B` tiles).

**The bridge (centre of rows 11-12)** — A 2-tile-wide × 3-tile-long wooden plank section laid across the lava. This is your **major pinch point** — when crossing under fire, the player has nowhere to dodge sideways. Bombs become valuable here. Slow status from frost weapons becomes valuable here.

**Southern arena (rows 14-18)** — Open ground after the bridge crossing. Where surviving enemies will pile up after chasing the player across. This is the second open kill zone.

**Eastern + western rocky outcrops** — `R` tiles forming irregular jagged shapes along the edges. These break up the perimeter wall, give the map a "you're in mountain foothills" feel, and provide minor cover spots for bullet-heaven dodging.

**Northern + southern rock fingers (the `R R . . .` patterns at the corners)** — Rock formations extending into the play area, narrowing it slightly at the top and bottom. Subtle compression of space.

### What this map teaches mechanically

1. **Hazards exist.** Lava is the player's first encounter with terrain that *threatens* rather than just blocks. (For now, lava is just a Walls collider — but in a future Track D pass it'll deal damage on contact, and the lesson learned in this district will pay off.)
2. **Choke points matter.** The bridge teaches the player to think about where they're going *before* enemies pile up, not in the middle of being surrounded.
3. **Map memory.** Unlike the open Outskirts, players will start to remember specific spots: "the corner past the bridge," "the rock outcrop south-east." This is the start of map literacy.

### Tile suggestions

- **Volcanic dirt:** dark brown / charcoal grass-equivalent. Look for "scorched earth" or "barren" tiles.
- **Stone path:** light grey cobble. Use 2-3 variants for randomness.
- **Lava:** if the pack has lava tiles, use them. If not, **paint water tiles on Walls and tint them red/orange** via the SpriteRenderer's color tint on the Tilemap Renderer (Inspector → Color). Or use red brick tiles as a placeholder.
- **Rocks:** larger boulder tiles, irregular shapes.
- **Bridge:** the wooden plank tiles you already have from Outskirts (good visual callback).
- **Cliffs:** the dark rocky-edge tiles, if your pack has them. Otherwise just use the perimeter stone wall doubled up.

### What to avoid

- ❌ Don't make the lava river straight horizontal. Diagonal or jagged is much more interesting.
- ❌ Don't add a second bridge. The single-crossing tension is the entire point.
- ❌ Don't fill the southern arena with rocks. After the bridge stress, players need an open space to breathe.

---

## District 3 — Angel Citadel

### Role

*Prepare for the boss.* Visually distinct from the previous two (warm gold/marble vs. dark earth/lava). Structured, symmetric, ceremonial. Funnels the player toward a central plaza where the dragon will spawn. This is the climax.

### Recommended size

**40 × 40 tiles** — squarer than the others to support the symmetric plaza design.

### Macro layout (ASCII abstraction — note the radial symmetry)

```
########################################
#R R R R . . . . . . . . . . . . R R R R#
#R . . . . . . . . . . . . . . . . . . R#
#R . . , , , , , , , , , , , , , , . . R#
#R . . , . . . . . . . . . . . . , . . R#
#R . . , . C . . . . . . . . C . , . . R#
#R . . , . . . . . . . . . . . . , . . R#
#R , , , , . . . . . . . . . . , , , , R#
#R , . . . . . . . . . . . . . . . . , R#
#R , . . . . . . . . . . . . . . . . , R#
#R , . . . . C . . . F . . C . . . . , R#
#R , . . . . . . . F F F . . . . . . , R#
#R , . . . . . . . . F . . . . . . . , R#
#R , . . . . C . . . . . . C . . . . , R#
#R , . . . . . . . . . . . . . . . . , R#
#R , . . . . . . . . . . . . . . . . , R#
#R , , , , . . . . . . . . . . , , , , R#
#R . . , . . . . . . . . . . . . , . . R#
#R . . , . C . . . . . . . . C . , . . R#
#R . . , . . . . . . . . . . . . , . . R#
#R . . , , , , , , , , , , , , , , . . R#
#R . . . . . . . . . . . . . . . . . . R#
#R . . . . . . . . . . . P . . . . . . R#
#R R R R . . . . . . . . . . . . R R R R#
########################################
```

**Legend:**
- `#` = ornate stone wall (Walls — heavily decorated)
- `R` = ornamental rocks / marble outcrops at corners (Walls)
- `.` = marble / pale stone floor (Ground)
- `,` = gold-trim path (Ground — lighter, ornate variant)
- `C` = column / pillar (Walls — single-tile cover spots)
- `F` = central fountain (Decoration on a small footprint, OR Walls if you want it impassable)
- `P` = player spawn (south entrance)

### Region-by-region walkthrough

**South entrance (around `P`, rows 21-22)** — The player spawns in a small antechamber at the south. A clear marble pathway leads north toward the courtyard.

**The approach pathway (column 11-12 from south to centre)** — A narrow gold-trimmed path leading from the spawn to the central courtyard. Visually directs the player. Wide enough for movement but visually distinct.

**The central courtyard (rows 4-19, centre)** — A large open marble floor enclosed by an inner wall of pillars (`C`) and an inner gold-trim path (`,` ring). Roughly 16×12 tiles of open space. **The fountain (F) sits at the centre.** This is the boss arena — when the dragon spawns, it spawns here.

**The pillar ring** — Eight columns positioned at the corners and cardinal points of the inner courtyard, each 1 tile in footprint. These give the player **cover during the boss fight**: dodge behind a column to break line of sight from a ranged attack. They also break up the otherwise-empty arena visually.

**The outer cloister (between perimeter and inner ring)** — A walkable space surrounding the central courtyard. The player can circle the boss arena, approach from any cardinal direction, retreat through the outer ring if the centre gets overwhelming.

**Corner outcrops (`R R R R` patterns)** — Decorative marble corner pieces. Visual flourish to keep the perimeter from feeling like a square box.

### What this map does for the player experience

1. **Visual contrast from earlier districts.** Outskirts was muddy green; Foothills was charcoal-red; Citadel is **bright gold/cream/white**. The mood shift is immediate. The player feels they've arrived somewhere important.
2. **Symmetry signals "boss arena."** Players have learned (through Halo, Dark Souls, every fighting game) that symmetric, structured arenas mean *boss fight imminent*. Use this convention deliberately.
3. **The columns reward map awareness.** Learning to weave around them during the dragon fight is the skill ceiling moment of the run.
4. **The fountain is the focal point.** The player's eye lands on it the moment they enter. Perfect spot for the boss to spawn (or for a cinematic "the dragon descends" moment if you ever add one).

### Tile suggestions

- **Marble floor:** the lightest stone tile in your pack. White, cream, or pale grey.
- **Gold path:** a yellow / sand-coloured path tile, ideally with a slight ornate pattern.
- **Pillars:** column tiles. Most Kenney packs have at least one pillar/column tile suitable for this.
- **Fountain:** if your pack has a fountain tile (Roguelike RPG Pack does have decorative water features), use it. If not: a small water tile (3×3) surrounded by stone trim. The fountain should be on the Decoration layer (no collider) so the dragon can spawn on top of it.
- **Ornate walls:** the most-detailed stone wall variants. Save these for this district to make it feel premium.

### What to avoid

- ❌ Don't break the symmetry with random props. Symmetry IS the design here.
- ❌ Don't put hazards (water/lava) inside the central courtyard. The boss fight needs unobstructed dodging space.
- ❌ Don't make the columns 2×2 tiles. They should be small enough that the player can corner-dodge around them quickly.

---

## Implementation checklist

For each district, work through this list in order. Don't skip steps.

### Per-district checklist

- [ ] **Step 1.** Write the role sentence on paper. Read it aloud. Commit.
- [ ] **Step 2.** Sketch the macro layout on paper using the templates above as guides.
- [ ] **Step 3.** Block out in Unity using only ground + wall tiles. 30 minutes. Play it.
- [ ] **Step 4.** If shape feels good, terrain pass: paths, water, varied ground.
- [ ] **Step 5.** Decoration pass on a third Tilemap layer (no collider): props, gravestones, dead trees, etc.
- [ ] **Step 6.** Save as prefab in `Assets/Art/DistrictPrefabs/`.
- [ ] **Step 7.** Ping the AI assistant with **"District [name] v1 done"** + prefab path. The code wiring (`MapDefinition.TilemapPrefabPath` + `MapTransitionDirector` instantiation) ships at that point.

### Project-wide checklist (before any painting)

- [ ] Tilemap import settings confirmed (Filter Mode: Point, Compression: None, Pixels Per Unit: 16).
- [ ] At least 15-20 reference screenshots collected from inspirational games (Vampire Survivors, Hades, etc.).
- [ ] Decoration tilemap layer added on each district's Grid (separate from Ground and Walls).
- [ ] Composite Collider 2D + Rigidbody 2D Static + `Composite Operation = Merge` configured on Walls.
- [ ] Ground tilemap has NO collider components.

---

## Common pitfalls (learned the hard way)

1. **Painting detail too early.** The most common first-time map mistake. Paint shape first; trust the process.
2. **Symmetric and uniformly-sized districts.** A 40×40 square map for every district is boring. Vary the proportions: Outskirts 40×30, Foothills 45×35, Citadel 40×40.
3. **No clear focal point.** If you can't say what the map's landmark is, neither can the player.
4. **Decoration on the collider layer.** Gravestones, dead trees, fences should be on the Decoration tilemap (no collider) unless they're explicitly meant to block. Otherwise the player gets caught on invisible obstacles.
5. **Lava = water with red tint, but on Walls.** A common shortcut. Just remember to set the Tilemap Renderer's Color tint to red/orange so it reads as lava instead of red water.
6. **Forgetting Order in Layer.** Walls on Order in Layer 1, Decoration on 2, Ground on 0. Otherwise walls render below the floor.

---

## Recommended next move

**Don't paint anything else yet.** Spend 30 minutes browsing 20-30 reference screenshots, sketch the three districts on paper using the role-definition + macro-layout discipline above (5 minutes per map), then redo Outskirts as a 30-minute block-out using only two tiles. Play it.

If shape is good, proceed to terrain + decoration. If not, redo the macro sketch.

The key insight: **good maps come from process, not from talent.** Iteration on bad shape produces a beautiful bad map. Iteration on good shape produces a great map. Spend time on the macro before the micro and you'll build maps faster than working detail-first.

When Outskirts v1 is ready (block-out painted, prefab saved), ping me — code wiring is one Edit away.
