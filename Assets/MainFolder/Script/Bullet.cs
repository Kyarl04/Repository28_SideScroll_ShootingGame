using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 총알의 이동, 수명, 충돌 처리를 담당하는 클래스.
/// 최적화를 위해 Instantiate/Destroy를 사용하지 않고 BulletPooler를 통해 재사용됩니다.
/// </summary>
public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    public int poolIndex; // 자신이 속한 풀(Pool)의 고유 번호
    
    [Header("Sprite Settings")]
    [Tooltip("이미지가 아래를 향하면 90, 위를 향하면 -90을 입력하세요.")]
    public float rotationOffset = 90f;

    [Header("Stats")]
    public float damage = 10f;
    
    [Header("Upgraded Features")]
    public float lifetime = 5f; // 총알의 최대 수명 (메모리 누수 방지용)
    private float currentLifetime;

    /// <summary>
    /// 오브젝트 풀에서 꺼내질 때 호출되어 방향과 속도를 초기화하는 함수
    /// </summary>
    public void Setup(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;
        currentLifetime = lifetime; 

        // 날아가는 방향에 맞춰 총알의 이미지를 회전시킵니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);
    }

    void Update()
    {
        // 물리 엔진(Rigidbody)에 의존하지 않고 Transform을 직접 이동시켜 연산 부하를 최소화합니다.
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        
        // 1. 화면 밖으로 나갔을 때 풀로 반환
        if (IsOffScreen())
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex);
            return; 
        }
        
        // 2. 수명이 다했을 때 풀로 반환 (어딘가에 껴서 무한히 날아가는 버그 방지)
        currentLifetime -= Time.deltaTime;
        if (currentLifetime <= 0)
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex);
            return; 
        }
    }

    /// <summary>
    /// 메인 카메라의 뷰포트 좌표계를 활용하여 화면 밖으로 나갔는지 판별합니다.
    /// </summary>
    private bool IsOffScreen()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 충돌 대상의 태그를 확인하고 적중 시 스스로를 풀로 반환합니다.
        if (other.CompareTag("Enemy"))
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex); 
        }
        else if (other.CompareTag("Player"))
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex); 
        }
    }
}