using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RandomizedDiceDisplayer : MonoBehaviour
{
    Image icon;
    int lastRandSpriteIndex = -1;

    void Start()
    {
        icon = GetComponent<Image>();
    }

    public void DisplaySingleIcon(Sprite sp)
    {
        StopAllCoroutines();
        icon.sprite = sp;

    }

    public void DisplayRandomIcons(StatManager.StatInfo[] stats)
    {
        Sprite[] sprites = new Sprite[stats.Length];
        for (int i = 0; i < stats.Length; i++)
            sprites[i] = stats[i].icon;

        StartCoroutine(CycleRandomIcons(sprites));
    }

    IEnumerator CycleRandomIcons(Sprite[] sprites)
    {
        int rand = 0;
        while(true)
        {
            rand = Random.Range(0, sprites.Length);
            icon.sprite = sprites[rand];
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
        }
    }
}
