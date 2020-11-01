using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCut2 : MonoBehaviour
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





        bool[] sides = new bool[3];
        int[] indices;
        int p1, p2, p3;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {
            indices = _targetMesh.GetIndices(sub);

            _frontMeshData.subMeshIndices.Add(new List<int>());
            _frontMeshData.triangleFragments.Add(new List<TriangleFragment>());
            _frontMeshData.rectangleFragments.Add(new List<RectangleFragment>());

            _backMeshData.subMeshIndices.Add(new List<int>());
            _backMeshData.triangleFragments.Add(new List<TriangleFragment>());
            _backMeshData.rectangleFragments.Add(new List<RectangleFragment>());

            for (int i = 0; i < indices.Length; i += 3)
            {
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];

                //planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
                sides[0] = _slashPlane.GetSide(_targetVertices[p1]);
                sides[1] = _slashPlane.GetSide(_targetVertices[p2]);
                sides[2] = _slashPlane.GetSide(_targetVertices[p3]);

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

        sw.Stop();
        Debug.Log(sw.ElapsedMilliseconds + "ms");

        _frontMeshData.ConnectFragments();
        _backMeshData.ConnectFragments();

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
        int f1 = 0, f2 = 0, b1 = 0, b2 = 0;

        int faceType = 0;//面が外側を向くように整列させるために使用

        for (int side = 0; side < 3; side++)
        {
            p = vertexIndices[side];

            if (sides[side])
            {
                faceType += (side); //これの合計値によって面の割り方の種類を6つに分類できる
                if (!didset_front)
                {
                    didset_front = true;
                    f1 = p;
                    frontPoints[0] = frontPoints[1] = _targetVertices[p];
                    frontUVs[0] = frontUVs[1] = _targetUVs[p];
                    frontNormals[0] = frontNormals[1] = _targetNormals[p];
                }
                else
                {
                    f2 = p;
                    twoPointsInFront = true;
                    frontPoints[1] = _targetVertices[p];
                    frontUVs[1] = _targetUVs[p];
                    frontNormals[1] = _targetNormals[p];
                }
            }
            else
            {
                faceType -= (side);
                if (!didset_back)
                {
                    didset_back = true;
                    b1 = p;
                    backPoints[0] = backPoints[1] = _targetVertices[p];
                    backUVs[0] = backUVs[1] = _targetUVs[p];
                    backNormals[0] = backNormals[1] = _targetNormals[p];
                }
                else
                {
                    b2 = p;
                    backPoints[1] = _targetVertices[p];
                    backUVs[1] = _targetUVs[p];
                    backNormals[1] = _targetNormals[p];
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


        if (twoPointsInFront) //入力されてきた頂点の回り方で面を作ると面の向きが外側になるので, 回り方を変えないように打ち込んでいく
        {
            if (faceType == 1)
            {
                _frontMeshData.rectangleFragments[submesh].Add(
                new RectangleFragment(f2, f1, newVertex1, newVertex2, new Vector2[2] { newUV1, newUV2 }, new Vector3[2] { newNormal1, newNormal2 })
                );
                _backMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(b1, newVertex2, newVertex1, new Vector2[2] { newUV2, newUV1 }, new Vector3[2] { newNormal2, newNormal1 })
                    );
            }
            else
            {
                _frontMeshData.rectangleFragments[submesh].Add(
                new RectangleFragment(f1, f2, newVertex2, newVertex1, new Vector2[2] { newUV2, newUV1 }, new Vector3[2] { newNormal2, newNormal1 })
                );
                _backMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(b1, newVertex1, newVertex2, new Vector2[2] { newUV1, newUV2 }, new Vector3[2] { newNormal1, newNormal2 })
                    );
            }
            
        }
        else
        {
            if (faceType == -1)
            {
                _backMeshData.rectangleFragments[submesh].Add(
              new RectangleFragment(b2, b1, newVertex1, newVertex2, new Vector2[2] { newUV1, newUV2 }, new Vector3[2] { newNormal1, newNormal2 })
              );
                _frontMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(f1, newVertex2, newVertex1, new Vector2[2] { newUV2, newUV1 }, new Vector3[2] { newNormal2, newNormal1 })
                    );
            }
            else
            {
                _backMeshData.rectangleFragments[submesh].Add(
              new RectangleFragment(b1, b2, newVertex2, newVertex1, new Vector2[2] { newUV2, newUV1 }, new Vector3[2] { newNormal2, newNormal1 })
              );
                _frontMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(f1, newVertex1, newVertex2, new Vector2[2] { newUV1, newUV2 }, new Vector3[2] { newNormal1, newNormal2 })
                    );
            }
           
        }


    }

    public class MeshData
    {
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<int> triangles = new List<int>();
        public List<List<int>> subMeshIndices = new List<List<int>>();


        public List<List<TriangleFragment>> triangleFragments = new List<List<TriangleFragment>>();
        public List<List<RectangleFragment>> rectangleFragments = new List<List<RectangleFragment>>();

        int[] trackedArray;//三角ポリゴンごとに頂点を増やすと処理が重くなるので同じ頂点をもつポリゴンには同じ頂点を当てている
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




            int trackNum = CheckIndex(p1);

            if (trackNum == -1)
            {
                

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


            trackNum = CheckIndex(p2);

            if (trackNum == -1)
            {

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

            trackNum = CheckIndex(p3);

            if (trackNum == -1)
            {

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

            NonCheck();
            NonCheck();
            NonCheck();

        }

        const float threshold = 0.001f;
        public void ConnectFragments()
        {
            //三角形と四角形で同一平面にあるやつをくっつける.(四角形の切断を想定しているので1つ相手が見つかったらループを抜ける)
            for (int sub = 0; sub < subMeshIndices.Count; sub++)
            {

                for(int j=0;j<triangleFragments[sub].Count;j++)
                {
                    var triangle = triangleFragments[sub][j];

                    for (int i = 0; i < rectangleFragments[sub].Count; i++)
                    {
                        var rectangle = rectangleFragments[sub][i];
                        if (Mathf.Abs(Vector3.Dot(triangle.cutLine, rectangle.cutLine)) > 1- threshold)
                        {
                            for(int n = 0; n < 2; n++)
                            {
                                for(int m = 0; m < 2; m++)
                                {
                                    if (triangle.cutPoints[n]==rectangle.cutPoints[m])
                                    {
                                        int indexInTriangle= (n + 1)% 2;
                                        int indexInRectangle = (m + 1) % 2;

                                        AddTriangle(
                                            rectangle.notCutPointIndecies[0],
                                            rectangle.notCutPointIndecies[1],
                                            rectangle.cutPoints[indexInRectangle],
                                            rectangle.normals[indexInRectangle],
                                            rectangle.uvs[indexInRectangle],
                                            sub
                                            );
                                        if (m == 0) //面を貼る向きを決める
                                        {
                                            AddTriangle(
                                            triangle.notCutPointIndecies,
                                            triangle.cutPoints[indexInTriangle],
                                            rectangle.cutPoints[indexInRectangle],
                                            new Vector3[2] {  triangle.normals[indexInTriangle] , rectangle.normals[indexInRectangle]},
                                            new Vector2[2] {  triangle.uvs[indexInTriangle], rectangle.uvs[indexInRectangle] },
                                            sub
                                            );
                                        }
                                        else
                                        {
                                            AddTriangle(
                                            triangle.notCutPointIndecies,
                                            rectangle.cutPoints[indexInRectangle],
                                            triangle.cutPoints[indexInTriangle],
                                            new Vector3[2] { rectangle.normals[indexInRectangle], triangle.normals[indexInTriangle] },
                                            new Vector2[2] { rectangle.uvs[indexInRectangle], triangle.uvs[indexInTriangle] },
                                            sub
                                            );
                                        }
                                        
                                        rectangleFragments[sub].RemoveAt(i);
                                        triangleFragments[sub].RemoveAt(j);
                                        j -= 1;//リストの配列でループしてるので配列を消した場合は辻褄を合わせる
                                        goto FINDFRAGMENT;
                                    }
                                }
                            }
                        }
                    }
                    FINDFRAGMENT:;
                }


                //三角形同士で同一平面にあるやつをくっつける.(四角形の切断を想定しているので1つ相手が見つかったらループを抜ける)
                for (int first = 0; first < triangleFragments[sub].Count; first++) 
                {
                    var fTriangle = triangleFragments[sub][first];
                    for (int second = first+1; second < triangleFragments[sub].Count; second++)
                    {
                        var sTriangle = triangleFragments[sub][second];
                        if (Mathf.Abs(Vector3.Dot(fTriangle.cutLine, sTriangle.cutLine)) > 1 - threshold)
                        {
                            for (int n = 0; n < 2; n++)
                            {
                                for (int m = 0; m < 2; m++)
                                {
                                    if (fTriangle.cutPoints[n] == sTriangle.cutPoints[m])
                                    {
                                        if (n == 0)
                                        {
                                            AddTriangle(
                                                fTriangle.notCutPointIndecies,
                                                sTriangle.cutPoints[0],
                                                fTriangle.cutPoints[1],
                                                new Vector3[2] { sTriangle.normals[0], fTriangle.normals[1] },
                                                new Vector2[2] { sTriangle.uvs[0], fTriangle.uvs[1] },
                                                sub
                                                );
                                        }
                                        else
                                        {
                                            AddTriangle(
                                                fTriangle.notCutPointIndecies,
                                                fTriangle.cutPoints[0],
                                                sTriangle.cutPoints[1],
                                                new Vector3[2] { fTriangle.normals[0], sTriangle.normals[1] },
                                                new Vector2[2] { fTriangle.uvs[0], sTriangle.uvs[1] },
                                                sub
                                                );
                                        }
                                        
                                        triangleFragments[sub].RemoveAt(second);
                                        triangleFragments[sub].RemoveAt(first);
                                        first -= 1;

                                        goto FINDFRAGMENT2;
                                    }
                                }
                            }
                        }
                    }
                FINDFRAGMENT2:;
                }

                //四角形同士で同一平面にあるやつをくっつける.(四角形の切断を想定しているので1つ相手が見つかったらループを抜ける)
                for (int first = 0; first < rectangleFragments[sub].Count;first++)
                {
                    RectangleFragment fRectangle = rectangleFragments[sub][first];
                    for(int second = first + 1; second < rectangleFragments[sub].Count; second++)
                    {
                        RectangleFragment sRectangle = rectangleFragments[sub][second];
                        if (Mathf.Abs(Vector3.Dot(fRectangle.cutLine, sRectangle.cutLine)) > 1 - threshold)
                        {
                            for (int n = 0; n < 2; n++)
                            {
                                for (int m = 0; m < 2; m++)
                                {
                                    if (fRectangle.cutPoints[n] == sRectangle.cutPoints[m])
                                    {
                                        if (n == 0)
                                        {
                                            AddTriangle(
                                                fRectangle.notCutPointIndecies[0],
                                                fRectangle.notCutPointIndecies[1],
                                                fRectangle.cutPoints[1],
                                                fRectangle.normals[1],
                                                fRectangle.uvs[1],
                                                sub
                                                );
                                            AddTriangle(
                                                sRectangle.notCutPointIndecies[0],
                                                sRectangle.notCutPointIndecies[1],
                                                sRectangle.cutPoints[0],
                                                sRectangle.normals[0],
                                                sRectangle.uvs[0],
                                                sub
                                                );
                                            AddTriangle(
                                                fRectangle.notCutPointIndecies[1],
                                                sRectangle.cutPoints[0],
                                                fRectangle.cutPoints[1],
                                                new Vector3[2] { sRectangle.normals[0], fRectangle.normals[1] },
                                                new Vector2[2] { sRectangle.uvs[0], fRectangle.uvs[1] },
                                                sub
                                                );
                                        }
                                        else
                                        {
                                            AddTriangle(
                                                sRectangle.notCutPointIndecies[0],
                                                sRectangle.notCutPointIndecies[1],
                                                sRectangle.cutPoints[1],
                                                sRectangle.normals[1],
                                                sRectangle.uvs[1],
                                                sub
                                                );
                                            AddTriangle(
                                                fRectangle.notCutPointIndecies[0],
                                                fRectangle.notCutPointIndecies[1],
                                                fRectangle.cutPoints[0],
                                                fRectangle.normals[0],
                                                fRectangle.uvs[0],
                                                sub
                                                );
                                            AddTriangle(
                                                sRectangle.notCutPointIndecies[1],
                                                fRectangle.cutPoints[0],
                                                sRectangle.cutPoints[1],
                                                new Vector3[2] { fRectangle.normals[0], sRectangle.normals[1] },
                                                new Vector2[2] { fRectangle.uvs[0], sRectangle.uvs[1] },
                                                sub
                                                );
                                        }

                                        rectangleFragments[sub].RemoveAt(second);
                                        rectangleFragments[sub].RemoveAt(first);
                                        first -= 1;

                                        goto FINDFRAGMENT3;
                                    }
                                }
                            }
                        }
                    }
                FINDFRAGMENT3:;
                }

                //最後にペアを作れなかったやつらを追加して終了
                foreach(TriangleFragment triangle in triangleFragments[sub])
                {
                    AddTriangle(
                        triangle.notCutPointIndecies,
                        triangle.cutPoints[0],
                        triangle.cutPoints[1],
                        triangle.normals,
                        triangle.uvs,
                        sub
                        );
                }

                foreach(RectangleFragment rectangle in rectangleFragments[sub])
                {
                    AddTriangle(
                        rectangle.notCutPointIndecies[0],
                        rectangle.notCutPointIndecies[1],
                        rectangle.cutPoints[0],
                        rectangle.normals[0],
                        rectangle.uvs[0],
                        sub
                        );
                    AddTriangle(
                        rectangle.notCutPointIndecies[0],
                        rectangle.cutPoints[0],
                        rectangle.cutPoints[1],
                        rectangle.normals,
                        rectangle.uvs,
                        sub
                        );
                }

            }
        }

        void AddTriangle(int notCutIndex0, int notCutIndex1, Vector3 cutPoint, Vector3 normal, Vector2 uv, int submeshNum)
        {
            Vector3 notCutPoint0 = _targetVertices[notCutIndex0];
            Vector3 notCutPoint1 = _targetVertices[notCutIndex1];
            //int p1 = 0;
            //int p2 = 1;
            //int p3 = 2;

            int last_index = vertices.Count;

            int trackNum = CheckIndex(notCutIndex0);

            if (trackNum == -1)
            {

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(notCutPoint0);
                normals.Add(_targetNormals[notCutIndex0]);
                uvs.Add(_targetUVs[notCutIndex0]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }

            trackNum = CheckIndex(notCutIndex1);

            if (trackNum == -1)
            {

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(notCutPoint1);
                normals.Add(_targetNormals[notCutIndex1]);
                uvs.Add(_targetUVs[notCutIndex1]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }

            subMeshIndices[submeshNum].Add(last_index);
            triangles.Add(last_index);
            vertices.Add(cutPoint);
            normals.Add(normal);
            uvs.Add(uv);
            NonCheck(); //新しく作られた頂点は元のメッシュの頂点indexを持っていないのでtrackNumだけ進めておく
        }

        

        void AddTriangle(int notCutIndex, Vector3 cutPoint1, Vector3 cutPoint2, Vector3[] normals3, Vector2[] uvs3, int submeshNum)
        {
            Vector3 notCutPoint = _targetVertices[notCutIndex];
                //int p1 = 0;
                //int p2 = 1;
                //int p3 = 2;

            int last_index = vertices.Count;

            int trackNum = CheckIndex(notCutIndex);

            if (trackNum == -1)
            {

                subMeshIndices[submeshNum].Add(last_index);
                triangles.Add(last_index);
                vertices.Add(notCutPoint);
                normals.Add(_targetNormals[notCutIndex]);
                uvs.Add(_targetUVs[notCutIndex]);

                last_index += 1;
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }

            subMeshIndices[submeshNum].Add(last_index);
            triangles.Add(last_index);
            vertices.Add(cutPoint1);
            normals.Add(normals3[0]); //normal配列には新しい頂点の2つ分しか入ってない
            uvs.Add(uvs3[0]);
            NonCheck();
            last_index += 1;

            subMeshIndices[submeshNum].Add(last_index);
            triangles.Add(last_index);
            vertices.Add(cutPoint2);
            normals.Add(normals3[1]);
            uvs.Add(uvs3[1]);
            NonCheck(); //新しく作られた頂点は元のメッシュの頂点indexを持っていないのでtrackNumだけすすめる(何も入れないとtrackedIndexとverticesの対応が崩れる)

        }
        

            public void ClearAll()
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            subMeshIndices.Clear();

            triangleFragments.Clear();
            rectangleFragments.Clear();

            trackedArray = new int[_targetVertices.Length + 50];
            trackedNum = 0;
        }

    }

    //三角ポリゴンを分割するとき, 三角になったほうをTriangleFragmentに, 四角になったほうをRectangleFragmentにいれている
    //これらは分割中はListにためておくだけにして, 最後にくっつけられるもの(同一平面にあるやつ)はくっつけて出力

    public class TriangleFragment
    {
        public Vector3 cutLine; //ポリゴンを分けたときの線
        public Vector3[] cutPoints;
        public int notCutPointIndecies;
        public Vector3[] normals;
        public Vector2[] uvs;
        public TriangleFragment(int notCutPoint1Index, Vector3 cutPoint1, Vector3 cutPoint2, Vector2[] uvs, Vector3[] normals)
        {
            cutLine = (cutPoint1 - cutPoint2).normalized;
            cutPoints = new Vector3[2] { cutPoint1, cutPoint2 };
            this.uvs = uvs;
            this.normals = normals;
            notCutPointIndecies = notCutPoint1Index;
        }

    }

    public class RectangleFragment
    {
        public Vector3 cutLine; //ポリゴンを分けたときの線
        public Vector3[] cutPoints;
        public int[] notCutPointIndecies;
        public Vector3[] normals;
        public Vector2[] uvs;
        public RectangleFragment(int notCutPoint1Index, int notCutPoint2Index, Vector3 cutPoint1, Vector3 cutPoint2, Vector2[] uvs, Vector3[] normals)
        {
            cutLine = (cutPoint1 - cutPoint2).normalized;
            cutPoints = new Vector3[2] { cutPoint1, cutPoint2 };
            this.uvs = uvs;
            this.normals = normals;
            notCutPointIndecies = new int[2] { notCutPoint1Index, notCutPoint2Index };
        }

    }

}



