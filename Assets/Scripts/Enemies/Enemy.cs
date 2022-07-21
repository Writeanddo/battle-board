using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [System.Serializable]
    public class EnemyStats
    {
        public int health = 30;
        [HideInInspector]
        public int maxHealth;
        public float maxSpeed;
        public int damage;
        public float playerKnockbackAmount;
        public GameObject projectile;
        public bool invincible;
        public bool bigEnemy;
    }

    public EnemyStats stats;
    public bool isDead;

    protected GameManager gm;
    protected Animator anim;
    protected SpriteRenderer spr;
    protected Rigidbody2D rb;
    protected PlayerController ply;
    protected StatManager sm;

    public Vector2 knockbackVelocity;
    protected Vector2 movementVelocity;
    protected Vector2 additionalVelocity;
    int currentNode;

    // Start is called before the first frame update
    void Start()
    {
        stats.maxHealth = stats.health;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = movementVelocity + knockbackVelocity;
        SlowKnockbackVelocity();
    }

    protected void GetReferences()
    {
        ply = FindObjectOfType<PlayerController>();
        spr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        anim = spr.GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
        sm = FindObjectOfType<StatManager>();
    }

    public abstract void UpdateMovement();
    public abstract void UpdateAnimations();

    public abstract void DamagePlayer();

    protected void LookAtPlayer()
    {
        spr.flipX = ply.transform.position.x < transform.position.x;
    }

    protected void SlowKnockbackVelocity()
    {
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, 0.5f);
    }

    public void ReceiveDamage(int damage, Vector3 hitPosition, float knockbackMultiplier)
    {
        StartCoroutine(ReceiveDamageCoroutine(damage, hitPosition, knockbackMultiplier));
    }

    IEnumerator ReceiveDamageCoroutine(int damage, Vector3 hitPosition, float knockbackMultiplier)
    {
        if (gm == null || stats.invincible)
            yield break;

        spr.color = Color.red;
        float shakeAmount = 1;
        stats.health -= damage;

        Vector2 dir = (transform.position - hitPosition).normalized;
        Instantiate(gm.gm_gameRefs.hitNumber, transform.position + Vector3.up, Quaternion.identity).GetComponent<HitNumberScript>().Initialize(Mathf.Clamp(damage, 1, 6));

        if (stats.health <= 0)
            StartCoroutine(Die(dir * knockbackMultiplier));
        else
            knockbackVelocity = dir * 10 * knockbackMultiplier;

        while (spr.color.g < 0.9f)
        {
            spr.color = Color.Lerp(spr.color, Color.white, 0.1f);
            spr.transform.localPosition = new Vector2(Random.Range(-shakeAmount, shakeAmount), Random.Range(-shakeAmount, shakeAmount));
            shakeAmount /= 1.5f;
            yield return new WaitForFixedUpdate();
        }
        spr.transform.localPosition = Vector2.zero;
    }

    IEnumerator Die(Vector2 dir)
    {
        if (isDead)
            yield break;

        knockbackVelocity = dir * 15;
        GetComponent<Collider2D>().enabled = false;
        isDead = true;
        yield return null;
    }

    protected void DestroyEnemy()
    {
        if (stats.bigEnemy || sm.currentUltraStat.info.id.Contains("goodstuff_bigboom"))
            Instantiate(gm.gm_gameRefs.bigExplosion, transform.position, Quaternion.identity);
        else
            Instantiate(gm.gm_gameRefs.smokeClouds[0], transform.position, Quaternion.identity);

        gm.gm_gameVars.kills++;
        Destroy(this.gameObject);
    }

    /*public void FollowPath()
    {
        if (currentNode == -1)
            currentNode = GetNearestNode();

        Vector2 dir = Vector2.zero;

        // Move towards nearest node
        if (waves.enemyPath.Length > 0)
            dir = ((Vector2)waves.enemyPath[currentNode].transform.position - (Vector2)transform.position).normalized;

        rb.velocity = Vector2.Lerp(rb.velocity, dir * randSpeedMultiplier * stats.pathSpeedMultiplier, stats.movementAccuracy);
        if (stats.useDirctionalAnimation)
            CheckAndPlayClip(stats.animationPrefix + "_Walk" + GetCompassPointFromAngle(AngleBetween(waves.enemyPath[currentNode].position)));

        if (Vector2.Distance(transform.position, waves.enemyPath[currentNode].position) < 0.5f)
            currentNode = GetNextNode();
    }

    int GetNearestNode()
    {
        int closestNode = 0;
        float storedLength = 1000;

        // Decide which way along the path we'll move
        pathDirection = 1;
        if (!waves.dontRandomizeDirection)
        {
            int pathDir = Random.Range(0, 2);
            switch (pathDir)
            {
                case (0):
                    pathDirection = -1;
                    break;
                case (1):
                    pathDirection = 1;
                    break;
            }
        }

        // Raycast to all nodes
        for (int i = 0; i < waves.enemyPath.Length; i++)
        {
            Vector2 dir = (waves.enemyPath[i].position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 50);
            if (hit && storedLength > Vector2.Distance(transform.position, waves.enemyPath[i].position))
            {
                closestNode = i;
                storedLength = Vector2.Distance(transform.position, waves.enemyPath[i].position);
            }
        }
        return closestNode;
    }

    // Returns index of next node on path
    int GetNextNode()
    {
        if ((currentNode == 0 && pathDirection == -1) || (currentNode == waves.enemyPath.Length - 1 && pathDirection == 1))
        {
            if (waves.useLoopPath)
            {
                if (currentNode == 0 && pathDirection == -1)
                    currentNode = waves.enemyPath.Length - 1;
                else
                    currentNode = 0;

                return currentNode;
            }
            else
                pathDirection *= -1;
        }

        return currentNode + pathDirection;
    }*/
}
