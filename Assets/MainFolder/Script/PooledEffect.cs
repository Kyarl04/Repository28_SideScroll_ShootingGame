using UnityEngine;

public class PooledEffect : MonoBehaviour
{
    public int effectPoolIndex; // 자신이 태어난 풀의 번호
    private ParticleSystem ps;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        // 1. 활성화될 때 파티클을 강제로 다시 재생시킵니다.
        ps.Clear();
        ps.Play();
        
        // 2. 파티클 수명이 다 되면 비활성화하는 예약 (Stop Action이 안 먹힐 때 대비)
        Invoke("DisableEffect", ps.main.duration + ps.main.startLifetime.constantMax);
    }

    private void DisableEffect()
    {
        // 3. 비활성화 시 풀로 반환!
        if (BulletPooler.Instance != null)
        {
            BulletPooler.Instance.ReturnEffect(gameObject, effectPoolIndex);
        }
    }
}