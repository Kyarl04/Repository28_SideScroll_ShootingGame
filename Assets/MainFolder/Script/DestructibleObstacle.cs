using UnityEngine;

/// <summary>
/// 보스가 소환하는 파괴 가능한 특수 탄막(오브젝트) 클래스.
/// </summary>
public class DestructibleObstacle : MonoBehaviour
{
    private float moveSpeed;
    private float currentHp;
    private int myPoolIndex;
    public int effectIndex { get; private set; }

    public void Initialize(float speed, float hp, int clearEffectIdx, int poolIdx)
    {
        moveSpeed = speed;
        currentHp = hp;
        effectIndex = clearEffectIdx;
        myPoolIndex = poolIdx;
    }

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

        if (IsOffScreenLeft())
        {
            ReturnToPool();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            Explode();
        }
    }

    public void Explode()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayObstacleDestroy();
        }

        if (BulletPooler.Instance != null)
        {
            GameObject effect = BulletPooler.Instance.GetEffect(effectIndex, transform.position, Quaternion.identity);
            
            if (effect != null)
            {
                var pooledEffect = effect.GetComponent<PooledEffect>();
                if (pooledEffect != null) pooledEffect.effectPoolIndex = effectIndex;
            }
        }

        ReturnToPool();
    }

    // ==========================================
    // [수정됨] 외부(레이저)에서 안전하게 회수할 수 있도록 public으로 변경
    // ==========================================
    public void ReturnToPool()
    {
        gameObject.SetActive(false);
        if (BulletPooler.Instance != null)
        {
            BulletPooler.Instance.ReturnBullet(gameObject, myPoolIndex);
        }
    }

    private bool IsOffScreenLeft()
    {
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
        return viewportPos.x < -0.1f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet") || other.CompareTag("PlayerBullet"))
        {
            Bullet bulletScript = other.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                TakeDamage(bulletScript.damage);
                BulletPooler.Instance.ReturnBullet(other.gameObject, bulletScript.poolIndex);
            }
        }
        else if (other.CompareTag("Player"))
        {
            Explode();
        }
    }
}