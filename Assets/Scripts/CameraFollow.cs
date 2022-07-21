using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Vector2 mouseOffset;
    Vector2 mousePos;
    PlayerController ply;
    WaveManager wm;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void GetRefs()
    {
        ply = FindObjectOfType<PlayerController>();
        wm = FindObjectOfType<WaveManager>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (ply == null)
            return;

        Vector2 mousePos = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)/ 20;

        if (wm != null && wm.inBattle)
            mouseOffset = Vector2.Lerp(mouseOffset, mousePos * 5, 0.25f);
        else
            mouseOffset = Vector2.Lerp(mouseOffset, Vector2.zero, 0.05f);

        transform.position = ply.transform.position + (Vector3)mouseOffset + Vector3.back * 10;
    }
}
