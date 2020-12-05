using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempClass : MonoBehaviour
{

    public int meshcount;
    MeshFilter meshFilter;
    public Vector3 boundy;
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh.RecalculateBounds();
        boundy = meshFilter.mesh.bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        meshcount = meshFilter.mesh.vertexCount;
    }
}
