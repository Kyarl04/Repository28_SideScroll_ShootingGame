using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class LaserBulletClear : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("총알이 지워질 때 터질 이펙트의 풀(Pool) 인덱스 번호 (예: 0)")]
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
            
            if (other.CompareTag("EnemyBullet"))
            {
                // ==========================================
                // [추가된 부분] 총알이 사라지기 전에 그 위치(other.transform.position)에 이펙트를 터뜨립니다!
                // ==========================================
                if (BulletPooler.Instance != null)
                {
                    GameObject effect = BulletPooler.Instance.GetEffect(clearEffectIndex, other.transform.position, Quaternion.identity);
                    
                    // PooledEffect 스크립트가 있다면 인덱스를 세팅해 줍니다 (Enemy.cs와 동일한 방식)
                    if (effect != null) 
                    {
                        var pooledEffect = effect.GetComponent<PooledEffect>();
                        if (pooledEffect != null)
                        {
                            pooledEffect.effectPoolIndex = clearEffectIndex;
                        }
                    }
                }

                // 기존 총알 지우는 코드
                Bullet b = other.GetComponent<Bullet>();
                
                if (b != null)
                {
                    BulletPooler.Instance.ReturnBullet(other.gameObject, b.poolIndex);
                }
                else
                {
                    other.gameObject.SetActive(false);
                }
            }
        }
    }
}