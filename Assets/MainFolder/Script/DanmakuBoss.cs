using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;       
using Danmaku.Data;      

[System.Serializable]
public class BossPhase
{
    public DanmakuData pattern;
    public float phaseHP = 1000f;
    public GameObject transitionEffect;
    public Sprite newBackground;
}

public class DanmakuBoss : MonoBehaviour
{
    [Header("Animation")]
    public Animator anim; // 코이시 애니메이터 연결

    [Header("Phase Settings")]
    public List<BossPhase> phases;
    public Vector3 centerPosition = new Vector3(0, 3f, 0); 
    
    private int currentPhaseIndex = 0;
    public bool isTransitioning = false; 
    
    private Transform player;
    private Enemy enemyScript;

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        enemyScript = GetComponent<Enemy>();

        // [애니메이션] 1. 보스 등장 (Intro)
        if (anim != null) anim.SetTrigger("Intro");

        if (phases.Count > 0)
        {
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
    }

    private IEnumerator StartPhaseRoutine(int index)
    {
        isTransitioning = false;
        BossPhase currentPhase = phases[index];

        if (enemyScript != null) enemyScript.SetupHP(currentPhase.phaseHP);

        // [애니메이션] 2. 스펠카드/페이즈 전개 (SpellCard)
        if (anim != null) anim.SetTrigger("SpellCard");

        if (currentPhase.pattern != null)
        {
            yield return StartCoroutine(ExecuteDanmaku(currentPhase.pattern));
        }
    }

    public void OnPhaseDefeated()
    {
        if (isTransitioning) return;
        StartCoroutine(PhaseTransitionRoutine());
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        SoundManager.Instance.PlayBossPhaseChange(); // 페이즈 전환음 추가
        isTransitioning = true;
        
        StopAllCoroutines(); 
        transform.DOKill(); 

        if (enemyScript != null) enemyScript.HideUI();
        if (phases[currentPhaseIndex].transitionEffect != null)
        {
            Instantiate(phases[currentPhaseIndex].transitionEffect, transform.position, Quaternion.identity);
        }

        // [애니메이션] 3. 페이즈 격파 시 충격 (GuardBreak)
        if (anim != null) anim.SetTrigger("GuardBreak");

        // 중앙으로 복귀 (이동 애니메이션 적용)
        MoveTo(centerPosition, 1.5f);

        yield return new WaitForSeconds(3.0f);

        currentPhaseIndex++;
        
        if (currentPhaseIndex < phases.Count)
        {
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
        else
        {
            Instantiate(phases[currentPhaseIndex - 1].transitionEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
            Debug.Log("보스 클리어!");
        }
    }

    // ==========================================
    // [애니메이션] 4. 깃허브 방식 이동 애니메이션 처리
    // ==========================================
    private void MoveTo(Vector3 targetPos, float duration)
    {
        if (transform.position == targetPos) return;

        // 이동할 방향 계산 (왼쪽이면 -1, 오른쪽이면 1)
        int horizontal = 0;
        if (targetPos.x < transform.position.x) horizontal = -1;
        else if (targetPos.x > transform.position.x) horizontal = 1;

        // 몸을 기울이는 애니메이션 시작
        if (anim != null) anim.SetInteger("Horizontal", horizontal);

        // DOTween으로 이동 후, 도착하면 몸을 다시 정면(0)으로 되돌림
        transform.DOMove(targetPos, duration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (anim != null) anim.SetInteger("Horizontal", 0);
        });
    }

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
            
            // 기존 DOMove 대신, 애니메이션이 연동된 MoveTo 함수를 사용!
            MoveTo(targetPos, 1.5f);
            
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