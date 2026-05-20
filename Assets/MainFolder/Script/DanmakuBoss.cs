using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;       
using Danmaku.Data;      

// 인스펙터에서 편하게 페이즈를 조립하기 위한 클래스
[System.Serializable]
public class BossPhase
{
    [Tooltip("이 페이즈에 사용할 탄막/이동 패턴")]
    public DanmakuData pattern;
    [Tooltip("이 페이즈의 보스 체력")]
    public float phaseHP = 1000f;
    [Tooltip("페이즈 진입 시 터트릴 파티클 이펙트 프리팹 (선택)")]
    public GameObject transitionEffect;
    [Tooltip("페이즈 진입 시 변경할 배경 이미지 (선택)")]
    public Sprite newBackground;
}

public class DanmakuBoss : MonoBehaviour
{
    [Header("Phase Settings")]
    [Tooltip("페이즈를 원하는 만큼 추가하세요!")]
    public List<BossPhase> phases;
    public Vector3 centerPosition = new Vector3(0, 3f, 0); // 복귀할 원래 위치
    
    // 상태 변수
    private int currentPhaseIndex = 0;
    public bool isTransitioning = false; // 무적 및 전환 상태
    
    private Transform player;
    private Enemy enemyScript;
    private PlayerDrivenParallax parallaxScript;

    private void Start()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        enemyScript = GetComponent<Enemy>();
        parallaxScript = FindObjectOfType<PlayerDrivenParallax>(); // 씬에 있는 패럴랙스 배경 자동 찾기

        if (phases.Count > 0)
        {
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
    }

    // 1. 특정 페이즈 시작
    private IEnumerator StartPhaseRoutine(int index)
    {
        isTransitioning = false;
        BossPhase currentPhase = phases[index];

        // 체력 및 UI 세팅
        if (enemyScript != null) enemyScript.SetupHP(currentPhase.phaseHP);

        // 탄막 및 이동 패턴 실행
        if (currentPhase.pattern != null)
        {
            yield return StartCoroutine(ExecuteDanmaku(currentPhase.pattern));
        }
    }

    // 2. 체력이 0이 되었을 때 Enemy 스크립트에서 호출됨
    public void OnPhaseDefeated()
    {
        if (isTransitioning) return;
        StartCoroutine(PhaseTransitionRoutine());
    }

    // 3. 페이즈 전환 연출 (이동, 배경 변경, 이펙트)
    private IEnumerator PhaseTransitionRoutine()
    {
        isTransitioning = true;
        
        // 진행 중인 모든 탄막 발사 및 이동 코루틴 강제 정지
        StopAllCoroutines(); 
        transform.DOKill(); // DOTween 이동 강제 취소

        // UI 끄기 및 파티클 발생
        if (enemyScript != null) enemyScript.HideUI();
        if (phases[currentPhaseIndex].transitionEffect != null)
        {
            Instantiate(phases[currentPhaseIndex].transitionEffect, transform.position, Quaternion.identity);
        }

        // 중앙으로 복귀 (1.5초 동안 부드럽게)
        transform.DOMove(centerPosition, 1.5f).SetEase(Ease.InOutQuad);

        // 배경 전환 연출 (PlayerDrivenParallax의 기능 활용)
        if (parallaxScript != null && phases[currentPhaseIndex].newBackground != null)
        {
            parallaxScript.ChangePhase(phases[currentPhaseIndex].newBackground);
        }

        // 전환 연출을 감상하며 3초 대기
        yield return new WaitForSeconds(3.0f);

        // 다음 페이즈로 넘어가기
        currentPhaseIndex++;
        
        if (currentPhaseIndex < phases.Count)
        {
            // 다음 페이즈 시작
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
        else
        {
            // 모든 페이즈 끝 (보스 파괴)
            Instantiate(phases[currentPhaseIndex - 1].transitionEffect, transform.position, Quaternion.identity); // 마지막 폭발
            Destroy(gameObject);
            Debug.Log("보스 클리어!");
        }
    }

    // ============================================
    // [아래는 기존의 ExecuteDanmaku, FireBarrageRoutine, Fire, SpawnBullet 코드 그대로 유지]
    // ============================================

    private IEnumerator ExecuteDanmaku(DanmakuData danmaku)
    {
        if (danmaku.move != null && danmaku.move.type == DanmakuMoveType.RandomMove)
        {
            yield return new WaitForSeconds(danmaku.move.startDelay);
            Vector3 targetPos = new Vector3(
                Random.Range(danmaku.move.minX, danmaku.move.maxX),
                Random.Range(danmaku.move.minY, danmaku.move.maxY),
                0
            );
            transform.DOMove(targetPos, 1.5f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(1.5f); 
        }

        foreach (var barrage in danmaku.data)
        {
            StartCoroutine(FireBarrageRoutine(barrage));
        }
    }

    private IEnumerator FireBarrageRoutine(BarrageData barrage)
    {
        yield return new WaitForSeconds(barrage.startDelay);
        while (true)
        {
            Fire(barrage.fireData, barrage.shotData);
            yield return new WaitForSeconds(barrage.interval);
        }
    }

    private void Fire(FireData fireData, ShotData shotData)
    {
        Vector3 centerDir = fireData.startDir;
        if (fireData.directionType == DirectionType.Aimed && player != null)
        {
            centerDir = (player.position - transform.position).normalized;
        }

        centerDir = Quaternion.Euler(0, 0, fireData.startAngle) * centerDir;
        
        float bulletSpeed = shotData.speed.value;
        int pIndex = shotData.prefabIndex; 

        if (fireData.type == FireType.Round) 
        {
            int count = 16; 
            float angleStep = 360f / count;
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, angleStep * i) * centerDir;
                SpawnBullet(dir, bulletSpeed, pIndex); 
            }
        }
        else if (fireData.type == FireType.Sector) 
        {
            int count = 5;
            float spreadAngle = 15f; 
            float startAngle = -((count - 1) * spreadAngle) / 2f;
            for (int i = 0; i < count; i++)
            {
                Vector3 dir = Quaternion.Euler(0, 0, startAngle + (spreadAngle * i)) * centerDir;
                SpawnBullet(dir, bulletSpeed, pIndex); 
            }
        }
        else if (fireData.type == FireType.Spray) 
        {
            int count = 8;
            for (int i = 0; i < count; i++)
            {
                float randomAngle = Random.Range(-45f, 45f);
                Vector3 dir = Quaternion.Euler(0, 0, randomAngle) * centerDir;
                SpawnBullet(dir, bulletSpeed, pIndex); 
            }
        }
    }
    
    private void SpawnBullet(Vector3 dir, float speed, int prefabIndex)
    {
        if (BulletPooler.Instance == null) return;
        GameObject bullet = BulletPooler.Instance.GetBullet(prefabIndex, transform.position, Quaternion.identity);
        
        if (bullet != null)
        {
            Bullet b = bullet.GetComponent<Bullet>();
            b.poolIndex = prefabIndex;
            b.Setup(dir, speed);
        }
    }
}