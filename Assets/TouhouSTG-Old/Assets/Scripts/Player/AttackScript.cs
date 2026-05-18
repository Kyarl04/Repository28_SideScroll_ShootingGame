using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캐릭터의 공격 및 탄막 생성을 제어하는 스크립트입니다.
/// </summary>
public class AttackScript : MonoBehaviour 
{
    // ==========================================
    // [설정 변수]
    // ==========================================
    
    [Header("발사체 설정")]
    [Tooltip("발사될 자탄 프리팹")]
    public Transform shotPrefab;

    [Tooltip("연사 속도 (발사 간격 초 단위)")]
    public float shootingRate = 0.25f;

    // ==========================================
    // [내부 변수]
    // ==========================================
    
    private float shootCooldown; // 현재 남은 쿨타임

    /// <summary>
    /// 무기가 다시 발사 가능한 상태인지 확인합니다.
    /// </summary>
    public bool CanAttack
    {
        get { return shootCooldown <= 0f; }
    }

    // ==========================================
    // [유니티 생명주기]
    // ==========================================

    void Start()
    {
        // 시작 시 즉시 발사 가능하도록 쿨타임 초기화
        shootCooldown = 0f;
    }

    void Update()
    {
        // 매 프레임 쿨타임 감소
        if (shootCooldown > 0)
        {
            shootCooldown -= Time.deltaTime;
        }
    }

    // ==========================================
    // [발사 로직]
    // ==========================================

    /// <summary>
    /// 외부(주로 PlayerScript나 EnemyScript)에서 호출하여 공격을 실행합니다.
    /// </summary>
    /// <param name="isEnemy">적의 공격인지 여부</param>
    public void Attack(bool isEnemy)
    {
        if (CanAttack)
        {
            // 쿨타임 재설정
            shootCooldown = shootingRate;

            // 사선 방향 계산 (오른쪽 기준 위/아래 3도씩 벌어진 탄막)
            Vector3 dir1 = Quaternion.AngleAxis(3, Vector3.forward) * Vector3.right;
            Vector3 dir2 = Quaternion.AngleAxis(-3, Vector3.forward) * Vector3.right;

            // 탄막 생성 및 발사
            CreateBullet(transform.position, dir1, 0, shotPrefab, 20);
            CreateBullet(transform.position, dir2, 0, shotPrefab, 20);

            // [참고] 사운드 재생 코드는 현재 주석 처리됨
            /*
            if (isEnemy) SoundEffectsHelper.Instance.MakeEnemyShotSound();
            else SoundEffectsHelper.Instance.MakePlayerShotSound();
            */
        }
    }

    /// <summary>
    /// 실제 자탄(Bullet) 오브젝트를 생성하고 초기값을 설정합니다.
    /// </summary>
    /// <param name="gun">발사 시작 위치</param>
    /// <param name="dir">날아갈 방향</param>
    /// <param name="distance">발사 지점으로부터의 오프셋 거리</param>
    /// <param name="shotPrefab">사용할 탄알 프리팹</param>
    /// <param name="speed">탄알 속도</param>
    /// <returns>생성된 탄알의 Transform</returns>
    private Transform CreateBullet(Vector3 gun, Vector3 dir, float distance, Transform shotPrefab, float speed)
    {
        // 1. 프리팹 생성
        var shotTransform = Instantiate(shotPrefab) as Transform;

        // 2. ShotScript 컴포넌트 설정
        ShotScript shot = shotTransform.gameObject.GetComponent<ShotScript>();
        if (shot != null)
        {
            shot.isEnemyShot = false; // 플레이어 탄막으로 설정
            shot.direction = dir;     // 비행 방향 설정
            shot.lifetime = 3f;        // 생존 시간(초)
            shot.SetSpeed(speed);      // 이동 속도 설정
        }

        // 3. 발사 위치 조정
        shotTransform.position = gun + dir.normalized * distance;

        return shotTransform;
    }
}