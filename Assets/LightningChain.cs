using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningChain : MonoBehaviour
{
    SpriteRenderer spr;
    GameManager gm;

    Vector2 nextEnemyPosition;
    List<Transform> alreadyHitTransforms;

    public void Initialize(Vector2 startPos, Transform _nextEnemy, List<Transform> _alreadyHitTransforms)
    {
        spr = GetComponent<SpriteRenderer>();
        gm = FindObjectOfType<GameManager>();
        alreadyHitTransforms = _alreadyHitTransforms;
        nextEnemyPosition = _nextEnemy.position;
        Vector2 endPos = _nextEnemy.position;
        Vector2 dir = (endPos - startPos).normalized;
        transform.right = dir;
        spr.size = new Vector2(Vector2.Distance(startPos, endPos), 1);
        transform.position = (startPos + endPos) / 2;
    }

    public void SpawnLightningBall()
    {
        Instantiate(gm.gm_gameRefs.lightningBall).GetComponent<LightningBall>().Initialize(alreadyHitTransforms, nextEnemyPosition);
        Destroy(this.gameObject);
    }
}
