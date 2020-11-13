using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SkinnedMeshCut : MonoBehaviour
{
    static Mesh _targetMesh;
    static Vector3[] _targetVertices;
    static Vector3[] _convertedVertices; //アニメーションに合わせた頂点情報を入れる
    static Vector3[] _targetNormals;
    static Vector2[] _targetUVs;
    static BoneWeight[] _targetBoneWeights;//この4つはめっちゃ大事でこれ書かないと10倍くらい重くなる(for文中で使うから参照渡しだとやばい)
    static Matrix4x4[] _targetBindPoses;
    static MeshData _frontMeshData = new MeshData(); //切断面の法線に対して表側
    static MeshData _backMeshData = new MeshData(); //裏側
    static Plane _slashPlane;//切断平面
    static bool[] _isFront;//頂点が切断面に対して表にあるか裏にあるか


    /// <summary>
    /// gameObjectを切断して2つのMeshにして返します.
    /// 1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です.
    /// 何度も切るようなオブジェクトでは
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <returns></returns>
    public static Mesh[] CutMesh(SkinnedMeshRenderer target, Vector3 planeAnchorPoint, Vector3 planeNormalDirection)
    {
        _targetMesh = target.sharedMesh; //Mesh情報取得
        _targetVertices = _targetMesh.vertices;
        _targetNormals = _targetMesh.normals;
        _targetUVs = _targetMesh.uv; //for文で_targetMeshから参照するのは非常に重くなるのでここで配列に格納してfor文ではここから渡す
        _targetBoneWeights = _targetMesh.boneWeights;//頂点がどのボーンと連動しているかを表す. 1つの頂点につきボーンは4つまで登録してある
        _frontMeshData.ClearAll(); //staticなクラスで宣言してあるのでここで初期化(staticで宣言する意味は高速化のため？？？)
        _backMeshData.ClearAll();


        _slashPlane = new Plane(planeNormalDirection, planeAnchorPoint); //切断平面


        _targetBindPoses = _targetMesh.bindposes;
        Transform[] bones = target.bones;
        int bonesLength = bones.Length;
        Matrix4x4[] bonesMatrix = new Matrix4x4[bonesLength];
        for (int i = 0; i < bonesLength; ++i)
        {
            //Meshに登録されている頂点に_targetBindPoses(ボーンオフセット行列?)をかけるとボーンを原点とした座標になおせる
            //bones[b0].localToWorldMatrixでワールド座標に変換して, ワールド座標上で切断判定を行っていく
            bonesMatrix[i] = bones[i].localToWorldMatrix * _targetBindPoses[i];
        }




        int num = _targetVertices.Length;
        _convertedVertices = new Vector3[num];
        _isFront = new bool[num];
        for (int i = 0; i < num; i++) //Meshに入っている頂点座標は初期状態(Tポーズ)の位置なので, アニメーションを考慮した位置に変換する
        {
            BoneWeight bw = _targetBoneWeights[i];
            int b0 = bw.boneIndex0;//頂点が連動するボーンのindex
            int b1 = bw.boneIndex1;
            int b2 = bw.boneIndex2;
            int b3 = bw.boneIndex3;

            float w0 = bw.weight0;//各ボーンへの依存度
            float w1 = bw.weight1;
            float w2 = bw.weight2;
            float w3 = bw.weight3;
            Vector3 v = _targetVertices[i];
            //Meshの頂点をそれぞれのボーンの移動に合わせて動かし, 最後に重み付けをして新しい頂点配列をつくる
            Vector3 v0 = bonesMatrix[b0].MultiplyPoint3x4(v);
            Vector3 v1 = bonesMatrix[b1].MultiplyPoint3x4(v);
            Vector3 v2 = bonesMatrix[b2].MultiplyPoint3x4(v);
            Vector3 v3 = bonesMatrix[b3].MultiplyPoint3x4(v);

            _convertedVertices[i] = v0 * w0 + v1 * w1 + v2 * w2 + v3 * w3;

            _isFront[i] = _slashPlane.GetSide(_convertedVertices[i]);//planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
        }





        bool[] sides = new bool[3]; //先に宣言しておいてfor文中で使いまわしたほうが早い???
        int[] indices;
        int p1, p2, p3;

        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {
            indices = _targetMesh.GetIndices(sub);

            _frontMeshData.subMeshIndices.Add(new List<int>());

            _backMeshData.subMeshIndices.Add(new List<int>());

            int length = indices.Length; //先にintに入れておいたほうが処理が早いかも???
            for (int i = 0; i < length; i += 3)
            {
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];

                sides[0] = _isFront[p1];
                sides[1] = _isFront[p2];
                sides[2] = _isFront[p3];



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


        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";
        frontMesh.vertices = _frontMeshData.vertices.ToArray();
        frontMesh.triangles = _frontMeshData.triangles.ToArray();
        frontMesh.normals = _frontMeshData.normals.ToArray();
        frontMesh.uv = _frontMeshData.uvs.ToArray();
        frontMesh.boneWeights = _frontMeshData.boneWeights.ToArray();
        frontMesh.bindposes = _targetMesh.bindposes; //ボーンの構造が変わっていないのでこれは切断前後で変化しない

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
        backMesh.boneWeights = _backMeshData.boneWeights.ToArray();
        backMesh.bindposes = _targetMesh.bindposes;

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
        Vector3[] frontNormals = new Vector3[2]; //ポリゴンの頂点が3つなのにfront-backで配列が2つずつなのは対象的な処理を行うため
        Vector2[] frontUVs = new Vector2[2];
        Vector3[] backPoints = new Vector3[2];
        Vector3[] backNormals = new Vector3[2];
        Vector2[] backUVs = new Vector2[2];

        bool didset_front = false; //表側に1つ頂点を置いたかどうか
        bool didset_back = false;
        bool twoPointsInFront = false;//表側に点が2つあるか(これがfalseのときは裏側に点が2つある)

        int p = 0;
        int f0 = 0, f1 = 0, b0 = 0, b1 = 0; //頂点のindex番号を格納するのに使用

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
                    f0 = f1 = p;
                    //点が1つしかない側(front or back)ではelseの処理が行われないので2つの配列要素に代入(後で使う)
                    frontPoints[0] = frontPoints[1] = _convertedVertices[p];
                    frontUVs[0] = frontUVs[1] = _targetUVs[p];
                    frontNormals[0] = frontNormals[1] = _targetNormals[p];
                }
                else
                {
                    f1 = p;
                    twoPointsInFront = true;
                    frontPoints[1] = _convertedVertices[p];
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
                    b0 = b1 = p;
                    backPoints[0] = backPoints[1] = _convertedVertices[p];
                    backUVs[0] = backUVs[1] = _targetUVs[p];
                    backNormals[0] = backNormals[1] = _targetNormals[p];
                }
                else
                {
                    b1 = p;
                    backPoints[1] = _convertedVertices[p];
                    backUVs[1] = _targetUVs[p];
                    backNormals[1] = _targetNormals[p];
                }
            }
        }

        float normalizedDistance = 0f;
        float distance = 0f;

        //frontPoints[0]からbackPoints[0]に伸びるrayが切断面とぶつかるときの長さを取得
        _slashPlane.Raycast(new Ray(frontPoints[0], (backPoints[0] - frontPoints[0]).normalized), out distance);
        //0～1に変換(0だったらfrontPoints[0], 1だったらbackPoints[0], 0.5だったら中点)
        normalizedDistance = distance / (backPoints[0] - frontPoints[0]).magnitude;

        //Lerpで切断によってうまれる新しい頂点の情報を生成
        Vector3 newVertex1 = Vector3.Lerp(_targetVertices[f0], _targetVertices[b0], normalizedDistance);
        Vector2 newUV1 = Vector2.Lerp(frontUVs[0], backUVs[0], normalizedDistance);
        Vector3 newNormal1 = Vector3.Lerp(frontNormals[0], backNormals[0], normalizedDistance);
        int newWeightIndex1;
        if (normalizedDistance > 0.5f)
        {
            newWeightIndex1 = b0;
        }
        else
        {
            newWeightIndex1 = f0;
        }

        _slashPlane.Raycast(new Ray(frontPoints[1], (backPoints[1] - frontPoints[1]).normalized), out distance);
        normalizedDistance = distance / (backPoints[1] - frontPoints[1]).magnitude;

        Vector3 newVertex2 = Vector3.Lerp(_targetVertices[f1], _targetVertices[b1], normalizedDistance);
        Vector2 newUV2 = Vector2.Lerp(frontUVs[1], backUVs[1], normalizedDistance);
        Vector3 newNormal2 = Vector3.Lerp(frontNormals[1], backNormals[1], normalizedDistance);


        int newWeightIndex2;
        if (normalizedDistance > 0.5f)
        {
            newWeightIndex2 = b1;
        }
        else
        {
            newWeightIndex2 = f1;
        }

        if (twoPointsInFront) //切断面の表側に点が2つあるか裏側に2つあるかで分ける
        {
            if (faceType == 1)//入力する頂点の順番でメッシュの裏表が決まるので正しくなるように場合分け(faceType=1になるのは3つの頂点の切断平面との関係が表→裏→表の順で入力されてきたとき)
            {
                _frontMeshData.AddTriangle(f1, f0, newWeightIndex1, newVertex1, newNormal1, newUV1, submesh);
                _frontMeshData.AddTriangle(f1, new int[2] { newWeightIndex1,newWeightIndex2 }, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
                _backMeshData.AddTriangle(b0, new int[2] { newWeightIndex2, newWeightIndex1 } ,newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
            }
            else
            {
                _frontMeshData.AddTriangle(f0, f1, newWeightIndex1, newVertex1, newNormal1, newUV1, submesh);
                _frontMeshData.AddTriangle(f1, new int[2] { newWeightIndex2, newWeightIndex1 }, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
                _backMeshData.AddTriangle(b0, new int[2] { newWeightIndex1, newWeightIndex2 }, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
            }

        }
        else
        {
            //(faceType=-1になるのは3つの頂点の切断平面との関係が裏→表→裏の順で入力されてきたとき)
            if (faceType == -1)
            {
                _backMeshData.AddTriangle(b1, b0, newWeightIndex1, newVertex1, newNormal1, newUV1, submesh);
                _backMeshData.AddTriangle(b1, new int[2] { newWeightIndex1, newWeightIndex2 }, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
                _frontMeshData.AddTriangle(f0, new int[2] { newWeightIndex2, newWeightIndex1 }, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
            }
            else
            {
                _backMeshData.AddTriangle(b0, b1, newWeightIndex1, newVertex1, newNormal1, newUV1, submesh);
                _backMeshData.AddTriangle(b1, new int[2] { newWeightIndex2, newWeightIndex1 }, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
                _frontMeshData.AddTriangle(f0, new int[2] { newWeightIndex1, newWeightIndex2 }, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
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
        public List<BoneWeight> boneWeights = new List<BoneWeight>();

        int[] trackedArray; //_targetVerticesとverticesの対応をとっている
        int trackedVertexNum = 0; //登録された頂点の数


        //すでに登録された頂点かどうかを判断(同じ頂点が複数の三角面を構成しているときはこれがないと三角面の数だけ頂点が増えてしまう)
        bool CheckIndex(int target)
        {
            if ((trackNum = trackedArray[target]) != 0)
            {
                return true;
            }
            else
            {
                trackedArray[target] = trackedVertexNum;

                return false;
            }
        }

        private int trackNum;

        public void AddTriangle(int p1, int p2, int p3, int submeshNum)
        {

            if (CheckIndex(p1))
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);

            }
            else
            {
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[p1]);
                normals.Add(_targetNormals[p1]);
                uvs.Add(_targetUVs[p1]);
                boneWeights.Add(_targetBoneWeights[p1]);
                trackedVertexNum++;
            }


            if (CheckIndex(p2))
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[p2]);
                normals.Add(_targetNormals[p2]);
                uvs.Add(_targetUVs[p2]);
                boneWeights.Add(_targetBoneWeights[p2]);
                trackedVertexNum++;
            }

            if (CheckIndex(p3))
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[p3]);
                normals.Add(_targetNormals[p3]);
                uvs.Add(_targetUVs[p3]);
                boneWeights.Add(_targetBoneWeights[p3]);
                trackedVertexNum++;
            }
        }

        public void AddTriangle(int notCutIndex0, int notCutIndex1, int weightIndex, Vector3 cutPoint, Vector3 normal, Vector2 uv, int submeshNum)
        {

            if (CheckIndex(notCutIndex0))
            {

                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);

            }
            else
            {
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[notCutIndex0]);
                normals.Add(_targetNormals[notCutIndex0]);
                uvs.Add(_targetUVs[notCutIndex0]);
                boneWeights.Add(_targetBoneWeights[notCutIndex0]);
                trackedVertexNum++;

            }

            if (CheckIndex(notCutIndex1))
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);

            }
            else
            {
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[notCutIndex1]);
                normals.Add(_targetNormals[notCutIndex1]);
                uvs.Add(_targetUVs[notCutIndex1]);
                boneWeights.Add(_targetBoneWeights[notCutIndex1]);
                trackedVertexNum++;

            }

            subMeshIndices[submeshNum].Add(trackedVertexNum);
            triangles.Add(trackedVertexNum);
            vertices.Add(cutPoint);
            normals.Add(normal);
            uvs.Add(uv);
            boneWeights.Add(_targetBoneWeights[weightIndex]);//暫定的な処置
            trackedVertexNum++;
        }

        public void AddTriangle(int notCutIndex, int[] weightIndices, Vector3 cutPoint1, Vector3 cutPoint2, Vector3[] normals3, Vector2[] uvs3, int submeshNum)
        {

            if (CheckIndex(notCutIndex))
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[notCutIndex]);
                normals.Add(_targetNormals[notCutIndex]);
                trackedVertexNum++;
                boneWeights.Add(_targetBoneWeights[notCutIndex]);
                uvs.Add(_targetUVs[notCutIndex]);
            }

            subMeshIndices[submeshNum].Add(trackedVertexNum);
            triangles.Add(trackedVertexNum);
            vertices.Add(cutPoint1);
            normals.Add(normals3[0]); //normal配列には新しい頂点の2つ分しか入ってない
            uvs.Add(uvs3[0]);
            boneWeights.Add(_targetBoneWeights[weightIndices[0]]);//暫定的処理
            trackedVertexNum++;

            subMeshIndices[submeshNum].Add(trackedVertexNum);
            triangles.Add(trackedVertexNum);
            vertices.Add(cutPoint2);
            normals.Add(normals3[1]);
            uvs.Add(uvs3[1]);
            boneWeights.Add(_targetBoneWeights[weightIndices[1]]);//暫定的処理
            trackedVertexNum++;

        }

        public void ClearAll()
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            subMeshIndices.Clear();
            boneWeights.Clear();


            trackedArray = new int[_targetVertices.Length];
            trackedVertexNum = 0;

        }

    }

}



