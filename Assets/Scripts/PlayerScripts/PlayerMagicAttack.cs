using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerMagicAttack : MonoBehaviour
{
    private enum MagicPattern
    {
        EmberWave,
        FrostRay,
        VenomBloom,
        StormArc
    }

    [Header("Input")]
    [SerializeField] private Camera mainCam;

    [Header("Spell")]
    [SerializeField] private bool magicEnabled;
    [SerializeField] private MagicPattern pattern = MagicPattern.EmberWave;
    [SerializeField] private int baseDamage = 30;
    [SerializeField] private float range = 7.5f;
    [SerializeField] private float beamWidth = 0.55f;
    [SerializeField] private float cooldown = 0.55f;
    [SerializeField] private float visualDuration = 0.16f;
    [SerializeField] private DamageElement element = DamageElement.Lightning;
    [SerializeField] private StatusEffect status = StatusEffect.Shock;
    [SerializeField] private float statusChance = 0.3f;
    [SerializeField] private float statusDuration = 1.6f;
    [SerializeField] private float statusStrength = 0.28f;
    [SerializeField] private Color spellColor = new Color(0.45f, 0.92f, 1f, 0.9f);

    [Header("Spell Pattern Tuning")]
    [SerializeField] private float emberConeAngle = 74f;
    [SerializeField] private int stormChainCount = 4;
    [SerializeField] private float stormChainRadius = 2.65f;

    private PlayerInput _playerInput;
    private InputAction _fire;
    private PlayerStats _stats;
    private FactionMember _faction;
    private float _cooldownRemaining;

    public void ConfigureForCharacter(PlayableCharacterChoice character, StartingWeaponChoice primaryChoice)
    {
        magicEnabled = character == PlayableCharacterChoice.HumanArcanist;

        if (!magicEnabled)
            return;

        switch (primaryChoice)
        {
            case StartingWeaponChoice.FrostLance:
                ConfigureSpell(MagicPattern.FrostRay, DamageElement.Frost, StatusEffect.Slow, 32, 7.25f, 0.62f, 0.62f, 0.45f, 2.5f, 0.38f, new Color(0.55f, 0.9f, 1f, 0.9f));
                break;

            case StartingWeaponChoice.VenomCaster:
                ConfigureSpell(MagicPattern.VenomBloom, DamageElement.Poison, StatusEffect.Poison, 28, 6.6f, 1.05f, 0.74f, 0.55f, 3.2f, 0.38f, new Color(0.45f, 1f, 0.42f, 0.88f));
                break;

            case StartingWeaponChoice.StormNeedler:
                ConfigureSpell(MagicPattern.StormArc, DamageElement.Lightning, StatusEffect.Shock, 30, 5.55f, 0.5f, 0.5f, 0.42f, 1.55f, 0.34f, new Color(1f, 0.95f, 0.28f, 0.95f));
                break;

            case StartingWeaponChoice.EmberRepeater:
            default:
                ConfigureSpell(MagicPattern.EmberWave, DamageElement.Fire, StatusEffect.Burn, 30, 5.35f, 1.08f, 0.58f, 0.4f, 2.4f, 0.32f, new Color(1f, 0.43f, 0.14f, 0.86f));
                break;
        }
    }

    public void AddRange(float amount)
    {
        range += Mathf.Max(0f, amount);
        Debug.Log($"ARCANIST SPELL RANGE: {range:0.00}");
    }

    public void AddBeamWidth(float amount)
    {
        beamWidth += Mathf.Max(0f, amount);
        Debug.Log($"ARCANIST SPELL SIZE: {beamWidth:0.00}");
    }

    public void ReduceCooldown(float seconds)
    {
        cooldown = Mathf.Max(0.16f, cooldown - Mathf.Max(0f, seconds));
        Debug.Log($"ARCANIST SPELL COOLDOWN: {cooldown:0.00}s");
    }

    public void AddStatusChance(float amount)
    {
        statusChance = Mathf.Clamp01(statusChance + Mathf.Max(0f, amount));
        Debug.Log($"ARCANIST STATUS CHANCE: {statusChance:P0}");
    }

    private void ConfigureSpell(
        MagicPattern newPattern,
        DamageElement newElement,
        StatusEffect newStatus,
        int newDamage,
        float newRange,
        float newBeamWidth,
        float newCooldown,
        float newStatusChance,
        float newStatusDuration,
        float newStatusStrength,
        Color newSpellColor)
    {
        pattern = newPattern;
        element = newElement;
        status = newStatus;
        baseDamage = Mathf.Max(1, newDamage);
        range = Mathf.Max(1f, newRange);
        beamWidth = Mathf.Max(0.1f, newBeamWidth);
        cooldown = Mathf.Max(0.05f, newCooldown);
        statusChance = Mathf.Clamp01(newStatusChance);
        statusDuration = Mathf.Max(0f, newStatusDuration);
        statusStrength = Mathf.Clamp01(newStatusStrength);
        spellColor = newSpellColor;
    }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _stats = GetComponent<PlayerStats>();
        _faction = FactionMember.Ensure(gameObject, FactionType.Human);

        if (mainCam == null)
            mainCam = Camera.main;
    }

    private void OnEnable()
    {
        if (_playerInput == null)
            _playerInput = GetComponent<PlayerInput>();

        _fire = _playerInput.actions["Fire"];
        _playerInput.actions.Enable();
    }

    private void Update()
    {
        if (!magicEnabled)
            return;

        _cooldownRemaining -= Time.deltaTime;

        if (_cooldownRemaining > 0f || _fire == null || !_fire.IsPressed())
            return;

        Vector2 direction = GetAimDirection();
        CastSpell(direction);
        _cooldownRemaining = cooldown;
    }

    private Vector2 GetAimDirection()
    {
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam == null || Mouse.current == null)
            return transform.right.sqrMagnitude > 0.01f ? (Vector2)transform.right : Vector2.right;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mouseWorld.z = 0f;
        Vector2 direction = mouseWorld - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            direction = transform.right.sqrMagnitude > 0.01f ? (Vector2)transform.right : Vector2.right;

        return direction.normalized;
    }

    private void CastSpell(Vector2 direction)
    {
        direction = direction.sqrMagnitude > 0.001f ? direction.normalized : Vector2.right;

        switch (pattern)
        {
            case MagicPattern.FrostRay:
                CastFrostRay(direction);
                break;

            case MagicPattern.VenomBloom:
                CastVenomBloom(direction);
                break;

            case MagicPattern.StormArc:
                CastStormArc(direction);
                break;

            case MagicPattern.EmberWave:
            default:
                CastEmberWave(direction);
                break;
        }
    }

    private void CastFrostRay(Vector2 direction)
    {
        Vector2 origin = transform.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range + beamWidth);
        List<EnemyHealth> damagedEnemies = new List<EnemyHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemy = GetValidEnemy(hits[i]);
            if (enemy == null || damagedEnemies.Contains(enemy))
                continue;

            Vector2 targetPosition = enemy.transform.position;
            Vector2 toTarget = targetPosition - origin;
            float forwardDistance = Vector2.Dot(toTarget, direction);
            if (forwardDistance < 0f || forwardDistance > range)
                continue;

            Vector2 closestPoint = origin + direction * forwardDistance;
            float perpendicularDistance = Vector2.Distance(targetPosition, closestPoint);
            if (perpendicularDistance > beamWidth)
                continue;

            if (TryDamageEnemy(enemy, CreateDamagePacket(origin)))
                damagedEnemies.Add(enemy);
        }

        SpawnRayVisual(origin, direction);
    }

    private void CastEmberWave(Vector2 direction)
    {
        Vector2 origin = transform.position;
        float coneAngle = GetEmberConeAngle();
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, range);
        List<EnemyHealth> damagedEnemies = new List<EnemyHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemy = GetValidEnemy(hits[i]);
            if (enemy == null || damagedEnemies.Contains(enemy))
                continue;

            Vector2 toTarget = (Vector2)enemy.transform.position - origin;
            if (toTarget.sqrMagnitude <= 0.001f || toTarget.magnitude > range)
                continue;

            float angle = Vector2.Angle(direction, toTarget.normalized);
            if (angle > coneAngle * 0.5f)
                continue;

            if (TryDamageEnemy(enemy, CreateDamagePacket(origin)))
                damagedEnemies.Add(enemy);
        }

        SpawnConeVisual(origin, direction, coneAngle);
    }

    private void CastVenomBloom(Vector2 direction)
    {
        Vector2 origin = transform.position;
        Vector2 center = origin + direction * range;
        float radius = GetVenomBloomRadius();
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);
        List<EnemyHealth> damagedEnemies = new List<EnemyHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemy = GetValidEnemy(hits[i]);
            if (enemy == null || damagedEnemies.Contains(enemy))
                continue;

            if (TryDamageEnemy(enemy, CreateDamagePacket(center)))
                damagedEnemies.Add(enemy);
        }

        SpawnBloomVisual(center, radius);
    }

    private void CastStormArc(Vector2 direction)
    {
        Vector2 origin = transform.position;
        EnemyHealth firstTarget = FindFirstLineTarget(origin, direction, range, Mathf.Max(beamWidth, 0.45f));
        List<Vector2> arcPoints = new List<Vector2> { origin };
        List<EnemyHealth> hitEnemies = new List<EnemyHealth>();

        if (firstTarget == null)
        {
            arcPoints.Add(origin + direction * range);
            SpawnStormVisual(arcPoints);
            return;
        }

        EnemyHealth currentTarget = firstTarget;
        Vector2 damageSource = origin;
        int safeChainCount = Mathf.Max(1, stormChainCount);

        for (int i = 0; i < safeChainCount && currentTarget != null; i++)
        {
            Vector2 targetPosition = currentTarget.transform.position;
            float damageMultiplier = i == 0 ? 1f : 0.82f;

            if (TryDamageEnemy(currentTarget, CreateDamagePacket(damageSource, damageMultiplier)))
            {
                hitEnemies.Add(currentTarget);
                arcPoints.Add(targetPosition);
                SpawnCircleFlash(targetPosition, beamWidth * 1.35f, new Color(spellColor.r, spellColor.g, spellColor.b, 0.36f));
            }

            damageSource = targetPosition;
            currentTarget = FindRandomChainTarget(targetPosition, GetStormChainRadius(), hitEnemies);
        }

        SpawnStormVisual(arcPoints);
    }

    private DamagePacket CreateDamagePacket(Vector2 sourcePosition, float damageMultiplier = 1f)
    {
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * (_stats != null ? _stats.DamageMultiplier : 1f)));
        DamagePacket packet = new DamagePacket
        {
            amount = Mathf.Max(1, Mathf.RoundToInt(damage * Mathf.Max(0.05f, damageMultiplier))),
            element = element,
            splashRadius = 0f,
            sourcePos = sourcePosition,
            status = Random.value <= statusChance ? status : StatusEffect.None,
            statusDuration = statusDuration,
            statusStrength = statusStrength
        };
        packet.Clamp();

        return packet;
    }

    private EnemyHealth GetValidEnemy(Collider2D hit)
    {
        if (hit == null)
            return null;

        EnemyHealth enemy = hit.GetComponentInParent<EnemyHealth>();
        if (enemy == null || enemy.IsDead || !IsHostile(enemy))
            return null;

        return enemy;
    }

    private bool IsHostile(EnemyHealth enemy)
    {
        if (enemy == null || _faction == null)
            return true;

        FactionMember targetFaction = enemy.GetComponentInParent<FactionMember>();
        if (targetFaction == null)
            return true;

        return targetFaction != _faction && FactionTargeting.AreHostile(_faction, targetFaction);
    }

    private bool TryDamageEnemy(EnemyHealth enemy, DamagePacket packet)
    {
        return enemy != null && FactionCombat.TryApplyDamage(enemy.gameObject, packet, _faction, applyPlayerKnockback: false);
    }

    private EnemyHealth FindFirstLineTarget(Vector2 origin, Vector2 direction, float maxRange, float width)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, maxRange + width);
        EnemyHealth bestTarget = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemy = GetValidEnemy(hits[i]);
            if (enemy == null)
                continue;

            Vector2 targetPosition = enemy.transform.position;
            Vector2 toTarget = targetPosition - origin;
            float forwardDistance = Vector2.Dot(toTarget, direction);
            if (forwardDistance < 0f || forwardDistance > maxRange)
                continue;

            Vector2 closestPoint = origin + direction * forwardDistance;
            float perpendicularDistance = Vector2.Distance(targetPosition, closestPoint);
            if (perpendicularDistance > width)
                continue;

            if (forwardDistance < bestDistance)
            {
                bestDistance = forwardDistance;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    private EnemyHealth FindRandomChainTarget(Vector2 origin, float radius, List<EnemyHealth> alreadyHit)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);
        List<EnemyHealth> candidates = new List<EnemyHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyHealth enemy = GetValidEnemy(hits[i]);
            if (enemy == null || alreadyHit.Contains(enemy) || candidates.Contains(enemy))
                continue;

            candidates.Add(enemy);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    private float GetEmberConeAngle()
    {
        return Mathf.Clamp(emberConeAngle + (beamWidth - 1f) * 18f, 52f, 118f);
    }

    private float GetVenomBloomRadius()
    {
        return Mathf.Max(1.2f, beamWidth * 1.75f);
    }

    private float GetStormChainRadius()
    {
        return Mathf.Max(1.75f, stormChainRadius + beamWidth * 0.55f);
    }

    private void SpawnRayVisual(Vector2 origin, Vector2 direction)
    {
        GameObject visual = new GameObject("ArcanistSpellVisual");
        LineRenderer line = visual.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction.normalized * range);
        line.startWidth = beamWidth * 0.42f;
        line.endWidth = beamWidth * 0.16f;
        line.numCapVertices = 8;
        line.sortingLayerName = "Actors";
        line.sortingOrder = 13;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = spellColor;
        line.endColor = new Color(spellColor.r, spellColor.g, spellColor.b, 0.08f);

        GameObject impact = new GameObject("ArcanistSpellImpact");
        impact.transform.SetParent(visual.transform, false);
        impact.transform.position = origin + direction.normalized * range;
        impact.transform.localScale = Vector3.one * (beamWidth * 1.35f);

        SpriteRenderer impactRenderer = impact.AddComponent<SpriteRenderer>();
        impactRenderer.sprite = PickupSpriteFactory.CircleSprite;
        impactRenderer.color = new Color(spellColor.r, spellColor.g, spellColor.b, 0.38f);
        impactRenderer.sortingLayerName = "Actors";
        impactRenderer.sortingOrder = 14;

        Destroy(visual, visualDuration);
    }

    private void SpawnConeVisual(Vector2 origin, Vector2 direction, float coneAngle)
    {
        GameObject visual = new GameObject("ArcanistEmberWaveVisual");
        visual.transform.position = origin + direction.normalized * 0.1f;
        visual.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        MeshFilter filter = visual.AddComponent<MeshFilter>();
        MeshRenderer renderer = visual.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.material.color = new Color(spellColor.r, spellColor.g, spellColor.b, 0.38f);
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 13;

        filter.mesh = CreateArcMesh(range, coneAngle, segments: 22);
        Destroy(visual, Mathf.Max(visualDuration, 0.18f));
    }

    private void SpawnBloomVisual(Vector2 center, float radius)
    {
        GameObject root = new GameObject("ArcanistVenomBloomVisual");
        root.transform.position = center;

        SpawnCircleFlash(center, radius * 1.12f, new Color(spellColor.r, spellColor.g, spellColor.b, 0.2f), root.transform);

        int puffs = 8;
        for (int i = 0; i < puffs; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius * 0.62f;
            float scale = Random.Range(radius * 0.42f, radius * 0.76f);
            Color puffColor = new Color(spellColor.r, spellColor.g, spellColor.b, Random.Range(0.18f, 0.34f));
            SpawnCircleFlash(center + offset, scale, puffColor, root.transform);
        }

        Destroy(root, Mathf.Max(visualDuration, 0.42f));
    }

    private void SpawnStormVisual(List<Vector2> points)
    {
        if (points == null || points.Count < 2)
            return;

        GameObject visual = new GameObject("ArcanistStormArcVisual");
        LineRenderer line = visual.AddComponent<LineRenderer>();
        List<Vector3> jaggedPoints = BuildJaggedLinePoints(points);

        line.useWorldSpace = true;
        line.positionCount = jaggedPoints.Count;
        for (int i = 0; i < jaggedPoints.Count; i++)
            line.SetPosition(i, jaggedPoints[i]);

        line.startWidth = Mathf.Max(0.08f, beamWidth * 0.32f);
        line.endWidth = Mathf.Max(0.035f, beamWidth * 0.14f);
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.sortingLayerName = "Actors";
        line.sortingOrder = 16;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = spellColor;
        line.endColor = new Color(1f, 1f, 1f, 0.12f);

        Destroy(visual, Mathf.Max(visualDuration, 0.12f));
    }

    private List<Vector3> BuildJaggedLinePoints(List<Vector2> points)
    {
        List<Vector3> jaggedPoints = new List<Vector3>();
        jaggedPoints.Add(points[0]);

        for (int i = 1; i < points.Count; i++)
        {
            Vector2 start = points[i - 1];
            Vector2 end = points[i];
            Vector2 segment = end - start;
            Vector2 perpendicular = segment.sqrMagnitude > 0.001f ? new Vector2(-segment.y, segment.x).normalized : Vector2.up;
            Vector2 midpoint = Vector2.Lerp(start, end, 0.5f) + perpendicular * Random.Range(-0.28f, 0.28f);

            jaggedPoints.Add(midpoint);
            jaggedPoints.Add(end);
        }

        return jaggedPoints;
    }

    private void SpawnCircleFlash(Vector2 position, float scale, Color color, Transform parent = null)
    {
        GameObject flash = new GameObject("ArcanistSpellFlash");
        if (parent != null)
            flash.transform.SetParent(parent, false);

        flash.transform.position = position;
        flash.transform.localScale = Vector3.one * Mathf.Max(0.08f, scale);

        SpriteRenderer renderer = flash.AddComponent<SpriteRenderer>();
        renderer.sprite = PickupSpriteFactory.CircleSprite;
        renderer.color = color;
        renderer.sortingLayerName = "Actors";
        renderer.sortingOrder = 15;

        if (parent == null)
            Destroy(flash, Mathf.Max(visualDuration, 0.16f));
    }

    private static Mesh CreateArcMesh(float radius, float arcAngle, int segments)
    {
        int safeSegments = Mathf.Max(3, segments);
        Vector3[] vertices = new Vector3[safeSegments + 2];
        int[] triangles = new int[safeSegments * 3];

        vertices[0] = Vector3.zero;
        float startAngle = -arcAngle * 0.5f;
        float step = arcAngle / safeSegments;

        for (int i = 0; i <= safeSegments; i++)
        {
            float angle = (startAngle + step * i) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
        }

        for (int i = 0; i < safeSegments; i++)
        {
            int triIndex = i * 3;
            triangles[triIndex] = 0;
            triangles[triIndex + 1] = i + 1;
            triangles[triIndex + 2] = i + 2;
        }

        Mesh mesh = new Mesh
        {
            name = "ArcanistSpellArc",
            vertices = vertices,
            triangles = triangles
        };
        mesh.RecalculateBounds();
        return mesh;
    }
}
