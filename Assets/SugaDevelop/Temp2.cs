using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Temp2 : MultiSticksColliderDynamic
{
    private void Start()
    {
        SetCollider(new StickLocalPos[3] {
            new StickLocalPos(Vector3.down,Vector3.up),
            new StickLocalPos(Vector3.left,Vector3.right),
            new StickLocalPos(Vector3.back,Vector3.forward)});
    }

    public override void OnCollision(CollisionInfo collisionInfo)
    {
        print("hit");
    }
}
