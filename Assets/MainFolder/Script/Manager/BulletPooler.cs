using System.Collections.Generic;
using UnityEngine;

public class BulletPooler : MonoBehaviour
{
    public static BulletPooler Instance;

    [Header("1. Bullet Prefabs (인덱스 순서대로 등록)")]
    public GameObject[] bulletPrefabs; 
    public int poolSizePerBullet = 100;

    [Header("2. Effect Prefabs (인덱스 순서대로 등록)")]
    public GameObject[] effectPrefabs; // 새로 추가됨!
    public int poolSizePerEffect = 20;  // 이펙트는 총알보다 적게

    // 큐들의 리스트 (프리팹 개수만큼 큐를 생성)
    private List<Queue<GameObject>> bulletPools = new List<Queue<GameObject>>();
    private List<Queue<GameObject>> effectPools = new List<Queue<GameObject>>(); // 새로 추가됨!

    private void Awake() 
    {
        Instance = this;
        InitializeBulletPools();
        InitializeEffectPools(); // 새로 추가됨!
    }

    // ============================================
    // [Bullet 관리 로직 - 기존과 동일]
    // ============================================
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
                obj.SetActive(false);
                newPool.Enqueue(obj);
            }
            bulletPools.Add(newPool);
        }
    }

    public GameObject GetBullet(int index, Vector3 position, Quaternion rotation)
    {
        if (index < 0 || index >= bulletPools.Count) return null;

        while (bulletPools[index].Count > 0)
        {
            GameObject obj = bulletPools[index].Dequeue();
            if (obj == null) continue;
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        GameObject newObj = Instantiate(bulletPrefabs[index], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        return newObj;
    }

    public void ReturnBullet(GameObject obj, int index)
    {
        if (obj == null) return;
        
        // [핵심 해결책] 이미 비활성화된(풀에 들어간) 총알은 두 번 받지 않습니다.
        if (!obj.activeSelf) return; 

        if (index < 0 || index >= bulletPools.Count)
        {
            Destroy(obj); 
            return;
        }
        obj.SetActive(false);
        bulletPools[index].Enqueue(obj);
    }

    // ============================================
    // [NEW] Effect 관리 로직 (새로 추가됨!)
    // ============================================
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

    // 이펙트를 꺼내는 함수
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

        GameObject newObj = Instantiate(effectPrefabs[index], transform);
        newObj.transform.position = position;
        newObj.transform.rotation = rotation;
        return newObj;
    }

    // 이펙트를 돌려보내는 함수 (이펙트 스스로 호출하게 만들 것입니다)
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