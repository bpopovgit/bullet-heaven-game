using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private float iFrameTime = 0.3f;
    [SerializeField] private float knockbackForce = 6f;

    private int _hp;
    private bool _invuln;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _hp = maxHP;
        _rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int amount, Vector2 sourcePos)
    {
        if (_invuln) return;

        _hp -= amount;
        if (_rb)
        {
            var dir = ((Vector2)transform.position - sourcePos).normalized;
            _rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
        }
        if (_hp <= 0)
        {
            Debug.Log("Player died");
            // TODO: respawn / reset
        }
        else
        {
            StartCoroutine(Invulnerability());
        }
    }

    private System.Collections.IEnumerator Invulnerability()
    {
        _invuln = true;
        yield return new WaitForSeconds(iFrameTime);
        _invuln = false;
    }
}
