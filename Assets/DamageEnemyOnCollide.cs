using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEnemyOnCollide : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
            collision.GetComponent<Enemy>().ReceiveDamage(1, transform.position, 1f);
    }
}
