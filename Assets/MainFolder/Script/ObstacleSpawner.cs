using UnityEngine;
using System.Collections;

/// <summary>
/// 특정 페이즈 동안 파괴 가능한 탄막을 우측 경계에서 주기적으로 소환하는 매니저 클래스.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    [Header("오브젝트 풀링 세팅")]
    [Tooltip("BulletPooler에 등록한 파괴 가능 탄의 프리팹 인덱스")]
    public int obstaclePoolIndex = 1; 
    [Tooltip("탄이 터질 때 재생할 이펙트 인덱스")]
    public int explosionEffectIndex = 0;

    [Header("레벨 디자인 파라미터 (조절 가능)")]
    [Tooltip("탄들이 스폰되는 시간 간격")]
    public float spawnInterval = 0.5f;
    [Tooltip("탄들의 비행 속도")]
    public float obstacleSpeed = 4f;
    [Tooltip("탄들이 버틸 수 있는 체력")]
    public float obstacleHealth = 20f;

    [Header("스폰 위치 제한 (Y축 범위)")]
    public float minY = -4f;
    public float maxY = 4f;

    private Coroutine spawnRoutine;

    /// <summary>
    /// 보스 스크립트에서 특정 페이즈가 시작될 때 이 함수를 호출하여 스폰을 켭니다.
    /// </summary>
    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    /// <summary>
    /// 페이즈가 끝나거나 보스가 격추되면 소환을 중단합니다.
    /// </summary>
    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        Camera cam = Camera.main;

        while (true)
        {
            if (BulletPooler.Instance != null)
            {
                // 화면 우측 외부 좌표 계산 (Viewport 1.1 이면 화면 오른쪽 바깥)
                Vector3 spawnWorldPos = cam.ViewportToWorldPoint(new Vector3(1.1f, 0, 0));
                // Y축은 랜덤하게 분산시켜 다양한 높이에서 날아오도록 설정
                spawnWorldPos.y = Random.Range(minY, maxY);
                spawnWorldPos.z = 0;

                // 풀에서 파괴 가능한 탄을 꺼내옵니다.
                GameObject obstacle = BulletPooler.Instance.GetBullet(obstaclePoolIndex, spawnWorldPos, Quaternion.identity);
                
                if (obstacle != null)
                {
                    DestructibleObstacle targetScript = obstacle.GetComponent<DestructibleObstacle>();
                    if (targetScript != null)
                    {
                        // 인스펙터에 기입된 속도, 체력 데이터를 주입(Dependency Injection)하여 동적 조절 가능케 함
                        targetScript.Initialize(obstacleSpeed, obstacleHealth, explosionEffectIndex, obstaclePoolIndex);
                    }
                }
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}