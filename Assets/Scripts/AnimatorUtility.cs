using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorUtility : MonoBehaviour
{
    public bool gameManagerIsTarget;
    public GameObject sendMessageTarget;
    public AudioClip[] sfx;

    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        if (gameManagerIsTarget)
            sendMessageTarget = gm.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendMessageToTarget(string message)
    {
        sendMessageTarget.SendMessage(message, SendMessageOptions.DontRequireReceiver);
    }

    public void PlaySFX(int index)
    {
        gm.PlaySFX(sfx[index]);
    }

    public void PlayRandomSFX()
    {
        gm.PlaySFX(sfx[Random.Range(0, sfx.Length)]);
    }

    public void SelfDestruct()
    {
        Destroy(this.gameObject);
    }
}
