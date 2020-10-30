using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd2 : CollisionObject
{
    public override CollisionManager.ColliderType ColliderType { get => CollisionManager.ColliderType.EnemyWeapon; }
    [SerializeField] Transform start, end;
    protected override List<Transform[]> LinesOfTrabsform => new List<Transform[]> { new Transform[2] { start, end } };
    // Start is called before the first frame update
    // Update is called once per frame\

    protected override void StartOfCollisionInstance()
    {

    }

    protected override void UpdateOfCollisionInstance()
    {
        CheckCollision();
    }

    public override void OnCollision(CollisionInfo collision)
    {
        if (collision.colliderType == CollisionManager.ColliderType.PlayerBody)
        {
            //print(+collision.hitpoint.x + " , " + collision.hitpoint.y + " , " + collision.hitpoint.z);
        }
    }
}
