using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;       
using Danmaku.Data;      
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 각 페이즈별 정보(탄막 데이터, 체력, 배경 및 이펙트 등)를 담는 데이터 클래스
/// </summary>
[System.Serializable]
public class BossPhase
{
    public DanmakuData pattern; // 탄막 발사 패턴 데이터
    public float phaseHP = 1000f;
    
    [Tooltip("페이즈가 박살 날 때(죽을 때) 터지는 이펙트")]
    public GameObject transitionEffect;

    [Tooltip("다음 페이즈 체력이 차오를 때 보스 몸에서 나오는 아우라 이펙트")]
    public GameObject auraEffect; 

    public Sprite[] newBackground; // 페이즈 변경 시 교체될 패럴랙스 배경 이미지들

    [Header("Transition Settings")]
    public Material customDissolveMaterial; // 배경 전환 셰이더 머티리얼
}

/// <summary>
/// 보스의 상태(페이즈 전환, 체력), 이동, 탄막 패턴 발동을 종합적으로 제어하는 메인 스크립트.
/// </summary>
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

    [Header("Background Transition")]
    public Image bgPanelImage; 
    public AutoParallaxSTG parallaxManager;
    public string bgDissolveProperty = "_D_Intensity"; 
    public float bgTransitionDuration = 1.5f;

    private Material bgMaterial; 
    private GameObject activeAuraInstance;
    
    private int currentPhaseIndex = 0;
    public bool isTransitioning = false; // 패턴 일시 정지 및 무적 판정을 위한 플래그
    
    private float currentPhaseHP;
    private Transform player;

    [Header("Enemy Spawner")]
    public ObstacleSpawner obstacleSpawner;
    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        // 플레이어 오브젝트를 찾아 방향 기반 탄막 발사(Aimed)에 사용하기 위해 캐싱
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        // 배경 전환용 UI 머티리얼 초기화
        if (bgPanelImage != null)
        {
            bgPanelImage.color = new Color(1, 1, 1, 1);
            if (bgPanelImage.material != null && bgPanelImage.material.HasProperty(bgDissolveProperty))
            {
                bgPanelImage.material = new Material(bgPanelImage.material);
                bgPanelImage.material.SetFloat(bgDissolveProperty, 1f);
            }
        }

        if (anim != null) anim.SetTrigger("Intro");

        // 첫 번째 페이즈 시작
        if (phases.Count > 0)
        {
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
    }

    // =========================================================
    // [보스 페이즈(Phase) 관리 로직]
    // =========================================================

    /// <summary>
    /// 지정된 페이즈를 시작하기 위한 연출(아우라, 체력 회복)과 패턴 초기화를 담당하는 코루틴
    /// </summary>
    private IEnumerator StartPhaseRoutine(int index)
    {
        isTransitioning = true; // 무적 상태 돌입 및 기존 패턴 멈춤
        BossPhase currentPhase = phases[index];

        currentPhaseHP = currentPhase.phaseHP;

        // UI 설정 (스펠카드 이름 출력 및 체력바 리셋)
        if (hpBarContainer != null) hpBarContainer.SetActive(true);
        if (hpBarFill != null) hpBarFill.fillAmount = 0f; 

        if (spellNameText != null && currentPhase.pattern != null)
        {
            spellNameText.text = currentPhase.pattern.danmakuName;
            spellNameText.gameObject.SetActive(true);
        }

        if (index == 0) yield return new WaitForSeconds(1.5f); // 최초 등장 대기

        // 페이즈 기 모으기 연출 (아우라 생성)
        if (currentPhase.auraEffect != null)
        {
            activeAuraInstance = Instantiate(currentPhase.auraEffect, transform.position, Quaternion.identity, transform);
        }

        // Lerp를 이용한 시각적 체력바 차오름 연출 (1초)
        if (hpBarFill != null)
        {
            float fillDuration = 1.0f; 
            float timer = 0f;
            
            while (timer < fillDuration)
            {
                timer += Time.deltaTime;
                hpBarFill.fillAmount = Mathf.Lerp(0f, 1f, timer / fillDuration);
                yield return null; 
            }
            hpBarFill.fillAmount = 1f; 
        }

        if (anim != null) anim.SetTrigger("SpellCard");

        isTransitioning = false; // 무적 해제 및 패턴 시작

        if (obstacleSpawner != null)
        {
            if (index == 2 || index == 3) // 원하는 페이즈 번호를 적어주세요. (0부터 시작하므로 1은 두 번째 페이즈)
                obstacleSpawner.StartSpawning();
            else
                obstacleSpawner.StopSpawning(); // 다른 페이즈면 끕니다.
        }
        
        // 이동 패턴 및 탄막 패턴 실행
        if (currentPhase.pattern != null && currentPhase.pattern.move != null)
        {
            StartCoroutine(BossMovementRoutine(currentPhase.pattern.move));
        }

        if (currentPhase.pattern != null)
        {
            yield return StartCoroutine(ExecuteDanmaku(currentPhase.pattern));
        }
    }

    /// <summary>
    /// 외부(Enemy.cs나 무기 스크립트)에서 보스에게 데미지를 입힐 때 호출
    /// </summary>
    public void BossTakeDamage(float damage)
    {
        if (isTransitioning) return; // 페이즈 전환 중에는 데미지 무시

        currentPhaseHP -= damage;
        
        // 체력바 갱신
        if (hpBarFill != null)
        {
            hpBarFill.fillAmount = currentPhaseHP / phases[currentPhaseIndex].phaseHP;
        }

        // 현재 페이즈 체력이 모두 소진되었을 때
        if (currentPhaseHP <= 0)
        {
            currentPhaseHP = 0;
            OnPhaseDefeated();
        }
    }

    /// <summary>
    /// 페이즈 클리어 시 호출. 탄막 소거 및 연출 코루틴을 시작합니다.
    /// </summary>
    public void OnPhaseDefeated()
    {
        if (isTransitioning) return;
        
        isTransitioning = true; 
        
        // 실행 중인 모든 공격 및 이동 코루틴, 트윈 강제 종료
        StopAllCoroutines(); 
        transform.DOKill();  

        if (obstacleSpawner != null) obstacleSpawner.StopSpawning();
        
        ClearAllBullets(); 
        
        StartCoroutine(PhaseTransitionRoutine());
    }

    /// <summary>
    /// 페이즈 파괴 후 다음 페이즈로 넘어가거나 게임 클리어로 이어지는 연출 코루틴
    /// 배경 전환 셰이더 조작이 포함되어 있습니다.
    /// </summary>
    private IEnumerator PhaseTransitionRoutine()
    {
        if (spellNameText != null) spellNameText.gameObject.SetActive(false);
        if (hpBarContainer != null) hpBarContainer.SetActive(false);

        // 아우라 이펙트를 부드럽게 종료
        if (activeAuraInstance != null)
        {
            ParticleSystem ps = activeAuraInstance.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop(); 
            Destroy(activeAuraInstance, 2.0f);
            activeAuraInstance = null; 
        }
        
        if (SoundManager.Instance != null) SoundManager.Instance.PlayBossPhaseChange();
        
        // 파괴 이펙트 연출
        if (phases[currentPhaseIndex].transitionEffect != null)
        {
            Instantiate(phases[currentPhaseIndex].transitionEffect, transform.position, Quaternion.identity);
        }

        if (anim != null) anim.SetTrigger("GuardBreak");

        // 보스를 화면 중앙으로 강제 이동
        MoveTo(centerPosition, 1.5f);

        int nextPhaseIndex = currentPhaseIndex + 1;

        // 다음 페이즈가 남아있다면 배경 전환 실행
        if (nextPhaseIndex < phases.Count)
        {
            BossPhase nextPhase = phases[nextPhaseIndex];
            Sprite[] nextBgs = nextPhase.newBackground;
            Material transitionMat = nextPhase.customDissolveMaterial; 

            if (bgPanelImage != null)
            {
                if (transitionMat != null)
                {
                    Material tempMat = new Material(transitionMat);
                    bgPanelImage.material = tempMat;

                    // 1. 화면 까매짐 연출 (페이드 아웃)
                    yield return tempMat.DOFloat(0f, bgDissolveProperty, bgTransitionDuration).WaitForCompletion();

                    // 2. 패럴랙스 배경 데이터 교체
                    if (nextBgs != null && nextBgs.Length > 0 && parallaxManager != null) 
                    {
                        parallaxManager.ChangeBackgroundSprites(nextBgs);
                    }

                    // 3. 화면 밝아짐 연출 (페이드 인)
                    yield return tempMat.DOFloat(1f, bgDissolveProperty, bgTransitionDuration).WaitForCompletion();
                }
                else
                {
                    if (nextBgs != null && nextBgs.Length > 0 && parallaxManager != null) parallaxManager.ChangeBackgroundSprites(nextBgs);
                    yield return new WaitForSeconds(1.5f);
                }
            }

            currentPhaseIndex++;
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
        else // 모든 페이즈를 클리어했을 경우
        {
            yield return new WaitForSeconds(3.0f);

            if (spellNameText != null)
            {
                spellNameText.text = "GAME CLEAR!";
                spellNameText.gameObject.SetActive(true);
            }
            
            if (GameManager.Instance != null) GameManager.Instance.ShowGameClear();
            Destroy(gameObject); // 보스 최종 처치
        }
    }

    // =========================================================
    // [유틸리티 및 이동/공격 로직]
    // =========================================================

    private IEnumerator BossMovementRoutine(DanmakuMove moveData)
    {
        yield return new WaitForSeconds(moveData.startDelay);

        while (!isTransitioning)
        {
            if (moveData.type == DanmakuMoveType.RandomMove)
            {
                // 지정된 범위 내에서 랜덤 위치를 계산하여 이동
                float x = Random.Range(moveData.minX, moveData.maxX);
                float y = Random.Range(moveData.minY, moveData.maxY);
                Vector3 targetPos = new Vector3(x, y, 0) + moveData.startPosition; 

                MoveTo(targetPos, moveData.duration);
                yield return new WaitForSeconds(moveData.duration + moveData.interval);
            }
            else yield return null;
        }
    }

    /// <summary>
    /// 화면에 남아있는 보스의 모든 총알을 풀링 매니저로 강제 반환합니다 (페이즈 종료 시 사용)
    /// </summary>
    private void ClearAllBullets()
    {
        Bullet[] activeBullets = FindObjectsOfType<Bullet>();
        foreach (Bullet b in activeBullets)
        {
            if (b.gameObject.activeSelf && b.gameObject.CompareTag("EnemyBullet"))
            {
                if (BulletPooler.Instance != null) BulletPooler.Instance.ReturnBullet(b.gameObject, b.poolIndex);
                else b.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// DOTween을 활용한 부드러운 위치 이동 및 애니메이션 동기화 함수
    /// </summary>
    private void MoveTo(Vector3 targetPos, float duration)
    {
        if (transform.position == targetPos) return;

        // 이동 방향에 맞춰 애니메이터 변수(Horizont) 세팅
        int horizontal = targetPos.x < transform.position.x ? -1 : (targetPos.x > transform.position.x ? 1 : 0);
        if (anim != null) anim.SetInteger("Horizont", horizontal); 

        // Ease.InOutSine을 사용하여 시작과 끝을 부드럽게 감가속
        transform.DOMove(targetPos, duration).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            if (anim != null) anim.SetInteger("Horizont", 0); 
        });
    }

    // =========================================================
    // [탄막(Danmaku) 생성 코어 로직] 
    // =========================================================
    
    private IEnumerator ExecuteDanmaku(DanmakuData danmaku)
    {
        // 다중 패턴(Barrage)을 동시에 병렬로 실행
        foreach (var barrage in danmaku.data) StartCoroutine(FireBarrageRoutine(barrage));
        yield return null;
    }

    private IEnumerator FireBarrageRoutine(BarrageData barrage)
    {
        yield return new WaitForSeconds(barrage.startDelay);
        int counter = 0;

        while (true)
        {
            float deltaStartAngle = 0f;

            // 탄막의 회전 오프셋(발사할 때마다 각도가 꺾이는 기믹) 계산
            if (barrage.fireOffset != null && barrage.fireOffset.type != OffsetType.None && barrage.fireOffset.cycle > 0)
            {
                int cycleIndex = counter % barrage.fireOffset.cycle;
                if (barrage.fireOffset.reciprocate && (counter / barrage.fireOffset.cycle) % 2 == 1)
                {
                    cycleIndex = barrage.fireOffset.cycle - cycleIndex;
                }

                float delta = barrage.fireOffset.range / barrage.fireOffset.cycle;
                cycleIndex -= barrage.fireOffset.startCycleIndex;
                if (barrage.fireOffset.type == OffsetType.Linear) deltaStartAngle = cycleIndex * delta;
            }

            StartCoroutine(FireCoroutine(barrage.fireData, barrage.shotData, deltaStartAngle));
            yield return new WaitForSeconds(barrage.interval);
            counter++;
        }
    }

    /// <summary>
    /// 실제 총알을 수학적 계산(원형, 부채꼴, 무작위)에 맞춰 발사하는 함수
    /// </summary>
    private IEnumerator FireCoroutine(FireData fireData, ShotData shotData, float deltaStartAngle)
    {
        float currentGroupAngle = 0f;
        int loopNum = (fireData.group != null && fireData.group.num > 0) ? fireData.group.num : 1;

        for (int g = 0; g < loopNum; g++)
        {
            // 플레이어를 향해 조준하는(Aimed) 기믹 처리
            Vector3 centerDir = fireData.startDir;
            if (fireData.directionType == DirectionType.Aimed && player != null)
            {
                centerDir = (player.position - transform.position).normalized;
            }

            float finalAngle = fireData.startAngle + deltaStartAngle + currentGroupAngle;
            Vector3 finalDir = Quaternion.Euler(0, 0, finalAngle) * centerDir;
            
            float bulletSpeed = shotData.speed.value;
            int prefabIndex = shotData.prefabIndex;

            // 모양에 따른 총알 각도 계산 및 생성
            if (fireData.type == FireType.Round) // 원형(360도) 전방위 발사
            {
                int count = fireData.count > 1 ? fireData.count : 16; 
                float angleStep = 360f / count;
                for (int i = 0; i < count; i++)
                {
                    Vector3 dir = Quaternion.Euler(0, 0, angleStep * i) * finalDir;
                    SpawnBullet(dir, bulletSpeed, prefabIndex); 
                }
            }
            else if (fireData.type == FireType.Sector) // 부채꼴(샷건 형태) 발사
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
            else if (fireData.type == FireType.Spray) // 흩뿌리기(무작위 각도) 발사
            {
                int count = fireData.count;
                for (int i = 0; i < count; i++)
                {
                    float randomAngle = Random.Range(-45f, 45f);
                    Vector3 dir = Quaternion.Euler(0, 0, randomAngle) * finalDir;
                    SpawnBullet(dir, bulletSpeed, prefabIndex); 
                }
            }

            // 다중 그룹 발사일 경우 딜레이 적용
            if (fireData.group != null) currentGroupAngle += fireData.group.deltaAngle;
            if (fireData.group != null && fireData.group.interval > 0) yield return new WaitForSeconds(fireData.group.interval);
        }
    }
    
    private void SpawnBullet(Vector3 dir, float speed, int prefabIndex)
    {
        if (BulletPooler.Instance == null) return;
        // 풀링 시스템을 경유하여 메모리 낭비 없이 총알 재사용
        GameObject bullet = BulletPooler.Instance.GetBullet(prefabIndex, transform.position, Quaternion.identity);
        if (bullet != null)
        {
            Bullet b = bullet.GetComponent<Bullet>();
            b.poolIndex = prefabIndex;
            b.Setup(dir, speed); // 방향과 속도 할당
        }
    }
}