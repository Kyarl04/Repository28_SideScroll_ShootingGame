using UnityEngine;
using System.Collections;
using DG.Tweening;       // DOTween 필수 네임스페이스
using Danmaku.Data;      // 업로드하신 ScriptableObject 데이터 사용

public class DanmakuBoss : MonoBehaviour
{
    [Header("Pattern Data")]
    [Tooltip("유니티 인스펙터에서 우클릭으로 생성한 DanmakuData를 넣으세요")]
    public DanmakuData currentPattern; 
    
    private Transform player;

    private void Start()
    {
        // 씬에서 플레이어를 자동으로 찾음
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        // 패턴 데이터가 있으면 실행
        if (currentPattern != null)
        {
            StartCoroutine(ExecuteDanmaku(currentPattern));
        }
    }

    private IEnumerator ExecuteDanmaku(DanmakuData danmaku)
    {
        // ============================================
        // 1. DOTween을 활용한 이동 로직 (DanmakuMove)
        // ============================================
        if (danmaku.move != null && danmaku.move.type == DanmakuMoveType.RandomMove)
        {
            yield return new WaitForSeconds(danmaku.move.startDelay);
            
            // 데이터에 정의된 최소/최대 범위 내에서 랜덤 위치 선정
            Vector3 targetPos = new Vector3(
                Random.Range(danmaku.move.minX, danmaku.move.maxX),
                Random.Range(danmaku.move.minY, danmaku.move.maxY),
                0
            );

            // [DOTween] 목표 위치로 1.5초 동안 부드럽게(InOutSine) 이동
            transform.DOMove(targetPos, 1.5f).SetEase(Ease.InOutSine);
            
            // 이동이 끝날 때까지 대기
            yield return new WaitForSeconds(1.5f); 
        }

        // ============================================
        // 2. 탄막 발사 로직 (BarrageData 읽기)
        // ============================================
        foreach (var barrage in danmaku.data)
        {
            StartCoroutine(FireBarrageRoutine(barrage));
        }
    }

    private IEnumerator FireBarrageRoutine(BarrageData barrage)
    {
        // 발사 전 딜레이 대기
        yield return new WaitForSeconds(barrage.startDelay);

        // interval 간격으로 반복 사격 (무한 발사)
        while (true)
        {
            Fire(barrage.fireData, barrage.shotData);
            yield return new WaitForSeconds(barrage.interval);
        }
    }

    // 실제 총알을 쏘는 핵심 함수
    private void Fire(FireData fireData, ShotData shotData)
    {
        // 1. 기준 방향 설정 (조준형 vs 고정형)
        Vector3 centerDir = fireData.startDir;
        if (fireData.directionType == DirectionType.Aimed && player != null)
        {
            centerDir = (player.position - transform.position).normalized;
        }

        // 시작 각도 오프셋 적용
        centerDir = Quaternion.Euler(0, 0, fireData.startAngle) * centerDir;
        
        float bulletSpeed = shotData.speed.value;
        int pIndex = shotData.prefabIndex; // <--- 데이터에서 프리팹 인덱스를 가져옵니다!

        // 2. FireType(Round, Sector, Spray)에 따른 패턴 발사
        if (fireData.type == FireType.Round) // 360도 원형 방사
        {
            int count = 16; 
            float angleStep = 360f / count;
            
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, angleStep * i) * centerDir;
                SpawnBullet(dir, bulletSpeed, pIndex); // <--- 인덱스 추가!
            }
        }
        else if (fireData.type == FireType.Sector) // 부채꼴 모양
        {
            int count = 5;
            float spreadAngle = 15f; 
            float startAngle = -((count - 1) * spreadAngle) / 2f;

            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, startAngle + (spreadAngle * i)) * centerDir;
                SpawnBullet(dir, bulletSpeed, pIndex); // <--- 인덱스 추가!
            }
        }
        else if (fireData.type == FireType.Spray) // 랜덤 흩뿌리기
        {
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                float randomAngle = Random.Range(-45f, 45f);
                Vector3 dir = Quaternion.Euler(0, 0, randomAngle) * centerDir;
                SpawnBullet(dir, bulletSpeed, pIndex); // <--- 인덱스 추가!
            }
        }
    }
    
    private void SpawnBullet(Vector3 dir, float speed, int prefabIndex)
    {
        if (BulletPooler.Instance == null) return;

        // 인덱스를 넘겨서 원하는 총알을 꺼냄
        GameObject bullet = BulletPooler.Instance.GetBullet(prefabIndex, transform.position, Quaternion.identity);
        
        if (bullet != null)
        {
            Bullet b = bullet.GetComponent<Bullet>();
            b.poolIndex = prefabIndex;
            
            // [핵심 해결] Rigidbody2D.velocity 대신 업그레이드된 Setup() 함수를 호출합니다!
            // 이 함수가 불려야 총알의 방향, 속도, 수명(Lifetime)이 정상적으로 세팅됩니다.
            b.Setup(dir, speed);
        }
    }
}