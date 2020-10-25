using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd2 : CollisionObject
{
    protected override CollisionManager.ColliderType ColliderType { get => CollisionManager.ColliderType.EnemyWeapon; }
    [SerializeField] Transform start, end;
    // Start is called before the first frame update
    void Start()
    {
        if (start == null || end == null) { Debug.LogError("Nullllllllll"); }
        SetCollider();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        CheckCollision();    
    }

    public override void OnCollision()
    {
        print("hit");
    }
    protected override void SetCollider()
    {
        AddHitLine(start,end);
    }
}
