using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitNumberScript : MonoBehaviour
{
    public Sprite[] numbers;
    SpriteRenderer spr;

    public void Initialize(int value)
    {
        spr = transform.GetChild(0).GetComponent<SpriteRenderer>();
        spr.sprite = numbers[value - 1];
    }
}
