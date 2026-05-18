using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 공격의 기반이 되는 클래스입니다. 
/// 다양한 탄알 프리팹 관리와 공격 상태(쿨타임, 휴식)를 제어합니다.
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    // ==========================================
    // [탄알 프리팹 설정] - 유니티 인스펙터에서 할당
    // ==========================================
    
    [Header("타원형 탄알 (Oval)")]
    public Transform oval_m0_green, oval_m0_blue;
    public Transform oval_big_blue, oval_big_red, oval_big_green;

    [Header("원형 탄알 (Circle)")]
    public Transform circle_m0_red, circle_m0_blue, circle_m0_green;
    public Transform circle_m1_red, circle_m1_blue, circle_m1_green;
    public Transform circle_m2_red, circle_m2_blue, circle_m2_green;
    public Transform circle_big_red, circle_big_blue, circle_big_green;

    [Header("특수 모양 탄알")]
    public Transform star_green, star_red, star_yellow, star_purple;
    public Transform star_big_green;
    public Transform knife_white, knife_blue;
    public Transform heart_red, heart_orange;

    [Header("부적 탄알 (Amulet)")]
    public Transform amulet_red, amulet_purple, amulet_blue, amulet_green, amulet_azure, amulet_yellow, amulet_black, amulet_white;
    public Transform amulet_red_s, amulet_blue_s, amulet_red_x, amulet_blue_x;

    // ==========================================
    // [참조 및 상태 변수]
    // ==========================================
    
    public Transform player;         // 플레이어 위치 참조
    protected EnemyBarrage barrage;  // 탄막 생성 컴포넌트

    [Header("타이머 및 카운터")]
    protected float cooldown1 = 0f;  // 탄막 패턴별 개별 쿨타임
    protected float cooldown2 = 0f;
    protected float cooldown3, cooldown4, cooldown5;
    protected float rest = 0;        // 공격 중지(휴식) 시간

    protected int flag = 1;          // 공격 단계/상태 구분용 플래그
    protected int shootCount = 0;    // 발사 횟수 카운트

    // 화면 경계 좌표 (월드 좌표계 기준)
    protected Vector3 upleft, upright, downleft, downright;

    // ==========================================
    // [초기화]
    // ==========================================

    void Awake()
    {
        // 탄막 생성기 가져오기 및 플레이어 설정
        barrage = transform.GetComponent<EnemyBarrage>();
        if (barrage != null) barrage.SetPlayer(player);

        // 화면 끝 지점의 월드 좌표 계산 (이동 제한이나 탄막 생성 위치 계산용)
        downleft = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
        downright = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0));
        upleft = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 0));
        upright = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
    }

    // ==========================================
    // [공격 제어 함수]
    // ==========================================

    /// <summary> 특정 시간 동안 공격을 멈춥니다. </summary>
    public void Rest(float time)
    {
        rest = time;
    }

    void Update()
    {
        if (player == null) return;

        // 1. 휴식 시간 체크
        if (rest > 0)
        {
            rest -= Time.deltaTime;
            return; // 휴식 중엔 아래 로직(공격)을 실행하지 않음
        }

        // 2. 모든 쿨타임 실시간 감소
        UpdateCooldowns();

        // 3. 공격 실행 (상속받은 클래스에서 구현된 Attack 호출)
        Attack();
    }

    /// <summary> 모든 쿨타임 변수를 시간에 따라 감소시킵니다. </summary>
    private void UpdateCooldowns()
    {
        if (cooldown1 > 0) cooldown1 -= Time.deltaTime;
        if (cooldown2 > 0) cooldown2 -= Time.deltaTime;
        if (cooldown3 > 0) cooldown3 -= Time.deltaTime;
        if (cooldown4 > 0) cooldown4 -= Time.deltaTime;
        if (cooldown5 > 0) cooldown5 -= Time.deltaTime;
    }

    // ==========================================
    // [가상 함수 - 상속용]
    // ==========================================

    /// <summary> 실제 탄막 패턴을 정의합니다. 자식 클래스에서 override 하여 사용합니다. </summary>
    public virtual void Attack() { }

    /// <summary> 공격 패턴 단계(Phase)를 설정합니다. </summary>
    public virtual void SetFlag(int i) { flag = i; }

    public int GetFlag() { return flag; }
}