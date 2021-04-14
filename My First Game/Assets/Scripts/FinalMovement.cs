using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//bug:1、坠落过程中，如果按了跳跃键，会在落地时进行一次跳跃————>即不是处于地面时，按跳跃键，会在落地时执行
//修改方案：：1、坠落时按跳无反应（做不到）
//2、利用若没按跳跃键，y轴速度是否是负以及是否在ground层，来测试是否是从平台衰落，

//BUG：从高平台跑步坠落时，如果按了跳跃键，不会立即跳跃，而是落地时进行一次跳跃。 
//else if (jumpPressed  jumpCount  0  isJump)
//如果将这行代码中的 isJump 改为 !isGround,
//这个bug就没了，但新的问题：坠落时可以进行2次跳跃。
//有没有办法可以做到在坠落时按跳跃无法跳跃且正常落地呢？

//45行-50行初步解决该问题
//但是出现新问题：第一次跳跃在坠落时，再按一次跳跃，经常无效，若按多次跳跃则能检测到


public class FinalMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D coll;
    private Animator anim;

    public float speed, jumpForce;
    public Transform groundCheck;
    public LayerMask ground;

    public bool isGround, isJump;//判断player是否在地面， 判断player是否在跳跃   默认false

    bool jumpPressed;//默认false
    public int jumpCount;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        //46行-50行初步解决bug问题
        //但是出现新问题：第一次跳跃在坠落时，再按一次跳跃，经常无效，若按多次跳跃则能检测到，暂时注释
        /* if (!Input.GetButtonDown("Jump") && rb.velocity.y < 0 && !coll.IsTouchingLayers(ground))//判断是否处于平台坠落状态
        {
            jumpPressed = false;
        }
        else*/
        if (Input.GetButtonDown("Jump") && jumpCount > 0)// 每帧检测是否按下跳跃键 且跳跃次数大于0 
        {
            jumpPressed = true;
        }
    }

    private void FixedUpdate()
    {
        

        isGround = Physics2D.OverlapCircle(groundCheck.position, 0.1f, ground);

        GroundMovement();

        Jump();

        SwitchAnim();
    }

    void GroundMovement()//地面移动
    {
        float horizontalMove = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(horizontalMove * speed, rb.velocity.y);//速度

        if(horizontalMove != 0)
        {
            transform.localScale = new Vector3(horizontalMove, 1, 1);//player朝向
        }
    }

    void Jump()//跳跃
    {

        if (isGround)//在地面
        {
            jumpCount = 2;
            isJump = false;
        }

        if (jumpPressed && isGround)//若按下跳跃且在地面
        {
            isJump = true;// 将 isJump 设为true
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);//跳跃一次
            jumpCount--;
            jumpPressed = false;//将jumpPressed设为false
        }
        
        else if (jumpPressed && jumpCount > 0 & isJump)//实现2段跳    若按下跳跃 且 不在地面 且 跳跃次数大于0 
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce); //跳跃一次
            jumpCount--;
            jumpPressed = false;
        }   

    }

    void SwitchAnim()//动画
    {
        anim.SetFloat("running", Mathf.Abs(rb.velocity.x));
       
        if (isGround)
        {
            anim.SetBool("falling", false);
            //anim.SetBool("idle", true);

        }
        else if(!isGround && rb.velocity.y > 0)
        {
            anim.SetBool("jumping", true);
        }
        else if(rb.velocity.y < 0)
        {
            anim.SetBool("jumping", false);
            anim.SetBool("falling", true);
        } 

    }


}
