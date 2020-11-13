using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//できるだけ軽い処理を目指した. ""絶対に""2つ以上のマテリアルを含むオブジェクトに使わないように!!!
public class MeshCutLight : MonoBehaviour
{
    static Vector3[] _targetVertices;
    static Vector3[] _targetNormals;
    static Vector2[] _targetUVs;   //この3つはめっちゃ大事でこれ書かないと10倍くらい重くなる
    static int[] _targetTriangles;

    static int[] _vertexArray;//登録された頂点, 分断される2つのメッシュで共同で使う(片方が前から詰めて,もう片方は後ろから詰める)
    static int _trackNum;

    static int _frontTrackedVertexNum; //登録された頂点の数, 
    static List<int> _frontTriangles = new List<int>();
    static int[] _frontTrackedArray; //_targetVerticesとverticesの対応をとっている


    static int _backTrackedVertexNum = 0; //登録された頂点の数
    static List<int> _backTriangles = new List<int>();
    static int[] _backTrackedArray; //_targetVerticesとverticesの対応をとっている


    /// <summary>
    /// gameObjectを雑に切断して2つのMeshにして返します. 切断面がザラザラなのでハイポリにしか使えませんが処理はちょっと早いです. 1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です.
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <returns></returns>
    public static Mesh[] CutMeshRough(GameObject target, Vector3 planeAnchorPoint, Vector3 planeNormalDirection)
    {
        Mesh targetMesh = target.GetComponent<MeshFilter>().mesh;
        _targetVertices = targetMesh.vertices;
        _targetNormals = targetMesh.normals;
        _targetUVs = targetMesh.uv;
        _targetTriangles = targetMesh.triangles;


        int num = _targetVertices.Length; //頂点の数
        int vArrayNum = num + (num >> 3);//num/32の意味. ビット演算のほうが割り算より早いらしい
        _vertexArray = new int[vArrayNum + 1];
        _frontTrackedArray = new int[num];
        _backTrackedArray = new int[num];

        _frontTriangles.Clear();
        _frontTrackedVertexNum = 0;


        _backTriangles.Clear();
        _backTrackedVertexNum = 0;


        var anchor = target.transform.InverseTransformPoint(planeAnchorPoint);
        //localscaleに合わせてPlaneに入れるnormalに補正をかける
        var nor = Vector3.Scale(target.transform.localScale, target.transform.InverseTransformDirection(planeNormalDirection));

        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();


        int p1, p2, p3;
        int tlength = _targetTriangles.Length;
        Vector3 v;
        for (int i = 0; i < tlength; i += 3)
        {
            p1 = _targetTriangles[i];
            p2 = _targetTriangles[i + 1];
            p3 = _targetTriangles[i + 2];

            //planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)

            v = _targetVertices[p1] - anchor;
            //sw.Start();
            if ((v.x * nor.x + v.y * nor.y + v.z * nor.z) > 0) //1番目の頂点のある側に残りの2つのポリゴンを持っていく(切断面はジグザグ)
            {
                if ((_trackNum = _frontTrackedArray[p1]) != 0)
                {
                    _frontTriangles.Add(_trackNum);
                }
                else
                {
                    _frontTrackedArray[p1] = _frontTrackedVertexNum;
                    _frontTriangles.Add(_frontTrackedVertexNum);
                    _vertexArray[_frontTrackedVertexNum++] = p1;
                }

                if ((_trackNum = _frontTrackedArray[p2]) != 0)
                {
                    _frontTriangles.Add(_trackNum);
                }
                else
                {
                    _frontTrackedArray[p2] = _frontTrackedVertexNum;
                    _frontTriangles.Add(_frontTrackedVertexNum);
                    _vertexArray[_frontTrackedVertexNum++] = p2;
                }

                if ((_trackNum = _frontTrackedArray[p3]) != 0)
                {
                    _frontTriangles.Add(_trackNum);
                }
                else
                {
                    _frontTrackedArray[p3] = _frontTrackedVertexNum;
                    _frontTriangles.Add(_frontTrackedVertexNum);
                    _vertexArray[_frontTrackedVertexNum++] = p3;
                }
            }
            else
            {
                if ((_trackNum = _backTrackedArray[p1]) != 0)
                {
                    _backTriangles.Add(_trackNum);
                }
                else
                {
                    _backTrackedArray[p1] = _backTrackedVertexNum;
                    _backTriangles.Add(_backTrackedVertexNum);
                    _vertexArray[vArrayNum - _backTrackedVertexNum] = p1;
                    _backTrackedVertexNum++;
                }

                if ((_trackNum = _backTrackedArray[p2]) != 0)
                {
                    _backTriangles.Add(_trackNum);
                }
                else
                {
                    _backTrackedArray[p2] = _backTrackedVertexNum;
                    _backTriangles.Add(_backTrackedVertexNum);
                    _vertexArray[vArrayNum - _backTrackedVertexNum] = p2;
                    _backTrackedVertexNum++;


                }

                if ((_trackNum = _backTrackedArray[p3]) != 0)
                {
                    _backTriangles.Add(_trackNum);
                }
                else
                {
                    _backTrackedArray[p3] = _backTrackedVertexNum;
                    _backTriangles.Add(_backTrackedVertexNum);
                    _vertexArray[vArrayNum - _backTrackedVertexNum] = p3;
                    _backTrackedVertexNum++;
                }
            }
            //sw.Stop();
        }

        Vector3[] frontVertexArray = new Vector3[_frontTrackedVertexNum];
        Vector3[] frontNormalArray = new Vector3[_frontTrackedVertexNum];
        Vector2[] frontUVArray = new Vector2[_frontTrackedVertexNum];




        Vector3[] backVertexArray = new Vector3[_backTrackedVertexNum];
        Vector3[] backNormalArray = new Vector3[_backTrackedVertexNum];
        Vector2[] backUVArray = new Vector2[_backTrackedVertexNum];

        int a;
        for (int i = 0; i < _frontTrackedVertexNum; ++i) //list.Addよりこっちのやり方のほうが若干早かった
        {
            a = _vertexArray[i];
            frontVertexArray[i] = _targetVertices[a];
            frontNormalArray[i] = _targetNormals[a];
            frontUVArray[i] = _targetUVs[a];
        }

        Mesh frontMesh = new Mesh();
        frontMesh.vertices = frontVertexArray;
        frontMesh.triangles = _frontTriangles.ToArray();
        frontMesh.normals = frontNormalArray;
        frontMesh.uv = frontUVArray;




        for (int i = 0; i < _backTrackedVertexNum; ++i)
        {
            a = _vertexArray[vArrayNum - i];
            backVertexArray[i] = _targetVertices[a];
            backNormalArray[i] = _targetNormals[a];
            backUVArray[i] = _targetUVs[a];
        }

        Mesh backMesh = new Mesh();
        backMesh.vertices = backVertexArray;
        backMesh.triangles = _backTriangles.ToArray();
        backMesh.normals = backNormalArray;
        backMesh.uv = backUVArray;

        return new Mesh[2] { frontMesh, backMesh };
    }

}



