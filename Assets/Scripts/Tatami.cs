using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tatami : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
    }
}
