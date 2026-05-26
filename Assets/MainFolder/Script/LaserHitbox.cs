using UnityEngine;
using System.Collections.Generic;

public class LaserHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("레이저에 닿아있는 동안 1초당 적에게 입히는 데미지")]
    public float damagePerSecond = 50f;

    [Header("Effect Settings")]
    [Tooltip("BulletPooler에 등록된 피격 이펙트의 인덱스 번호")]
    public int hitEffectPoolIndex = 0; // 우리는 보통 0번에 타격 이펙트를 넣었었죠?

    // 현재 이펙트가 붙어있는 적들을 관리하는 딕셔너리
    // Key: 공격받는 적의 Transform, Value: 적에게 붙어있는 이펙트 오브젝트
    private Dictionary<Transform, GameObject> activeEffects = new Dictionary<Transform, GameObject>();

    // 스크립트가 켜질 때 딕셔너리 초기화
    private void OnEnable()
    {
        activeEffects.Clear();
    }

    // 스크립트가 꺼질 때(레이저 발사 종료 시) 붙어있던 이펙트들을 모두 회수
    private void OnDisable()
    {
        RemoveAllEffects();
    }

    private void Update()
    {
        // 딕셔너리 내에 데미지를 입고 있는 적들에게 지속적으로 피해를 줍니다.
        // Update나 FixedUpdate에서 처리하는 것이 렉이 덜 걸립니다.
        List<Transform> keys = new List<Transform>(activeEffects.Keys);
        foreach (var enemyTransform in keys)
        {
            if (enemyTransform == null || !enemyTransform.gameObject.activeSelf)
            {
                // 적이 죽거나 비활성화되었다면 리스트에서 제거 (다음 프레임에)
                continue;
            }

            Enemy enemyScript = enemyTransform.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                // 지속 데미지 적용
                enemyScript.TakeDamage(damagePerSecond * Time.deltaTime);
            }
        }
    }

    // 1. 적이 레이저 범위에 처음 들어왔을 때 (이펙트 소환 및 부착)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Transform enemyTrans = other.transform;

            // 이미 이펙트가 붙어있는 적이라면 무시
            if (activeEffects.ContainsKey(enemyTrans)) return;

            // BulletPooler에서 이펙트를 꺼냅니다.
            if (BulletPooler.Instance != null)
            {
                // 적의 위치(other.transform.position)에 이펙트를 꺼냅니다.
                GameObject effect = BulletPooler.Instance.GetEffect(hitEffectPoolIndex, enemyTrans.position, Quaternion.identity);

                if (effect != null)
                {
                    // [핵심] 꺼낸 이펙트가 적을 따라다니도록 자식(Child)으로 등록합니다.
                    effect.transform.SetParent(enemyTrans);
                    
                    // 이펙트 스크립트에 풀 정보를 입력해주어야 나중에 스스로 풀에 돌아갈 수 있습니다.
                    PooledEffect pe = effect.GetComponent<PooledEffect>();
                    if(pe != null) pe.effectPoolIndex = hitEffectPoolIndex;

                    // 딕셔너리에 저장하여 관리
                    activeEffects.Add(enemyTrans, effect);
                }
            }
        }
    }

    // 2. 적이 레이저 범위를 벗어났을 때 (이펙트 회수 및 자식 해제)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Transform enemyTrans = other.transform;

            // 이 적에게 붙어있는 이펙트를 찾아서 회수합니다.
            if (activeEffects.TryGetValue(enemyTrans, out GameObject effect))
            {
                if (BulletPooler.Instance != null && effect != null)
                {
                    // 1. 자식 해제
                    effect.transform.SetParent(null);
                    // 2. 풀에 반환
                    BulletPooler.Instance.ReturnEffect(effect, hitEffectPoolIndex);
                }
                activeEffects.Remove(enemyTrans);
            }
        }
    }

    // 모든 이펙트를 안전하게 회수하는 함수
    private void RemoveAllEffects()
    {
        foreach (var kvp in activeEffects)
        {
            if (kvp.Value != null)
            {
                // 자식 해제 및 풀 반환
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