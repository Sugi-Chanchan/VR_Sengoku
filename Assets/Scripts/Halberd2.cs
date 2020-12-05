using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd2 : SwordCollider
{
    public override CollisionManager.ColliderType ColliderType { get => CollisionManager.ColliderType.PlayerWeapon; }
    // Start is called before the first frame update
    // Update is called once per frame\




    public override void OnCollision(CollisionInfo collision)
    {
        if (collision.colliderType == CollisionManager.ColliderType.PlayerBody)
        {
            //print(+collision.hitpoint.x + " , " + collision.hitpoint.y + " , " + collision.hitpoint.z);
        }
    }
}
