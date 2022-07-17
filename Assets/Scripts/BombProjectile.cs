using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombProjectile : MonoBehaviour
{
    bool initialized = false;
    public GameObject explosionProjectile;
    public float speed = 25;

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
        StartCoroutine(WaitAndExplode());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rb.velocity = Vector2.MoveTowards(rb.velocity, Vector2.zero, 0.2f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag != "Player" && !hasHit)
        {
            gm.PlaySFX(gm.gm_gameSfx.generalSfx[Random.Range(4, 8)]);
            int damage = Random.Range(1, 7);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.8f);
            for(int i = 0; i < hits.Length; i++)
            {
                if(hits[i] != null && hits[i].tag == "Enemy")
                    hits[i].GetComponent<Enemy>().ReceiveDamage(damage, transform.position);
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
        Instantiate(explosionProjectile, transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }
}
