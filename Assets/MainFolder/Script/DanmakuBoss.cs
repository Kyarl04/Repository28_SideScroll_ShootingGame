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
    
    [Tooltip("페이즈가 박살 날 때(죽을 때) 터지는 이펙트")]
    public GameObject transitionEffect;

    // ==========================================
    // [추가된 변수] 기를 모으는 아우라 이펙트
    // ==========================================
    [Tooltip("다음 페이즈 체력이 차오를 때 보스 몸에서 나오는 아우라 이펙트")]
    public GameObject auraEffect; 

    public Sprite[] newBackground;

    [Header("Transition Settings")]
    public Material customDissolveMaterial; 
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

    [Header("Background Transition")]
    public Image bgPanelImage; // SpriteRenderer 대신 UI Image(Panel)를 사용합니다.
    [Tooltip("배경 스크롤을 담당하는 AutoParallaxSTG 오브젝트를 연결할 것")]
    public AutoParallaxSTG parallaxManager;
    public string bgDissolveProperty = "_D_Intensity"; 
    public float bgTransitionDuration = 1.5f;

    private Material bgMaterial; // 배경 머티리얼 인스턴스
    private GameObject activeAuraInstance;
    
    private int currentPhaseIndex = 0;
    public bool isTransitioning = false; 
    
    private float currentPhaseHP;
    private Transform player;

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) player = pObj.transform;

        if (bgPanelImage != null)
        {
            bgPanelImage.color = new Color(1, 1, 1, 1);
            
            // 시작 머티리얼이 있다면 디졸브 수치를 1(온전한 상태)로 보장
            if (bgPanelImage.material != null && bgPanelImage.material.HasProperty(bgDissolveProperty))
            {
                bgPanelImage.material = new Material(bgPanelImage.material);
                bgPanelImage.material.SetFloat(bgDissolveProperty, 1f);
            }
        }

        if (anim != null) anim.SetTrigger("Intro");

        if (phases.Count > 0)
        {
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
    }

    private IEnumerator StartPhaseRoutine(int index)
    {
        isTransitioning = true; 
        BossPhase currentPhase = phases[index];

        currentPhaseHP = currentPhase.phaseHP;

        if (hpBarContainer != null) hpBarContainer.SetActive(true);
        if (hpBarFill != null) hpBarFill.fillAmount = 0f; 

        if (spellNameText != null && currentPhase.pattern != null)
        {
            spellNameText.text = currentPhase.pattern.danmakuName;
            spellNameText.gameObject.SetActive(true);
        }

        if (index == 0)
        {
            yield return new WaitForSeconds(1.5f);
        }

        // =========================================================
        // [수정됨] 새로운 페이즈의 아우라 켜기 (파괴하지 않고 계속 유지!)
        // =========================================================
        if (currentPhase.auraEffect != null)
        {
            activeAuraInstance = Instantiate(currentPhase.auraEffect, transform.position, Quaternion.identity, transform);
        }

        // 체력바 차오르는 연출 (1초 소요)
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

        // ❌ (기존에 있던 2초 뒤 파괴하는 코드는 완전히 삭제했습니다!) ❌

        if (anim != null) anim.SetTrigger("SpellCard");

        isTransitioning = false; 
        
        // ... (아래는 기존 이동 및 탄막 코드 동일) ...
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

        // [추가된 탐정 코드] 보스가 맞을 때마다 콘솔 창에 데미지와 남은 체력을 보고합니다!
        Debug.Log($"보스 피격! 들어온 데미지: {damage} / 현재 남은 체력: {currentPhaseHP}"); 

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
        
        ClearAllBullets(); 
        
        StartCoroutine(PhaseTransitionRoutine());
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        if (spellNameText != null) spellNameText.gameObject.SetActive(false);
        if (hpBarContainer != null) hpBarContainer.SetActive(false);

        if (activeAuraInstance != null)
        {
            // 이펙트가 뚝 끊기지 않게 파티클 생성을 멈춤
            ParticleSystem[] allPs = activeAuraInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in allPs)
            {
                ps.Stop();
            }
            
            // 잔상이 사라질 여유 시간을 주고 완전 파괴
            Destroy(activeAuraInstance, 2.0f);
            
            // 다음 페이즈를 위해 변수 비우기
            activeAuraInstance = null; 
        }

        if (SoundManager.Instance != null) SoundManager.Instance.PlayBossPhaseChange();
        if (phases[currentPhaseIndex].transitionEffect != null)
        {
            Instantiate(phases[currentPhaseIndex].transitionEffect, transform.position, Quaternion.identity);
        }

        if (anim != null) anim.SetTrigger("GuardBreak");

        MoveTo(centerPosition, 1.5f);

        // =========================================================
        // [수정됨] 머티리얼 선택 및 배경 교체 로직
        // =========================================================
        int nextPhaseIndex = currentPhaseIndex + 1;

        if (nextPhaseIndex < phases.Count)
        {
            BossPhase nextPhase = phases[nextPhaseIndex];
            Sprite[] nextBgs = nextPhase.newBackground;
            Material transitionMat = nextPhase.customDissolveMaterial; // 인스펙터에서 넣은 머티리얼

            if (bgPanelImage != null)
            {
                if (transitionMat != null)
                {
                    Material tempMat = new Material(transitionMat);
                    bgPanelImage.material = tempMat;

                    // 1. 화면 까매짐
                    yield return tempMat.DOFloat(0f, bgDissolveProperty, bgTransitionDuration).WaitForCompletion();

                    // 2. 까매졌을 때 패럴랙스 매니저에게 여러 장의 이미지를 통째로 넘겨 교체 명령!
                    if (nextBgs != null && nextBgs.Length > 0 && parallaxManager != null) 
                    {
                        parallaxManager.ChangeBackgroundSprites(nextBgs);
                    }

                    // 3. 화면 밝아짐
                    yield return tempMat.DOFloat(1f, bgDissolveProperty, bgTransitionDuration).WaitForCompletion();
                }
                else
                {
                    // 연출 없이 즉시 교체
                    if (nextBgs != null && nextBgs.Length > 0 && parallaxManager != null) 
                    {
                        parallaxManager.ChangeBackgroundSprites(nextBgs);
                    }

                    if (bgPanelImage.material != null && bgPanelImage.material.HasProperty(bgDissolveProperty))
                    {
                        bgPanelImage.material.SetFloat(bgDissolveProperty, 1f);
                    }
                    yield return new WaitForSeconds(1.5f);
                }
            }

            currentPhaseIndex++;
            StartCoroutine(StartPhaseRoutine(currentPhaseIndex));
        }
        else
        {
            // 최종 클리어 시 대기
            yield return new WaitForSeconds(3.0f);

            if (spellNameText != null)
            {
                spellNameText.text = "GAME CLEAR!";
                spellNameText.gameObject.SetActive(true);
            }
            
            if (GameManager.Instance != null) GameManager.Instance.ShowGameClear();

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

    private void ClearAllBullets()
    {
        // 태그 대신 스크립트를 직접 찾아 '진짜 총알(최상위 오브젝트)'만 확실하게 골라냅니다.
        Bullet[] activeBullets = FindObjectsOfType<Bullet>();
        
        foreach (Bullet b in activeBullets)
        {
            // 현재 화면에 켜져있고, 보스가 쏜 총알(EnemyBullet)인 것만 회수합니다.
            if (b.gameObject.activeSelf && b.gameObject.CompareTag("EnemyBullet"))
            {
                if (BulletPooler.Instance != null)
                {
                    BulletPooler.Instance.ReturnBullet(b.gameObject, b.poolIndex);
                }
                else
                {
                    // 풀러가 없을 경우에만 안전하게 끕니다.
                    b.gameObject.SetActive(false);
                }
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

    private IEnumerator FireBarrageRoutine(BarrageData barrage)
    {
        yield return new WaitForSeconds(barrage.startDelay);
        int counter = 0;

        while (true)
        {
            float deltaStartAngle = 0f;

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

            StartCoroutine(FireCoroutine(barrage.fireData, barrage.shotData, deltaStartAngle));
            
            yield return new WaitForSeconds(barrage.interval);
            counter++;
        }
    }

    private IEnumerator FireCoroutine(FireData fireData, ShotData shotData, float deltaStartAngle)
    {
        float currentGroupAngle = 0f;
        int loopNum = (fireData.group != null && fireData.group.num > 0) ? fireData.group.num : 1;

        for (int g = 0; g < loopNum; g++)
        {
            Vector3 centerDir = fireData.startDir;
            if (fireData.directionType == DirectionType.Aimed && player != null)
            {
                centerDir = (player.position - transform.position).normalized;
            }

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

            if (fireData.group != null)
            {
                currentGroupAngle += fireData.group.deltaAngle;
            }

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