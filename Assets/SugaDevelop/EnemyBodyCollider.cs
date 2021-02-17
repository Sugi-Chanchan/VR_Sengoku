using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBodyCollider : StickColliderDynamic {

    [SerializeField] GameObject body;
    [SerializeField] GameObject deathVoiceBox;
    public override void OnCollision(CollisionInfo collisionInfo)
    {
        if (collisionInfo.collisionObject.tag != "Weapon") { return; }


        Instantiate(deathVoiceBox, transform.position,Quaternion.identity);
        Destroy(gameObject);
    }
}
