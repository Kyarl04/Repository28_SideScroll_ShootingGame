using UnityEngine;
using System.Collections;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public Transform[] firePoints = new Transform[4];
    public float bulletSpeed = 10f;
    public float fireInterval = 0.1f;

    [Header("Laser Settings")]
    public GameObject laserObject;
    public ParticleSystem magicCircle;
    public float laserDuration = 1.5f;
    public float laserCooldown = 3.0f;

    // 상태 변수
    private float nextFireTime = 0f;
    private float lastLaserTime = -999f;
    private bool isInvincible = false;
    private bool isLaserActive = false;

    // --- 외부(PlayerController)에서 호출하는 함수들 ---

    public void TryFire()
    {
        if (Time.time >= nextFireTime)
        {
            FireBullets();
            nextFireTime = Time.time + fireInterval;
        }
    }

    public void TryActivateLaser()
    {
        if (Time.time >= lastLaserTime + laserCooldown && !isLaserActive)
        {
            StartCoroutine(ShootLaser());
        }
    }

    // --- 내부 동작 로직 ---

    private void FireBullets()
    {
        bool isFocused = Input.GetKey(KeyCode.LeftShift);
        
        for (int i = 0; i < firePoints.Length; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoints[i].position, Quaternion.identity);
            Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
            
            // 저속 모드면 일직선, 아니면 부채꼴 (spread 값을 조절하여 퍼짐 정도 수정)
            float spread = 0.05f; 
            float angle = (i < 2) ? (spread * (2 - i)) : (-spread * (i - 1));
            Vector2 dir = isFocused ? Vector2.right : new Vector2(1f, angle).normalized;
            
            bRb.velocity = dir * bulletSpeed;
        }
    }

    private IEnumerator ShootLaser()
    {
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