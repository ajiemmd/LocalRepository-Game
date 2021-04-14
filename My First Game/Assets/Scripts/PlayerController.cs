using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//已知BUG：1、从高平台下落时，不是下落动画，无法击杀目标 ————> 99行解决
public class PlayerController : MonoBehaviour
{
    [SerializeField]private Rigidbody2D rb;//在前面加上[SerializeField]即可在编辑器看到变量
    private Animator anim;
    public Collider2D Coll;
    public Collider2D DisColl;
    public Transform CellingCheck,GroundCheck;
    //public AudioSource jumpAudio, hurtAudio, cherryAudio;
    public Joystick joystick;

    public float speed;
    public float jumpForce;
    public LayerMask ground;
    [SerializeField]
    private int Cherry;
     public Text CherryNum;
    private bool isHurt;//默认是false
    private bool isGround;
    private int extraJump;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        if (!isHurt)//要求:受伤跳过移动方法，默认isHurt是false。那么在OnCollisionEnter2D方法中设置判定player碰到时，将ishurt改为ture
        {
            Movement();
        }
        SwitchAnim();
        isGround = Physics2D.OverlapCircle(GroundCheck.position, 0.2f,ground);
    }

    private void Update()
    {
        //Jump();

        NewJump();
        Crouch();
        CherryNum.text = Cherry.ToString();
    }

    void Movement()//移动
    {
        //PC代码
        float horizontalMove = Input.GetAxis("Horizontal");
        float faceDircetion = Input.GetAxisRaw("Horizontal");

        if (horizontalMove != 0)//角色移动
        {
            rb.velocity = new Vector2(horizontalMove * speed * Time.fixedDeltaTime, rb.velocity.y);
            anim.SetFloat("running", Mathf.Abs(faceDircetion));
        }

        if (faceDircetion != 0)//角色朝向
        {
            transform.localScale = new Vector3(faceDircetion, 1, 1);
        }

        ////移动端Android
        //float horizontalMove = joystick.Horizontal;
        //float faceDircetion = joystick.Horizontal;

        //if (horizontalMove != 0)//角色移动
        //{
        //    rb.velocity = new Vector2(horizontalMove * speed * Time.fixedDeltaTime, rb.velocity.y);
        //    anim.SetFloat("running", Mathf.Abs(horizontalMove));
        //}

        //if (faceDircetion > 0)//角色朝向
        //{
        //    transform.localScale = new Vector3(1, 1, 1);
        //}
        //if (faceDircetion < 0)//角色朝向
        //{
        //    transform.localScale = new Vector3(-1, 1, 1);
        //}


        Crouch();//调用蹲下方法

    }

    //切换动画
    void SwitchAnim()
    {
        //anim.SetBool("idle", false);

        if(rb.velocity.y < 0 && !Coll.IsTouchingLayers(ground))//判断坠落  高处下落时也触发下落动画
        {
            anim.SetBool("falling", true);
        }
        if (anim.GetBool("jumping")) 
        {
            if (rb.velocity.y < 0)
            {
                anim.SetBool("jumping", false);
                anim.SetBool("falling", true);//下落动画
            }
        }else if (isHurt)
        {
            anim.SetBool("hurt", true);

            if(Mathf.Abs(rb.velocity.x) < 0.1f)//当速度降到0时，修改受伤状态为正常
            {
                anim.SetBool("hurt", false);
                //anim.SetBool("idle", true);
                anim.SetFloat("running", 0);//将速度设为0，不然会导致player碰撞后保持跑步状态
                isHurt = false;
            }
        }
        else if (Coll.IsTouchingLayers(ground)) 
        {
            anim.SetBool("falling", false);
            //anim.SetBool("idle", true);
        }


    }

    //碰撞触发器
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //收集物品
        if (collision.tag == "Collection")
        {
            collision.tag = "Null";
            collision.GetComponent<Animator>().Play("IsGot");
            //cherryAudio.Play();
            SoundManager.instance.CherryAudio();
            //Destroy(collision.gameObject);
            //Cherry += 1;//
            //CherryNum.text = Cherry.ToString();
        }

        if (collision.tag == "DeadLine")
        {
            GetComponent<AudioSource>().enabled = false; //将所有音频禁用

            //Invoke("Restart", 2f);//延迟俩秒 调用Restart方法
            Invoke(nameof(Restart), 2f);//这种写法较好：不会因为字符串和方法名不一致导致错误
        }

    }


    //碰撞/消灭敌人
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (anim.GetBool("falling"))
            {
                enemy.JumpOn();  
                rb.velocity = new Vector2(rb.velocity.x, jumpForce * 0.7f * Time.fixedDeltaTime);//再 小跳 一次
                anim.SetBool("jumping", true);  
                //受伤
            }else if(transform.position.x < collision.gameObject.transform.position.x)
            {
                rb.velocity = new Vector2(-5, rb.velocity.y);
                //hurtAudio.Play();
                SoundManager.instance.HurtAudio();
                isHurt = true;
            }
            else if (transform.position.x > collision.gameObject.transform.position.x)
            {
                rb.velocity = new Vector2(5, rb.velocity.y);
                //hurtAudio.Play();
                SoundManager.instance.HurtAudio();
                isHurt = true;

            }
            //casul


        }
    }

    //蹲下 PC
    //void Crouch()
    //{
    //    if (!Physics2D.OverlapCircle(ceilingCheck.position,0.2f,ground))//判定，若头部顶点位置，0.2F半径内有ground层级，则不能起来
    //    {

    //        if (Input.GetButton("Crouch"))
    //        {
    //            anim.SetBool("crouching", true);
    //            disColl.enabled = false;
    //        }
    //        else 
    //        {
    //            anim.SetBool("crouching", false);
    //            disColl.enabled = true;
    //        }

    //    }
    //}

    
    void Crouch()//蹲下  Android
    {
        if (!Physics2D.OverlapCircle(CellingCheck.position, 0.2f, ground))//判定，若头部顶点位置，0.2F半径内有ground层级，则不能起来
        {

            if (joystick.Vertical < -0.5f)
            {
                anim.SetBool("crouching", true);
                DisColl.enabled = false;
            }
            else
            {
                anim.SetBool("crouching", false);
                DisColl.enabled = true;
            }

        }
    }

    //void Jump()   ////角色跳跃  PC
    //{
    //    if (Input.GetButtonDown("Jump") && coll.IsTouchingLayers(ground)) 
    //    {
    //        rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.fixedDeltaTime);
    //        jumpAudio.Play();
    //        anim.SetBool("jumping", true);
    //    }
    //}


    //void Jump()//角色跳跃 Android
    //{
    //    if (joystick.Vertical > 0.5f && Coll.IsTouchingLayers(ground))//-1f~1f
    //    {
    //        rb.velocity = new Vector2(rb.velocity.x, jumpForce * Time.fixedDeltaTime);
    //        //jumpAudio.Play();
    //        anim.SetBool("jumping", true);
    //    }
    //}



    void NewJump()//新的跳跃代码
    {
        if (isGround)
        {
            extraJump = 1;
        }
        if (Input.GetButtonDown("Jump") && extraJump > 0)
        {
            rb.velocity = Vector2.up * jumpForce;//Vector2.up是简写new Vector2 (0,1)
            extraJump--;
            SoundManager.instance.JumpAudio();
            anim.SetBool("jumping", true);
        }
        if (Input.GetButtonDown("Jump") && extraJump == 0 && isGround)
        {
            rb.velocity = Vector2.up * jumpForce;//Vector2.up是简写new Vector2 (0,1)
            anim.SetBool("jumping", true);

        }
    }

    void Restart()//重置当前场景

    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);//重新载入当前场景
    }


    public void CherryCount()
    {
        Cherry += 1;
    }

}
