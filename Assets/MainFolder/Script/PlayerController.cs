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
    private bool isInvincible = false;
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

        // ==========================================
        // 3. 깃허브 방식 애니메이션 연동
        // ==========================================
        if (anim != null)
        {
            // 깃허브 원본처럼 'Horizontal' 파라미터에 int 값(-1, 0, 1)을 넘겨줍니다.
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
        SoundManager.Instance.PlayPlayerHit(); // 플레이어 피격음 추가
        
        currentLives--;
        UpdateHeartUI();

        // 깃허브 원본처럼 피격 시 'Hit' 애니메이션을 재생합니다.
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        if (currentLives <= 0) 
        {
            Debug.Log("Game Over");
            gameObject.SetActive(false); // 깃허브 방식: 죽으면 플레이어 숨기기
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

    private IEnumerator StartInvincibility()
    {
        isInvincible = true;
        
        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < invincibilityDuration)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); 
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.color = new Color(1, 1, 1, 1f);   
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval * 2;
        }

        spriteRenderer.color = new Color(1, 1, 1, 1f); 
        isInvincible = false;
    }

    private void SetScreenBoundaries()
    {
        Camera cam = Camera.main;
        minBound = cam.ViewportToWorldPoint(new Vector2(0, 0)) + new Vector3(padding, padding, 0);
        maxBound = cam.ViewportToWorldPoint(new Vector2(1, 1)) - new Vector3(padding, padding, 0);
    }
}