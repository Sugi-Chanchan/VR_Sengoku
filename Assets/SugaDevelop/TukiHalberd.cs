using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TukiHalberd : MultiStickColliderDynamic
{



    [SerializeField] Cavalry cavalryScript;
    public Material cutSurfaceMaterial;
    public override void OnCollision(CollisionInfo collisionInfo)
    {


        switch (collisionInfo.collisionObject.tag)
        {
            case "Weapon":Cutted();break;
            default: break;
        }

        void Cutted()
        {
            Polygon polygon = collisionInfo.hitPolygon;
            Vector3 normal = polygon.normal;
            if (Vector3.Dot(normal, transform.up) >0) { normal *= -1; }

            Vector3 hitpoint = collisionInfo.hitPoints[0];
            (var copy, var original) = MeshCut.CutMesh(this.gameObject, hitpoint, normal, true, cutSurfaceMaterial,false);

            if (copy == null) { return; }
            copy.AddComponent<Rigidbody>();

            cavalryScript.WeaponCutted();
            Destroy(this);
        }

    }

    

}
