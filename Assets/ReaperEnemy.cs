using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaperEnemy : Enemy
{
    WaveManager wm;
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        wm = FindObjectOfType<WaveManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateMovement();
        UpdateAnimations();

        // Die if this is the last enemy
        if (wm.AllEnemiesInvincible() && wm.enemiesLeft == 0)
        {
            DestroyEnemy();
        }
    }

    public override void UpdateMovement()
    {
        Vector2 dir = (ply.transform.position - transform.position).normalized;
        if (movementVelocity.magnitude < stats.maxSpeed)
        {
            movementVelocity = Vector2.MoveTowards(movementVelocity, dir * stats.maxSpeed, 0.15f);
        }
        movementVelocity = Vector2.MoveTowards(movementVelocity, Vector2.zero, 0.1f);
        rb.velocity = movementVelocity + knockbackVelocity;
    }

    public override void UpdateAnimations()
    {
        LookAtPlayer();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            DamagePlayer();
        }
    }

    IEnumerator RerollPlayerStat()
    {
        yield return null;
        DestroyEnemy();
    }

    public override void DamagePlayer()
    {
        ply.ReceiveDamage(stats.damage);
        ply.knockbackForceFromEnemies = (ply.transform.position - transform.position).normalized * stats.playerKnockbackAmount;
    }
}
