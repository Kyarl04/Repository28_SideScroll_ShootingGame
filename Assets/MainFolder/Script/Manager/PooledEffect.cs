using UnityEngine;

/// <summary>
/// 오브젝트 풀링과 연동되어 파티클 시스템 재생 종료 후 스스로를 풀에 재반환하는 독립 생명주기 클래스.
/// </summary>
public class PooledEffect : MonoBehaviour
{
    public int effectPoolIndex; // 반환될 부모 풀 배열 주소
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // 재사용 시 잔상이 남지 않도록 버퍼를 완전히 초기화 후 강제 동적 재생
        ps.Clear();
        ps.Play();
        
        // 파티클의 고유 지속 시간 및 지연 최대 수명을 정밀 계산하여 Invoke 스케줄링 예약
        Invoke("DisableEffect", ps.main.duration + ps.main.startLifetime.constantMax);
    }

    private void DisableEffect()
    {
        if (BulletPooler.Instance != null)
        {
            BulletPooler.Instance.ReturnEffect(gameObject, effectPoolIndex);
        }
    }
}