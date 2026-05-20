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
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(10f); // 데미지 적용
            
            // [추가된 부분] 총알을 즉시 비활성화(없앰) 처리합니다.
            Bullet bulletScript = other.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                // BulletPooler를 사용하는 총알이면 풀로 반환
                BulletPooler.Instance.ReturnBullet(other.gameObject, bulletScript.poolIndex);
            }
            else
            {
                // 구형 방식(Instantiate)으로 만든 총알이라면 그냥 파괴
                other.gameObject.SetActive(false); 
            }
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