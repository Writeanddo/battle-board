using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBall : MonoBehaviour
{
    GameManager gm;
    public void Initialize(List<Transform> alreadyHitTransforms, Vector2 startPosition)
    {
        gm = FindObjectOfType<GameManager>();
        gm.PlaySFXStoppable(gm.gm_gameSfx.generalSfx[9]);
        transform.position = startPosition;
        alreadyHitTransforms.RemoveAll(item => item == null);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 4);
        for(int i = 0; i < hits.Length; i++)
        {
            if(hits[i].tag == "Enemy" && !alreadyHitTransforms.Contains(hits[i].transform))
            {
                hits[i].GetComponent<Enemy>().ReceiveDamage(1, transform.position, 0.5f);
                alreadyHitTransforms.Add(hits[i].transform);
                Instantiate(gm.gm_gameRefs.lightningChain).GetComponent<LightningChain>().Initialize(transform.position, hits[i].transform, alreadyHitTransforms);
            }
        }
    }
}
