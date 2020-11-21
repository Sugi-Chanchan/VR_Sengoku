using System.Collections.Generic;
using UnityEngine;
using System;


public class MeshCut : MonoBehaviour
{
    static Mesh _targetMesh;
    static Vector3[] _targetVertices;
    static Vector3[] _targetNormals;
    static Vector2[] _targetUVs;   //この3つはめっちゃ大事でこれ書かないと10倍くらい重くなる(for文中で使うから参照渡しだとやばい)
    static MeshData _frontMeshData = new MeshData(); //切断面の法線に対して表側
    static MeshData _backMeshData = new MeshData(); //裏側
    static Plane _slashPlane;//切断平面
                             //平面の方程式はn・r=h(nは法線,rは位置ベクトル,hはconst(=_planeValue))
    static Vector3 _planeNormal;
    static float _planeVlue;
    static bool[] _isFront;//頂点が切断面に対して表にあるか裏にあるか

    static int[] _trackedArray;

    static (bool, int)[] _VerticesInfo;


    /// <summary>
    /// <para>gameObjectを切断して2つのMeshにして返します.1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です.</para>
    /// <para>何度も切るようなオブジェクトでも頂点数が増えないように処理をしてあるほか, 簡単な物体なら切断面を縫い合わせることもできます</para>
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <param name="sewMesh">切断後にMeshを縫い合わせるか否か(切断面が2つ以上できるときはfalseにしてシェーダーのCull Frontで切断面を表示するようにする)</param>
    /// <returns></returns>
    public static Mesh[] CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool sewMesh)
    {

        _targetMesh = targetMesh; //Mesh情報取得
        _targetVertices = _targetMesh.vertices;
        _targetNormals = _targetMesh.normals;
        _targetUVs = _targetMesh.uv; //for文で_targetMeshから参照するのは非常に重くなるのでここで配列に格納してfor文ではここから渡す
        _frontMeshData.ClearAll(); //staticなクラスで宣言してあるのでここで初期化(staticで宣言する意味は高速化のため？？？)
        _backMeshData.ClearAll();


        Vector3 scale = targetTransform.localScale;//localscaleに合わせてPlaneに入れるnormalに補正をかける


        _slashPlane = new Plane(Vector3.Scale(scale, targetTransform.transform.InverseTransformDirection(planeNormalDirection)), targetTransform.transform.InverseTransformPoint(planeAnchorPoint));
        _planeNormal = Vector3.Scale(scale, targetTransform.transform.InverseTransformDirection(planeNormalDirection));
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        _planeVlue = Vector3.Dot(_planeNormal, anchor);

        _frontMeshData.cutNormal = -_slashPlane.normal;
        _backMeshData.cutNormal = _slashPlane.normal;//それぞれ法線を入力
        _frontMeshData.sewMesh = sewMesh;
        _backMeshData.sewMesh = sewMesh;//切断面を縫い合わせるかどうか


        int verticesLength = _targetVertices.Length;
        _trackedArray = new int[verticesLength];
        _isFront = new bool[verticesLength];

        {
            float pnx = _planeNormal.x;
            float pny = _planeNormal.y;
            float pnz = _planeNormal.z;

            float ancx = anchor.x;
            float ancy = anchor.y;
            float ancz = anchor.z;


            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 pos = _targetVertices[i];
                //planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
                if (_isFront[i] = (pnx * (pos.x - ancx) + pny * (pos.y - ancy) + pnz * (pos.z - ancz)) > 0)
                {

                    _frontMeshData.vertices.Add(pos);
                    _frontMeshData.normals.Add(_targetNormals[i]);
                    _frontMeshData.uvs.Add(_targetUVs[i]);

                    _trackedArray[i] = _frontMeshData.trackedVertexNum++;
                }
                else
                {
                    _backMeshData.vertices.Add(pos);
                    _backMeshData.normals.Add(_targetNormals[i]);
                    _backMeshData.uvs.Add(_targetUVs[i]);

                    _trackedArray[i] = _backMeshData.trackedVertexNum++;
                }
            }

        }

        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {

            int[] indices = _targetMesh.GetIndices(sub);


            _frontMeshData.subMeshIndices.Add(new List<int>());//subMeshが増えたことを追加してる

            _backMeshData.subMeshIndices.Add(new List<int>());


            _frontMeshData.fragmentDicList.Add(new Dictionary<int, List<Fragment>>());
            _backMeshData.fragmentDicList.Add(new Dictionary<int, List<Fragment>>());

            List<int> frontSubmeshIndices = _frontMeshData.subMeshIndices[sub];
            List<int> backSubmeshIndices = _backMeshData.subMeshIndices[sub];

            for (int i = 0; i < indices.Length; i += 3)
            {
                int p1, p2, p3;
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];


                //予め計算しておいた結果を持ってくる(ここで計算すると同じ頂点にたいして何回も同じ計算をすることになるから最初にまとめてやっている(そのほうが処理時間が速かった))
                bool side1 = _isFront[p1];
                bool side2 = _isFront[p2];
                bool side3 = _isFront[p3];


                if (side1 && side2 && side3)//3つとも表側, 3つとも裏側のときはそのまま出力
                {
                    frontSubmeshIndices.Add(_trackedArray[p1]);
                    frontSubmeshIndices.Add(_trackedArray[p2]);
                    frontSubmeshIndices.Add(_trackedArray[p3]);
                }
                else if (!side1 && !side2 && !side3)
                {
                    backSubmeshIndices.Add(_trackedArray[p1]);
                    backSubmeshIndices.Add(_trackedArray[p2]);
                    backSubmeshIndices.Add(_trackedArray[p3]);
                }
                else
                {
                    //三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する.
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { p1, p2, p3 }, sub);
                }

            }
        }


        _frontMeshData.ConnectFragments(); //切断されたMeshの破片は最後にくっつけられるところはくっつけて出力
        _backMeshData.ConnectFragments();



        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";
        frontMesh.vertices = _frontMeshData.vertices.ToArray();
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
        backMesh.normals = _backMeshData.normals.ToArray();
        backMesh.uv = _backMeshData.uvs.ToArray();

        backMesh.subMeshCount = _backMeshData.subMeshIndices.Count;
        for (int i = 0; i < _backMeshData.subMeshIndices.Count; i++)
        {
            backMesh.SetIndices(_backMeshData.subMeshIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        return new Mesh[2] { frontMesh, backMesh };
    }


    public static GameObject[] CutMesh(GameObject targetGameObject, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool sewMesh)
    {
        if (!targetGameObject.GetComponent<MeshFilter>())
        {
            Debug.LogError("引数のオブジェクトにはMeshFilterをアタッチしろ!");
            return null;
        }
        else if (!targetGameObject.GetComponent<MeshRenderer>())
        {
            Debug.LogError("引数のオブジェクトにはMeshrendererをアタッチしろ!");
            return null;
        }

        Mesh mesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform transform = targetGameObject.transform;

        Mesh[] meshes = CutMesh(mesh, transform, planeAnchorPoint, planeNormalDirection, sewMesh);

        targetGameObject.GetComponent<MeshFilter>().mesh = meshes[0];

        GameObject fragment = new GameObject("Fragment", typeof(MeshFilter), typeof(MeshRenderer));
        fragment.transform.position = targetGameObject.transform.position;
        fragment.transform.rotation = targetGameObject.transform.rotation;
        fragment.transform.localScale = targetGameObject.transform.localScale;
        fragment.GetComponent<MeshFilter>().mesh = meshes[1];
        fragment.GetComponent<MeshRenderer>().materials = targetGameObject.GetComponent<MeshRenderer>().materials;

        if (targetGameObject.GetComponent<MeshCollider>())
        {
            MeshCollider targertMeshCollider = targetGameObject.GetComponent<MeshCollider>();
            targertMeshCollider.sharedMesh = meshes[0];
            MeshCollider fragMeshCollider = fragment.AddComponent<MeshCollider>();
            fragMeshCollider.sharedMesh = meshes[1];
            fragMeshCollider.convex = targertMeshCollider.convex;
        }
        if (targetGameObject.GetComponent<Rigidbody>()||targetGameObject.transform.root.GetComponent<Rigidbody>())
        {
            fragment.GetComponent<Rigidbody>();
        }


        return new GameObject[2] { targetGameObject, fragment };

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
                    frontPoints[0] = frontPoints[1] = _targetVertices[p];
                }
                else
                {
                    f1 = p;
                    twoPointsInFront = true;
                    frontPoints[1] = _targetVertices[p];
                }
            }
            else
            {
                faceType -= (side);
                if (!didset_back)
                {
                    didset_back = true;
                    b0 = b1 = p;
                    backPoints[0] = backPoints[1] = _targetVertices[p];
                }
                else
                {
                    b1 = p;
                    backPoints[1] = _targetVertices[p];
                }
            }
        }


        float normalizedDistance0 = (_planeVlue - Vector3.Dot(_planeNormal, frontPoints[0])) / (Vector3.Dot(_planeNormal, backPoints[0] - frontPoints[0]));

        //Lerpで切断によってうまれる新しい頂点の情報を生成
        Vector3 newVertexpos0 = Vector3.Lerp(frontPoints[0], backPoints[0], normalizedDistance0);


        float normalizedDistance1 = (_planeVlue - Vector3.Dot(_planeNormal, frontPoints[1])) / (Vector3.Dot(_planeNormal, backPoints[1] - frontPoints[1]));
        Vector3 newVertexPos1 = Vector3.Lerp(frontPoints[1], backPoints[1], normalizedDistance1);





        Vector3 cutLine = (newVertexPos1 - newVertexpos0).normalized;

        int KEY_CUTLINE;


        float normalizedDistance0_b = 1 - normalizedDistance0;
        float normalizedDistance1_b = 1 - normalizedDistance1;




        Fragment fragF;
        Fragment fragB;

        NewVertex newVertexf0 = new NewVertex(f0, b0, normalizedDistance0, newVertexpos0);
        NewVertex newVertexf1 = new NewVertex(f1, b1, normalizedDistance1, newVertexPos1);
        NewVertex newVertexb0 = new NewVertex(b0, f0, normalizedDistance0_b, newVertexpos0);
        NewVertex newVertexb1 = new NewVertex(b1, f1, normalizedDistance1_b, newVertexPos1);


        if (twoPointsInFront) //切断面の表側に点が2つあるか裏側に2つあるかで分ける
        {
            if (faceType == 1)//入力されてきた頂点の回り方で面を作ると面の向きが外側になるので, 回り方を変えないように打ち込んでいく
            {
                fragF = new Fragment(newVertexf0, newVertexf1, true);
                fragB = new Fragment(newVertexb1, newVertexb0, false);
                KEY_CUTLINE = MakeIntFromVector3(cutLine);
            }
            else
            {
                fragF = new Fragment(newVertexf1, newVertexf0, true);
                fragB = new Fragment(newVertexb0, newVertexb1, false);
                KEY_CUTLINE = MakeIntFromVector3(-cutLine);
            }

        }
        else
        {
            if (faceType == -1)
            {

                fragF = new Fragment(newVertexf1, newVertexf0, false);
                fragB = new Fragment(newVertexb0, newVertexb1, true);
                KEY_CUTLINE = MakeIntFromVector3(-cutLine);

            }
            else
            {
                fragF = new Fragment(newVertexf0, newVertexf1, false);
                fragB = new Fragment(newVertexb1, newVertexb0, true);
                KEY_CUTLINE = MakeIntFromVector3(cutLine);
            }

        }
        Dictionary<int, List<Fragment>> frontFragmentDic = _frontMeshData.fragmentDicList[submesh];
        Dictionary<int, List<Fragment>> backFragmentDic = _backMeshData.fragmentDicList[submesh];

        List<Fragment> fraglist;
        if (frontFragmentDic.TryGetValue(KEY_CUTLINE, out fraglist))
        {
            fraglist.Add(fragF);
            backFragmentDic[KEY_CUTLINE].Add(fragB);
        }
        else
        {
            fraglist = new List<Fragment>();
            fraglist.Add(fragF);
            frontFragmentDic.Add(KEY_CUTLINE, fraglist);

            List<Fragment> bfraglist = new List<Fragment>();
            bfraglist.Add(fragB);
            backFragmentDic.Add(KEY_CUTLINE, bfraglist);
        }

    }


    public class MeshData
    {
        const int maxVerticesSize = 30000;
        public List<Vector3> vertices = new List<Vector3>(maxVerticesSize);
        public List<Vector3> normals = new List<Vector3>(maxVerticesSize);
        public List<Vector2> uvs = new List<Vector2>(maxVerticesSize);




        public List<List<int>> subMeshIndices = new List<List<int>>(maxVerticesSize * 2);

        public List<Dictionary<int, List<Fragment>>> fragmentDicList = new List<Dictionary<int, List<Fragment>>>(100);


        public bool sewMesh;//切断後にMeshの切断面を縫い合わせるか
        public Vector3 cutNormal;   //切断面の法線



        public int trackedVertexNum; //登録された頂点の数

        public void ConnectFragments()
        {

            for (int submesh = 0; submesh < fragmentDicList.Count; submesh++)
            {
                List<Fragment> outPutFragments = new List<Fragment>();

                foreach (List<Fragment> fragmentList in fragmentDicList[submesh].Values)
                {
                    for (int first = 0; first < fragmentList.Count; first++)
                    {
                        Fragment fFragment = fragmentList[first];
                        for (int second = first + 1; second < fragmentList.Count; second++)
                        {
                            Fragment sFragment = fragmentList[second];
                            //print(Convert.ToString(fFragment.newVertices[0].KEY_VERTEX, 16) + ":" + Convert.ToString(sFragment.newVertices[1].KEY_VERTEX, 16));
                            if (fFragment.newVertices[0].KEY_INDEX == sFragment.newVertices[1].KEY_INDEX)
                            {
                                if (fFragment.isRectangle && sFragment.isRectangle)
                                {
                                    AddTriangle(
                                        fFragment.newVertices[0].leftVertexIndex,
                                        sFragment.newVertices[0].leftVertexIndex,
                                        fFragment.newVertices[1].leftVertexIndex,
                                        submesh
                                        );
                                }
                                fFragment.newVertices[0] = sFragment.newVertices[0];
                            }
                            else if (fFragment.newVertices[1].KEY_INDEX == sFragment.newVertices[0].KEY_INDEX)
                            {
                                if (fFragment.isRectangle && sFragment.isRectangle)
                                {
                                    AddTriangle(
                                        fFragment.newVertices[1].leftVertexIndex,
                                        fFragment.newVertices[0].leftVertexIndex,
                                        sFragment.newVertices[1].leftVertexIndex,
                                        submesh
                                        );
                                }
                                fFragment.newVertices[1] = sFragment.newVertices[1];
                            }
                            else
                            {
                                continue;//どちらにも当てはまらなかった場合, 以下の処理は行わない
                            }

                            fFragment.isRectangle = fFragment.isRectangle || sFragment.isRectangle;
                            fragmentList.RemoveAt(second);
                            first--;
                            goto FINDCOUPLE; //もう1度同じループからやり直す
                        }

                        fFragment.KEY_POSITION0 = MakeIntFromVector3(fFragment.newVertices[0].position);
                        fFragment.KEY_POSITION1 = MakeIntFromVector3(fFragment.newVertices[1].position);
                        outPutFragments.Add(fFragment);

                    FINDCOUPLE:;
                    }

                }


                if (sewMesh && outPutFragments.Count > 0)
                {
                    int criterionIndex;
                    Fragment firstFragment = outPutFragments[0];
                    AddTriangle(firstFragment, submesh);
                    SetCutSurfaceVertex(firstFragment);
                    criterionIndex = firstFragment.cutSurfaceIndex0;
                    outPutFragments.RemoveAt(0);

                    int cutSurfaceSubmesh = 0;

                    foreach (Fragment fragment in outPutFragments)
                    {
                        AddTriangle(fragment, submesh);
                        SetCutSurfaceVertex(fragment);
                        if (fragment.cutSurfaceIndex1 != criterionIndex)
                        {
                            Sew(fragment.cutSurfaceIndex1,
                                fragment.cutSurfaceIndex0,
                                criterionIndex, cutSurfaceSubmesh);
                        }
                    }
                }
                else
                {
                    foreach (Fragment fragment in outPutFragments)
                    {
                        AddTriangle(fragment, submesh);
                    }
                }





            }



        }

        //三角ポリゴンの追加
        public void AddTriangle(int p1, int p2, int p3, int submeshNum)
        {
            int index_of_thisMesh;

            index_of_thisMesh = _trackedArray[p1];
            subMeshIndices[submeshNum].Add(index_of_thisMesh);

            index_of_thisMesh = _trackedArray[p2];
            subMeshIndices[submeshNum].Add(index_of_thisMesh);

            index_of_thisMesh = _trackedArray[p3];
            subMeshIndices[submeshNum].Add(index_of_thisMesh);

        }

        void AddTriangle(Fragment fragment, int submeshNum)
        {
            int left0 = fragment.newVertices[0].leftVertexIndex;
            int left1 = fragment.newVertices[1].leftVertexIndex;

            if (fragment.isRectangle)
            {
                subMeshIndices[submeshNum].Add(_trackedArray[left1]);
                subMeshIndices[submeshNum].Add(_trackedArray[left0]);
                SetNewVertex(fragment.newVertices[1], submeshNum);
            }
            subMeshIndices[submeshNum].Add(_trackedArray[left0]);
            SetNewVertex(fragment.newVertices[0], submeshNum);
            SetNewVertex(fragment.newVertices[1], submeshNum);
        }





        //このDictionaryは切断で新しくできた頂点を登録しておいて, すでに登録されていたら新規追加しないようにして頂点数を抑えるのに用いる 
        Dictionary<int, int> trackDic = new Dictionary<int, int>();
        //切断で新しく生成された頂点を新しいMeshに追加する関数
        private void SetNewVertex(NewVertex newVertex, int submeshNum)
        {
            int vertexIndex;
            if (trackDic.TryGetValue(newVertex.KEY_INDEX, out vertexIndex))
            {
                subMeshIndices[submeshNum].Add(vertexIndex);
            }
            else
            {
                vertexIndex = trackedVertexNum;
                float parameter = newVertex.dividingParameter;
                int left = newVertex.leftVertexIndex;
                int lost = newVertex.lostVertexIndex;

                trackDic.Add(newVertex.KEY_INDEX, vertexIndex);

                subMeshIndices[submeshNum].Add(vertexIndex);
                normals.Add(Vector3.Lerp(_targetNormals[left], _targetNormals[lost], parameter));
                vertices.Add(newVertex.position);
                uvs.Add(Vector2.Lerp(_targetUVs[left], _targetUVs[lost], parameter));

                trackedVertexNum++;
            }


        }

        //切断面を構成する頂点が増えないように登録しておく.
        //trackDicと分けている理由はCubeなど位置が同じで法線が違う頂点があったときに側面は区別したいけど切断面では1つにまとめたいから
        Dictionary<int, int> cutSurfaceTrackDic = new Dictionary<int, int>();

        void SetCutSurfaceVertex(Fragment fragment)
        {
            int index = 0;
            if (cutSurfaceTrackDic.TryGetValue(fragment.KEY_POSITION0, out index))
            {
                fragment.cutSurfaceIndex0 = index;
            }
            else
            {


                cutSurfaceTrackDic.Add(fragment.KEY_POSITION0, trackedVertexNum);
                normals.Add(cutNormal);
                Vector3 position = fragment.newVertices[0].position;
                vertices.Add(position);
                uvs.Add(new Vector2(position.x, position.z));//暫定的な処置
                fragment.cutSurfaceIndex0 = trackedVertexNum;
                trackedVertexNum++;
            }


            if (cutSurfaceTrackDic.TryGetValue(fragment.KEY_POSITION1, out index))
            {
                fragment.cutSurfaceIndex1 = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(fragment.KEY_POSITION1, trackedVertexNum);
                normals.Add(cutNormal);
                Vector3 position = fragment.newVertices[1].position;
                vertices.Add(position);
                uvs.Add(new Vector2(position.x, position.z));//暫定的な処置
                fragment.cutSurfaceIndex1 = trackedVertexNum;
                trackedVertexNum++;
            }
        }


        //最後の縫い合わせを行う(AddTriangleとやってることはほぼ同じ). ここに入る頂点はすでに登録済みのものだけなはずなのでtrackチェックを行わない
        void Sew(int p1, int p2, int p3, int submeshNum)
        {
            subMeshIndices[submeshNum].Add(p1);
            subMeshIndices[submeshNum].Add(p2);
            subMeshIndices[submeshNum].Add(p3);
        }


        public void ClearAll()//staticなclassとして使っているので毎回中身をresetする必要あり
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            subMeshIndices.Clear();

            trackedVertexNum = 0;

            trackDic.Clear();
            cutSurfaceTrackDic.Clear();

            fragmentDicList.Clear();

        }

    }


    public class Fragment
    {

        public NewVertex[] newVertices;
        public bool isRectangle;

        public int KEY_POSITION0;
        public int KEY_POSITION1;
        public int cutSurfaceIndex0;
        public int cutSurfaceIndex1;


        public Fragment(NewVertex newVertex0, NewVertex newVertex1, bool _isRectangle)
        {
            newVertices = new NewVertex[2] { newVertex0, newVertex1 };
            isRectangle = _isRectangle;
        }
    }

    public readonly struct NewVertex
    {
        public readonly int leftVertexIndex;
        public readonly int lostVertexIndex;
        public readonly float dividingParameter;
        public readonly int KEY_INDEX;
        public readonly Vector3 position;

        public NewVertex(int left, int lost, float parameter, Vector3 vertexPosition)
        {
            leftVertexIndex = left;
            lostVertexIndex = lost;
            KEY_INDEX = (left << 16) | lost;
            dividingParameter = parameter;
            position = vertexPosition;
        }
    }

    const int amp = 1 << 14;
    const int filter = 0x000003FF;
    public static int MakeIntFromVector3(Vector3 vec)
    {

        int cutLineX = ((int)(vec.x * amp) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp) & filter);


        return cutLineX | cutLineY | cutLineZ;
    }
}

