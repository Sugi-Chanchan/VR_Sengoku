using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDummy : CollisionObject {


    void Awake()
    {
        Application.targetFrameRate = 160;
    }

    public override CollisionManager.ColliderType ColliderType => CollisionManager.ColliderType.PlayerBody;
    [SerializeField] Transform start, end;
    protected override List<Transform[]> LinesOfTrabsform => new List<Transform[]> { new Transform[2] { start, end } };
    // Start is called before the first frame update
    protected override void StartOfCollisionInstance()
    {
       

    }

    protected override void UpdateOfCollisionInstance()
    {
        CheckCollision();
        transform.position += -Vector3.up * Time.deltaTime*0.1f;
        cutted = 0;
    }
    // Update is called once per frame

    float cutted = 0;
    public override void OnCollision(CollisionInfo collision)
    {
        if (cutted > 0) { return; }
        cutted += 1;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Polygon polygon = collision.polygon;
        Vector3 normal = Vector3.Cross(polygon.vertices[1]-polygon.vertices[0],polygon.vertices[2]-polygon.vertices[0]).normalized;

        var meshes = MeshCut.CutMeshRough(gameObject, polygon.vertices[0], normal);
        //print(polygon.vertices[0]+","+polygon.vertices[1]+","+polygon.vertices[2]);

        Mesh mesh1, mesh2;
        if (normal.y > 0)
        {
            mesh1 = meshes[0];
            mesh2 = meshes[1];
        }
        else
        {
            mesh1 = meshes[1];
            mesh2 = meshes[0];
        }

        GetComponent<MeshFilter>().mesh = mesh1;

        GameObject fragment = new GameObject("Fragmet", typeof(MeshFilter), typeof(MeshRenderer));
        fragment.transform.position = this.transform.position;
        fragment.transform.rotation = this.transform.rotation;
        fragment.transform.localScale = this.transform.localScale;
        fragment.GetComponent<MeshFilter>().mesh = mesh2;
        fragment.GetComponent<MeshRenderer>().materials = this.GetComponent<MeshRenderer>().materials;

        fragment.AddComponent<Rigidbody>();

        //Debug.Break();
        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds + "ms");
    }
}
