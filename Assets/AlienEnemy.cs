using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienEnemy : Enemy
{
    Vector2 dir;
    SpriteRenderer gunSprite;
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        gunSprite = spr.transform.GetChild(0).GetComponent<SpriteRenderer>();
        StartCoroutine(GetRandomDirection());
        StartCoroutine(RandomShooting());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateMovement();
        UpdateAnimations();
        SlowKnockbackVelocity();
    }

    public override void UpdateMovement()
    {
        if (isDead)
        {
            //rb.velocity = knockbackVelocity;
            // if (rb.velocity == Vector2.zero)
            DestroyEnemy();
            return;
        }

        if (movementVelocity.magnitude < stats.maxSpeed)
        {
            movementVelocity = Vector2.MoveTowards(movementVelocity, dir * stats.maxSpeed, 0.25f);
        }
        movementVelocity = Vector2.MoveTowards(movementVelocity, Vector2.zero, 0.1f);
        rb.velocity = movementVelocity + knockbackVelocity;
    }

    IEnumerator RandomShooting()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 8f));
            Vector2 bulletDir = ((ply.transform.position - transform.position).normalized + Vector3.Cross(dir, -Vector3.forward).normalized * Random.Range(-0.5f, 0.5f)).normalized;
            Projectile p = Instantiate(stats.projectile, transform.position, Quaternion.identity).GetComponent<Projectile>();
            p.Initialize(bulletDir);
            p.transform.right = bulletDir;
        }
    }

    IEnumerator GetRandomDirection()
    {
        while (true)
        {
            dir = (ply.transform.position - transform.position).normalized;
            dir = (dir + (Vector2)Vector3.Cross(dir, -Vector3.forward).normalized * Random.Range(-1f, 1f) * 4).normalized;
            yield return new WaitForSeconds(Random.Range(1.5f, 4f));
        }
    }

    public override void UpdateAnimations()
    {
        spr.flipX = rb.velocity.x < 0;
        gunSprite.flipX = spr.flipX;

        LookAtPlayer();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" || collision.tag == "Player")
        {
            knockbackVelocity = (transform.position - collision.transform.position).normalized * 8;
            if (collision.tag == "Player")
                DamagePlayer();
        }
    }

    public override void DamagePlayer()
    {
        ply.ReceiveDamage(stats.damage);
        ply.knockbackForceFromEnemies = (ply.transform.position - transform.position).normalized * stats.playerKnockbackAmount;
    }
}
