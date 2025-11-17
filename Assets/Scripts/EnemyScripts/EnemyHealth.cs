using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 30;
    [SerializeField] private bool destroyOnDeath = true;
    private int _hp;

    private void Awake() => _hp = maxHP;

    public void TakeDamage(int amount)
    {
        _hp -= amount;
        if (_hp <= 0)
        {
            if (destroyOnDeath) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
