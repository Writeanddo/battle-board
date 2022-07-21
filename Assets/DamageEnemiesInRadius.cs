using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageEnemiesInRadius : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].tag == "Enemy")
                hits[i].GetComponent<Enemy>().ReceiveDamage(Random.Range(1, 7), transform.position, 1.5f);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}