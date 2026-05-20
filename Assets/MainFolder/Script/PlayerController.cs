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
    private PlayerWeapon weapon; // 무기 스크립트 참조

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
        currentLives = maxLives;
        UpdateHeartUI();
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        weapon = GetComponent<PlayerWeapon>(); // 같은 오브젝트의 무기 스크립트를 가져옴
        SetScreenBoundaries();
    }

    private void Update()
    {
        // 1. 이동 입력
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        isFocused = Input.GetKey(KeyCode.LeftShift);

        // 2. 무기 명령
        if (Input.GetKey(KeyCode.Z)) weapon.TryFire();
        if (Input.GetKeyDown(KeyCode.X)) weapon.TryActivateLaser();
    }

    private void FixedUpdate()
    {
        float currentSpeed = isFocused ? moveSpeed * focusSpeedMultiplier : moveSpeed;
        Vector2 nextPos = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
        nextPos.x = Mathf.Clamp(nextPos.x, minBound.x, maxBound.x);
        nextPos.y = Mathf.Clamp(nextPos.y, minBound.y, maxBound.y);
        rb.MovePosition(nextPos);
    }

    //HP
    public void LoseLife()
    {
        currentLives--;
        UpdateHeartUI();

        if (currentLives <= 0) 
        {
            // 게임 오버 처리
            Debug.Log("Game Over");
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

    //Blink Effect
    private IEnumerator StartInvincibility()
    {
        isInvincible = true;
        
        // 깜빡임 효과 (0.1초 간격으로 투명도 조절)
        float blinkInterval = 0.1f;
        float timer = 0f;

        while (timer < invincibilityDuration)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); // 반투명
            yield return new WaitForSeconds(blinkInterval);
            spriteRenderer.color = new Color(1, 1, 1, 1f);   // 불투명
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval * 2;
        }

        spriteRenderer.color = new Color(1, 1, 1, 1f); // 원상복구
        isInvincible = false;
    }

    private void SetScreenBoundaries()
    {
        Camera cam = Camera.main;
        minBound = cam.ViewportToWorldPoint(new Vector2(0, 0)) + new Vector3(padding, padding, 0);
        maxBound = cam.ViewportToWorldPoint(new Vector2(1, 1)) - new Vector3(padding, padding, 0);
    }


}