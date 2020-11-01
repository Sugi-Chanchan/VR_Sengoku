using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCut4 : MonoBehaviour
{
    static Mesh _targetMesh;
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
        _targetMesh = target.GetComponent<MeshFilter>().mesh;
        _frontMeshData.ClearAll();
        _backMeshData.ClearAll();
        print(_targetMesh.vertices.Length);

        Vector3 scale = target.transform.localScale;
        //localscaleに合わせてPlaneに入れるnormalに補正をかける
        Vector3 scaleCorrection = new Vector3(1 / scale.z, 1 / scale.y, 1 / scale.z);

        _slashPlane = new Plane(Vector3.Scale(scale, target.transform.InverseTransformDirection(planeNormalDirection)), target.transform.InverseTransformPoint(planeAnchorPoint));
        //_slashPlane = new Plane(Vector3.up, target.transform.InverseTransformPoint(planeAnchorPoint));





        bool[] sides = new bool[3];
        int[] indices;
        int p1, p2, p3;

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
                sides[0] = _slashPlane.GetSide(_targetMesh.vertices[p1]);
                sides[1] = _slashPlane.GetSide(_targetMesh.vertices[p2]);
                sides[2] = _slashPlane.GetSide(_targetMesh.vertices[p3]);

                if (sides[0] == sides[1] && sides[0] == sides[2])
                {
                    if (sides[0])
                    {
                        _frontMeshData.AddTriangle(p1, p2, p3, sub);
                    }
                    else
                    {
                        _backMeshData.AddTriangle(p1, p2, p3, sub);
                    }
                }
                else
                {
                    //三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する.
                    Sepalate(sides, new int[3] { p1, p2, p3 }, sub);
                }

            }
        }



        Material[] mats = target.GetComponent<MeshRenderer>().sharedMaterials;


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


        target.name = "FrontSide";
        target.GetComponent<MeshFilter>().mesh = frontMesh;
        GameObject frontSideObj = target;

        GameObject backSideObj = new GameObject("BackSide", typeof(MeshFilter), typeof(MeshRenderer));
        backSideObj.transform.position = target.transform.position;
        backSideObj.transform.rotation = target.transform.rotation;
        backSideObj.transform.localScale = target.transform.localScale;
        backSideObj.GetComponent<MeshFilter>().mesh = backMesh;

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
        _frontMeshData.ClearAll();
        _backMeshData.ClearAll();
        print(_targetMesh.vertices.Length);

        Vector3 scale = target.transform.localScale;
        //localscaleに合わせてPlaneに入れるnormalに補正をかける
        Vector3 scaleCorrection = new Vector3(1 / scale.z, 1 / scale.y, 1 / scale.z);

        _slashPlane = new Plane(Vector3.Scale(scale, target.transform.InverseTransformDirection(planeNormalDirection)), target.transform.InverseTransformPoint(planeAnchorPoint));
        //_slashPlane = new Plane(Vector3.up, target.transform.InverseTransformPoint(planeAnchorPoint));





        bool[] sides = new bool[3];
        int[] indices;
        int p1, p2, p3;

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
                sides[0] = _slashPlane.GetSide(_targetMesh.vertices[p1]);
                sides[1] = _slashPlane.GetSide(_targetMesh.vertices[p2]);
                sides[2] = _slashPlane.GetSide(_targetMesh.vertices[p3]);

                if (sides[0] == sides[1] && sides[0] == sides[2])
                {
                    if (sides[0])
                    {
                        _frontMeshData.AddTriangle(p1, p2, p3, sub);
                    }
                    else
                    {
                        _backMeshData.AddTriangle(p1, p2, p3, sub);
                    }
                }
                else
                {
                    //三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する.
                    Sepalate(sides, new int[3] { p1, p2, p3 }, sub);
                }

            }
        }

        _frontMeshData.DebugFunction();


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


    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        Vector3[] frontPoints = new Vector3[2];
        Vector3[] frontNormals = new Vector3[2];
        Vector2[] frontUVs = new Vector2[2];
        Vector3[] backPoints = new Vector3[2];
        Vector3[] backNormals = new Vector3[2];
        Vector2[] backUVs = new Vector2[2];

        bool didset_front = false;
        bool didset_back = false;
        bool twoPointsInFront = false;//表側に点が2つあるか(これがfalseのときは裏側に点が2つある)

        int p = 0;
        for (int side = 0; side < 3; side++)
        {
            p = vertexIndices[side];

            if (sides[side])
            {
                if (!didset_front)
                {
                    didset_front = true;

                    frontPoints[0] = frontPoints[1] = _targetMesh.vertices[p];
                    frontUVs[0] = frontUVs[1] = _targetMesh.uv[p];
                    frontNormals[0] = frontNormals[1] = _targetMesh.normals[p];
                }
                else
                {
                    twoPointsInFront = true;
                    frontPoints[1] = _targetMesh.vertices[p];
                    frontUVs[1] = _targetMesh.uv[p];
                    frontNormals[1] = _targetMesh.normals[p];
                }
            }
            else
            {
                if (!didset_back)
                {
                    didset_back = true;

                    backPoints[0] = backPoints[1] = _targetMesh.vertices[p];
                    backUVs[0] = backUVs[1] = _targetMesh.uv[p];
                    backNormals[0] = backNormals[1] = _targetMesh.normals[p];
                }
                else
                {
                    backPoints[1] = _targetMesh.vertices[p];
                    backUVs[1] = _targetMesh.uv[p];
                    backNormals[1] = _targetMesh.normals[p];
                }
            }
        }

        float normalizedDistance = 0f;
        float distance = 0f;

        _slashPlane.Raycast(new Ray(frontPoints[0], (backPoints[0] - frontPoints[0]).normalized), out distance);
        normalizedDistance = distance / (backPoints[0] - frontPoints[0]).magnitude;
        Vector3 newVertex1 = Vector3.Lerp(frontPoints[0], backPoints[0], normalizedDistance);
        Vector2 newUV1 = Vector2.Lerp(frontUVs[0], backUVs[0], normalizedDistance);
        Vector3 newNormal1 = Vector3.Lerp(frontNormals[0], backNormals[0], normalizedDistance);


        _slashPlane.Raycast(new Ray(frontPoints[1], (backPoints[1] - frontPoints[1]).normalized), out distance);
        normalizedDistance = distance / (backPoints[1] - frontPoints[1]).magnitude;
        Vector3 newVertex2 = Vector3.Lerp(frontPoints[1], backPoints[1], normalizedDistance);
        Vector2 newUV2 = Vector2.Lerp(frontUVs[1], backUVs[1], normalizedDistance);
        Vector3 newNormal2 = Vector3.Lerp(frontNormals[1], backNormals[1], normalizedDistance);

        print(frontUVs[0] + ", " + backUVs[0] + ", " + newUV1);
        if (twoPointsInFront)
        {
            _frontMeshData.AddTriangle(
                new Vector3[] { frontPoints[0], newVertex1, newVertex2 },
                new Vector3[] { frontNormals[0], newNormal1, newNormal2 },
                new Vector2[] { frontUVs[0], newUV1, newUV2 },
                newNormal1,
                submesh
            );

            _frontMeshData.AddTriangle(
                new Vector3[] { frontPoints[0], frontPoints[1], newVertex2 },
                new Vector3[] { frontNormals[0], frontNormals[1], newNormal2 },
                new Vector2[] { frontUVs[0], frontUVs[1], newUV2 },
                newNormal2,
                submesh
            );

            _backMeshData.AddTriangle(
                new Vector3[] { backPoints[0], newVertex1, newVertex2 },
                new Vector3[] { backNormals[0], newNormal1, newNormal2 },
                new Vector2[] { backUVs[0], newUV1, newUV2 },
                newNormal1,
                submesh
            );
        }
        else
        {
            _frontMeshData.AddTriangle(
                new Vector3[] { frontPoints[0], newVertex1, newVertex2 },
                new Vector3[] { frontNormals[0], newNormal1, newNormal2 },
                new Vector2[] { frontUVs[0], newUV1, newUV2 },
                newNormal1,
                submesh
            );

            _backMeshData.AddTriangle(
                new Vector3[] { backPoints[0], newVertex1, newVertex2 },
                new Vector3[] { backNormals[0], newNormal1, newNormal2 },
                new Vector2[] { backUVs[0], newUV1, newUV2 },
                newNormal1,
                submesh
            );

            _backMeshData.AddTriangle(
                new Vector3[] { backPoints[0], backPoints[1], newVertex2 },
                new Vector3[] { backNormals[0], backNormals[1], newNormal2 },
                new Vector2[] { backUVs[0], backUVs[1], newUV2 },
                newNormal2,
                submesh
            );
        }





    }

    public class MeshData
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<int> triangles = new List<int>();
        public List<List<int>> subMeshIndices = new List<List<int>>();

        private List<int> trackedIndex = new List<int>();
        public void AddTriangle(int p1, int p2, int p3, int submeshNum)
        {
            int last_index = vertices.Count;




            int trackNum = trackedIndex.IndexOf(p1);

            if (trackNum == -1)
            {
                trackedIndex.Add(p1);

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(_targetMesh.vertices[p1]);
                normals.Add(_targetMesh.normals[p1]);
                uvs.Add(_targetMesh.uv[p1]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }


            trackNum = trackedIndex.IndexOf(p2);

            if (trackNum == -1)
            {
                trackedIndex.Add(p2);

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(_targetMesh.vertices[p2]);
                normals.Add(_targetMesh.normals[p2]);
                uvs.Add(_targetMesh.uv[p2]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }

            trackNum = trackedIndex.IndexOf(p3);

            if (trackNum == -1)
            {
                trackedIndex.Add(p3);

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(_targetMesh.vertices[p3]);
                normals.Add(_targetMesh.normals[p3]);
                uvs.Add(_targetMesh.uv[p3]);


            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }



            //subMeshIndices[submeshNum].Add(last_index);
            //subMeshIndices[submeshNum].Add(last_index + 1);
            //subMeshIndices[submeshNum].Add(last_index + 2);

            //triangles.Add(last_index);
            //triangles.Add(last_index + 1);
            //triangles.Add(last_index + 2);

            //vertices.Add(_targetMesh.vertices[p1]);
            //vertices.Add(_targetMesh.vertices[p2]);
            //vertices.Add(_targetMesh.vertices[p3]);

            //normals.Add(_targetMesh.normals[p1]);
            //normals.Add(_targetMesh.normals[p2]);
            //normals.Add(_targetMesh.normals[p3]);

            //uvs.Add(_targetMesh.uv[p1]);
            //uvs.Add(_targetMesh.uv[p2]);
            //uvs.Add(_targetMesh.uv[p3]);
        }


        //MeshCutではこれに頂点情報とかを逐次追加していって最終的にこれの中身をMeshにいれる
        public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submeshNum)
        {
            // 引数の3頂点から法線を計算
            Vector3 calculated_normal = Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

            int p1 = 0;
            int p2 = 1;
            int p3 = 2;

            // 引数で指定された法線と逆だった場合はインデックスの順番を逆順にする（つまり面を裏返す）
            if (Vector3.Dot(calculated_normal, faceNormal) < 0)
            {
                p1 = 2;
                p2 = 1;
                p3 = 0;
            }

            int last_index = vertices.Count;








            subMeshIndices[submeshNum].Add(last_index + 0);
            subMeshIndices[submeshNum].Add(last_index + 1);
            subMeshIndices[submeshNum].Add(last_index + 2);

            triangles.Add(last_index + 0);
            triangles.Add(last_index + 1);
            triangles.Add(last_index + 2);

            vertices.Add(points3[p1]);
            vertices.Add(points3[p2]);
            vertices.Add(points3[p3]);

            normals.Add(normals3[p1]);
            normals.Add(normals3[p2]);
            normals.Add(normals3[p3]);

            uvs.Add(uvs3[p1]);
            uvs.Add(uvs3[p2]);
            uvs.Add(uvs3[p3]);

            trackedIndex.Add(-1);
            trackedIndex.Add(-1);
            trackedIndex.Add(-1);

        }

        public void ClearAll()
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            subMeshIndices.Clear();

            trackedIndex.Clear();
        }
        
        public void DebugFunction()
        {
            foreach(Vector2 uv in uvs)
            {
                print(uv);
            }
            int a = 1;
        }
    }
}



