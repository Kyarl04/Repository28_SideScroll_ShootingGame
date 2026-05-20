using System.Collections.Generic;
using UnityEngine;

public class BulletPooler : MonoBehaviour
{
    public static BulletPooler Instance;

    [Header("Bullet Prefabs (인덱스 순서대로 등록)")]
    [Tooltip("0:기본탄, 1:빨간탄, 2:파란탄 등등...")]
    public GameObject[] bulletPrefabs; 
    public int poolSizePerPrefab = 100;

    // 큐들의 리스트 (프리팹 개수만큼 큐를 생성)
    private List<Queue<GameObject>> pools = new List<Queue<GameObject>>();

    private void Awake() 
    {
        Instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        for (int i = 0; i < bulletPrefabs.Length; i++)
        {
            Queue<GameObject> newPool = new Queue<GameObject>();
            
            // 프리팹마다 100개씩 미리 생성
            for (int j = 0; j < poolSizePerPrefab; j++)
            {
                GameObject obj = Instantiate(bulletPrefabs[i], transform);
                obj.SetActive(false);
                newPool.Enqueue(obj);
            }
            pools.Add(newPool);
        }
    }

    // [핵심] 이제 인덱스 번호를 받아서 해당 큐에서 총알을 꺼냅니다.
    public GameObject GetBullet(int index, Vector3 position, Quaternion rotation)
    {
        // 잘못된 인덱스 방지
        if (index < 0 || index >= pools.Count) 
        {
            Debug.LogError($"BulletPooler: {index}번 인덱스가 없습니다!");
            return null;
        }

        if (pools[index].Count > 0)
        {
            GameObject obj = pools[index].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 큐가 비었을 경우 추가 생성 (동적 확장)
            GameObject obj = Instantiate(bulletPrefabs[index], transform);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            return obj;
        }
    }

    // 반환할 때 이 총알이 몇 번 인덱스인지 알아야 하므로 이름(name)을 활용합니다.
    // (더 완벽한 방법은 Bullet 스크립트 안에 자신의 index를 저장해두는 것입니다)
    public void ReturnBullet(GameObject obj, int index)
    {
        obj.SetActive(false);
        pools[index].Enqueue(obj);
    }
}