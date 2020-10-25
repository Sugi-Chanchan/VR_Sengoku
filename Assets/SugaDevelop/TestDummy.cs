using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : CollisionObject {


    void Awake()
    {
        Application.targetFrameRate = 160;
    }

    protected override CollisionManager.ColliderType ColliderType => CollisionManager.ColliderType.PlayerBody;
    [SerializeField] Transform start, end;
    // Start is called before the first frame update
    void Start()
    {
        SetCollider();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        CheckCollision();
    }

    public override void OnCollision()
    {
        Debug.Break();
    }

    protected override void SetCollider()
    {
        AddHitLine(start, end);
    }
}
