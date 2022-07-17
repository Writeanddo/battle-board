using System.Collections;
using UnityEngine;

public class QueenEnemy : Enemy
{
    bool performingMovement;
    CapsuleCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
    }

    void GetReferences()
    {
        col = GetComponent<CapsuleCollider2D>();
        ply = FindObjectOfType<PlayerController>();
        spr = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        gm = FindObjectOfType<GameManager>();
    }

    void FixedUpdate()
    {
        UpdateMovement();
        SlowKnockbackVelocity();
    }

    public override void UpdateMovement()
    {
        if (isDead)
        {
            //rb.velocity = knockbackVelocity;
            if (rb.velocity == Vector2.zero)
                DestroyEnemy();
            return;
        }

        if (performingMovement)
            return;

        int xDir = 1;
        int yDir = 1;
        float xDistance = Mathf.Abs(ply.transform.position.x - transform.position.x);
        float yDistance = Mathf.Abs(ply.transform.position.y - transform.position.y);

        if (ply.transform.position.x < transform.position.x)
            xDir = -1;
        if (ply.transform.position.y < transform.position.y)
            yDir = -1;

        float ratio = xDistance / yDistance;
        if (ratio >= 2)
            yDir = 0;
        else if (ratio <= 0.5f)
            xDir = 0;


        int numSpaces = Mathf.RoundToInt(Vector2.Distance(transform.position, ply.transform.position) / 2) * 2 + 4;
        StartCoroutine(MovePieceTowards(xDir, yDir, numSpaces));
    }

    IEnumerator MovePieceTowards(float x, float y, float distance)
    {
        performingMovement = true;
        movementVelocity = Vector2.zero;

        float adjustedDistance = ((Vector2.right * x + Vector2.up * y) * distance).magnitude;

        Vector2 startingPos = transform.position;
        while (Vector2.Distance(transform.position, startingPos) < adjustedDistance)
        {
            if (movementVelocity.magnitude < stats.maxSpeed)
            {
                movementVelocity = Vector2.MoveTowards(movementVelocity, stats.maxSpeed * (Vector2.right * x + Vector2.up * y), 0.3f);
            }
            rb.velocity = movementVelocity;
            yield return new WaitForFixedUpdate();
        }

        rb.velocity = Vector2.zero;
        transform.position = startingPos + (Vector2.right * x + Vector2.up * y) * distance;
        yield return new WaitForSeconds(Random.Range(0.25f, 0.5f));

        // Attack
        col.enabled = false;
        anim.Play("Queen_Slam", 0, 0);
        yield return new WaitForSeconds(3.25f);
        col.enabled = true;
        performingMovement = false;
    }

    public override void UpdateAnimations()
    {

    }

    public void Shake()
    {
        gm.ScreenShake();
    }

    public void ShootProjectiles()
    {
        Vector2[] directions = new Vector2[8];
        directions[0] = new Vector2(1, 1);
        directions[1] = new Vector2(1, -1);
        directions[2] = new Vector2(-1, -1);
        directions[3] = new Vector2(-1, 1);
        directions[4] = new Vector2(1, 0);
        directions[5] = new Vector2(-1, 0);
        directions[6] = new Vector2(0, -1);
        directions[7] = new Vector2(0, 1);

        for (int i = 0; i < 8; i++)
            Instantiate(stats.projectile, transform.position, Quaternion.identity).GetComponent<Projectile>().Initialize(directions[i]);

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled)
            return;
        if (collision.tag == "Player")
            DamagePlayer();
    }

    public override void DamagePlayer()
    {
        ply.ReceiveDamage(stats.damage);
        ply.knockbackForceFromEnemies = (ply.transform.position - transform.position).normalized * stats.playerKnockbackAmount;
    }
}
