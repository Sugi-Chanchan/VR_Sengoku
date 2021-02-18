using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OPTatami : StickColliderDynamic
{
    public Material cutSurfaceMaterial;
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

        copy.transform.parent=original.transform.parent;
        audioSource.PlayOneShot(clip);

        if(this.transform.parent.parent.name == "tatami_kiba")
        {
            Invoke("ChangeSceneKiba", 0.5f);
        }

        if (this.transform.parent.parent.name == "tatami_yabu")
        {
            Invoke("ChangeSceneYabusame", 0.5f);
        }
    }

    void ChangeSceneKiba()
    {
        SceneManager.LoadScene("騎馬");
    }

    void ChangeSceneYabusame()
    {
        SceneManager.LoadScene("流鏑馬");
    }

}
