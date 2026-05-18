using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 파티클 이펙트 생성 및 화면 내 탄막 제거를 관리하는 매니저 클래스입니다.
/// </summary>
public class SpecialEffectsHelper : MonoBehaviour {
    
    public static SpecialEffectsHelper Instance; // 싱글톤 인스턴스
    
    [Header("파티클 프리팹")]
    public ParticleSystem hitEffect;       // 타격 효과
    public ParticleSystem explosionEffect; // 폭발 효과
    public ParticleSystem enemyOverEffect; // 적 파괴 효과

    void Awake()
    {
        if (Instance != null) Debug.LogError("SpecialEffectsHelper 인스턴스가 이미 존재합니다!");
        Instance = this;
    }

    /// <summary> 타격 위치에 효과 생성 </summary>
    public void Hit(Vector3 position) => instantiate(hitEffect, position);

    /// <summary> 폭발 위치에 효과 생성 </summary>
    public void Explosion(Vector3 position) => instantiate(explosionEffect, position);

    /// <summary> 적 파괴 위치에 효과 생성 </summary>
    public void DefeatEnemy(Vector3 position) => instantiate(enemyOverEffect, position);

    /// <summary> 화면 내의 모든 적 탄막을 회수(삭제) </summary>
    public void ClearEnemyBullet()
    {
        GameObject[] m_Bullets = GameObject.FindGameObjectsWithTag("EnemyBullet");
        foreach(var bullet in m_Bullets)
        {
            // ShotScript의 Recycle 메서드를 호출하여 오브젝트 풀로 반환
            bullet.GetComponent<ShotScript>().Recycle();
        }
    }

    /// <summary> 화면 내의 모든 플레이어 탄막을 삭제 </summary>
    public void ClearPlayerBullet()
    {
        GameObject[] m_Bullets = GameObject.FindGameObjectsWithTag("PlayerBullet");
        foreach (var bullet in m_Bullets)
        {
            Destroy(bullet);
        }
    }

    /// <summary> 파티클 시스템 인스턴스화 </summary>
    private ParticleSystem instantiate(ParticleSystem prefab, Vector3 position)
    {
        if (prefab == null) return null;
        ParticleSystem newParticleSystem = Instantiate(prefab, position, Quaternion.identity);
        
        return newParticleSystem;
    }
}