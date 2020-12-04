using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tatami : MonoBehaviour
{
    Transform tatami;
    // Start is called before the first frame update
    void Start()
    {
        /*
        Mesh tatamiMesh = this.GetComponent<MeshFilter>().mesh;
        Mesh[] barabaraTatamiMesh;

        barabaraTatamiMesh = MeshCut.CutMesh(tatamiMesh, this.transform, this.transform.position+Vector3.up, new Vector3(1, 1, 1), true);
        this.GetComponent<MeshFilter>().mesh = barabaraTatamiMesh[1];

        GameObject kiraretaTatami = new GameObject("kiraretyatta", typeof(MeshFilter), typeof(MeshRenderer));
        kiraretaTatami.transform.position = this.transform.position;
        kiraretaTatami.transform.rotation = this.transform.rotation;
        kiraretaTatami.transform.localScale = this.transform.localScale;
        kiraretaTatami.GetComponent<MeshFilter>().mesh = barabaraTatamiMesh[0];
        kiraretaTatami.GetComponent<MeshRenderer>().materials = this.GetComponent<MeshRenderer>().materials;

        kiraretaTatami.AddComponent<Rigidbody>();

        MeshCollider kiraretaMeshCollider;

        kiraretaMeshCollider = kiraretaTatami.AddComponent<MeshCollider>();
        kiraretaMeshCollider.convex = true;

        kiraretaMeshCollider.sharedMesh = barabaraTatamiMesh[0];

        this.GetComponent<MeshCollider>().sharedMesh = barabaraTatamiMesh[1];
        */

        //GameObject[] barabaraTatami = MeshCut.CutMesh(this.gameObject, this.transform.position + Vector3.up, new Vector3(-1, -1, -1), true);
        //barabaraTatami[1].AddComponent<Rigidbody>();

        tatami = this.transform.GetChild(0).GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        Vector3 hitPos = Vector3.zero, hitNor = Vector3.zero;
        print("collision");
        foreach (ContactPoint point in other.contacts)
        {
            hitPos = point.point;
            hitNor = point.normal;

            print(hitPos);
            //print(hitNor);
        }
        if (other.transform.tag == ("Blade")) {
            print("iku---");
            GameObject[] barabaraTatami = MeshCut.CutMesh(tatami.gameObject, tatami.transform.position + Vector3.up, hitPos, true);
            barabaraTatami[1].AddComponent<Rigidbody>();
        }
    }
}
