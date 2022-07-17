using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15;
    public int damage = 4;

    bool initialized = false;
    bool hasHit;

    Rigidbody2D rb;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Initialize(Vector2 dir)
    {
        gm = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = dir * speed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && !hasHit)
        {
            hasHit = true;
            collision.GetComponent<PlayerController>().ReceiveDamage(damage);
            Destroy(this.gameObject);
        }
    }
}
