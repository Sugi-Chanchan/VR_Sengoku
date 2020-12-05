//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class MeshCutTest : SwordCollider {

//    public Text text;
//    void Awake()
//    {
//        //Application.targetFrameRate = 160;
//    }
//    public Material cutMaterial;
//    public override CollisionManager.ColliderType ColliderType => CollisionManager.ColliderType.PlayerBody;
//    [SerializeField] Transform start, end;
//    [SerializeField] PhysicMaterial tatamiPhysics;
//    protected override List<Transform[]> transformList => new List<Transform[]> { new Transform[2] { start, end } };
//    // Start is called before the first frame update
//    protected override void Start_()
//    {
//        Invoke("Ready", 1);
//        Invoke("Test", 1);
//        //InvokeRepeating("Test", 1, 1);
//        //Test();
//    }

//    protected override void LateUpdate_()
//    {
//        SetCollision();
//        //transform.position += -Vector3.up * Time.deltaTime*0.1f;
//    }
//    // Update is called once per frame


//    void Ready()
//    {
//        ready = true;
//    }

//    bool ready = false;
//    int count = 0;
//    float time = 0;
//    float[] sum = new float[5];
//    public override void OnCollision(CollisionInfo collision)
//    {
        

//        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

//        Polygon polygon = collision.polygon;
//        Vector3 normal = Vector3.Cross(polygon.vertices[1] - polygon.vertices[0], polygon.vertices[2] - polygon.vertices[0]).normalized;
//        Transform target = transform.Find("Cube");
//        Mesh[] meshes = new Mesh[2];
//        sw.Start();
//        //for(int o = 0; o < 20; o++)
//        {
//        meshes = MeshCut.CutMesh(GetComponent<MeshFilter>().mesh,transform, polygon.vertices[0], normal,true,cutMaterial);
//        }

//        sw.Stop();

//        Mesh mesh1, mesh2;
//        if (normal.y > 0)
//        {
//            mesh1 = meshes[0];
//            mesh2 = meshes[1];
//        }
//        else
//        {
//            mesh1 = meshes[1];
//            mesh2 = meshes[0];
//        }
//        {
//            //Dictionary<Vector3, int> countDic = new Dictionary<Vector3, int>();
//            //for (int i = 0; i < mesh1.vertexCount; i++)
//            //{
//            //    Vector3 v = mesh1.vertices[i];
//            //    if (countDic.ContainsKey(v))
//            //    {
//            //        countDic[v] = countDic[v] + 1;
//            //    }
//            //    else
//            //    {
//            //        countDic.Add(v, 1);
//            //    }
//            //}

//            //foreach (KeyValuePair<Vector3, int> kvp in countDic)
//            //{
//            //    print(transform.localToWorldMatrix.MultiplyPoint(kvp.Key) + ":" + kvp.Value + "個");
//            //}
//        }
//        GetComponent<MeshFilter>().mesh = mesh1;

//        GameObject fragment = new GameObject("Fragment", typeof(MeshFilter), typeof(MeshRenderer));
//        fragment.transform.position = transform.position;
//        fragment.transform.rotation = transform.rotation;
//        fragment.transform.localScale = transform.localScale;

//        fragment.GetComponent<MeshFilter>().mesh = mesh2;
//        fragment.GetComponent<MeshRenderer>().materials = GetComponent<MeshRenderer>().materials;
//        fragment.AddComponent<Rigidbody>();

//        {
//            //GetComponent<MeshCollider>().sharedMesh = mesh1;
//            //MeshCollider fragmentMeshCollider = fragment.AddComponent<MeshCollider>();
//            //fragmentMeshCollider.convex = true;
//            //fragmentMeshCollider.sharedMesh = mesh2;
//            //fragmentMeshCollider.material = tatamiPhysics;
//        }
//        //time += sw.ElapsedMilliseconds;
//        sum[count % 5] = sw.ElapsedMilliseconds;
//        count++;
//        if (count > 20)
//        {
//            time = sum[0] + sum[1] + sum[2] + sum[3] + sum[4];
//            print(time /5);
//        }
//        //print(sw.ElapsedMilliseconds);
//    }

//    Vector3[] n = new Vector3[] { new Vector3(0, 1, 0), new Vector3(1, 1, 0) };
//    Vector3[] v = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0.1202f, 0.1f, 0.0024f) };
//    int c = 0;
//    static bool a=false;
//    void Test()
//    {
//        if (a == true) return;
//        a = true;
//        Vector3 normal = new Vector3(0,1,0);
//        Vector3 anchor = transform.position+Vector3.up;

//        MeshCut.CutMesh(gameObject, anchor, normal);

//        //Mesh[] meshes = new Mesh[2];
//        //{
//        //    meshes = MeshCut.CutMesh(GetComponent<MeshFilter>().mesh, transform, anchor, normal, true);
//        //}
//        //Mesh mesh1, mesh2;
//        //if (normal.y > 0)
//        //{
//        //    mesh1 = meshes[0];
//        //    mesh2 = meshes[1];
//        //}
//        //else
//        //{
//        //    mesh1 = meshes[1];
//        //    mesh2 = meshes[0];
//        //}
//        //GetComponent<MeshFilter>().mesh = mesh1;

//        //GameObject fragment = new GameObject("Fragment", typeof(MeshFilter), typeof(MeshRenderer));
//        //fragment.transform.position = transform.position;
//        //fragment.transform.rotation = transform.rotation;
//        //fragment.transform.localScale = transform.localScale;

//        //fragment.GetComponent<MeshFilter>().mesh = mesh2;
//        //fragment.GetComponent<MeshRenderer>().materials = GetComponent<MeshRenderer>().materials;
//        //fragment.AddComponent<Rigidbody>();


//    }

//}
