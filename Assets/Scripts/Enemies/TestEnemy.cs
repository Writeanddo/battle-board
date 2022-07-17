using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemy : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void UpdateMovement()
    {
        throw new System.NotImplementedException();
    }

    public override void UpdateAnimations()
    {
        throw new System.NotImplementedException();
    }

    public override void DamagePlayer()
    {
        throw new System.NotImplementedException();
    }


}
