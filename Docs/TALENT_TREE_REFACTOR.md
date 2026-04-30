# Talent Tree Refactor — Deepened 4-Tier Run Talents

## Why this change

Recent commits (`9ad44fa` and `a702eb0`) shipped the *skeleton* of roadmap item #5: nodes have IDs, single-parent prerequisites, in-run point spending, and a pre-run preview panel. But the trees were only **2 levels deep** — one root with three sibling children. That's a fan, not a tree, and it doesn't deliver the player-facing depth the design vision (20 Min Till Dawn / Diablo / PoE-style talent trees) calls for.

This refactor delivers four pieces that together turn the fan into a real talent tree:

1. **Deepen each tree from 2 → 4 tiers** (4 nodes → 9 nodes).
2. **Add point-count prerequisites** — the classic "spend N points in this tree to unlock the next tier" gate.
3. **Add mutually-exclusive capstones** — picking one capstone locks its twin, forcing a build-identity choice.
4. **Render visual path lines** in the pre-run preview so the parent/child topology reads at a glance.

I deliberately kept this a *data + UI* change. No ScriptableObject migration, no cross-tree edges, no in-run tree visualization. Those are roadmap items; this iteration is focused.

---

## Architecture overview

The talent system has three layers that were touched:

| Layer | File | Role |
|---|---|---|
| Definitions | `Assets/Scripts/GameSystems/TalentCatalog.cs` | Static catalog of every talent node, plus helpers for filtering, requirement text, and option construction |
| State | `Assets/Scripts/GameSystems/RunTalentState.cs` | Per-run point counts keyed by talent id (untouched in this refactor) |
| Pre-run UI | `Assets/Scripts/UIScripts/MainMenuRuntime.cs` | Renders the talent browser panel with one tree per category |

The key boundary decision: **`RunTalentState` only knows talent IDs**, not which tree a talent belongs to. This is the right boundary — state is a flat point bag. So the new "X points spent in this tree" lookup lives in `TalentCatalog`, where row IDs are known. That's why I added `TalentCatalog.CountRowPoints(state, rowId)` instead of putting `GetRowPoints` on `RunTalentState`.

---

## Data model changes

### `RunTalentDefinition` — three new fields

```csharp
public readonly int Tier;
public readonly int RequiredRowPoints;
public readonly string[] MutuallyExclusiveWithIds;
```

- **`Tier`** — purely informational (1–4). Used in requirement text ("Tier 3 | …") and could later drive UI tier separators. It's not load-bearing for the unlock check, but having it explicit means I don't have to infer tier from parent chain depth at display time.
- **`RequiredRowPoints`** — how many total points must be spent across the *entire row* before this node is reachable. Set to 0 for roots, 1 for tier-2 (just one point in root), 4 for tier-3, 9 for tier-4 capstones. This is the gating mechanism that makes "depth" feel earned instead of a single-step parent check.
- **`MutuallyExclusiveWithIds`** — an array of node IDs that lock this node out if any of them have points. Capstone twins reference each other through this.

### `IsUnlocked(state, rowPoints)` — new signature

The old method was `IsUnlocked(state)` and only checked the parent. The new check is:

```csharp
public bool IsUnlocked(RunTalentState state, int rowPoints)
{
    if (state == null)
        return IsRoot && RequiredRowPoints == 0;

    if (!IsRoot && !state.HasPoints(ParentId))
        return false;

    if (rowPoints < RequiredRowPoints)
        return false;

    return !IsLockedByMutex(state);
}
```

Why `state` is a parameter and `rowPoints` is a separate parameter: the row-point count is computed at the catalog level (it iterates the catalog to sum points across the row), and the catalog is already iterating definitions when it asks `IsUnlocked`. Passing the precomputed total avoids re-iterating per node.

Why the `state == null` branch matters: the pre-run **preview** calls this without a player (no `RunTalentState` exists yet). In preview mode, only roots show as "unlocked"; everything else shows the requirement text instead. That's the same semantic as before, just restated.

### `IsLockedByMutex(state)` — new helper

```csharp
public bool IsLockedByMutex(RunTalentState state)
{
    if (state == null || MutuallyExclusiveWithIds == null) return false;

    for (int i = 0; i < MutuallyExclusiveWithIds.Length; i++)
        if (state.HasPoints(MutuallyExclusiveWithIds[i])) return true;

    return false;
}
```

