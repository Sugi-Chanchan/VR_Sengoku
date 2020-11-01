using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//切断面の処理を雑に行う. 雑な処理なのでハイポリゴンのオブジェクトにしか使えないがちょっと処理が軽い
public class MeshCutRough : MonoBehaviour
{
    static Mesh _targetMesh;
    static Vector3[] _targetVertices;
    static Vector3[] _targetNormals;
    static Vector2[] _targetUVs;   //この3つはめっちゃ大事でこれ書かないと10倍くらい重くなる
    static MeshData _frontMeshData = new MeshData(); //切断面の法線に対して表側
    static MeshData _backMeshData = new MeshData(); //裏側
    static Plane _slashPlane;

    /// <summary>
    /// gameObjectを切断して2つのgameObjjectにして返します. 1つ目のgameObjectが切断面の法線に対して表側, 2つ目が裏側です.
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <returns></returns>
    public static GameObject[] CutObject(GameObject target, Vector3 planeAnchorPoint, Vector3 planeNormalDirection)
    {

        Mesh[] meshes = CutMesh(target, planeAnchorPoint, planeNormalDirection);

        Material[] mats = target.GetComponent<MeshRenderer>().sharedMaterials;

        target.name = "FrontSide";
        target.GetComponent<MeshFilter>().mesh = meshes[0];
        GameObject frontSideObj = target;

        GameObject backSideObj = new GameObject("BackSide", typeof(MeshFilter), typeof(MeshRenderer));
        backSideObj.transform.position = target.transform.position;
        backSideObj.transform.rotation = target.transform.rotation;
        backSideObj.transform.localScale = target.transform.localScale;
        backSideObj.GetComponent<MeshFilter>().mesh = meshes[1];

        frontSideObj.GetComponent<MeshRenderer>().materials = mats;
        backSideObj.GetComponent<MeshRenderer>().materials = mats;

        return new GameObject[2] { frontSideObj, backSideObj };
    }

    /// <summary>
    /// gameObjectを切断して2つのMeshにして返します. 1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です.
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <returns></returns>
    public static Mesh[] CutMesh(GameObject target, Vector3 planeAnchorPoint, Vector3 planeNormalDirection)
    {
        _targetMesh = target.GetComponent<MeshFilter>().mesh;
        _targetVertices = _targetMesh.vertices;
        _targetNormals = _targetMesh.normals;
        _targetUVs = _targetMesh.uv;
        _frontMeshData.ClearAll();
        _backMeshData.ClearAll();
        print(_targetVertices.Length);

        Vector3 scale = target.transform.localScale;
        //localscaleに合わせてPlaneに入れるnormalに補正をかける
        Vector3 scaleCorrection = new Vector3(1 / scale.z, 1 / scale.y, 1 / scale.z);

        _slashPlane = new Plane(Vector3.Scale(scale, target.transform.InverseTransformDirection(planeNormalDirection)), target.transform.InverseTransformPoint(planeAnchorPoint));
        //_slashPlane = new Plane(Vector3.up, target.transform.InverseTransformPoint(planeAnchorPoint));





        int[] indices;
        int p1, p2, p3;

        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        //sw.Start();
        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {
            indices = _targetMesh.GetIndices(sub);

            _frontMeshData.subMeshIndices.Add(new List<int>());

            _backMeshData.subMeshIndices.Add(new List<int>());

            for (int i = 0; i < indices.Length; i += 3)
            {
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];

                //planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
                bool sides = _slashPlane.GetSide(_targetVertices[p1]); 

                    if (sides) //1番目の頂点のある側にポリゴンを持っていく(切断面はジグザグ)
                    {
                        _frontMeshData.AddTriangle(p1, p2, p3, sub);
                    }
                    else
                    {
                        _backMeshData.AddTriangle(p1, p2, p3, sub);
                    }


            }
        }
        //sw.Stop();
        //Debug.Log(sw.ElapsedMilliseconds + "ms");


        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";
        frontMesh.vertices = _frontMeshData.vertices.ToArray();
        frontMesh.triangles = _frontMeshData.triangles.ToArray();
        frontMesh.normals = _frontMeshData.normals.ToArray();
        frontMesh.uv = _frontMeshData.uvs.ToArray();

        frontMesh.subMeshCount = _frontMeshData.subMeshIndices.Count;
        for (int i = 0; i < _frontMeshData.subMeshIndices.Count; i++)
        {
            frontMesh.SetIndices(_frontMeshData.subMeshIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh back";
        backMesh.vertices = _backMeshData.vertices.ToArray();
        backMesh.triangles = _backMeshData.triangles.ToArray();
        backMesh.normals = _backMeshData.normals.ToArray();
        backMesh.uv = _backMeshData.uvs.ToArray();

        backMesh.subMeshCount = _backMeshData.subMeshIndices.Count;
        for (int i = 0; i < _backMeshData.subMeshIndices.Count; i++)
        {
            backMesh.SetIndices(_backMeshData.subMeshIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        return new Mesh[2] { frontMesh, backMesh };
    }


    public class MeshData
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<int> triangles = new List<int>();
        public List<List<int>> subMeshIndices = new List<List<int>>();

        //private List<int> trackedIndex = new List<int>();//三角ポリゴンごとに頂点を増やすと処理が重くなるので同じ頂点をもつポリゴンには同じ頂点を当てている

        int[] trackedArray; //_targetVerticesとverticesの対応をとっている
        int trackedNum = 0;


        int CheckIndex(int target)
        {
            int index;
            if ((index = trackedArray[target]) != 0)
            {
                return index;
            }
            else
            {
                trackedArray[target] = trackedNum;
                trackedNum++;
                return -1;
            }
        }

        void NonCheck()
        {
            trackedNum++;
        }


        public void AddTriangle(int p1, int p2, int p3, int submeshNum)
        {
            int last_index = vertices.Count;




            //int trackNum = trackedIndex.LastIndexOf(p1);
            int trackNum = CheckIndex(p1);


            if (trackNum == -1)
            {
                //trackedIndex.Add(p1);
                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(_targetVertices[p1]);
                normals.Add(_targetNormals[p1]);
                uvs.Add(_targetUVs[p1]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }


            //trackNum = trackedIndex.LastIndexOf(p2);
            trackNum = CheckIndex(p2);

            if (trackNum == -1)
            {
                //trackedIndex.Add(p2);

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(_targetVertices[p2]);
                normals.Add(_targetNormals[p2]);
                uvs.Add(_targetUVs[p2]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }

            //trackNum = trackedIndex.LastIndexOf(p3);
            trackNum = CheckIndex(p3);

            if (trackNum == -1)
            {
                //trackedIndex.Add(p3);

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(_targetVertices[p3]);
                normals.Add(_targetNormals[p3]);
                uvs.Add(_targetUVs[p3]);


            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }


        }

        public void ClearAll()
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            subMeshIndices.Clear();

            trackedArray = new int[_targetVertices.Length + 50];
            trackedNum = 0;
        }

    }



}



