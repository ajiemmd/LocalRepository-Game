using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Eager : Enemy
{

    private Rigidbody2D rb;
    public float speed = 10;

    public Transform highpoint, lowpoint;
    private float highy, lowy;

    private bool isup = true;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        transform.DetachChildren();
        highy = highpoint.position.y;
        lowy = lowpoint.position.y; 
        Destroy(highpoint.gameObject);
        Destroy(lowpoint.gameObject);

       
    }


    void Update()
    {
        Movement();
    }


    void Movement()
    {
        if (isup)
        {
            rb.velocity = new Vector2(rb.velocity.x, speed);

            if (transform.position.y > highy)
            {
                rb.velocity = new Vector2(rb.velocity.x, -speed);
                isup = false;
            }
        }
        else 
        {
            rb.velocity = new Vector2(rb.velocity.x, -speed);
            if(transform.position.y < lowy)
            {
                rb.velocity = new Vector2(rb.velocity.x, speed);
                isup = true;
            }
        }


    }
}
