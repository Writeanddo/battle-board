using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DynamicSpriteSort : MonoBehaviour
{
    public bool overrideSort;
    public bool useParentPosition;
    public Transform positionReference;
    public int sortMultiplier = 1;
    public float yOffset = 0;
    public int minLayer = -1000;
    public int maxLayer = 1000;

    Transform player;
    SpriteRenderer spr;
    TilemapRenderer tile;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>().transform;
        spr = GetComponent<SpriteRenderer>();
        tile = GetComponent<TilemapRenderer>();

        if(minLayer == 0 && maxLayer == 0)
        {
            minLayer = -1000;
            maxLayer = 1000;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!overrideSort)
        {
            int sort = 0;
            float yPos;
            if (useParentPosition)
                yPos = transform.parent.position.y;
            else if(positionReference != null)
                yPos = positionReference.position.y;
            else
                yPos = transform.position.y;
            sort = Mathf.Clamp(Mathf.RoundToInt((player.transform.position.y - yPos + yOffset) * 10) * sortMultiplier, minLayer, maxLayer);

            if (spr != null)
                spr.sortingOrder = sort;
            else if (tile != null)
                tile.sortingOrder = sort;
        }
    }
}
