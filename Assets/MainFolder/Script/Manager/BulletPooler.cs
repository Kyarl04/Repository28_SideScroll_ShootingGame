using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링(Object Pooling) 패턴을 활용한 가비지 컬렉션(GC) 부하 최적화 클래스.
/// 탄막 슈팅 게임 특성상 빈번한 Instantiate/Destroy로 인한 메모리 단편화 및 프레임 드랍을 원천 차단합니다.
/// </summary>
public class BulletPooler : MonoBehaviour
{
    public static BulletPooler Instance;

    [Header("1. Bullet Prefabs (인덱스 순서대로 등록)")]
    public GameObject[] bulletPrefabs; 
    public int poolSizePerBullet = 100;

    [Header("2. Effect Prefabs (인덱스 순서대로 등록)")]
    public GameObject[] effectPrefabs; 
    public int poolSizePerEffect = 20;  

    // 동적 다형성 관리를 위한 인덱스 매핑 기반 Queue 자료구조 리스트
    private List<Queue<GameObject>> bulletPools = new List<Queue<GameObject>>();
    private List<Queue<GameObject>> effectPools = new List<Queue<GameObject>>(); 

    private void Awake() 
    {
        Instance = this;
        InitializeBulletPools();
        InitializeEffectPools(); 
    }

    private void InitializeBulletPools()
    {
        bulletPools.Clear();
        for (int i = 0; i < bulletPrefabs.Length; i++)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            for (int j = 0; j < poolSizePerBullet; j++)
            {
                if (bulletPrefabs[i] == null) continue;
                GameObject obj = Instantiate(bulletPrefabs[i], transform);
                obj.SetActive(false); // 메모리 할당 후 즉시 비활성화하여 대기 상태 진입
                newPool.Enqueue(obj);
            }
            bulletPools.Add(newPool);
        }
    }

    /// <summary>
    /// 풀에서 총알 오브젝트를 디큐(Dequeue)하여 재사용 가능한 상태로 활성화합니다.
    /// </summary>
    public GameObject GetBullet(int index, Vector3 position, Quaternion rotation)
    {
        if (index < 0 || index >= bulletPools.Count) return null;

        while (bulletPools[index].Count > 0)
        {
            GameObject obj = bulletPools[index].Dequeue();
            if (obj == null) continue; // 예외적으로 파괴된 오브젝트 탐색 시 스킵
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        // 런타임 중 예측 범위를 초과하여 풀이 고갈될 경우를 대비한 동적 확장 안전장치
        GameObject newObj = Instantiate(bulletPrefabs[index], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        return newObj;
    }

    /// <summary>
    /// 사용이 끝난 총알을 풀에 인큐(Enqueue)하여 반환합니다.
    /// </summary>
    public void ReturnBullet(GameObject obj, int index)
    {
        if (obj == null) return;
        
        // 중복 반환 요청 발생 시 동일 객체가 중복 인큐되어 발생하는 런타임 순환 참조 에러 방지
        if (!obj.activeSelf) return; 

        if (index < 0 || index >= bulletPools.Count)
        {
            Destroy(obj); 
            return;
        }
        obj.SetActive(false);
        bulletPools[index].Enqueue(obj);
    }

    private void InitializeEffectPools()
    {
        effectPools.Clear();
        for (int i = 0; i < effectPrefabs.Length; i++)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            for (int j = 0; j < poolSizePerEffect; j++)
            {
                if (effectPrefabs[i] == null) continue;
                GameObject obj = Instantiate(effectPrefabs[i], transform);
                obj.SetActive(false);
                newPool.Enqueue(obj);
            }
            effectPools.Add(newPool);
        }
    }

    /// <summary>
    /// 풀에서 이펙트 오브젝트를 인스턴스화 및 디큐하여 반환합니다. 원본 소실 예외 방지 처리가 적용되었습니다.
    /// </summary>
    public GameObject GetEffect(int index, Vector3 position, Quaternion rotation)
    {
        if (index < 0 || index >= effectPools.Count) return null;

        while (effectPools[index].Count > 0)
        {
            GameObject obj = effectPools[index].Dequeue();
            if (obj == null) continue;
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        // 인스펙터 바인딩 실수 혹은 에디터 캐시 소실로 인한 원본 프리팹 누락 사전 차단 보강 코드
        if (effectPrefabs[index] == null)
        {
            Debug.LogError($"❌ [BulletPooler] Effect Prefabs의 {index}번 칸이 비어있거나 파괴된 오브젝트입니다!");
            return null;
        }

        GameObject newObj = Instantiate(effectPrefabs[index], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        return newObj;
    }

    public void ReturnEffect(GameObject obj, int index)
    {
        if (obj == null) return;
        if (index < 0 || index >= effectPools.Count)
        {
            Destroy(obj); 
            return;
        }
        obj.SetActive(false);
        effectPools[index].Enqueue(obj);
    }
}