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
    
    [Header("Blink Effect")]  
    public bool isInvincible = false;
    [SerializeField] private float invincibilityDuration = 2.0f;
    [SerializeField] private SpriteRenderer spriteRenderer;

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
        
        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < invincibilityDuration)
        {
            // 실시간으로 옵션창에 설정된 투명도 값을 가져옵니다. (매니저가 없으면 기본값 1f)
            float targetAlpha = GameOptionManager.Instance != null ? GameOptionManager.Instance.CurrentPlayerAlpha : 1f;

            // 1. 설정된 투명도보다 더 옅게 만들어서 깜빡이는 효과 (예: 설정값의 30%)
            spriteRenderer.color = new Color(1, 1, 1, targetAlpha * 0.3f); 
            yield return new WaitForSeconds(blinkInterval);
            
            // 2. 다시 설정된 투명도로 복구
            // (도중에 유저가 ESC를 누르고 슬라이더를 바꿨을 수도 있으니 한번 더 값을 가져옵니다)
            targetAlpha = GameOptionManager.Instance != null ? GameOptionManager.Instance.CurrentPlayerAlpha : 1f;
            spriteRenderer.color = new Color(1, 1, 1, targetAlpha);   
            yield return new WaitForSeconds(blinkInterval);
            
            timer += blinkInterval * 2;
        }

        // 루프가 끝난 뒤, 완전히 옵션 매니저의 투명도로 최종 복구!
        float finalAlpha = GameOptionManager.Instance != null ? GameOptionManager.Instance.CurrentPlayerAlpha : 1f;
        spriteRenderer.color = new Color(1, 1, 1, finalAlpha); 
        
        isInvincible = false;
    }

    private void SetScreenBoundaries()
    {
        Camera cam = Camera.main;
        minBound = cam.ViewportToWorldPoint(new Vector2(0, 0)) + new Vector3(padding, padding, 0);
        maxBound = cam.ViewportToWorldPoint(new Vector2(1, 1)) - new Vector3(padding, padding, 0);
    }
}