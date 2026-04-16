using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Slider healthSlider;

    private void OnEnable()
    {
        FindPlayerHealthIfNeeded();

        if (playerHealth != null)
            playerHealth.HealthChanged += HandleHealthChanged;

        Refresh();
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        Refresh();
    }

    private void FindPlayerHealthIfNeeded()
    {
        if (playerHealth != null)
            return;

        playerHealth = FindObjectOfType<PlayerHealth>();
    }

    private void Refresh()
    {
        if (playerHealth == null)
            return;

        HandleHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);
    }

    private void HandleHealthChanged(int currentHP, int maxHP)
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHP} / {maxHP}";

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = Mathf.Max(1, maxHP);
            healthSlider.value = Mathf.Clamp(currentHP, 0, maxHP);
        }
    }
}
