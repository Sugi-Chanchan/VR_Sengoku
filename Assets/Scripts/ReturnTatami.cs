using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnTatami : StickColliderDynamic
{
    public Material cutSurfaceMaterial;
    public AudioClip clip;
    [SerializeField] AudioSource audioSource;
    bool cutted=false;
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

        copy.transform.parent = original.transform.parent;


        audioSource.PlayOneShot(clip);

        if (!cutted)
        {
            Invoke("SceneChange", 0.5f);
            cutted = true;
        }
    }

    void SceneChange()
    {
        SceneManager.LoadScene("OP画面");
    }
}