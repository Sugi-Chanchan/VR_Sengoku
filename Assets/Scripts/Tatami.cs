using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tatami : MonoBehaviour
{
    Transform tatamiTransform;
    GameObject[] blades;
    Vector3[] bladesPositionTemp, bladesDirection;

    int i;
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

        tatamiTransform = this.transform.GetChild(0).GetChild(0);
        if (blades == null)
        {
            blades = GameObject.FindGameObjectsWithTag("Blade");
        }

        i = blades.Length;
        bladesPositionTemp = new Vector3[i];
        bladesDirection = new Vector3[i];
    }

    // Update is called once per frame
    void Update()
    {
        for(int k = 0; k < i; k++)
        {
            bladesDirection[k] = blades[k].transform.position - bladesPositionTemp[k];
            bladesPositionTemp[k] = blades[k].transform.position;
        }
        print(bladesDirection[0]);
    }

    private void OnCollisionEnter(Collision other)
    {
        Vector3 hitPos = Vector3.zero, hitNor = Vector3.zero;
        print("collision");
        foreach (ContactPoint point in other.contacts)
        {
            hitPos = point.point;
            hitNor = Vector3.Cross(point.normal, other.transform.right);

            //print(hitPos);
            print(hitNor);
        }
        if (other.transform.tag == ("Blade") && bladesDirection[0].magnitude > 0.2) {
            (GameObject barabaraTatamiCopy, GameObject barabaraTatamiOriginal) = MeshCut.CutMesh(tatamiTransform.gameObject, hitPos, bladesDirection[0], true);
            Rigidbody fragRigid = barabaraTatamiOriginal.AddComponent<Rigidbody>();
            fragRigid.useGravity = true;
            fragRigid.isKinematic = false;
        }
    }
}
