using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TatamiDifficulty : StickColliderDynamic
{
    public Material cutSurfaceMaterial;
    bool gamestart = false;
    [SerializeField] Level level;
    public AudioClip clip;
    [SerializeField] AudioSource audioSource;
    public override void OnCollision(CollisionInfo collisionInfo)
    {

        if (collisionInfo.collisionObject.tag != "Weapon") return;

        Polygon polygon = collisionInfo.hitPolygon;
        Vector3 normal = polygon.normal;
        if (normal.y < 0) { normal *= -1; }
        Vector3 hitpoint = collisionInfo.hitPoints[0];
        (var copy, var original) = MeshCut.CutMesh(this.gameObject, hitpoint, normal, true, cutSurfaceMaterial);
        if (copy == null) { return; }
        var cr = copy.GetComponent<Rigidbody>();
        cr.useGravity = true;
        cr.isKinematic = false;

        copy.tag = "Tatami";

        if (!gamestart)
        {
            gamestart = true;
            (GameManager.Instance as KibaManager).GameStart(level, this.gameObject);
        }

        audioSource.PlayOneShot(clip);
    }


}
