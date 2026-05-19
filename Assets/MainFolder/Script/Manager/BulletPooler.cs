using System.Collections.Generic;
using UnityEngine;

public class BulletPooler : MonoBehaviour
{
    public static BulletPooler Instance;
    public GameObject bulletPrefab;
    public int poolSize = 200;
    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake() => Instance = this;

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }
        return null; // 부족하면 추가 생성 로직 필요
    }

    public void ReturnBullet(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}