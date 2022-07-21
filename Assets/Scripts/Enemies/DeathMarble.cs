using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathMarble : Enemy
{
    bool canMove = false;

    Vector2 dir;
    Transform cam;
    WaveManager wm;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        transform.position = ply.transform.position + Random.Range(4, 8f) * Vector3.right + Random.Range(-6, 6f) * Vector3.up;
        StartCoroutine(FallOntoBoard());
    }

    protected void GetReferences()
    {
        wm = FindObjectOfType<WaveManager>();
        cam = GameObject.Find("CameraHolder").transform;
        ply = FindObjectOfType<PlayerController>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
        spr = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        sm = FindObjectOfType<StatManager>();
    }

    void FixedUpdate()
    {
        UpdateMovement();

        // Die if this is the last enemy
        if(wm.AllEnemiesInvincible() && wm.enemiesLeft == 0)
        {
            DestroyEnemy();
        }
    }

    IEnumerator FallOntoBoard()
    {
        yield return new WaitForSeconds(0.5f);
        anim.Play("Marble_Appear", 0, 0);
        yield return new WaitForSeconds(0.75f);
        dir = new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f)).normalized;
        canMove = true;
    }

    public override void UpdateMovement()
    {
        if (!canMove)
            return;
        if (movementVelocity.magnitude < stats.maxSpeed)
        {
            movementVelocity = Vector2.MoveTowards(movementVelocity, dir * stats.maxSpeed, 0.25f);
        }

        rb.velocity = movementVelocity;
        spr.transform.eulerAngles -= new Vector3(0, 0, rb.velocity.magnitude * Mathf.Sign(rb.velocity.x)) * 1.5f;

        if (transform.position.x - cam.transform.position.x <= -15)
        {
            transform.position = new Vector2(cam.transform.position.x - 15, transform.position.y);
            dir = new Vector2(Mathf.Abs(dir.x), dir.y);
        }
        if (transform.position.x - cam.transform.position.x >= 15)
        {
            transform.position = new Vector2(cam.transform.position.x + 15, transform.position.y);
            dir = new Vector2(-Mathf.Abs(dir.x), dir.y);
        }
        if (transform.position.y - cam.transform.position.y <= -10.25f)
        {
            transform.position = new Vector2(transform.position.x, cam.transform.position.y - 10.25f);
            dir = new Vector2(dir.x, Mathf.Abs(dir.y));
        }
        if (transform.position.y - cam.transform.position.y >= 10.25f)
        {
            transform.position = new Vector2(transform.position.x, cam.transform.position.y + 10.25f);
            dir = new Vector2(dir.x, -Mathf.Abs(dir.y));
        }

        movementVelocity = dir * rb.velocity.magnitude;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" || collision.tag == "Player" || collision.tag == "Wall")
        {
            dir = (transform.position - (Vector3)collision.ClosestPoint(transform.position)).normalized;
            if (collision.tag == "Player")
                DamagePlayer();
        }
    }

    public override void DamagePlayer()
    {
        ply.ReceiveDamage(stats.damage);
        ply.knockbackForceFromEnemies = (ply.transform.position - transform.position).normalized * stats.playerKnockbackAmount;
    }

    public override void UpdateAnimations()
    {
        throw new System.NotImplementedException();
    }
}
