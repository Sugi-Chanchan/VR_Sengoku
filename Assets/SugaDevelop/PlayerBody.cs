using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBody : StickColliderDynamic
{
    

    public override void OnCollision(CollisionInfo collisionInfo)
    {
        if (collisionInfo.collisionObject.tag == "EnemyWeapon")
        {

            print("You Lose!");
        }
    }
}
