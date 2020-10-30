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
        //transform.position += -Vector3.up * Time.deltaTime*0.1f;
        
    }
    // Update is called once per frame

    float cutted = 0;
    public override void OnCollision(CollisionInfo collision)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        if (cutted > 0) { cutted -= 0.07f; return; }
        cutted +=1;
        Polygon polygon = collision.polygon;
        Vector3 normal = Vector3.Cross(polygon.vertices[1]-polygon.vertices[0],polygon.vertices[2]-polygon.vertices[0]).normalized;
        print(normal);
        var gos = MeshCut.Cut(gameObject, polygon.vertices[0], normal);
        //print(polygon.vertices[0]+","+polygon.vertices[1]+","+polygon.vertices[2]);
        gos[1].AddComponent<Rigidbody>();

        //Debug.Break();
        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds + "ms");
    }
}
