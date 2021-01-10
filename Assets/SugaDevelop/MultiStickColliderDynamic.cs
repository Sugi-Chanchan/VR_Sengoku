using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiStickColliderDynamic : PolygonCollider
{
    [SerializeField] ColliderLine[] colliderLines;
    Polygon[] polygons;

    [System.Serializable]
    class ColliderLine
    {
        public Transform start, end;
        [HideInInspector]public Vector3 preStart,preEnd;
    }

    protected override void OnEnableCollision()
    {
        base.OnEnableCollision();
        polygons = new Polygon[colliderLines.Length*2];
        for(int i = 0; i < polygons.Length; i++)
        {
            polygons[i] = new Polygon();
        }
    }

    protected override Polygon[] SetPolygons()
    {
        for (int i = 0; i < colliderLines.Length; i++)
        {
            ColliderLine colliderLine = colliderLines[i];
            Vector3 startPos = colliderLine.start.position;
            Vector3 endPos = colliderLine.end.position;
            Vector3 preStartPos = colliderLine.preStart;
            Vector3 preEndPos = colliderLine.preEnd;

            polygons[2*i].Set(preStartPos, preEndPos, startPos);
            polygons[2 * i + 1].Set(startPos, endPos, preEndPos);
        }

        return polygons;
    }

    protected override void SendCollisionData()
    {
        base.SendCollisionData();
        SetPrePos();
    }


    void SetPrePos()
    {
        for(int i = 0; i < colliderLines.Length; i++)
        {
            ColliderLine colliderLine = colliderLines[i];
            colliderLine.preStart = colliderLine.start.position;
            colliderLine.preEnd = colliderLine.end.position;
        }
    }
}


