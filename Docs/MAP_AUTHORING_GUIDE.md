# Map Authoring Guide — Tilemap Workflow

This is the step-by-step for getting a CC0 tile pack into Unity, painting the first district (Zombie Outskirts), and wiring it into `MapTransitionDirector` so each district swaps its own world. We'll use **Kenney's "Roguelike RPG Pack"** (pixel art, 16×16, CC0). You'll do the Unity Editor clicks; I'll do any code wiring. Ping me at the **CHECKPOINT** lines if anything is unclear or doesn't match what you see.

---

## Phase 0 — Decisions already locked

- **Tile-based** (Unity Tilemap), not hand-painted.
- **Free CC0 art** from Kenney.nl.
- **Closed arena** per district (the player can hit walls).
- Three districts: **Zombie Outskirts**, **Demon Foothills**, **Angel Citadel**.

---

## Phase 1 — Download the pack

1. Go to **https://kenney.nl/assets** and search for **"Roguelike RPG"**. The pack you want is "Roguelike RPG Pack" (or whatever Kenney's current naming is — it's the one with **outdoor tiles + dungeon tiles + knights/skeletons/zombies all in one pack**, NOT the smaller "Tiny Dungeon").
2. Click **Download** (no payment, no email — Kenney's site is genuinely free).
3. You'll get a `.zip`. Extract it somewhere temporary. Inside you'll see folders like `Tiles/`, `Tilemap/`, `Spritesheet/`, plus a `License.txt`.

**What we care about:**
- A spritesheet PNG (often called something like `roguelike-rpg-pack.png` or `colored-transparent_packed.png`). This contains all the tiles in one image.
- Optionally an XML or JSON file that describes where each tile is in the sheet — handy but not required.

If the pack ships with multiple sheet variants (e.g. "colored", "fantasy"), grab the **colored / packed transparent** one. We want PNG with transparency, not flat colors.

**CHECKPOINT 1.** You should now have a folder somewhere like `~/Downloads/kenney_roguelike-rpg-pack/` with at least one PNG spritesheet inside.

---

## Phase 2 — Folder structure in Unity

We'll set this up so imported third-party art is clearly separated from your project-original work. Open your project in Unity Editor.

### Folders to create in the Project window

Right-click in the Project window → **Create → Folder**, and create this hierarchy:

```
Assets/
├── ThirdParty/
│   └── Kenney/
│       └── RoguelikeRPGPack/        ← Kenney pack goes here, untouched
├── Art/
│   ├── Tiles/                       ← Sliced sprite assets you'll use as tiles
│   ├── TilePalettes/                ← Tile Palette assets (.asset)
│   └── DistrictPrefabs/             ← Per-district prefabs (we'll build later)
├── Settings/
│   └── Tilemaps/                    ← Optional: tilemap render settings
└── Scenes/                          ← (existing)
```

The reasons:
- **`ThirdParty/Kenney/`** keeps imported assets isolated from your code. If Kenney ever updates a pack and you re-import, you replace one folder, not the whole project.
- **`Art/Tiles/`** is where Unity-generated `Tile` ScriptableObjects will live (one per painted tile). Different from raw sprites.
- **`Art/TilePalettes/`** holds the Tile Palette `.asset` files used by the Tile Palette window.
- **`Art/DistrictPrefabs/`** is where we'll save the painted tilemaps as reusable prefabs.

**CHECKPOINT 2.** Project window shows the four new folders.

---

## Phase 3 — Import the pack

1. **Drag the unzipped Kenney folder** from your OS file browser into Unity's `Assets/ThirdParty/Kenney/RoguelikeRPGPack/` folder.
   - Or use **Assets → Import New Asset…** to import individual files.
2. Wait for Unity to import. Pixel-art packs are small; this is fast.

**Once imported, you'll see** the spritesheet PNGs and any docs/JSON the pack came with.

### Configure import settings on the spritesheet

This is the most important step. Unity's defaults are wrong for pixel art.

1. In the Project window, click the **spritesheet PNG** (the big one with all the tiles).
2. In the **Inspector**, set:
   - **Texture Type:** `Sprite (2D and UI)`
   - **Sprite Mode:** `Multiple` (we're going to slice it)
   - **Pixels Per Unit:** `16` (matches the 16×16 tile size)
   - **Filter Mode:** `Point (no filter)` — this preserves crisp pixels. Default `Bilinear` will blur them.
   - **Compression:** `None` — pixel art compresses badly with Unity's defaults.
   - **Wrap Mode:** `Clamp`
3. Click **Apply** at the bottom of the Inspector.

### Slice the sheet

1. With the spritesheet still selected, click **Sprite Editor** in the Inspector.
2. In the Sprite Editor window: **Slice → Type: Grid By Cell Size → Pixel Size: X=16, Y=16 → Slice → Apply**.
3. Close the Sprite Editor.

The PNG now has dozens (hundreds) of child sprites you can browse — expand the PNG in the Project window and you'll see them as individual tiles.

**CHECKPOINT 3.** You can expand the PNG in the Project window and see individual 16×16 sprite children.

---

## Phase 4 — Build a Tile Palette

The Tile Palette is Unity's painting tool. We need to create one and stock it with the sliced sprites.

1. **Window → 2D → Tile Palette**. A new window opens.
2. In the Tile Palette window: **Create New Palette**.
   - Name: `OutdoorPalette` (we'll make a separate one for dungeons later if needed).
   - Grid: `Rectangle`, Cell Size: `Manual` X=1, Y=1 (since Pixels Per Unit handles the world scale).
   - Save into `Assets/Art/TilePalettes/`.
3. Now drag sprites from your sliced spritesheet **into the empty palette grid**. Unity will ask where to save the auto-generated **Tile** assets — point it at `Assets/Art/Tiles/`. It'll create one `.asset` per sprite.
   - Start with the basics: a few **grass** tiles, a few **dirt** tiles, a few **path** tiles, and some **stone wall** tiles. You don't need to drag every single sprite — pick maybe 15-20 to start.
   - You can come back later and add more.

**CHECKPOINT 4.** You see your tiles in the Tile Palette window. Clicking a tile in the palette selects it for painting.

---

## Phase 5 — Paint Zombie Outskirts

We'll paint the first district directly into the Game scene.

1. Open `Game.unity` (Scenes folder).
2. **GameObject → 2D Object → Tilemap → Rectangular**. Unity creates a `Grid` GameObject with a `Tilemap` child. The Grid is the parent; the Tilemap is the layer you paint on.
3. Rename the Grid to `District_ZombieOutskirts_Grid`.
4. Rename the Tilemap child to `Ground`.

### Paint the floor

1. With the Tile Palette window open, make sure the **Ground** tilemap is selected as the active paint target (dropdown at the top of the Tile Palette window).
2. Pick a **grass tile** in the palette.
3. Open the Scene view. The brush tool should be active.
4. Click and drag in the Scene view to paint. Use the **brush size** controls (top-left of Tile Palette window) to paint larger areas.
5. **Paint a roughly 40×40 tile area** centered on origin. That's about 40 world units × 40 world units (since Pixels Per Unit = 16, each tile is 1 world unit).

Mix in dirt and path tiles for visual variety — pure grass looks sterile. Lay down a few "graveyard" props if your pack has them: gravestones, broken fences, dead trees. These belong on a **second tilemap layer** if you want them to render above the ground.

### Add a wall layer for the closed arena

1. In the Hierarchy, right-click the `District_ZombieOutskirts_Grid` → **2D Object → Tilemap → Rectangular** (creates a second tilemap as a sibling of `Ground`).
2. Rename it `Walls`.
3. Set its **Order in Layer** (in the Tilemap Renderer component) to `1` so it draws above Ground.
4. With `Walls` selected as the active paint target, paint a **boundary** of stone/wall tiles around the perimeter of the playable area. Make it look like ruined fences, broken stones, or a hedge — whatever fits the Zombie Outskirts theme.
5. Add components to the `Walls` GameObject:
   - **Tilemap Collider 2D** (so the player physically collides with it)
   - **Composite Collider 2D** (optional but recommended — merges all tile colliders into one shape, much more performant)
   - **Rigidbody 2D** with `Body Type: Static` (required by Composite Collider)
   - On the Tilemap Collider 2D: check `Used By Composite`.

**CHECKPOINT 5.** You can run the game (Play button) and the player should walk on grass, see the painted environment, and bump into the wall tiles at the perimeter.

If the camera is too zoomed-out or zoomed-in, adjust the `Camera.orthographicSize` (smaller number = more zoomed-in). For a bullet-heaven, somewhere between 5-9 is typical.

---

## Phase 6 — Save the district as a prefab

So we can swap districts in code, each district's tilemap should live as a prefab.

1. In the Hierarchy, drag `District_ZombieOutskirts_Grid` into `Assets/Art/DistrictPrefabs/`. Unity creates a prefab.
2. The Hierarchy version becomes a blue "prefab instance." That's fine.

**CHECKPOINT 6.** `Art/DistrictPrefabs/District_ZombieOutskirts_Grid.prefab` exists.

Now repeat **Phase 5 + 6 twice more** for:
- **Demon Foothills** — use rocky/dark tiles, lava if your pack has it, brimstone reds. Save as `District_DemonFoothills_Grid.prefab`.
- **Angel Citadel** — use stone/marble tiles, gold accents, brighter tones. Save as `District_AngelCitadel_Grid.prefab`.

Don't worry about getting the visuals perfect on the first pass. You can re-paint later by opening the prefab.

---

## Phase 7 — Wire prefabs to `MapTransitionDirector`

This is my part. Once you have at least the first district prefab built and saved, ping me with **"districts ready"** and I'll:

1. Add a `string TilemapPrefabPath` field to `MapDefinition`.
2. Update `MapCatalog` so each district references its prefab path (we'll move the prefabs into `Assets/Resources/Districts/` so `Resources.Load` can find them at runtime).
3. Update `MapTransitionDirector.ApplyCurrentDistrict` to:
   - Destroy the previous district's tilemap GameObject.
   - Instantiate the new district's tilemap prefab.
4. Remove the temporary `Camera.backgroundColor` tint logic (the tilemap will visually fill the world; the camera tint becomes peripheral).

**CHECKPOINT 7.** After my code change, starting a run shows the Zombie Outskirts tilemap. After 75 seconds, it fades out, swaps to the Demon Foothills tilemap, fades back in.

---

## Phase 8 — Polish later

Don't do this on the first pass. Once the basic system works:

- **Decoration tilemap layer.** A third tilemap above `Walls` for non-blocking decoration (gravestones, scattered bones, mushrooms). Order in Layer: 2.
- **Procedural variation.** Use Unity's **Random Tile** asset (built-in) so the same logical tile (grass) randomly picks from 4 visual variants when painted. Reduces the "tiled" look.
- **Per-district enemy theming.** Tint enemy sprites by district, or swap which enemy archetypes appear via the wave director.
- **Animated tiles.** Unity supports Animated Tile assets (water shimmer, lava bubbling). Easy to add later.
- **Hazards (Track D2).** Lava tiles in Demon Foothills that damage the player on contact. Implemented as a separate tilemap with a `TilemapCollider2D` that uses `Is Trigger`, plus a `HazardZone` script.

---

## Best practices to internalize

1. **Never edit the Kenney files directly.** If you want to recolor or modify a tile, *duplicate* it into your own folder first. Future re-imports won't blow away your changes.
2. **Pixels Per Unit must be consistent across all art.** All your sprites should use 16. If you mix 16 and 32 in the same scene, things will be different sizes for no reason.
3. **Prefer Composite Collider 2D for tilemap walls.** Without it, every painted wall tile is its own collider — that's hundreds of colliders. With Composite, you get one shape and dramatically better physics performance.
4. **Use Order in Layer, not z-position, to control sprite layering.** All tilemaps and sprites should be at z=0 in 2D.
5. **One Grid per district, multiple Tilemaps as children.** Don't reuse the same Grid across districts — they have different tile palettes and different sizes.
6. **Sorting Layers matter for actors vs. ground.** Enemies and the player should be on a sorting layer that draws above the ground tilemap. The project already uses an `Actors` sorting layer; make sure your tilemaps use a `Ground` or `Default` sorting layer below it.
7. **Start small.** A 40×40 tile arena is plenty for a first district. You can always expand. Don't paint a 200×200 map on day one.
8. **Save often.** `Ctrl+S` saves the scene. Tilemap painting writes to scene state; if you crash without saving, you lose paint work.

---

## What you do next

1. **Phase 1**: download the pack.
2. **Phase 2**: create the folder structure.
3. **Phase 3**: import + slice the sheet.
4. **Phase 4**: build the palette.
5. **Phase 5**: paint Zombie Outskirts.
6. **Phase 6**: save it as a prefab.
7. **Ping me with "Outskirts done"** — I'll wire the code so the existing district transition system uses it. We can do that BEFORE you paint the other two districts so you have an end-to-end playable loop early.

If anything in any phase doesn't match what you see in your version of Unity, screenshot it and drop the path in chat — I'll figure out the difference.
