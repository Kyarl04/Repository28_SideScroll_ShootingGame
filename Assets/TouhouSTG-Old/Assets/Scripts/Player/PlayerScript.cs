using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : PauseObject  {
    
    public Input_Key laser;

    //基础属性
    public int startLife = 2, startBomb = 2;
    private int life, bomb, score = 0;

    private Vector2 movement;

    // 1. 변수 이름을 _rigidbody2D로 통일하여 충돌 방지
    private Rigidbody2D _rigidbody2D;
    
    //控制闪烁
    private float blinkTime = 0;

    //检测碰撞
    private bool collision = false;

    //决死
    bool missing = false;
    private float missingTime;

    //保护时间
    private bool isProtected = false;
    private float protectTime = 0;

    // 2. 상속된 member와 이름이 겹치므로 new 키워드 추가하여 경고 해결
    private new SpriteRenderer renderer;

    void Awake()
    {
        // 3. 구식 프로퍼티 대신 GetComponent 사용
        _rigidbody2D = this.GetComponent<Rigidbody2D>();
        renderer = this.GetComponent<SpriteRenderer>();
        life = startLife;
        bomb = startBomb;
    }
    
    public int GetLife()
    {
        return life;
    }

    public void Bomb()
    {
        if (bomb > 0)
        {
            laser.Bomb(3.5f);
            Protect(4f);
            Blink(3f);
            bomb--;
        }
    }

    public void Protect(float time)
    {
        protectTime = time;
    }

    public void Miss()
    {
        if (!missing)
        {
            SoundEffectHelper.Instance.MakeMissSound();
            missing = true;
            missingTime = 0.15f;
        }
    }

    public int GetBomb()
    {
        return bomb;
    }

    public void Blink(float time)
    {
        blinkTime = time;
    }
    
    void Update()
    {
        if (_isPause) return;

        #region 射击控制
        bool shoot = Input.GetKey(KeyCode.Z);
        if (shoot)
        {
            AttackScript weapon = GetComponent<AttackScript>();
            if (weapon != null)
            {
                weapon.Attack(false);
            }
        }
        #endregion

        #region bomb释放
        bool bombrelease = Input.GetKey(KeyCode.X) && (!laser.IsBombing());
        if (bombrelease)
        {
            if(protectTime <= 0)
            {
                if (missing)
                {
                    missing = false;
                }
                Bomb();
            }
        }
        #endregion

        #region 决死与死亡
        if (missingTime > 0)
            missingTime -= Time.deltaTime;
        if(missing && missingTime <= 0)
        {
            life--;
            if (life >= 0)
            {
                bomb = startBomb;
                Protect(2.5f);
                Blink(2f);
            }
            else
            {
                Destroy(gameObject);
            }
            missing = false;
        }
        #endregion

        #region 闪烁
        if (blinkTime > 0)
        {
            if(blinkTime % 0.2 > 0.1f)
            {
                renderer.enabled = true;
            }
            else
            {
                renderer.enabled = false;
            }
            blinkTime -= Time.deltaTime;
        }
        else
        {
            renderer.enabled = true;
        }
        #endregion

        #region 保护
        if (protectTime > 0)
            protectTime -= Time.deltaTime;
        isProtected = (protectTime > 0);

        if (collision && (!isProtected))
        {
            Miss();
            collision = false;
        }
        #endregion

        bool focus = Input.GetKey(KeyCode.LeftShift);
        this.transform.Find("point").gameObject.SetActive(focus);
        this.transform.Find("魔法阵").gameObject.SetActive(laser.IsBombing());
    }

    void FixedUpdate()
    {
        // 4. 캐싱된 _rigidbody2D 사용
        if (_rigidbody2D != null)
            _rigidbody2D.velocity = movement;
    }

    void OnDestroy()
    {
        SpecialEffectsHelper.Instance.Explosion(transform.position);
        SpecialEffectsHelper.Instance.ClearPlayerBullet(); 
    }

    void OnTriggerEnter2D(Collider2D otherCollider)
    {
        if (isProtected) return;
        if (collision) return;

        ShotScript shot = otherCollider.gameObject.GetComponent<ShotScript>();
        if (shot != null)
        {
            if (shot.isEnemyShot)
            {
                SpecialEffectsHelper.Instance.Hit(shot.transform.position);
                collision = true;
            }
        }
    }

    void OnColliderEnter2D(Collider2D otherCollider)
    {
        if (isProtected) return;
        if (collision) return;

        EnemyScript enemy = otherCollider.gameObject.GetComponent<EnemyScript>();
        if (enemy != null)
        {
           SpecialEffectsHelper.Instance.Hit(enemy.transform.position);
           collision = true;
        }
    }
}