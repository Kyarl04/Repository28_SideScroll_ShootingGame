using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;       
using Danmaku.Data;      
using TMPro;
using UnityEngine.UI;

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
    public Animator anim; 

    [Header("UI Settings")]
    public TextMeshProUGUI spellNameText; 
    public Image hpBarFill; 
    public GameObject hpBarContainer;

    [Header("Phase Settings")]
    public List<BossPhase> phases;
    public Vector3 centerPosition = new Vector3(0, 3f, 0); 
    
    private int currentPhaseIndex = 0;
    public bool isTransitioning = false; 
    
    private float currentPhaseHP;
    private Transform player;

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

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

        currentPhaseHP = currentPhase.phaseHP;

        if (hpBarContainer != null) hpBarContainer.SetActive(true);
        if (hpBarFill != null) hpBarFill.fillAmount = 1f;

        if (spellNameText != null && currentPhase.pattern != null)
        {
            spellNameText.text = currentPhase.pattern.danmakuName;
            spellNameText.gameObject.SetActive(true);
        }

        if (anim != null) anim.SetTrigger("SpellCard");

        if (currentPhase.pattern != null && currentPhase.pattern.move != null)
        {
            StartCoroutine(BossMovementRoutine(currentPhase.pattern.move));
        }

        if (currentPhase.pattern != null)
        {
            yield return StartCoroutine(ExecuteDanmaku(currentPhase.pattern));
        }
    }

    public void BossTakeDamage(float damage)
    {
        if (isTransitioning) return;

        currentPhaseHP -= damage;
        
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = currentPhaseHP / phases[currentPhaseIndex].phaseHP;
        }

        if (currentPhaseHP <= 0)
        {
            currentPhaseHP = 0;
            OnPhaseDefeated();
        }
    }

    public void OnPhaseDefeated()
    {
        if (isTransitioning) return;
        
        isTransitioning = true; 
        
        StopAllCoroutines(); 
        transform.DOKill();  
        
        StartCoroutine(PhaseTransitionRoutine()); 
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        if (spellNameText != null) spellNameText.gameObject.SetActive(false);
        if (hpBarContainer != null) hpBarContainer.SetActive(false);

        if (SoundManager.Instance != null) SoundManager.Instance.PlayBossPhaseChange();
        if (phases[currentPhaseIndex].transitionEffect != null)
        {
            Instantiate(phases[currentPhaseIndex].transitionEffect, transform.position, Quaternion.identity);
        }

        if (anim != null) anim.SetTrigger("GuardBreak");

        MoveTo(centerPosition, 1.5f);

        yield return new WaitForSeconds(3.0f);

        currentPhaseIndex++;
        
        if (currentPhaseIndex < phases.Count)
        {
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
        else
        {
            if (spellNameText != null)
            {
                spellNameText.text = "GAME CLEAR!";
                spellNameText.gameObject.SetActive(true);
            }
            Debug.Log("보스 최종 클리어!");
            Destroy(gameObject);
        }
    }

    private IEnumerator BossMovementRoutine(DanmakuMove moveData)
    {
        yield return new WaitForSeconds(moveData.startDelay);

        while (!isTransitioning)
        {
            if (moveData.type == DanmakuMoveType.RandomMove)
            {
                float x = Random.Range(moveData.minX, moveData.maxX);
                float y = Random.Range(moveData.minY, moveData.maxY);
                Vector3 targetPos = new Vector3(x, y, 0) + moveData.startPosition; 

                MoveTo(targetPos, moveData.duration);

                yield return new WaitForSeconds(moveData.duration + moveData.interval);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void MoveTo(Vector3 targetPos, float duration)
    {
        if (transform.position == targetPos) return;

        int horizontal = 0;
        if (targetPos.x < transform.position.x) horizontal = -1;
        else if (targetPos.x > transform.position.x) horizontal = 1;

        if (anim != null) anim.SetInteger("Horizont", horizontal); 

        transform.DOMove(targetPos, duration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (anim != null) anim.SetInteger("Horizont", 0); 
        });
    }

    private IEnumerator ExecuteDanmaku(DanmakuData danmaku)
    {
        foreach (var barrage in danmaku.data)
        {
            StartCoroutine(FireBarrageRoutine(barrage));
        }
        yield return null;
    }

    // =========================================================
    // [핵심 변경] 회전 및 패턴 루프 로직이 완벽하게 구현된 부분입니다!
    // =========================================================
    private IEnumerator FireBarrageRoutine(BarrageData barrage)
    {
        yield return new WaitForSeconds(barrage.startDelay);
        int counter = 0;

        while (true)
        {
            float deltaStartAngle = 0f;

            // 1. Barrage Offset 계산 (와이퍼처럼 왔다갔다 하거나 서서히 각도가 틀어지는 효과)
            if (barrage.fireOffset != null && barrage.fireOffset.type != OffsetType.None && barrage.fireOffset.cycle > 0)
            {
                int cycleIndex = counter % barrage.fireOffset.cycle;
                if (barrage.fireOffset.reciprocate)
                {
                    if ((counter / barrage.fireOffset.cycle) % 2 == 1)
                    {
                        cycleIndex = barrage.fireOffset.cycle - cycleIndex;
                    }
                }

                float delta = barrage.fireOffset.range / barrage.fireOffset.cycle;
                cycleIndex -= barrage.fireOffset.startCycleIndex;

                if (barrage.fireOffset.type == OffsetType.Linear)
                {
                    deltaStartAngle = cycleIndex * delta;
                }
            }

            // 단일 발사가 아니라 Group(연사) 처리를 위해 코루틴으로 호출합니다.
            StartCoroutine(FireCoroutine(barrage.fireData, barrage.shotData, deltaStartAngle));
            
            yield return new WaitForSeconds(barrage.interval);
            counter++;
        }
    }

    private IEnumerator FireCoroutine(FireData fireData, ShotData shotData, float deltaStartAngle)
    {
        float currentGroupAngle = 0f;
        int loopNum = (fireData.group != null && fireData.group.num > 0) ? fireData.group.num : 1;

        // 2. Group 설정에 따른 반복 발사 및 각도 변화 (회전하는 탄막의 핵심)
        for (int g = 0; g < loopNum; g++)
        {
            Vector3 centerDir = fireData.startDir;
            if (fireData.directionType == DirectionType.Aimed && player != null)
            {
                centerDir = (player.position - transform.position).normalized;
            }

            // 시작 각도 + 와이퍼 오프셋 + 그룹 연사 회전각을 모두 합산
            float finalAngle = fireData.startAngle + deltaStartAngle + currentGroupAngle;
            Vector3 finalDir = Quaternion.Euler(0, 0, finalAngle) * centerDir;
            
            float bulletSpeed = shotData.speed.value;
            int prefabIndex = shotData.prefabIndex;

            if (fireData.type == FireType.Round) 
            {
                int count = fireData.count > 1 ? fireData.count : 16; 
                float angleStep = 360f / count;
                for (int i = 0; i < count; i++)
                {
                    Vector3 dir = Quaternion.Euler(0, 0, angleStep * i) * finalDir;
                    SpawnBullet(dir, bulletSpeed, prefabIndex); 
                }
            }
            else if (fireData.type == FireType.Sector) 
            {
                int count = fireData.count;
                float spreadAngle = fireData.sector != null ? fireData.sector.deltaAngle : 15f; 
                float startAngle = -((count - 1) * spreadAngle) / 2f;
                for (int i = 0; i < count; i++)
                {
                    Vector3 dir = Quaternion.Euler(0, 0, startAngle + (spreadAngle * i)) * finalDir;
                    SpawnBullet(dir, bulletSpeed, prefabIndex); 
                }
            }
            else if (fireData.type == FireType.Spray) 
            {
                int count = fireData.count;
                for (int i = 0; i < count; i++)
                {
                    float randomAngle = Random.Range(-45f, 45f);
                    Vector3 dir = Quaternion.Euler(0, 0, randomAngle) * finalDir;
                    SpawnBullet(dir, bulletSpeed, prefabIndex); 
                }
            }

            // 다음번 쏠 때 각도를 변경시킴
            if (fireData.group != null)
            {
                currentGroupAngle += fireData.group.deltaAngle;
            }

            // 간격만큼 대기
            if (fireData.group != null && fireData.group.interval > 0)
            {
                yield return new WaitForSeconds(fireData.group.interval);
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