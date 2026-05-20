using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    public int poolIndex;

    public void Setup(Vector2 dir, float spd)
    {
        direction = dir;
        speed = spd;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
        
        // 화면 밖으로 나가면 풀로 반환
        if (IsOffScreen())
        {
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex);
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
        // 적과 충돌했는지 확인
        if (other.CompareTag("Enemy"))
        {
            // 탄환 비활성화 및 풀로 반환할 때, poolIndex를 함께 넘겨줍니다!
            BulletPooler.Instance.ReturnBullet(gameObject, poolIndex); 
        }
    }
}