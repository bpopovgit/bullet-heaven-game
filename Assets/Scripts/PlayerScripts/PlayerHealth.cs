using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float iFrameTime = 0.3f;
    [SerializeField] private float knockbackForce = 6f;

    [Header("UI")]
    [SerializeField] private GameObject gameOverScreen;

    [Header("Hit VFX")]
    [SerializeField] private GameObject fireHitVFX;
    [SerializeField] private GameObject frostHitVFX;
    [SerializeField] private GameObject poisonHitVFX;
    [SerializeField] private GameObject lightningHitVFX;

    private int _hp;
    private bool _invuln;
    private Rigidbody2D _rb;
    private StatusReceiver _status;

    public int CurrentHP => _hp;
    public int MaxHP => maxHP;

    public event Action<int, int> HealthChanged;

    private void Awake()
    {
        _hp = maxHP;
        _rb = GetComponent<Rigidbody2D>();
        _status = GetComponent<StatusReceiver>();
    }

    private void Start()
    {
        PublishHealth();
    }

    public void TakeDamage(int amount, Vector2 sourcePos, bool applyKnockback = true)
    {
        if (_invuln || _hp <= 0) return;

        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        PublishHealth();
        GameAudio.PlayPlayerHit();
        Debug.Log($"Player took {amount} damage. HP now: {_hp}");

        if (applyKnockback && knockbackForce > 0f && _rb != null)
        {
            Vector2 dir = ((Vector2)transform.position - sourcePos).normalized;
            _rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }

        if (_hp <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(Invulnerability());
        }
    }

    public void TakeDamage(DamagePacket packet, bool applyKnockback = true)
    {
        Debug.Log($"PLAYER PACKET DAMAGE CALLED. Element = {packet.element}, Status = {packet.status}");

        packet.Clamp();

        bool wasInvuln = _invuln;
        bool wasDead = _hp <= 0;
        int hpBefore = _hp;

        TakeDamage(packet.amount, packet.sourcePos, applyKnockback);

        bool damageApplied = !wasInvuln && !wasDead && _hp < hpBefore;

        Debug.Log($"Damage applied = {damageApplied}, HP before = {hpBefore}, HP now = {_hp}");

        if (!damageApplied) return;

        Debug.Log($"Spawning hit VFX for element: {packet.element}");
        SpawnHitVFX(packet.element);

        if (_status != null)
            _status.ApplyStatus(packet);
    }

    public void TakeDamageDirect(int amount)
    {
        if (_hp <= 0) return;

        _hp = Mathf.Max(0, _hp - Mathf.Max(0, amount));
        PublishHealth();
        Debug.Log($"Player took {amount} DOT damage. HP now: {_hp}");

        if (_hp <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || _hp <= 0)
            return;

        _hp = Mathf.Min(maxHP, _hp + amount);
        PublishHealth();
        Debug.Log($"Player healed {amount}. HP now: {_hp}");
    }

    public void IncreaseMaxHP(int amount, bool healSameAmount)
    {
        if (amount <= 0)
            return;

        maxHP += amount;

        if (healSameAmount)
            Heal(amount);
        else
            PublishHealth();

        Debug.Log($"Player max HP increased by {amount}. Max HP now: {maxHP}");
    }

    private void PublishHealth()
    {
        HealthChanged?.Invoke(_hp, maxHP);
    }

    private void SpawnHitVFX(DamageElement element)
    {
        GameObject prefab = null;

        switch (element)
        {
            case DamageElement.Fire:
                prefab = fireHitVFX;
                break;
            case DamageElement.Frost:
                prefab = frostHitVFX;
                break;
            case DamageElement.Poison:
                prefab = poisonHitVFX;
                break;
            case DamageElement.Lightning:
                prefab = lightningHitVFX;
                break;
        }

        if (prefab != null)
            Instantiate(prefab, transform.position, Quaternion.identity);
    }

    private void Die()
    {
        Debug.Log("PLAYER DIED");
        GameAudio.PlayPlayerDeath();

        if (RunTimer.Instance != null)
            RunTimer.Instance.StopTimer();

        if (gameOverScreen != null)
            gameOverScreen.SetActive(true);

        Time.timeScale = 0f;
    }

    private System.Collections.IEnumerator Invulnerability()
    {
        _invuln = true;
        yield return new WaitForSeconds(iFrameTime);
        _invuln = false;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
