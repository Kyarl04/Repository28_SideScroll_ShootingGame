using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    [SerializeField] private GameObject explosionEffect; // 파괴 시 이펙트

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 태그를 사용하여 플레이어 탄환인지 확인
        if (other.CompareTag("Bullet"))
        {
            TakeDamage(10f); // 탄환의 데미지
            // 탄환은 즉시 오브젝트 풀로 반환 (Bullet 스크립트에서 처리)
        }
    }

    private void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        
        Destroy(gameObject); // 혹은 적 오브젝트 풀로 반환
    }
}