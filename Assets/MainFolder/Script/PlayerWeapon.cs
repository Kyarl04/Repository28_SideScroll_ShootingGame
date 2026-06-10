using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어의 기본 공격(탄막 발사)과 특수기(레이저 폭탄)를 담당하는 클래스.
/// 입력에 따른 발사 쿨타임 제어와 스킬 애니메이션 동기화가 구현되어 있습니다.
/// </summary>
public class PlayerWeapon : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform[] firePoints = new Transform[4];
    public float bulletSpeed = 10f;
    public float fireInterval = 0.1f; // 기본 공격 쿨타임

    [Header("Player Bullet Setup")]
    [Tooltip("BulletPooler에 등록된 플레이어 총알의 인덱스 번호 (예: 0)")]
    public int playerBulletPoolIndex = 0; 

    [Header("Laser Settings")]
    public GameObject laserObject;
    public ParticleSystem magicCircle;
    public float laserDuration = 1.5f;
    public float laserCooldown = 3.0f;

    [Header("Bomb (Laser) UI")]
    public int maxLasers = 3;           
    private int currentLasers;          
    public GameObject[] laserUIs;       

    // 상태 제어 변수들
    public PlayerController playerController;
    private float nextFireTime = 0f;
    private float lastLaserTime = -999f;
    private bool isLaserActive = false;
   
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>(); 
        
        currentLasers = maxLasers;
        UpdateLaserUI();
    }

    // 외부(PlayerController)에서 Z키를 누를 때 호출
    public void TryFire()
    {
        if (isLaserActive) return; // 스킬 사용 중에는 기본 공격 제한

        if (Time.time >= nextFireTime)
        {
            FireBullets();
            nextFireTime = Time.time + fireInterval;
        }
    }

    // 외부(PlayerController)에서 X키를 누를 때 호출
    public void TryActivateLaser()
    {
        if (Time.time >= lastLaserTime + laserCooldown && !isLaserActive && currentLasers > 0)
        {
            currentLasers--;     
            UpdateLaserUI();     
            StartCoroutine(ShootLaser());
        }
    }

    private void UpdateLaserUI()
    {
        for (int i = 0; i < laserUIs.Length; i++)
        {
            laserUIs[i].SetActive(i < currentLasers);
        }
    }

    private void FireBullets()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPlayerShoot();

        bool isFocused = Input.GetKey(KeyCode.LeftShift); // 집중 모드(저속) 확인
        
        for (int i = 0; i < firePoints.Length; i++)
        {
            // Instantiate 연산 부하를 줄이기 위해 풀에서 총알을 꺼내옵니다.
            GameObject bullet = BulletPooler.Instance.GetBullet(playerBulletPoolIndex, firePoints[i].position, Quaternion.identity);
            
            if (bullet != null)
            {
                // 집중 모드일 때는 정면으로 쏘고, 아닐 때는 약간 퍼지는(Spread) 각도로 계산
                float spread = 0.05f; 
                float angle = (i < 2) ? (spread * (2 - i)) : (-spread * (i - 1));
                Vector2 dir = isFocused ? Vector2.right : new Vector2(1f, angle).normalized;
                
                Bullet b = bullet.GetComponent<Bullet>();
                if (b != null)
                {
                    b.poolIndex = playerBulletPoolIndex; 
                    b.Setup(dir, bulletSpeed); // 물리 컴포넌트 없이 직접 세팅           
                }
            }
        }
    }

    /// <summary>
    /// 레이저(폭탄) 스킬 코루틴.
    /// 스킬 시전 시간 동안 플레이어에게 무적(Invincible) 판정을 부여하고 애니메이션을 제어합니다.
    /// </summary>
    private IEnumerator ShootLaser()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPlayerLaserFire();
        
        isLaserActive = true;
        if (playerController != null) playerController.isInvincible = true; // 무적 켜기
        lastLaserTime = Time.time;

        if (laserObject != null) laserObject.SetActive(true);
        if (magicCircle != null) magicCircle.Play();

        if (anim != null) anim.SetTrigger("SpellStart");

        yield return new WaitForSeconds(laserDuration); // 레이저 지속 시간 대기

        if (laserObject != null) laserObject.SetActive(false);
        if (magicCircle != null) magicCircle.Stop();

        if (anim != null) anim.SetTrigger("SpellEnd");

        isLaserActive = false;
        if (playerController != null) playerController.isInvincible = false; // 무적 끄기
    }
}