using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveScript : MonoBehaviour
{
    public Vector2 speed = new Vector2(15, 15);
    public Vector2 direction = new Vector2(1, 0);
    private Vector2 movement;

    // 1. 변수 이름 중복 방지를 위해 언더바(_)가 붙은 변수 사용
    private Rigidbody2D _rigidbody2D;

    void Awake()
    {
        // 2. 구식 프로퍼티 할당 대신 GetComponent 사용
        _rigidbody2D = this.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement = new Vector2(speed.x * direction.x, speed.y * direction.y);
    }

    void FixedUpdate()
    {
        // 3. GetComponent를 매 프레임 호출하는 대신 캐싱된 변수 사용 (성능 최적화)
        if (_rigidbody2D != null)
        {
            _rigidbody2D.velocity = movement;
        }
    }
}