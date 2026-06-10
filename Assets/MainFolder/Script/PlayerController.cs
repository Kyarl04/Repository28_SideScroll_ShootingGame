using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어의 이동(물리), 체력 관리, 피격 시 무적 처리(깜빡임)를 관리하는 메인 컨트롤러.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float focusSpeedMultiplier = 0.5f; // 저속 이동(Shift) 시 감속 비율
    [SerializeField] private float padding = 0.5f; // 화면 밖으로 나가지 못하게 하는 여백

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isFocused;
    private Vector2 minBound, maxBound;
    private PlayerWeapon weapon; 

    [Header("Animation")]
    public Animator anim;

    [Header("HP")]    
    public int maxLives = 3;
    private int currentLives;
    [SerializeField] private GameObject[] hearts;
    [SerializeField] private GameObject heartBreakEffectPrefab;

    [Header("Blink Effect")]  
    public bool isInvincible = false;
    [SerializeField] private float invincibilityDuration = 2.0f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject invincibilityEffectPrefab; 
    private GameObject activeInvincibilityEffect;

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        currentLives = maxLives;
        UpdateHeartUI();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        weapon = GetComponent<PlayerWeapon>(); 
        SetScreenBoundaries();
    }

    private void Update()
    {
        // 1. 입력 처리 (Input)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(inputX, inputY).normalized; // 대각선 이동 시 속도 증가 방지(Normalize)
        
        isFocused = Input.GetKey(KeyCode.LeftShift);

        // 2. 무기 및 스킬 트리거 (업데이트 프레임에서 감지)
        if (Input.GetKey(KeyCode.Z)) weapon.TryFire();
        if (Input.GetKeyDown(KeyCode.X)) weapon.TryActivateLaser();

        // 3. 방향 전환 애니메이션 세팅
        if (anim != null) anim.SetInteger("Horizontal", (int)inputX);
    }

    private void FixedUpdate()
    {
        // 물리 연산(FixedUpdate)을 통해 델타 타임에 안전하게 캐릭터를 이동시킵니다.
        float currentSpeed = isFocused ? moveSpeed * focusSpeedMultiplier : moveSpeed;
        Vector2 nextPos = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
        
        // 카메라 뷰포트(화면) 경계를 벗어나지 못하도록 Clamp 처리
        nextPos.x = Mathf.Clamp(nextPos.x, minBound.x, maxBound.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBound.y, maxBound.y);
        rb.MovePosition(nextPos);
    }

    /// <summary>
    /// 플레이어 피격 시 체력을 깎고 무적 코루틴을 시작하는 함수
    /// </summary>
    public void LoseLife()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPlayerHit(); 
        
        // 피격 시 UI 하트가 깨지는 연출 처리
        if (currentLives > 0 && currentLives <= hearts.Length)
        {
            int brokenHeartIndex = currentLives - 1; 
            
            if (heartBreakEffectPrefab != null && hearts[brokenHeartIndex] != null)
            {
                GameObject effect = Instantiate(heartBreakEffectPrefab, hearts[brokenHeartIndex].transform.position, Quaternion.identity, hearts[brokenHeartIndex].transform.parent);
                Destroy(effect, 1.5f);
            }
        }
        
        currentLives--;
        UpdateHeartUI();

        if (anim != null) anim.SetTrigger("Hit");

        if (currentLives <= 0) 
        {
            gameObject.SetActive(false); 
            if (GameManager.Instance != null) GameManager.Instance.ShowGameOver();
        }
        else 
        {
            StartCoroutine(StartInvincibility());
        }
    }

    private void UpdateHeartUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].SetActive(i < currentLives);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((other.CompareTag("EnemyBullet") || other.CompareTag("ObstacleBullet")) && !isInvincible)
        {
            LoseLife();
        }
    }

    /// <summary>
    /// 피격 시 무적 시간을 부여하고 캐릭터를 깜빡이게 하는 코루틴 (옵션 매니저의 투명도 동기화 포함)
    /// </summary>
    private IEnumerator StartInvincibility()
    {
        isInvincible = true;
        
        if (invincibilityEffectPrefab != null)
        {
            activeInvincibilityEffect = Instantiate(invincibilityEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < invincibilityDuration)
        {
            // 실시간으로 환경설정의 알파값을 가져와 깜빡임(Lerp) 연출과 충돌하지 않게 설계했습니다.
            float targetAlpha = GameOptionManager.Instance != null ? GameOptionManager.Instance.CurrentPlayerAlpha : 1f;

            spriteRenderer.color = new Color(1, 1, 1, targetAlpha * 0.3f); 
            yield return new WaitForSeconds(blinkInterval);
            
            targetAlpha = GameOptionManager.Instance != null ? GameOptionManager.Instance.CurrentPlayerAlpha : 1f;
            spriteRenderer.color = new Color(1, 1, 1, targetAlpha);   
            yield return new WaitForSeconds(blinkInterval);
            
            timer += blinkInterval * 2;
        }

        float finalAlpha = GameOptionManager.Instance != null ? GameOptionManager.Instance.CurrentPlayerAlpha : 1f;
        spriteRenderer.color = new Color(1, 1, 1, finalAlpha); 
        
        isInvincible = false;

        if (activeInvincibilityEffect != null)
        {
            ParticleSystem ps = activeInvincibilityEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();
            Destroy(activeInvincibilityEffect, ps != null ? 1.0f : 0f);
        }
    }

    private void SetScreenBoundaries()
    {
        Camera cam = Camera.main;
        minBound = cam.ViewportToWorldPoint(new Vector2(0, 0)) + new Vector3(padding, padding, 0);
        maxBound = cam.ViewportToWorldPoint(new Vector2(1, 1)) - new Vector3(padding, padding, 0);
    }
}