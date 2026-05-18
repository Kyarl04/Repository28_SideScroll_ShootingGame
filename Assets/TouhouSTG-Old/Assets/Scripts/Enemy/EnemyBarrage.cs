using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 다양한 탄막(Barrage) 패턴을 생성하고 제어하는 클래스입니다.
/// </summary>
public class EnemyBarrage : MonoBehaviour {

    private Transform player; // 자기조준탄(Sniper)을 위한 플레이어 참조

    public void SetPlayer(Transform p)
    {
        player = p;
    }

    // ==========================================
    // [기본 탄알 생성 메서드]
    // ==========================================

    public Transform CreateBullet(Vector3 gun, Vector3 dir, float distance, Transform shotPrefab, float speed)
    {
        Transform shotTransform = Instantiate(shotPrefab) as Transform;
        ShotScript shot = shotTransform.gameObject.GetComponent<ShotScript>();
        if (shot != null)
        {
            shot.isEnemyShot = true;
            shot.direction = dir;
            shot.SetSpeed(speed);
        }
        shotTransform.transform.position = gun + dir.normalized * distance;
        return shotTransform;
    }

    public Transform CreateBullet(Vector3 gun, Vector3 dir, Transform shotPrefab, float speed)
    {
        Transform shotTransform = Instantiate(shotPrefab) as Transform;
        ShotScript shot = shotTransform.gameObject.GetComponent<ShotScript>();
        if (shot != null)
        {
            shot.isEnemyShot = true;
            shot.direction = dir.normalized;
            shot.SetSpeed(speed);
        }
        shotTransform.position = gun;
        return shotTransform;
    }

