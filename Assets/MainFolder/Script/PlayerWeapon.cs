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

    // 상태 변수들
    private float nextFireTime = 0f;
    private float lastLaserTime = -999f;
    private bool isInvincible = false;
    private bool isLaserActive = false;

    public void TryFire()
    {
        if (Time.time >= nextFireTime)
        {
            FireBullets();
            nextFireTime = Time.time + fireInterval;
        }
    }

    private void FireBullets()
    {
        bool isFocused = Input.GetKey(KeyCode.LeftShift);
        
        for (int i = 0; i < firePoints.Length; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoints[i].position, Quaternion.identity);
            Rigidbody2D bRb = bullet.GetComponent<Rigidbody2D>();
            
            // i=0,1 (위쪽), i=2,3 (아래쪽) 방향 조절
            float angle = (i < 2) ? (0.2f * (2 - i)) : (-0.2f * (i - 1));
            Vector2 dir = isFocused ? Vector2.right : new Vector2(1f, angle).normalized;
            
            bRb.velocity = dir * bulletSpeed;
        }
    }

    public void TryActivateLaser()
    {
        if (Time.time >= lastLaserTime + laserCooldown && !isLaserActive)
        {
            StartCoroutine(ShootLaser());
        }
    }

    IEnumerator ShootLaser()
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