using UnityEngine;

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

    private void SetScreenBoundaries()
    {
        Camera cam = Camera.main;
        minBound = cam.ViewportToWorldPoint(new Vector2(0, 0)) + new Vector3(padding, padding, 0);
        maxBound = cam.ViewportToWorldPoint(new Vector2(1, 1)) - new Vector3(padding, padding, 0);
    }
}