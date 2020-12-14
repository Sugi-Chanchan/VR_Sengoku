using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : StickColliderDynamic
{
    public Material cutSurfaceMaterial;
    public override void OnCollision(CollisionInfo collisionInfo)
    {

        Polygon polygon = collisionInfo.hitPolygon;
        Vector3 normal = polygon.normal;
        //if (normal.y < 0) { normal *= -1; }
        Vector3 hitpoint = collisionInfo.hitPoints[0];
        (var copy,var original)=MeshCut.CutMesh(this.gameObject, hitpoint, normal, true, cutSurfaceMaterial);

        if (Vector3.Dot(endPos - startPos, normal) > 0)
        {
            copy.GetComponent<StickColliderDynamic>().startPos = hitpoint;
            endPos = hitpoint;
            
        }
        else
        {
            startPos = hitpoint;
            copy.GetComponent<StickColliderDynamic>().endPos = hitpoint;
        }
    }
   
   
}
