using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : SwordCollider {


    void Awake()
    {
        Application.targetFrameRate = 160;
    }
    Mesh[] cutMeshes=new Mesh[2];
    public override CollisionManager.ColliderType ColliderType => CollisionManager.ColliderType.PlayerBody;
    [SerializeField] Transform start, end;
    protected override List<Transform[]> transformList => new List<Transform[]> { new Transform[2] { start, end } };
    // Start is called before the first frame update
    protected override void Start_()
    {
        Invoke("Ready", 1.5f);

    }

    private void Update()
    {
        if (hit)
        {
            hit = false;
            CutMesh();
        }
    }

    protected override void LateUpdate_()
    {
        SetCollision();
        transform.position += -Vector3.up * Time.deltaTime*0.1f;
        
    }
    // Update is called once per frame


    void Ready()
    {
        ready = true;
    }

    bool ready=false;
    int count = 0;
    float time=0;
    bool hit=false;
    public override void OnCollision(CollisionInfo collision)
    {

        if (!ready||hit) { return; }
        hit = true;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
       
        Polygon polygon = collision.polygon;
        Vector3 normal = Vector3.Cross(polygon.vertices[1]-polygon.vertices[0],polygon.vertices[2]-polygon.vertices[0]).normalized;
        Transform target = transform.Find("Cube");
        Mesh[] meshes=new Mesh[2];
        sw.Start();
        //for(int o = 0; o < 20; o++)
        //{
            //meshes = SkinnedMeshCut.CutMesh(target.GetComponent<SkinnedMeshRenderer>(),target, polygon.vertices[0], normal);
            meshes = SkinnedMeshCut.CutMesh(target.GetComponent<SkinnedMeshRenderer>(), polygon.vertices[0], normal);
            //meshes = MeshCut.CutMeshRough(this.gameObject, polygon.vertices[0], normal);

        //}
        sw.Stop();
        //time += sw.ElapsedMilliseconds;
        Mesh mesh1, mesh2;
        if (normal.y > 0)
        {
            cutMeshes[0] = meshes[0];
            cutMeshes[1] = meshes[1];
        }
        else
        {
            cutMeshes[0] = meshes[1];
            cutMeshes[1] = meshes[0];
        }

        
        
        
        //print(sw.ElapsedMilliseconds);
        //count++;
        //if (count > 20)
        //{
        //    print(time / (float)count);
        //}
    }

    void CutMesh()
    {
        transform.Find("Cube").GetComponent<SkinnedMeshRenderer>().sharedMesh = cutMeshes[0];

        GameObject fragment = Instantiate(gameObject);
        SkinnedMeshRenderer smr = fragment.transform.Find("Cube").GetComponent<SkinnedMeshRenderer>();

        smr.sharedMesh = cutMeshes[1];
        smr.materials = this.transform.Find("Cube").GetComponent<SkinnedMeshRenderer>().materials;
        Animator anim1 = GetComponent<Animator>();
        Animator anim2 = fragment.GetComponent<Animator>();

        AnimatorStateInfo anist = anim1.GetCurrentAnimatorStateInfo(0);
        int key = anist.fullPathHash;
        float atime = anist.normalizedTime;
        anim2.Play(key, 0, atime);
        fragment.AddComponent<Rigidbody>();
    }
}