Intentionally uses `HasPoints` (not `IsRoot`/`IsMaxed`/etc.) — once you put a single point into a capstone, its twins lock immediately. There's no "you need 3 points before the lock kicks in" softening. That's the standard mutex semantic from PoE/Diablo.

Why exposed publicly (not just folded into `IsUnlocked`): the requirement text builder uses it to display "Locked by sibling capstone" specifically, separate from the generic "Locked: requires X" message. Different reason, different message.

### `TalentCatalog.CountRowPoints(state, rowId)` — new helper

```csharp
public static int CountRowPoints(RunTalentState state, string rowId)
{
    if (state == null || string.IsNullOrWhiteSpace(rowId)) return 0;

    int sum = 0;
    for (int i = 0; i < RunTalentDefinitions.Length; i++)
    {
        RunTalentDefinition definition = RunTalentDefinitions[i];
        if (definition.RowId == rowId)
            sum += state.GetPoints(definition.Id);
    }
    return sum;
}
```

Linear scan over ~54 entries per call. With 6 trees and ~9 nodes per tree, this is trivially fast even called per-node during UI refresh. No need for caching or a precomputed dict. If the catalog grows past a few hundred nodes, swap to a lazily-built `Dictionary<string, List<RunTalentDefinition>>` keyed by row.

### Mutex group bookkeeping — the `MutexAgainst` helper

```csharp
private static readonly string[] AttackPrimaryCapstoneMutex = { "atk_primary_overcharge", "atk_primary_annihilator" };
// … one per tree …

private static string[] MutexAgainst(string[] group, string self)
{
    List<string> others = new List<string>(group.Length);
    for (int i = 0; i < group.Length; i++)
        if (group[i] != self) others.Add(group[i]);
    return others.ToArray();
}
```

Why this shape: each capstone's `MutuallyExclusiveWithIds` array should contain *the other capstones in its group*, not itself. Listing the group once and computing each member's "others" by exclusion avoids hand-maintaining "A blocks B, B blocks A" pairs. If we ever scale to 3-way mutex groups, the same `MutexAgainst` call still works.

Note on initialization order: C# static field initializers execute in textual order. The mutex-group arrays are declared *before* `RunTalentDefinitions`, so they're populated when the catalog array calls `MutexAgainst(...)` during initialization. If you reorder these blocks, the mutex arrays will be `null` when the catalog tries to read them.

### Requirement text builder

```csharp
private static string BuildRequirementText(RunTalentDefinition definition)
{
    if (definition.IsRoot && definition.RequiredRowPoints == 0)
        return $"Tier {definition.Tier} | Root talent";

    List<string> parts = new List<string>();
    if (!definition.IsRoot)
        parts.Add($"Requires {GetTalentTitle(definition.ParentId)}");
    if (definition.RequiredRowPoints > 0)
        parts.Add($"{definition.RequiredRowPoints} pts in tree");
    if (definition.MutuallyExclusiveWithIds != null && definition.MutuallyExclusiveWithIds.Length > 0)
        parts.Add("Choose one capstone");

    return $"Tier {definition.Tier} | {string.Join(", ", parts)}";
}
```

This is the single source of truth for "what does it take to unlock this node?" Used by both the live tree node display (`BuildRunTalentNode`) and the per-option description (`CreateTalentOption`). Keeping the formatting in one helper means the level-up popup and the pre-run preview never disagree.

---

## Tree shape

Every tree is now this 4-tier, 9-node shape:

```
                       Root (Tier 1, max 5 pts)
                      /        |         \
                T2-A          T2-B         T2-C        ← Tier 2, max 5 pts each
                 |             |            |             gate: 1 row pt
                T3-A          T3-B         T3-C        ← Tier 3, max 3 pts each
                 |                          |             gate: 4 row pts
              T4-Left                    T4-Right      ← Tier 4 capstones, max 2 pts
                                                          gate: 9 row pts
                                                          mutually exclusive
```