    public Transform CreateBullet(Vector3 gun, Transform shotPrefab)
    {
        Transform shotTransform = Instantiate(shotPrefab) as Transform;
        shotTransform.position = gun;
        ShotScript shot = shotTransform.gameObject.GetComponent<ShotScript>();
        if (shot != null)
        {
            shot.SetSpeed(1);
            shot.SetDirection(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), 0));
        }
        return shotTransform;
    }

    private Transform CreateBullet(Transform gun, Vector3 dir, Transform shotPrefab, float speed)
    {
        return CreateBullet(gun.position, dir, shotPrefab, speed);
    }

    // ==========================================
    // [원형 및 곡선 탄막 패턴]
    // ==========================================

    public void FireCircularBullet(Vector3 gun, Vector3 dir, Vector3 center, float angleSpeed, Transform bullet, float speed)
    {
        Transform tempBullet = CreateBullet(gun, dir, bullet, speed);
        StartCoroutine(tempBullet.GetComponent<ShotScript>().CircularMove(center, angleSpeed));
    }

    public void FireCircularBullet(Vector3 gun, Vector3 dir, Transform bullet, float speed)
    {
        Transform tempBullet = CreateBullet(gun, dir, bullet, speed);
        StartCoroutine(tempBullet.GetComponent<ShotScript>().CircularMove(gun, 2f));
    }

    public void FireCircle(Vector3 center, int fire, float distance, float angleSpeed, float speed, Transform bullet, float startOffset)
    {
        Vector3 dir = Vector3.up;
        Quaternion offset = Quaternion.AngleAxis(startOffset, Vector3.forward);
        dir = offset * dir;

        Quaternion rotateQuate = Quaternion.AngleAxis((angleSpeed > 0 ? 360f : -360f) / fire, Vector3.forward);

        for (int i = 0; i < fire; i++)
        {
            Vector3 gun = dir.normalized * distance + center;
            ShotScript shot = CreateBullet(gun, dir, bullet, speed).GetComponent<ShotScript>();
            shot.SetCheckOut(false);
            dir = rotateQuate * dir;
            StartCoroutine(shot.CircularMove(center, angleSpeed));
        }
    }

    public IEnumerator FireCircleGroup(Vector3 center, int fire, float distance, float angleSpeed, float speed, Transform bullet, float startOffset, float angle, int firePerRound, float delay)
    {
        float totalAngle = startOffset;
        float deltaAngle = angle / firePerRound;
        for (int i = 0; i < firePerRound; i++)
        {
            FireCircle(center, fire, distance, angleSpeed, speed, bullet, totalAngle + deltaAngle * i);
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
        }
        yield return null;
    }

    // ==========================================
    // [부채꼴(Sector) 및 직선 탄막 패턴]
    // ==========================================

    public IEnumerator FireSector(Vector3 gun, Vector3 target, Transform bullet, int fire, float angle, float speed)
    {
        float deltaAngle = angle / (fire - 1);
        Vector3 dir = (target - gun).normalized;
        Quaternion offset = Quaternion.AngleAxis(-angle / 2, Vector3.forward);
        dir = offset * dir;

        for (int i = 0; i < fire; i++)
        {
            CreateBullet(gun, dir, bullet, speed);
            dir = Quaternion.AngleAxis(deltaAngle, Vector3.forward) * dir;
        }
        yield return null;
    }

    public IEnumerator FireSector(float centerOffset, Vector3 gun, Transform bullet, int fire, float angle, float speed)
    {
        float deltaAngle = angle / (fire - 1);
        Vector3 dir = Quaternion.AngleAxis(centerOffset, Vector3.forward) * Vector3.up;
        Quaternion offset = Quaternion.AngleAxis(-angle / 2, Vector3.forward);
        dir = offset * dir;
        for (int i = 0; i < fire; i++)
        {
            CreateBullet(gun, dir, bullet, speed);
            dir = Quaternion.AngleAxis(deltaAngle, Vector3.forward) * dir;
        }
        yield return null;
    }

    public void FireSectorBounce(Vector3 gun, Vector3 dir, Transform bullet, int fire, float angle, float speed)
    {
        float deltaAngle = angle / (fire - 1);
        Quaternion offset = Quaternion.AngleAxis(-angle / 2, Vector3.forward);
        dir = offset * dir;

        for (int i = 0; i < fire; i++)
        {
            ShotScript shot = CreateBullet(gun, dir, bullet, speed).GetComponent<ShotScript>();
            StartCoroutine(shot.BounceMode(2));
            dir = Quaternion.AngleAxis(deltaAngle, Vector3.forward) * dir;
        }
    }

    public IEnumerator FireSectorBounceGroup(Vector3 gun, Vector3 dir, Transform bullet, int fire, float angle, float speed, int group, float totalOffset, float delay)
    {
        Quaternion offset = Quaternion.AngleAxis(angle / group, Vector3.forward);
        for (int i = 0; i < group; i++)
        {
            FireSectorBounce(gun, dir, bullet, fire, angle, speed);
            dir = offset * dir;
            yield return new WaitForSeconds(delay);
        }
        yield return null;
    }

    public void FireLine(Vector3 gun, Vector3 fireDir, Vector3 moveDir, float distance, int fire, Transform bullet, float speed)
    {
        Vector3 firePoint = gun;
        Vector3 deltaFirePoint = distance * fireDir.normalized;
        for (int i = 0; i < fire; i++)
        {
            ShotScript shot = CreateBullet(firePoint, moveDir, bullet, speed).GetComponent<ShotScript>();
            if (shot != null) shot.SetCheckOut(false);
            firePoint = firePoint + deltaFirePoint;
        }
    }

    public IEnumerator FireLineGroup(Vector3 gun, Vector3 groupDir, Vector3 fireDir, Vector3 moveDir, float groupDistance, float fireDistance, int group, int fire, Transform bullet, float speed, float delay)
    {
        Vector3 firePoint = gun;
        Vector3 deltaFirePoint = groupDir.normalized * groupDistance;
        for (int i = 0; i < group; i++)
        {
            FireLine(firePoint, fireDir, moveDir, fireDistance, fire, bullet, speed);
            firePoint = firePoint + deltaFirePoint;
            yield return new WaitForSeconds(delay);
        }
        yield return null;
    }

    // ==========================================
    // [조준탄 및 회전 회돌이 탄막 패턴]
    // ==========================================

    public void FireSniper(Vector3 gun, Transform target, Transform shotPrefab, float speed)
    {
        Vector3 dir = (target.position - gun).normalized;
        CreateBullet(gun, dir, shotPrefab, speed);
    }

    public void CreateDelaySniper(Vector3 gun, Vector3 dir, float starttime, float pausetime, float speed, float newspeed, Transform prefab)
    {
        ShotScript bullet = CreateBullet(gun, dir, prefab, speed).GetComponent<ShotScript>();
        if (bullet != null)
        {
            bullet.lifetime = 20;
            bullet.SetCheckOut(false);
            StartCoroutine(bullet.DelaySniper(starttime + pausetime, newspeed, player));
        }
    }

    public void FireAroundDelaySniper(Vector3 gun, float starttime, float pausetime, float speed, float newspeed, int firePerRound, Transform prefab)
    {
        Vector3 dir = Vector3.up;
        Quaternion offset = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
        dir = offset * dir;
        Quaternion rotateQuate = Quaternion.AngleAxis(360f / firePerRound, Vector3.forward);

        for (int i = 0; i < firePerRound; i++)
        {
            CreateDelaySniper(gun, dir, starttime, pausetime, speed, newspeed, prefab);
            dir = rotateQuate * dir;
        }
    }

    public IEnumerator FireLineSniper(Vector3 gun, Vector3 dir, Transform bullet, float speed, int number, float deltaTime)
    {
        for (int i = 0; i < number; i++)
        {
            CreateBullet(gun, dir, bullet, speed);
            yield return new WaitForSeconds(deltaTime);
        }
        yield return null;
    }

    public IEnumerator FireCircum(float startOffset, Vector3 start, Transform bullet, float distance, float endTime, float changeTime, float angle, float speed, float delay, int n1, int n2)
    {
        Vector3 bulletDir = Vector3.up;
        Quaternion rotateQuate = Quaternion.AngleAxis(360f / n2 + startOffset, Vector3.forward);
        for (int j = 0; j < n1; j++)
        {
            for (int i = 0; i < n2; i++)
            {
                Vector3 creatPoint = start + bulletDir * distance;
                Transform tempBullet = CreateBullet(creatPoint, bulletDir, bullet, speed);
                ShotScript shot = tempBullet.GetComponent<ShotScript>();
                if (shot != null)
                {
                    shot.SetCheckOut(false);
                    shot.SetStraightMove(false);
                    shot.lifetime = 99;
                    StartCoroutine(shot.DirChangeMoveMode(endTime, changeTime, angle));
                    bulletDir = rotateQuate * bulletDir;
                }
            }
            yield return new WaitForSeconds(delay);
        }
        yield return null;
    }

    public IEnumerator FireCircum(Vector3 start, Transform bullet, float distance, float endTime, float changeTime, float angle, float speed, float delay, int n1, int n2)
    {
        StartCoroutine(FireCircum(0f, start, bullet, distance, endTime, changeTime, angle, speed, delay, n1, n2));
        yield return null;
    }

    // ==========================================
    // [원주(Around) 탄막 오버로딩 함수들]
    // ==========================================

    // 보스들이 가장 많이 찾는 핵심 함수들입니다.
    public void FireAround(float startOffset, Vector3 gun, Transform shotPrefab, int firePerRound, float speed)
    {
        Vector3 dir = Vector3.up;
        Quaternion offset = Quaternion.AngleAxis(startOffset, Vector3.forward);
        dir = offset * dir;
        Quaternion rotateQuate = Quaternion.AngleAxis(360f / firePerRound, Vector3.forward);

        for (int i = 0; i < firePerRound; i++)
        {
            CreateBullet(gun, dir, shotPrefab, speed);
            dir = rotateQuate * dir;
        }
    }

    public void FireAround(Vector3 gun, Transform shotPrefab, int firePerRound, float speed)
    {
        float rd = Random.Range(0f, 720f / firePerRound);
        FireAround(rd, gun, shotPrefab, firePerRound, speed);
    }

    public IEnumerator FireAroundGroup(Vector3 gun, float startOffset, int group, Transform bullet, int firePerRound, int round, float createdelay, float rounddelay, float speed)
    {
        Vector3 bulletDir = Vector3.up;
        Quaternion offset = Quaternion.AngleAxis(startOffset, Vector3.forward);
        bulletDir = offset * bulletDir;
        Quaternion rotateQuate = Quaternion.AngleAxis(360f / group, Vector3.forward);
        List<Transform> bullets = new List<Transform>();
        
        for (int i = 0; i < group; i++)
        {
            var tempBullet = CreateBullet(gun, bulletDir, bullet, speed);
            bulletDir = rotateQuate * bulletDir;
            bullets.Add(tempBullet);
        }

        yield return new WaitForSeconds(createdelay);
        for (int i = 0; i < group; i++)
        {
            if (bullets[i] != null) bullets[i].GetComponent<ShotScript>().SetSpeed(0);
        }
        for (int i = 0; i < round; i++)
        {
            for (int j = 0; j < bullets.Count; j++)
            {
                if (bullets[j] != null)
                    FireAround(0, bullets[j].position, bullet, firePerRound, speed);
            }
            yield return new WaitForSeconds(rounddelay);
        }
        for (int i = 0; i < group; i++)
        {
            if (bullets[i] != null) Destroy(bullets[i].gameObject);
        }
    }

    // ==========================================
    // [기타 특수 패턴 (랜덤, 반사, 사인)]
    // ==========================================

    public void FireRandom(Vector3 gun, Transform shotPrefab, float speed)
    {
        Vector3 dir = Vector3.up;
        float rd = Random.Range(0f, 360f);
        Quaternion offset = Quaternion.AngleAxis(rd, Vector3.forward);
        dir = offset * dir;
        CreateBullet(gun, dir, shotPrefab, speed);
    }

    public IEnumerator FireTurbine(float startOffset, float angle, Vector3 gun, float radius, float distance, int round, Transform bullet, int firePerRound, float speed, float delay)
    {
        Vector3 bulletDir = Vector3.up;
        Quaternion offset = Quaternion.AngleAxis(startOffset, Vector3.forward);
        bulletDir = offset * bulletDir;
        Quaternion rotateQuate = Quaternion.AngleAxis(angle, Vector3.forward);
        for (int i = 0; i < round; i++)
        {
            Vector3 firePoint = gun + bulletDir * radius;
            FireAround(firePoint, bullet, firePerRound, speed);
            yield return new WaitForSeconds(delay);
            bulletDir = rotateQuate * bulletDir;
            radius += distance;
        }
    }

    public IEnumerator FireLineBounce(Vector3 gun, Vector3 dir, Transform bullet, float speed, int number, float deltaTime)
    {
        for (int i = 0; i < number; i++)
        {
            ShotScript shot = CreateBullet(gun, dir, bullet, speed).GetComponent<ShotScript>();
            StartCoroutine(shot.BounceMode(1));
            yield return new WaitForSeconds(deltaTime);
        }
        yield return null;
    }

    public void CreateBounceBullet(Vector3 gun, Vector3 dir, float distance, Transform shotPrefab, float speed)
    {
        ShotScript bullet = CreateBullet(gun, dir, distance, shotPrefab, speed).GetComponent<ShotScript>();
        bullet.lifetime = 99f;
        StartCoroutine(bullet.BounceMode());
    }

    public void FireAroundBounce(Vector3 gun, float startOffset, Transform bullet, int firePerRound, float speed, int bounceTime)
    {
        Vector3 bulletDir = Quaternion.AngleAxis(startOffset, Vector3.forward) * Vector3.up;
        Quaternion rotateQuate = Quaternion.AngleAxis(360f / firePerRound, Vector3.forward);
        for (int i = 0; i < firePerRound; i++)
        {
            ShotScript tempBullet = CreateBullet(gun, bulletDir, bullet, speed).GetComponent<ShotScript>();
            tempBullet.lifetime = 99;
            StartCoroutine(tempBullet.BounceMode(bounceTime));
            bulletDir = rotateQuate * bulletDir;
        }
    }

    public IEnumerator FireBounceFirework(Vector3 gun, Vector3 dir, Transform prefab0, Transform prefab1, float speed0, float speed1, float angle, float lifetime1, float delay)
    {
        ShotScript bullet0 = CreateBullet(gun, dir, prefab0, speed0).GetComponent<ShotScript>();
        bullet0.lifetime = 25f;
        StartCoroutine(bullet0.BounceMode());

        while (bullet0 != null)
        {
            Vector3 firePoint = bullet0.transform.position;
            Vector3 direction = -bullet0.direction;
            float offset = Random.Range(-angle, angle);
            direction = Quaternion.AngleAxis(offset, Vector3.forward) * direction;
            ShotScript bullet1 = CreateBullet(firePoint, direction, prefab1, speed1).GetComponent<ShotScript>();
            bullet1.lifetime = lifetime1;
            yield return new WaitForSeconds(delay);
        }
        yield return null;
    }

    public IEnumerator FireRandomField(Vector3 gun, float deltaX, float deltaY, Vector3 dir, float speedMin, float speedMax, Transform bullet, int number)
    {
        for (int i = 0; i < number; i++)
        {
            Vector3 firePoint = gun + new Vector3(Random.Range(0f, deltaX), Random.Range(0f, deltaY));
            float speed = Random.Range(speedMin, speedMax);
            ShotScript shot = CreateBullet(firePoint, dir, bullet, speed).GetComponent<ShotScript>();
            if (shot != null) shot.SetCheckOut(false);
        }
        yield return null;
    }

    public IEnumerator FireRandomSniper(Vector3 gun, Vector3 dir, float angle, float minSpeed, float maxSpeed, Transform bullet, int number)
    {
        for (int i = 0; i < number; i++)
        {
            float speed = Random.Range(minSpeed, maxSpeed);
            Vector3 direction = Quaternion.AngleAxis(Random.Range(-angle / 2, angle / 2), Vector3.forward) * dir;
            CreateBullet(gun, direction, bullet, speed);
        }
        yield return null;
    }

    public IEnumerator FireSin(Vector3 gun, Vector3 dir, float A, float T, Transform bullet, float speed, float deltaTime, float totalTime, bool positive)
    {
        Vector3 _x = Quaternion.AngleAxis(90, Vector3.forward) * dir;
        float omega = 2 * 3.14159f / T;
        float time = 0;
        while (time < totalTime)
        {
            Vector3 firePoint = positive ? gun + A * Mathf.Sin(omega * time) * _x : gun - A * Mathf.Sin(omega * time) * _x;
            ShotScript shot = CreateBullet(firePoint, dir, bullet, speed).GetComponent<ShotScript>();
            if (shot != null) shot.SetCheckOut(false);
            time += Time.deltaTime;
            yield return new WaitForSeconds(deltaTime);
        }
        yield return null;
    }
}