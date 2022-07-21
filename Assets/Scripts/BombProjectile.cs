using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    bool initialized = false;
    public GameObject explosionProjectile;
    public GameObject bulletProjectile;
    public float speed = 25;

    bool hasHit;
    bool hasSplit;

    List<PlayerController.BulletType> types;

    Rigidbody2D rb;
    GameManager gm;
    Animator anim;
    Transform previouslyHitEnemy;
    PlayerController ply;
    Transform targetedEnemy;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
    }

    public void Initialize(Vector2 dir, List<PlayerController.BulletType> bulletTypes, Transform previousHit)
    {
        previouslyHitEnemy = previousHit;
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = dir * speed;
        types = bulletTypes;

        // Bigshot
        if (types.Contains(PlayerController.BulletType.bigshot))
            anim.Play("DiceBullet_Big");

        StartCoroutine(WaitAndExplode());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Homing
        if (types.Contains(PlayerController.BulletType.homing))
        {
            if (targetedEnemy == null)
            {
                print("Scanning");
                float currentDistance = 100;
                Transform nearest = null;
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 6);
                for(int i = 0; i < hits.Length; i++)
                {
                    float nextDistance = Vector2.Distance(hits[i].transform.position, transform.position);
                    if (hits[i].tag == "Enemy" && (nearest == null || nextDistance < currentDistance))
                    {
                        nearest = hits[i].transform;
                        currentDistance = nextDistance;
                    }
                }
                targetedEnemy = nearest;
            }
            else
            {
                rb.velocity = Vector2.MoveTowards(rb.velocity, (targetedEnemy.position - transform.position).normalized*speed, 3f);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy" && !hasHit && collision.transform != previouslyHitEnemy)
        {
            gm.PlaySFXStoppable(gm.gm_gameSfx.generalSfx[Random.Range(4, 8)]);

            int damage = Random.Range(1, 7);
            damage = Mathf.RoundToInt(damage * ply.buffs.damageMultiplier);

            if (types.Contains(PlayerController.BulletType.crit))
            {
                gm.PlaySFXStoppable(gm.gm_gameSfx.generalSfx[3]);
                damage *= 2;
            }

            float knockbackMultiplier = 1;
            if (types.Contains(PlayerController.BulletType.homerun))
                knockbackMultiplier = 2.5f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.8f);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i] != null && hits[i].tag == "Enemy")
                {
                    hits[i].GetComponent<Enemy>().ReceiveDamage(damage, transform.position, knockbackMultiplier);

                    // Chain Lightning
                    if (types.Contains(PlayerController.BulletType.lightning))
                        Instantiate(gm.gm_gameRefs.lightningBall, hits[i].transform.position, Quaternion.identity).GetComponent<LightningBall>().Initialize(new List<Transform>(), hits[i].transform.position);

                    // Splitshot
                    if (types.Contains(PlayerController.BulletType.splitshot) && !hasSplit)
                    {
                        int splitSize = 3;
                        List<PlayerController.BulletType> newType = new List<PlayerController.BulletType>();
                        if (types.Contains(PlayerController.BulletType.bigshot))
                            newType.Add(PlayerController.BulletType.bigshot);
                        if (types.Contains(PlayerController.BulletType.bigBoom))
                            newType.Add(PlayerController.BulletType.bigBoom);

                        if (types.Contains(PlayerController.BulletType.spreadshot))
                            splitSize = 1;
                        for (int j = 0; j < splitSize; j++)
                            Instantiate(bulletProjectile, transform.position, Quaternion.identity).GetComponent<BombProjectile>().Initialize(new Vector2(Random.Range(-1, 1f), Random.Range(-1, 1f)).normalized, newType, hits[i].transform);
                        hasSplit = true;
                    }

                }
            }
            Explode();
        }
    }

    IEnumerator WaitAndExplode()
    {
        //while (rb.velocity.magnitude > 0)
        //yield return null;

        yield return new WaitForSeconds(1f);
        Destroy(this.gameObject);
    }

    public void Explode()
    {
        if (hasHit)
            return;

        hasHit = true;
        gm.ScreenShake();

        if (types.Contains(PlayerController.BulletType.bigBoom))
            Instantiate(gm.gm_gameRefs.bigExplosion, transform.position, Quaternion.identity);
        else
            Instantiate(explosionProjectile, transform.position, Quaternion.identity);

        Destroy(this.gameObject);
    }
}
