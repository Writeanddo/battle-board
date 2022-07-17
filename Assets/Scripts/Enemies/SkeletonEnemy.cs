using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonEnemy : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        if (sm.EnemyHasStat("badstuff_speeddemon"))
            stats.maxSpeed *= 2;
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
            rb.velocity = knockbackVelocity;
            if (rb.velocity == Vector2.zero)
                DestroyEnemy();
            return;
        }

        Vector2 dir = (ply.transform.position - transform.position).normalized;
        if (movementVelocity.magnitude < stats.maxSpeed)
        {
            movementVelocity = Vector2.MoveTowards(movementVelocity, dir * stats.maxSpeed, 0.25f);
        }
        movementVelocity = Vector2.MoveTowards(movementVelocity, Vector2.zero, 0.1f);
        rb.velocity = movementVelocity + knockbackVelocity;
    }

    public override void UpdateAnimations()
    {
        if (isDead)
        {
            anim.SetFloat("WalkSpeed", 0);
            return;
        }

        anim.SetFloat("WalkSpeed", rb.velocity.magnitude / 3);
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
