using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일반 몬스터의 체력 관리 및 피격 처리를 담당하는 클래스.
/// 단, 이 스크립트가 보스(DanmakuBoss)에게 부착되어 있을 경우, 데미지를 보스 메인 코드로 위임(Delegate)하는 어댑터 역할을 합니다.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Stats (일반 몬스터용)")]
    [Tooltip("보스는 DanmakuBoss의 Phase HP를 따르므로 이 값이 무시됨.")]
    [SerializeField] private float maxHealth = 100f; 
    
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthBar; 
    [SerializeField] private GameObject healthBarContainer; 

    private DanmakuBoss bossScript; 

    private void Start()
    {
        bossScript = GetComponent<DanmakuBoss>();
        
        // 보스가 아닌 일반 독립적인 몬스터일 경우 스스로 체력을 초기화합니다.
        if (bossScript == null) 
        {
            SetupHP(maxHealth);
        }
    }

    public void SetupHP(float hp)
    {
        maxHealth = hp;
        currentHealth = maxHealth;
        
        if (healthBarContainer != null) healthBarContainer.SetActive(true);
        else if (healthBar != null) healthBar.gameObject.SetActive(true);
        
        if (healthBar != null) healthBar.fillAmount = 1f;
    }

    public void HideUI()
    {
        if (healthBarContainer != null) healthBarContainer.SetActive(false);
        else if (healthBar != null) healthBar.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayPlayerBulletHit();
            
            Bullet bulletScript = other.GetComponent<Bullet>();
            
            if (bulletScript != null)
            {
                TakeDamage(bulletScript.damage); 
                BulletPooler.Instance.ReturnBullet(other.gameObject, bulletScript.poolIndex);
            }
            
            if (BulletPooler.Instance != null)
            {
                GameObject effect = BulletPooler.Instance.GetEffect(0, other.transform.position, Quaternion.identity);
                if (effect != null) effect.GetComponent<PooledEffect>().effectPoolIndex = 0;
            }
        }
    }

    /// <summary>
    /// 외부에서 데미지를 가할 때 호출하는 함수
    /// </summary>
    public void TakeDamage(float damage)
    {
        // 1. 이 오브젝트가 보스라면 내 체력 연산을 무시하고 DanmakuBoss 메인 로직으로 넘깁니다 (결합도 낮춤)
        if (bossScript != null)
        {
            bossScript.BossTakeDamage(damage);
            return; 
        }

        // 2. 일반 몬스터의 자체 체력 계산
        currentHealth -= damage;
        
        if (healthBar != null) healthBar.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0; 
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject); // 필요 시 일반 몹도 풀링 반환으로 수정 가능
    }
}