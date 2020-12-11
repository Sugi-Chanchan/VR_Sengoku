using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tatamigiri : StickCollider
{
    public Material cutSurfaceMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnCollision(ColliderInfo colliderInfo)
    {
        Polygon polygon = colliderInfo.hitPolygon;
        //Vector3 normal = Vector3.Cross(polygon.vertices[1] - polygon.vertices[0], polygon.vertices[2] - polygon.vertices[0]);
        Vector3 normal = polygon.normal;
        if(normal.y > 0)
        {
            normal *= -1;
        }

        GameObject[] fragments = MeshCut.CutMesh(this.gameObject, colliderInfo.hitPoints[0], normal, true, cutSurfaceMaterial);
        fragments[1].AddComponent<Rigidbody>();
    }
}
