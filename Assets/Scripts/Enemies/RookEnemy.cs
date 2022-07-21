using System.Collections;
using UnityEngine;

public class RookEnemy : Enemy
{
    bool performingMovement;
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        transform.position = new Vector2(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
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
                DestroyEnemy();
            return;
        }

        if (performingMovement)
            return;

        if (Mathf.Abs(transform.position.x - ply.transform.position.x) > Mathf.Abs(transform.position.y - ply.transform.position.y))
        {
            int numSpaces = Mathf.RoundToInt(Vector2.Distance(transform.position, new Vector2(ply.transform.position.x, transform.position.y)) / 2) * 2;
            StartCoroutine(MovePieceTowards(Mathf.Sign(ply.transform.position.x - transform.position.x), 0, numSpaces));
        }
        else
        {
            int numSpaces = Mathf.RoundToInt(Vector2.Distance(transform.position, new Vector2(transform.position.x, ply.transform.position.y)) / 2) * 2;
            StartCoroutine(MovePieceTowards(0, Mathf.Sign(ply.transform.position.y - transform.position.y), numSpaces));
        }

        movementVelocity = Vector2.MoveTowards(movementVelocity, Vector2.zero, 0.1f);
        rb.velocity = movementVelocity + knockbackVelocity;
    }

    IEnumerator MovePieceTowards(float x, float y, float distance)
    {
        performingMovement = true;
        movementVelocity = Vector2.zero;

        Vector2 startingPos = transform.position;
        while (Vector2.Distance(transform.position, startingPos) < distance)
        {
            if (movementVelocity.magnitude < stats.maxSpeed)
            {
                movementVelocity = Vector2.MoveTowards(movementVelocity, stats.maxSpeed * (Vector2.right * x + Vector2.up * y).normalized, 0.3f);
            }
            rb.velocity = movementVelocity;
            yield return new WaitForFixedUpdate();

            if (transform.position.y < -21 || transform.position.y > 25 || transform.position.x < -24 || transform.position.x > 24)
            {
                performingMovement = false;
                transform.position = new Vector2(Mathf.Clamp(transform.position.x, -24, 24), Mathf.Clamp(transform.position.y, -21, 25));
                rb.velocity = Vector2.zero;
                yield break;
            }
        }

        rb.velocity = Vector2.zero;
        transform.position = startingPos + distance * Vector2.right * x + distance * Vector2.up * y;
        yield return new WaitForSeconds(Random.Range(0.75f, 1.25f));
        performingMovement = false;
    }

    public override void UpdateAnimations()
    {

    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
            DamagePlayer();
    }

    public override void DamagePlayer()
    {
        ply.ReceiveDamage(stats.damage);
        ply.knockbackForceFromEnemies = (ply.transform.position - transform.position).normalized * stats.playerKnockbackAmount;
    }
}
