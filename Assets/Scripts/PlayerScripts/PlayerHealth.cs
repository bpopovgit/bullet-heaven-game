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
    [SerializeField] private GameObject gameOverScreen;   // assign in Inspector

    private int _hp;
    private bool _invuln;
    private Rigidbody2D _rb;
    private StatusReceiver _status;

    private void Awake()
    {
        _hp = maxHP;
        _rb = GetComponent<Rigidbody2D>();
        _status = GetComponent<StatusReceiver>();
    }

    /// <summary>
    /// Take damage. sourcePos = where the hit came from.
    /// applyKnockback = false for bullets that should NOT push the player.
    /// </summary>
    public void TakeDamage(int amount, Vector2 sourcePos, bool applyKnockback = true)
    {
        if (_invuln || _hp <= 0) return;   // already dead or currently invulnerable

        _hp -= amount;
        Debug.Log($"Player took {amount} damage. HP now: {_hp}");

        // Optional knockback
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
        packet.Clamp();

        // Capture state before calling the int method
        bool wasInvuln = _invuln;
        bool wasDead = _hp <= 0;

        TakeDamage(packet.amount, packet.sourcePos, applyKnockback);

        // If damage was ignored, also ignore status
        if (wasInvuln || wasDead) return;

        if (_status != null)
            _status.ApplyStatus(packet);
    }

    public void TakeDamageDirect(int amount)
    {
        if (_hp <= 0) return; // already dead
        _hp -= amount;

        Debug.Log($"Player took {amount} DOT damage. HP now: {_hp}");

        if (_hp <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("PLAYER DIED");

        // Show Game Over UI if assigned
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        // Pause gameplay
        Time.timeScale = 0f;
    }

    private System.Collections.IEnumerator Invulnerability()
    {
        _invuln = true;
        yield return new WaitForSeconds(iFrameTime);
        _invuln = false;
    }

    // Called by the Restart button on the Game Over screen
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
