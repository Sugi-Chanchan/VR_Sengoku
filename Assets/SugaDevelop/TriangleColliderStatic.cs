using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleColliderStatic : PolygonCollider
{
    [SerializeField] Transform t0, t1, t2;

    Polygon[] polygons=new Polygon[1] {new Polygon()};
    protected override Polygon[] SetPolygons()
    {
        polygons[0].Set(t0.position, t1.position, t2.position);
        return polygons;
    }

    public override void OnCollision(ColliderInfo colliderInfo)
    {
        MyDebug.PutPoint("hitpoint1", colliderInfo.hitPoints[0]);
        MyDebug.PutPoint("hitpoint2", colliderInfo.hitPoints[1]);
    }
}
