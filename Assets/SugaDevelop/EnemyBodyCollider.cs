using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBodyCollider : StickColliderDynamic {

    [SerializeField] GameObject body;
    public override void OnCollision(CollisionInfo collisionInfo)
    {
        if (collisionInfo.collisionObject.tag != "Weapon") { return; }

        var normal = collisionInfo.hitPolygon.normal;
        if (normal.y < 0) normal *=-1;

        Destroy(gameObject);
    }
}
