using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 레이저 공격 판정을 처리하는 클래스.
/// OnTriggerStay의 프레임 드랍을 방지하기 위해 Dictionary 자료구조를 활용하여 지속 데미지를 부여합니다.
/// </summary>
public class LaserHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 50f;

    [Header("Effect Settings")]
    public int hitEffectPoolIndex = 0; 

    // 공격받고 있는 적들을 추적하기 위한 딕셔너리 (탐색 속도 O(1) 최적화)
    private Dictionary<Transform, GameObject> activeEffects = new Dictionary<Transform, GameObject>();

    private void OnEnable()
    {
        activeEffects.Clear();
    }

    private void OnDisable()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.StopLaserHitSFX(); 
        RemoveAllEffects();
    }

    private void Update()
    {
        // 딕셔너리에 등록된(레이저에 닿아있는) 적들에게만 Update에서 지속 데미지를 부여합니다.
        // 이는 물리 엔진의 OnTriggerStay보다 연산 비용이 훨씬 저렴합니다.
        List<Transform> keys = new List<Transform>(activeEffects.Keys);
        foreach (var enemyTransform in keys)
        {
            if (enemyTransform == null || !enemyTransform.gameObject.activeSelf) continue;

            Enemy enemyScript = enemyTransform.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// 적이 레이저에 닿았을 때 딕셔너리에 등록하고, 해당 적의 위치에 이펙트를 부착합니다.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (SoundManager.Instance != null) SoundManager.Instance.StartLaserHitSFX();
            
            Transform enemyTrans = other.transform;
            if (activeEffects.ContainsKey(enemyTrans)) return; // 중복 부착 방지

            if (BulletPooler.Instance != null)
            {
                GameObject effect = BulletPooler.Instance.GetEffect(hitEffectPoolIndex, enemyTrans.position, Quaternion.identity);

                if (effect != null)
                {
                    // 꺼낸 이펙트가 적을 따라다니도록 자식(Child) 오브젝트로 편입시킵니다.
                    effect.transform.SetParent(enemyTrans);
                    
                    PooledEffect pe = effect.GetComponent<PooledEffect>();
                    if(pe != null) pe.effectPoolIndex = hitEffectPoolIndex;

                    activeEffects.Add(enemyTrans, effect);
                }
            }
        }
    }

    /// <summary>
    /// 적이 레이저에서 벗어났을 때 이펙트를 떼어내고 풀에 반환합니다.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Transform enemyTrans = other.transform;

            if (activeEffects.TryGetValue(enemyTrans, out GameObject effect))
            {
                if (BulletPooler.Instance != null && effect != null)
                {
                    effect.transform.SetParent(null); // 자식 관계 해제
                    BulletPooler.Instance.ReturnEffect(effect, hitEffectPoolIndex);
                }
                activeEffects.Remove(enemyTrans);
            }
        }
    }

    private void RemoveAllEffects()
    {
        foreach (var kvp in activeEffects)
        {
            if (kvp.Value != null)
            {
                kvp.Value.transform.SetParent(null);
                if (BulletPooler.Instance != null)
                {
                    BulletPooler.Instance.ReturnEffect(kvp.Value, hitEffectPoolIndex);
                }
            }
        }
        activeEffects.Clear();
    }
}