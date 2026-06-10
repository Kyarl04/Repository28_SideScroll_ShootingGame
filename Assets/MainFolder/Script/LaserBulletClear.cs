using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class LaserBulletClear : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("일반 적 총알(EnemyBullet)이 지워질 때 터질 이펙트의 풀(Pool) 인덱스 번호")]
    public int clearEffectIndex = 0; 

    private Collider2D col;
    private ContactFilter2D filter;
    private List<Collider2D> results = new List<Collider2D>();

    private void Start()
    {
        col = GetComponent<Collider2D>();
        filter.useTriggers = true; 
        filter.useLayerMask = false; 
    }

    private void Update()
    {
        if (col == null) return;

        int count = col.OverlapCollider(filter, results);
        
        for (int i = 0; i < count; i++)
        {
            Collider2D other = results[i];
            
            // 1. 일반 적 총알 (EnemyBullet) 처리 로직
            if (other.CompareTag("EnemyBullet"))
            {
                if (BulletPooler.Instance != null)
                {
                    GameObject effect = BulletPooler.Instance.GetEffect(clearEffectIndex, other.transform.position, Quaternion.identity);
                    if (effect != null) 
                    {
                        var pooledEffect = effect.GetComponent<PooledEffect>();
                        if (pooledEffect != null) pooledEffect.effectPoolIndex = clearEffectIndex;
                    }
                }

                Bullet b = other.GetComponent<Bullet>();
                if (b != null) BulletPooler.Instance.ReturnBullet(other.gameObject, b.poolIndex);
                else other.gameObject.SetActive(false);
            }
            // =========================================================
            // 2. [NEW] 파괴 가능한 장애물 탄막 (ObstacleBullet) 처리 로직
            // =========================================================
            else if (other.CompareTag("ObstacleBullet"))
            {
                DestructibleObstacle obstacle = other.GetComponent<DestructibleObstacle>();
                if (obstacle != null)
                {
                    // [핵심 요구사항] 장애물이 태어날 때 세팅된 고유의 이펙트 번호(플레이어 탄 피격 이펙트)를 가져옵니다.
                    int obstacleEffectIndex = obstacle.effectIndex;

                    // 레이저 전용 이펙트가 아니라, 방금 가져온 장애물 전용 이펙트를 터뜨려 줍니다!
                    if (BulletPooler.Instance != null)
                    {
                        GameObject effect = BulletPooler.Instance.GetEffect(obstacleEffectIndex, other.transform.position, Quaternion.identity);
                        if (effect != null)
                        {
                            var pooledEffect = effect.GetComponent<PooledEffect>();
                            if (pooledEffect != null) pooledEffect.effectPoolIndex = obstacleEffectIndex;
                        }
                    }

                    // 이펙트를 재생했으므로 장애물 오브젝트는 중복 연산 없이 바로 풀로 안전하게 회수합니다.
                    obstacle.ReturnToPool();
                }
            }
        }
    }
}