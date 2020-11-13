using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugiTest : MonoBehaviour
{
    SkinnedMeshRenderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<SkinnedMeshRenderer>();
        var mesh = MeshTest(renderer.sharedMesh);
        renderer.sharedMesh = mesh;
    }

    // Update is called once per frame
    void Update()
    {
        print(renderer.sharedMesh.bindposes[0]);
    }

    
    Mesh MeshTest(Mesh mesh)
    {
        Mesh m = new Mesh();
        m.vertices = mesh.vertices;
        m.triangles = mesh.triangles;
        m.uv = mesh.uv;
        m.normals = mesh.normals;
        m.boneWeights = mesh.boneWeights;
        m.bindposes = mesh.bindposes;
        return m;
        
    }

}
