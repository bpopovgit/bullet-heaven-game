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

    private void Awake()
    {
        _hp = maxHP;
        _rb = GetComponent<Rigidbody2D>();
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
