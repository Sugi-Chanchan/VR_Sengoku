using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultiSticksColliderDynamic : PolygonCollider 
{
    StickLocalPos[] sticksLocalPos;
    PrePos[] prePos;
    private Polygon[] polygons;
    

    public void SetCollider(StickLocalPos[] localPos)
    {
        sticksLocalPos = localPos;
        prePos = new PrePos[localPos.Length];

        

        polygons= new Polygon[localPos.Length*2];
        for(int i = 0; i < polygons.Length; i++)
        {
            polygons[i] = new Polygon();
        }
    }

    protected override void OnEnableCollision()
    {

        for(int i = 0; i < sticksLocalPos.Length; i++)
        {
            prePos[i].start = transform.position + sticksLocalPos[i].start;
            prePos[i].end = transform.position + sticksLocalPos[i].end;
        }
    }


    
    protected override Polygon[] SetPolygons()
    {
        for(int i = 0; i < sticksLocalPos.Length; i++)
        {
            polygons[2*i].Set(
                prePos[i].start, 
                prePos[i].end, 
                transform.position + sticksLocalPos[i].start);
            polygons[2 * i+1].Set(
                transform.position + sticksLocalPos[i].start,
                transform.position + sticksLocalPos[i].end,
                prePos[i].end);
        }

        return polygons;
    }


    protected override void SendCollisionData()
    {
        base.SendCollisionData();

        for(int i = 0; i < sticksLocalPos.Length; i++)
        {
            prePos[i].start = transform.position + sticksLocalPos[i].start;
            prePos[i].end = transform.position + sticksLocalPos[i].end;
        }
    }


    public struct StickLocalPos
    {
        public StickLocalPos(Vector3 _start,Vector3 _end)
        {
            start = _start;
            end = _end;
        }

        public Vector3 start;
        public Vector3 end;
    }
    struct PrePos
    {
        public Vector3 start, end;
    }
}
