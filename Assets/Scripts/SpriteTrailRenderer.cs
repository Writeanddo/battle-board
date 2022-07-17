// Retrieved from https://forum.unity.com/threads/creating-a-trail-of-sprites-getting-current-sprite-in-animation.251629/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTrailRenderer : MonoBehaviour
{
    public bool spawnClones = true;
    public int ClonesPerSecond = 10;
    public float colorDecayMultiplier = 5;
    private SpriteRenderer sr;
    private Transform tf;
    private List<SpriteRenderer> clones;
    public Color colorPerSecond = new Color(255, 255, 255, 1f);
    void Start()
    {
        tf = GetComponent<Transform>();
        sr = GetComponent<SpriteRenderer>();
        clones = new List<SpriteRenderer>();
        StartTrailCoroutine();
    }

    private void OnEnable()
    {

    }

    void FixedUpdate()
    {
        for (int i = 0; i < clones.Count; i++)
        {
            clones[i].color -= colorPerSecond * Time.deltaTime * colorDecayMultiplier;
            clones[i].sortingOrder = sr.sortingOrder - 1;
            if (clones[i].color.a <= 0f || clones[i].transform.localScale == Vector3.zero)
            {
                Destroy(clones[i].gameObject);
                clones.RemoveAt(i);
                i--;
            }
        }
    }

    private void OnDestroy()
    {
        foreach (SpriteRenderer g in clones)
        {
            if(g != null)
                Destroy(g.gameObject);
        }
    }

    public void StartTrailCoroutine()
    {
        StartCoroutine(trail());
    }

    IEnumerator trail()
    {
        while (true)
        {
            if (spawnClones)
            {
                var clone = new GameObject("trailClone");
                clone.transform.position = tf.position;
                clone.transform.rotation = tf.rotation;

                var cloneRend = clone.AddComponent<SpriteRenderer>();
                cloneRend.sprite = sr.sprite;
                cloneRend.flipX = sr.flipX;
                cloneRend.sortingOrder = sr.sortingOrder - 1;
                clones.Add(cloneRend);
            }
            yield return new WaitForSeconds(1f / ClonesPerSecond);
        }
    }
}