Per-tree maximums:
- 5 (root) + 3 × 5 (T2) + 3 × 3 (T3) + 1 × 2 (only one capstone, since they're mutex) = **31 points** spendable per tree.
- With 6 trees, that's 186 points of total tree depth — enough that no single run could max everything, which is the goal.

### Why these specific gate values (1 / 4 / 9)?

- **T2 needs 1 row pt** — basically "you took the root once." Cheap gate; the tree is meant to fan early.
- **T3 needs 4 row pts** — root maxes at 5, so this is reachable purely from rooting up, but more naturally reached by also dipping into a T2 branch. Forces a small commitment, not a deep one.
- **T4 needs 9 row pts** — non-trivial. To hit 9 pts you have to invest in *multiple* tier-2 branches or fully max the root + one T2. This gate is the player's main "am I committed to this tree?" check, which is the right place for the binary capstone decision.

These numbers are tuned for a tree shape with 31 max points; if we change shape, retune the gates.

### Capstone parent assignment

Each capstone has a *specific* T3 parent (not "any T3"):
- `atk_primary_overcharge` → parent `atk_primary_focus` (the left T3)
- `atk_primary_annihilator` → parent `atk_primary_finisher` (the right T3)
- (the middle T3, `atk_primary_volley`, has no capstone child)

Same pattern across all six trees. This means the *middle* T3 branch is a "deep but no capstone" branch — picking it commits you to the row's row-point gate without forcing a capstone choice. That's intentional: it gives players a third path that doesn't engage the mutex tension.

The visual layout draws clean diagonal lines from T3-left → capstone-left and T3-right → capstone-right, with the middle T3 dangling.

### Character-specific scoping (preserved from existing system)

The existing `PlayerUpgradeOption.IsAvailableFor(character)` filter already lets options scope themselves to one playable character (Vanguard/Ranger/Arcanist). Tier-3 and tier-4 nodes that change *behavior* (Primary Focus, Primary Volley, Primary Finisher, capstones, Element Mastery, Element Overflow, Element Unleashed, Crossfire Master) all switch on `context.Character` and return scoped options. That preserves the "each character sees their flavored version of this node" behavior shipped in the previous loadout-aware-talents work — no regression there.

Generic stat nodes (HP, movespeed, pickup radius, bomb damage, skill cooldowns) stay `PlayerUpgradeScope.All` so every character sees the same effect.

---

## UI changes

### `RunTalentTreeView` — fields restructured

Old shape:
```csharp
public TalentCardView RootNode;
public TalentCardView LeftNode;
public TalentCardView RightNode;
public TalentCardView CapstoneNode;
```

New shape:
```csharp
public TalentCardView[] Nodes;          // length 9
public Image[] Connectors;              // length 8
public int[] ConnectorParentIndex;      // index into Nodes
```

Why drop the named fields: with 9 nodes per tree, named fields would be a lot of boilerplate, and the pattern is now "render whatever the catalog hands you." The `Nodes` array indexes match the order definitions appear in the catalog, which is sorted root → T2 → T3 → capstones for each row. So `Nodes[0]` is always the root, `Nodes[7..8]` are always the capstones.

### Topology tables — `TalentTreeNodePositions`, `TalentTreeNodeSizes`

Two parallel arrays declared at class scope:

```csharp
private static readonly Vector2[] TalentTreeNodePositions =
{
    new Vector2(0f, 138f),    // 0 — Tier 1 root
    new Vector2(-150f, 56f),  // 1 — Tier 2 left
    new Vector2(0f, 56f),     // 2 — Tier 2 middle
    new Vector2(150f, 56f),   // 3 — Tier 2 right
    new Vector2(-150f, -32f), // 4 — Tier 3 left
    new Vector2(0f, -32f),    // 5 — Tier 3 middle
    new Vector2(150f, -32f),  // 6 — Tier 3 right
    new Vector2(-90f, -136f), // 7 — Tier 4 capstone left  (parents node 4)
    new Vector2(90f, -136f)   // 8 — Tier 4 capstone right (parents node 6)
};
```

Vertical step is ~88px between tiers, horizontal step is 150px between siblings. Capstones sit closer to center (±90px) and lower (-136px) so the diagonal connectors from T3 outer columns fan inward, which reads as "two paths converge into one capstone choice each."

If you want to tune the layout, this is the only table to touch — the rest of the system is data-driven from these positions.

### `TalentTreeConnectorParents` / `Children` — connector topology

```csharp
private static readonly int[] TalentTreeConnectorParents  = { 0, 0, 0, 1, 2, 3, 4, 6 };
private static readonly int[] TalentTreeConnectorChildren = { 1, 2, 3, 4, 5, 6, 7, 8 };
```

Read pairwise: connector 0 goes from node 0 (root) to node 1 (T2-left), connector 3 goes from node 1 (T2-left) to node 4 (T3-left), and so on. Eight edges total — three from root, three between T2 and T3, two from T3-outer to capstones.

These tables are intentionally separate from the catalog's `ParentId` field. The catalog's parents drive *unlock logic*; these tables drive *visual lines*. They happen to match today, but I didn't want the renderer to introspect catalog data — that coupling would make it harder to display purely-cosmetic decorative connectors later (e.g. a faint "shared capstone synergy" line between trees).

### `CreateTreeConnector` — line math

```csharp
private static Image CreateTreeConnector(Transform parent, Vector2 fromAnchor, Vector2 toAnchor)
{
    GameObject lineObject = new GameObject("TreeConnector");
    lineObject.transform.SetParent(parent, false);

    Image image = lineObject.AddComponent<Image>();
    image.color = TreeConnectorIdleColor;
    image.raycastTarget = false;

    Vector2 delta = toAnchor - fromAnchor;
    float length = Mathf.Max(1f, delta.magnitude);
    Vector2 mid = (fromAnchor + toAnchor) * 0.5f;
    float angle = Mathf.Atan2(-delta.x, delta.y) * Mathf.Rad2Deg;

    RectTransform rect = lineObject.GetComponent<RectTransform>();
    rect.anchorMin = new Vector2(0.5f, 0.5f);
    rect.anchorMax = new Vector2(0.5f, 0.5f);
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = mid;
    rect.sizeDelta = new Vector2(2.5f, length);
    rect.localEulerAngles = new Vector3(0f, 0f, angle);

    return image;
}
```

The math derivation:
- A `RectTransform` with `sizeDelta = (thickness, length)` is a vertical rectangle by default — its long axis points up (+Y).
- To rotate it so the long axis aligns with the vector from `fromAnchor` to `toAnchor`, we need the rotation angle from the +Y axis to that direction.
- Unity's CCW-positive Z rotation θ takes (0, 1) to (-sin θ, cos θ). We want that to equal the unit direction `delta.normalized`.
- Solving: `θ = atan2(-delta.x, delta.y)` in radians. Multiply by `Mathf.Rad2Deg` for `localEulerAngles.z`.

`raycastTarget = false` matters — without it the connector lines would block clicks/hovers on the cards behind them.

`Mathf.Max(1f, delta.magnitude)` is paranoia: if two anchor positions ever collapse (shouldn't happen with this layout) we don't want a zero-sized image that Unity might render weirdly.

### Connector highlight logic

```csharp
private static void ApplyTreeConnectors(RunTalentTreeView view, RunUpgradeChainDisplayInfo tree)
{
    for (int i = 0; i < view.Connectors.Length; i++)
    {
        Image connector = view.Connectors[i];
        int parentIdx = view.ConnectorParentIndex[i];
        bool active = parentIdx < tree.Nodes.Length
            && !string.IsNullOrEmpty(tree.Nodes[parentIdx].StageText)
            && !tree.Nodes[parentIdx].StageText.StartsWith("0 ", System.StringComparison.Ordinal);

        connector.color = active ? TreeConnectorActiveColor : TreeConnectorIdleColor;
    }
}
```

A connector lights up (`TreeConnectorActiveColor`, warm gold @ 55% alpha) once the *parent* node has been invested in. Otherwise it's dim (`TreeConnectorIdleColor`, neutral grey @ 18% alpha).

The check `!StageText.StartsWith("0 ")` is a string sniff against the catalog-generated `"X / Y points"` format. It's slightly hacky — it assumes the catalog never produces a line starting with `"0 "` for an invested node. If we ever change `StageText` format, this needs to track the change. The robust alternative would be to expose a `Points` int on `RunUpgradeNodeDisplayInfo`, but that's wider surgery than this refactor wanted to take on.

### Locked-card dimming

```csharp
bool locked = !string.IsNullOrEmpty(node.RequirementText)
    && (node.RequirementText.StartsWith("Locked", System.StringComparison.OrdinalIgnoreCase));
ApplyTalentCardView(view, node.AccentColor, !locked, …);
```

`ApplyTalentCardView` already had a `bright` parameter for dimming dim cards. I reuse it: when the requirement text starts with `"Locked"` (which the catalog produces for both row-point gating and mutex locks), the card renders at the dimmed alpha. Same string-sniff caveat applies — if you change requirement-text formatting, recheck this.

### Layout sizing

Trees grew from 500×330 to 440×380. The old layout was 2 columns × 3 rows of trees with row spacing 338px and content size 1040×1120. With 380-tall trees, row spacing 338 is too tight (cards from adjacent rows would touch). Bumped:

| | Before | After |
|---|---|---|
| Tree size | 500×330 | 440×380 |
| Row spacing (Y stride) | 338 | 440 |
| Content size | 1040×1120 | 1040×1420 |
| Header Y | 520 | 670 |

The scroll view containing the trees is unchanged — just the inner content height grew, so the scroll bar becomes more useful at smaller window sizes.

---

## Things I deliberately did NOT do

- **No ScriptableObject migration.** The catalog stays as a hard-coded `RunTalentDefinition[]` with lambda effect/option builders. Pros of moving to SO assets: designer-editable, hot-reloadable, version-controllable as YAML. Cons: the lambdas (`context.Character` switches, character-scoped option builders) don't translate cleanly to ScriptableObject — you'd need a scripted-effect system to keep behavior expressivity. That's a separate, bigger refactor. The current array works and the cost of converting later is contained to one file.
- **No cross-tree edges.** Some games let an attack-tree node require a defense-tree investment. Adds complexity (`RequiredRowPoints` would need to be `RequiredPoints` per row-id) without much player-facing value at this stage.
- **No in-run tree visualization.** The level-up popup still surfaces upgrade *cards* (one per available node), not a tree-shaped picker. The pre-run preview is the only visual tree right now. Adding an in-run visual tree means new UI, pause-handling, and input wiring — ship-worthy on its own, not in scope here.
- **No tier separators / tier headers.** The `Tier` field is in the data but not yet rendered as e.g. a "Tier 2" label between rows. Easy to add later if the layout reads ambiguously.

---

## Files touched

- `Assets/Scripts/GameSystems/TalentCatalog.cs` — main refactor target
  - `RunTalentDefinition` class: 3 new fields, new constructor, new `IsUnlocked(state, rowPoints)`, new `IsLockedByMutex`
  - `TalentCatalog` class: 6 new mutex-group arrays, new `MutexAgainst` helper, new `CountRowPoints`, new `BuildRequirementText`, updated `BuildAvailableRunTalentOptions`, updated `BuildRunTalentNode`, updated `CreateTalentOption`
  - `RunTalentDefinitions` array: 24 entries → 54 entries
  - 30 new option-builder methods, 11 new effect-text methods
- `Assets/Scripts/UIScripts/MainMenuRuntime.cs`
  - `RunTalentTreeView` class: replaced 4 named fields with `Nodes[]`, `Connectors[]`, `ConnectorParentIndex[]`
  - 4 new static topology tables (`TalentTreeNodePositions`, `TalentTreeNodeSizes`, `TalentTreeConnectorParents`, `TalentTreeConnectorChildren`)
  - 2 new static color constants (`TreeConnectorActiveColor`, `TreeConnectorIdleColor`)
  - Replaced `CreateRunTalentTreeView` with array-driven 9-card construction
  - New `CreateTreeConnector` for thin rotated `Image` line segments
  - Replaced `RefreshRunTalentTrees` to iterate all 9 nodes
  - Updated `ApplyTreeNode` to dim locked cards
  - New `ApplyTreeConnectors` for connector highlighting
  - Adjusted scroll content size and tree row positions in `BuildTalentBrowserPanel`

`RunTalentState.cs` was *not* touched — the existing `GetPoints` / `HasPoints` / `AddPoint` interface was sufficient once the row-aware logic moved into `TalentCatalog`.

---

## Verification

- `dotnet build Assembly-CSharp.csproj` → 0 errors, 43 pre-existing warnings (all are Unity API deprecations like `FindObjectOfType` and `enableWordWrapping` in code I didn't touch).
- The talent system's contract with consumers is unchanged: `BuildAvailableRunTalentOptions` still returns a `List<PlayerUpgradeOption>`, `BuildCurrentRunTalentTrees` still returns `RunUpgradeChainDisplayInfo[]`, etc. So `LevelUpManager`, `PlayerExperience`, and `PlayerUpgradeOption.Apply()` continue to work without changes.

**Not yet verified:** I couldn't run Unity from my environment, so I haven't *visually* confirmed the trees render correctly. Things to eyeball in the **Single Player → Talent Browser** screen:

- All 6 trees show 9 cards each, no overlap.
- Connector lines reach exactly from parent-card center to child-card center (no off-by-anything).
- Connectors dim/brighten correctly as you spend points (only verifiable in-run, since the preview has no `RunTalentState`).
- Capstones (T4) sit just below the T3 row without colliding.
- "Attack" / "Defense" headers still sit above their columns.

If any line is misaligned, tune the `TalentTreeNodePositions` array in `MainMenuRuntime.cs`. If a card looks wrong, tune `TalentTreeNodeSizes` for that index.
