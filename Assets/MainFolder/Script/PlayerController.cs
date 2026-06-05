using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float focusSpeedMultiplier = 0.5f;
    [SerializeField] private float padding = 0.5f;

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
        if (anim == null) 
        {
            anim = GetComponent<Animator>();
        }
        
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
        // 1. 이동 입력 (깃허브 방식 적용)
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(inputX, inputY).normalized;
        
        isFocused = Input.GetKey(KeyCode.LeftShift);

        // 2. 무기 명령
        if (Input.GetKey(KeyCode.Z)) weapon.TryFire();
        if (Input.GetKeyDown(KeyCode.X)) weapon.TryActivateLaser();

        // 3. 깃허브 방식 애니메이션 연동
        if (anim != null)
        {
            anim.SetInteger("Horizontal", (int)inputX);
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = isFocused ? moveSpeed * focusSpeedMultiplier : moveSpeed;
        Vector2 nextPos = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
        nextPos.x = Mathf.Clamp(nextPos.x, minBound.x, maxBound.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBound.y, maxBound.y);
        rb.MovePosition(nextPos);
    }

    public void LoseLife()
    {
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPlayerHit(); 
        
        if (currentLives > 0 && currentLives <= hearts.Length)
        {
            int brokenHeartIndex = currentLives - 1; // 방금 사라질 하트의 배열 번호
            
            if (heartBreakEffectPrefab != null && hearts[brokenHeartIndex] != null)
            {
                // 부서지는 하트와 똑같은 위치에 이펙트를 생성합니다. (UI 캔버스 안에서 생성되도록 부모 설정)
                GameObject effect = Instantiate(heartBreakEffectPrefab, hearts[brokenHeartIndex].transform.position, Quaternion.identity, hearts[brokenHeartIndex].transform.parent);
                
                // 이펙트 재생이 끝나면 자동으로 지워지도록 1.5초 뒤 파괴
                Destroy(effect, 1.5f);
            }
        }
        
        currentLives--;
        UpdateHeartUI();

        if (anim != null) anim.SetTrigger("Hit");

        if (currentLives <= 0) 
        {
            gameObject.SetActive(false); // 플레이어 숨기기
            
            // [추가된 부분] 게임 오버 패널 띄우기
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
        if (other.CompareTag("EnemyBullet") && !isInvincible)
        {
            LoseLife();
        }
    }

    // ==========================================
    // [핵심 변경] 깜빡임 연출 중에도 옵션 매니저의 투명도를 따르도록 수정
    // ==========================================
    private IEnumerator StartInvincibility()
    {
        isInvincible = true;
        
        // ==========================================
        // [추가됨] 무적 시작 시 이펙트를 켭니다! (플레이어 몸에 붙여서 따라다니게)
        // ==========================================
        if (invincibilityEffectPrefab != null)
        {
            activeInvincibilityEffect = Instantiate(invincibilityEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < invincibilityDuration)
        {
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

        // ==========================================
        // [추가됨] 무적 시간이 끝나면 이펙트를 끕니다!
        // ==========================================
        if (activeInvincibilityEffect != null)
        {
            // 이펙트가 부자연스럽게 뚝 끊기지 않도록 파티클 재생만 멈춥니다.
            ParticleSystem ps = activeInvincibilityEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Stop();

            // 파티클 잔상이 사라질 여유 시간을 주고 완전 파괴 (파티클이 아니면 즉시 파괴)
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