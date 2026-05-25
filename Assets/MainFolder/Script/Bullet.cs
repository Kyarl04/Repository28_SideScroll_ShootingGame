using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    public int poolIndex;
    
    [Header("Sprite Settings")]
    [Tooltip("이미지가 아래를 향하면 90, 위를 향하면 -90을 입력하세요.")]
    public float rotationOffset = 90f;

    [Header("Stats")]
    public float damage = 10f;
    
    [Header("Upgraded Features")]
    public float lifetime = 5f; // 총알의 최대 수명 (화면 안에 멈춰있는 총알 방지)
    private float currentLifetime;

    public void Setup(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        currentLifetime = lifetime; // 태어날 때 수명 초기화

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        if (IsOffScreen())
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex);
            return; // [추가] 반환 후 아래 코드가 실행되지 않도록 즉시 멈춤!
        }
        
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0)
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex);
            return; // [추가] 반환 후 즉시 멈춤!
        }
    }

    private bool IsOffScreen()
    {
        // 간단한 화면 경계 체크
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 적과 충돌했는지 확인 (플레이어 총알일 경우)
        if (other.CompareTag("Enemy"))
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex); 
        }
        // 플레이어와 충돌했는지 확인 (적 총알일 경우) - 태그 이름은 프로젝트에 맞게 수정하세요
        else if (other.CompareTag("Player"))
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex); 
        }
    }
}