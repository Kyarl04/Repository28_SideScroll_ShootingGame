using UnityEngine;
using System.Collections;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform[] firePoints = new Transform[4];
    public float bulletSpeed = 10f;
    public float fireInterval = 0.1f;

    [Header("Player Bullet Setup")]
    [Tooltip("BulletPooler에 등록된 플레이어 총알의 인덱스 번호 (예: 0)")]
    public int playerBulletPoolIndex = 0; // 인스펙터에서 플레이어 총알 인덱스를 적어주세요.

    [Header("Laser Settings")]
    public GameObject laserObject;
    public ParticleSystem magicCircle;
    public float laserDuration = 1.5f;
    public float laserCooldown = 3.0f;

    [Header("Bomb (Laser) UI")]
    public int maxLasers = 3;           // 최대 폭탄(레이저) 개수
    private int currentLasers;          // 현재 남은 개수
    public GameObject[] laserUIs;       // 인스펙터에서 UI 이미지들을 배열로 넣습니다.

    // 상태 변수
    private float nextFireTime = 0f;
    private float lastLaserTime = -999f;
    private bool isInvincible = false;
    private bool isLaserActive = false;

    // --- 외부(PlayerController)에서 호출하는 함수들 ---

    private void Start()
    {
        // 시작 시 레이저 개수 초기화 및 UI 갱신
        currentLasers = maxLasers;
        UpdateLaserUI();
    }

    public void TryFire()
    {
        if (isLaserActive) return;

        if (Time.time >= nextFireTime)
        {
            FireBullets();
            nextFireTime = Time.time + fireInterval;
        }
    }

    public void TryActivateLaser()
    {
        if (Time.time >= lastLaserTime + laserCooldown && !isLaserActive && currentLasers > 0)
        {
            currentLasers--;     // 개수 1 감소
            UpdateLaserUI();     // UI 즉시 갱신 (이미지 1개 꺼짐)
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


    // --- 내부 동작 로직 ---

    private void FireBullets()
    {
        SoundManager.Instance.PlayPlayerShoot();

        bool isFocused = Input.GetKey(KeyCode.LeftShift);
        
        for (int i = 0; i < firePoints.Length; i++)
        {
            // 1. 구식 Instantiate 대신 오브젝트 풀링(BulletPooler) 사용
            GameObject bullet = BulletPooler.Instance.GetBullet(playerBulletPoolIndex, firePoints[i].position, Quaternion.identity);
            
            if (bullet != null)
            {
                // 방향 계산 (기존 코드와 동일)
                float spread = 0.05f; 
                float angle = (i < 2) ? (spread * (2 - i)) : (-spread * (i - 1));
                Vector2 dir = isFocused ? Vector2.right : new Vector2(1f, angle).normalized;
                
                // 2. Rigidbody 조작 대신 Bullet의 Setup() 함수를 호출!
                Bullet b = bullet.GetComponent<Bullet>();
                if (b != null)
                {
                    b.poolIndex = playerBulletPoolIndex; // 풀 인덱스 기억하기
                    b.Setup(dir, bulletSpeed);           // 회전 및 속도 세팅 실행!
                }
            }
        }
    }
    private IEnumerator ShootLaser()
    {
        SoundManager.Instance.PlayPlayerLaserFire();
        
        isLaserActive = true;
        isInvincible = true;
        lastLaserTime = Time.time;

        if (laserObject != null) laserObject.SetActive(true);
        if (magicCircle != null) magicCircle.Play();

        yield return new WaitForSeconds(laserDuration);

        if (laserObject != null) laserObject.SetActive(false);
        if (magicCircle != null) magicCircle.Stop();

        isLaserActive = false;
        isInvincible = false;
    }
}