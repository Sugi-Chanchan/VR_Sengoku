﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cuttable : StickCollider
{
    public Material cutSurfaceMaterial;
    public override void OnCollision(ColliderInfo collisionInfo)
    {

        Polygon polygon = collisionInfo.hitPolygon;
        Vector3 normal = Vector3.Cross(polygon.vertices[1] - polygon.vertices[0], polygon.vertices[2] - polygon.vertices[0]);
        if (normal.y < 0) { normal *= -1; }

        GameObject[] fragments = MeshCut.CutMesh(this.gameObject, collisionInfo.hitPoint, normal, true, cutSurfaceMaterial);

    }
}
