using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthBar; // 인스펙터에서 링 모양 Image를 여기에 드래그

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 태그 확인 (Bullet 태그가 정확히 설정되어 있는지 확인)
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(10f);
            
            // 탄환은 풀(Pool)로 반환하거나 삭제
            // (이미 BulletPooler를 쓴다면 BulletPooler.Instance.ReturnBullet(other.gameObject) 사용)
            Destroy(other.gameObject); 
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // 2. UI 갱신 (fillAmount는 0~1 사이 값)
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // 파괴 로직
        Destroy(gameObject);
    }
}