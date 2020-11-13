using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutTest : SwordCollider { 


    void Awake()
    {
        Application.targetFrameRate = 160;
    }

    public override CollisionManager.ColliderType ColliderType => CollisionManager.ColliderType.PlayerBody;
    [SerializeField] Transform start, end;
    [SerializeField] PhysicMaterial tatamiPhysics;
    protected override List<Transform[]> LinesOfTrabsform => new List<Transform[]> { new Transform[2] { start, end } };
    // Start is called before the first frame update
    protected override void StartOfCollisionInstance()
    {
        Invoke("Ready", 1);

    }

    protected override void UpdateOfCollisionInstance()
    {
        CheckCollision();
        //transform.position += -Vector3.up * Time.deltaTime*0.1f;
    }
    // Update is called once per frame


    void Ready()
    {
        ready = true;
    }

    bool ready = false;
    int count = 0;
    float time = 0;
    public override void OnCollision(CollisionInfo collision)
    {

        print("hit");

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        Polygon polygon = collision.polygon;
        Vector3 normal = Vector3.Cross(polygon.vertices[1] - polygon.vertices[0], polygon.vertices[2] - polygon.vertices[0]).normalized;
        Transform target = transform.Find("Cube");
        Mesh[] meshes = new Mesh[2];
        sw.Start();
        // for(int o = 0; o < 20; o++)
        //{
        meshes = MeshCut.CutMesh(GetComponent<MeshFilter>().mesh,transform, polygon.vertices[0], normal,true);
        //meshes = MeshCut.CutMeshOnce(this.gameObject, polygon.vertices[0], normal);

        //}
        sw.Stop();

        Mesh mesh1, mesh2;
        if (normal.y < 0)
        {
            mesh1 = meshes[0];
            mesh2 = meshes[1];
        }
        else
        {
            mesh1 = meshes[1];
            mesh2 = meshes[0];
        }

        //Dictionary<Vector3, int> countDic = new Dictionary<Vector3, int>();
        //for (int i = 0; i < mesh1.vertexCount; i++)
        //{
        //    Vector3 v = mesh1.vertices[i];
        //    if (countDic.ContainsKey(v))
        //    {
        //        countDic[v] = countDic[v] + 1;
        //    }
        //    else
        //    {
        //        countDic.Add(v, 1);
        //    }
        //}

        //foreach (KeyValuePair<Vector3, int> kvp in countDic)
        //{
        //    print(transform.localToWorldMatrix.MultiplyPoint(kvp.Key) + ":" + kvp.Value + "個");
        //}

        GetComponent<MeshFilter>().mesh = mesh1;
        GetComponent<MeshCollider>().sharedMesh = mesh1;

        GameObject fragment = new GameObject("Fragment", typeof(MeshFilter), typeof(MeshRenderer));
        fragment.transform.position = transform.position;
        fragment.transform.rotation = transform.rotation;
        fragment.transform.localScale = transform.localScale;

        fragment.GetComponent<MeshFilter>().mesh = mesh2;
        fragment.GetComponent<MeshRenderer>().materials = GetComponent<MeshRenderer>().materials;
        fragment.AddComponent<Rigidbody>();
        MeshCollider fragmentMeshCollider = fragment.AddComponent<MeshCollider>();
        fragmentMeshCollider.convex=true;
        fragmentMeshCollider.sharedMesh = mesh2;
        fragmentMeshCollider.material = tatamiPhysics;
        //time += sw.ElapsedMilliseconds;
        //count++;
        //if (count > 20)
        //{
        //    print(time / (float)count);
        //}
        //print(sw.ElapsedMilliseconds);
    }
}
