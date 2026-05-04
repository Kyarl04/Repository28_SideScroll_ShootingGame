using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : PauseObject {

    private Vector3 speed = new Vector3(10, 10, 0);
    private Vector3 movement;
    private Rigidbody2D rb2d;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (_isPause)
        {
            rb2d.velocity = Vector2.zero;
            return;
        }

        float inputX = 0;
        float inputY = 0;

        // 키 입력 확인
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);
        bool up = Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(KeyCode.DownArrow);

        if (right) inputX = 1;
        if (left) inputX = -1;
        if (up) inputY = 1;
        if (down) inputY = -1;

        // 대각선 이동 속도 보정 (정규화)
        if (inputX != 0 && inputY != 0)
        {
            inputX *= 0.7071f; // Mathf.Sqrt(2)/2
            inputY *= 0.7071f;
        }

        // 저속 모드 (Shift 키)
        bool focus = Input.GetKey(KeyCode.LeftShift);
        float multiplier = focus ? 0.4f : 1.0f;

        // 이동 벡터 계산
        movement = new Vector2(speed.x * inputX * multiplier, speed.y * inputY * multiplier);

        // 화면 밖으로 나가지 않도록 제한 (포지션 클램프는 Update에서 수행)
        LockInScreen();
    }

    void FixedUpdate()
    {
        if (_isPause) return;

        // 1. 에러 해결: x, y 대신 계산된 movement를 velocity에 직접 할당
        rb2d.velocity = movement;
    }

    // 화면 밖 제한 로직 함수화
    void LockInScreen()
    {
        var dist = (transform.position - Camera.main.transform.position).z;
        var leftBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).x;
        var rightBorder = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, dist)).x;
        // Viewport 좌표계 상에서 y축은 아래가 0, 위가 1입니다. 기존 코드의 위아래 로직을 수정했습니다.
        var bottomBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, dist)).y;
        var topBorder = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, dist)).y;

        transform.position = new Vector3(
          Mathf.Clamp(transform.position.x, leftBorder, rightBorder),
          Mathf.Clamp(transform.position.y, bottomBorder, topBorder),
          transform.position.z
        );
    }
}