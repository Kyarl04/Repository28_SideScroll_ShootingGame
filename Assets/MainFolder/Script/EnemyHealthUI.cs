using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    private float maxHealth = 100f;
    private float currentHealth;

    private void Start() => currentHealth = maxHealth;

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        // fillAmount는 0~1 사이값
        healthBar.fillAmount = currentHealth / maxHealth;
    }
}