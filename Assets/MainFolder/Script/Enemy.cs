using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Stats (일반 몬스터용)")]
    [Tooltip("보스는 DanmakuBoss의 Phase HP를 따르므로 이 값이 무시됨.")]
    [SerializeField] private float maxHealth = 100f; 
    
    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Image healthBar; 
    [SerializeField] private GameObject healthBarContainer; // 체력바 배경까지 끄기 위한 부모 객체 (선택)

    private DanmakuBoss bossScript; // 보스 스크립트 참조

    private void Start()
    {
        bossScript = GetComponent<DanmakuBoss>();
        
        // 보스가 아닌 일반 몬스터일 경우 스스로 체력 초기화
        if (bossScript == null) 
        {
            SetupHP(maxHealth);
        }
    }

    // 보스가 페이즈를 시작할 때 호출하는 함수
    public void SetupHP(float hp)
    {
        maxHealth = hp;
        currentHealth = maxHealth;
        
        if (healthBarContainer != null) healthBarContainer.SetActive(true);
        else if (healthBar != null) healthBar.gameObject.SetActive(true);
        
        if (healthBar != null) healthBar.fillAmount = 1f;
    }

    // 보스가 페이즈 전환 중일 때 UI를 숨기는 함수
    public void HideUI()
    {
        if (healthBarContainer != null) healthBarContainer.SetActive(false);
        else if (healthBar != null) healthBar.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayPlayerBulletHit();
            }
            
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

    public void TakeDamage(float damage)
    {
        // 1. 보스라면? -> 내 체력(0)은 무시하고 오직 DanmakuBoss 스크립트에게 데미지 처리를 온전히 떠넘깁니다!
        if (bossScript != null)
        {
            bossScript.BossTakeDamage(damage);
            return; // 떠넘겼으니 여기서 함수를 바로 끝냅니다.
        }

        // 2. 일반 몬스터라면? -> 기존처럼 자체 체력으로 계산합니다.
        currentHealth -= damage;
        
        if (healthBar != null) healthBar.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            currentHealth = 0; // 마이너스 방지
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}