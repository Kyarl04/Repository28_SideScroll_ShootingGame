using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 빈번한 탄알 생성을 최적화하기 위한 오브젝트 풀링 클래스입니다.
/// </summary>
public class ObjectPool : MonoBehaviour {
    
    public static ObjectPool Instance;

    // 이름(Key)별 대기 중인 오브젝트 리스트(Value)
    private Dictionary<string, List<GameObject>> pool = new Dictionary<string, List<GameObject>>();
    
    // 프리팹 캐싱용 딕셔너리
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    /// <summary>
    /// 오브젝트 풀에서 객체를 가져오거나 새로 생성합니다.
    /// </summary>
    public GameObject GetObj(GameObject prefab)
    {
        string objName = prefab.name;

        // 1. 풀에 재사용 가능한 오브젝트가 있는지 확인
        if (pool.ContainsKey(objName) && pool[objName].Count > 0)
        {
            GameObject result = pool[objName][0];
            pool[objName].RemoveAt(0);
            result.SetActive(true);
            return result;
        }

        // 2. 풀에 없다면 새로 생성
        GameObject newObj = Instantiate(prefab);
        newObj.name = objName; // 'Clone' 이름 제거
        return newObj;
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 회수합니다.
    /// </summary>
    public void RecycleObj(GameObject obj)
    {
        obj.SetActive(false);

        if (!pool.ContainsKey(obj.name))
        {
            pool.Add(obj.name, new List<GameObject>());
        }

        pool[obj.name].Add(obj);
    }
}