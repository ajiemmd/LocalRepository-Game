using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Frog : Enemy
{
    private Rigidbody2D rb;
    //private Animator Anim;
    private Collider2D Coll;
    public LayerMask Ground;
    private float leftx, rightx;

    public Transform leftpoint, rightpoint;
    public float Speed,JumpForce;

    private bool Faceleft = true;


    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        //Anim = GetComponent<Animator>();
        Coll = GetComponent<Collider2D>();

        transform.DetachChildren();//断绝父子物体的关系,不然俩个点会跟着青蛙一起动
        leftx = leftpoint.position.x;
        rightx = rightpoint.position.x;
        //Destroy(leftpoint.gameObject);
        //Destroy(rightpoint.gameObject);
    }

    void Update()
    {
        SwitchAnim();
    }

    //移动
    void Movement()
    {

        if (Faceleft)//面向左侧
        {
            if (Coll.IsTouchingLayers(Ground))
            {
                Anim.SetBool("jumping", true);
                rb.velocity = new Vector2(-Speed, JumpForce);
            }
        if (transform.position.x < leftx)//超过左侧点掉头
            {
                rb.velocity = new Vector2(0, 0);
                transform.localScale = new Vector3(-1, 1, 1);
                Faceleft = false;
            }
        }
        else//面向右侧
        {
            if (Coll.IsTouchingLayers(Ground))
            {
                Anim.SetBool("jumping", true);
                rb.velocity = new Vector2(Speed, JumpForce);
            }
            if (transform.position.x > rightx)//超过右侧点掉头
            {
                rb.velocity = new Vector2(0, 0);
                transform.localScale = new Vector3(1, 1, 1);
                Faceleft = true;
            }
        }

    }

   
    void SwitchAnim()
    {
        if(Anim.GetBool("jumping"))
        {
            if(rb.velocity.y < 0.1)
            {
                Anim.SetBool("jumping", false);
                Anim.SetBool("falling", true);
            }
        }    
        if(Coll.IsTouchingLayers(Ground) && Anim.GetBool("falling"))
        {
            Anim.SetBool("falling", false);
        }
    }

   

}


