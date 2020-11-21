

//MeshCut_Old
/*
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


        _isFront = new bool[_targetVertices.Length];
        for (int i = 0; i < _targetVertices.Length; i++)
        {
            Vector3 v = _targetVertices[i] - anchor;
            _isFront[i] = (_planeNormal.x * v.x + _planeNormal.y * v.y + _planeNormal.z * v.z) > 0;//planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
        }





        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {
            int[] indices;
            indices = _targetMesh.GetIndices(sub);

            _frontMeshData.subMeshIndices.Add(new List<int>());//subMeshが増えたことを追加してる
            _frontMeshData.triangleFragments.Add(new List<TriangleFragment>()); //切断したMeshの破片を入れるListもsubMeshごとに追加
            _frontMeshData.rectangleFragments.Add(new List<RectangleFragment>());

            _backMeshData.subMeshIndices.Add(new List<int>());
            _backMeshData.triangleFragments.Add(new List<TriangleFragment>());
            _backMeshData.rectangleFragments.Add(new List<RectangleFragment>());

            bool side;
            int p1, p2, p3;

            for (int i = 0; i < indices.Length; i += 3)
            {
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];


                //予め計算しておいた結果を持ってくる(ここで計算すると同じ頂点にたいして何回も同じ計算をすることになるから最初にまとめてやっている(そのほうが処理時間が速かった))
                side = _isFront[p1];


                if (side == _isFront[p2] && side == _isFront[p3])//3つとも表側, 3つとも裏側のときはそのまま出力
                {
                    if (side)//表側か裏側かを識別
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
                    Sepalate(new bool[3] { side, _isFront[p2], _isFront[p3] }, new int[3] { p1, p2, p3 }, sub);
                }

            }
        }


        _frontMeshData.ConnectFragments(); //切断されたMeshの破片は最後にくっつけられるところはくっつけて出力
        _frontMeshData.SetZeroVertex();
        _backMeshData.ConnectFragments();
        _backMeshData.SetZeroVertex();

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

    /// <summary>
    /// <para>gameObjectを切断して2つのMeshにして返します.
    /// 1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です.</para>
    /// <para>切断の処理が雑なので何度も斬ると頂点数が膨れ上がります. そのかわり一回斬るだけならこっちが速い, あと切断面の縫い合わせはできない</para>
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <returns></returns>
    public static Mesh[] CutMeshOnce(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection)
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


        _isFront = new bool[_targetVertices.Length];
        for (int i = 0; i < _targetVertices.Length; i++)
        {
            Vector3 v = _targetVertices[i] - anchor;
            _isFront[i] = (_planeNormal.x * v.x + _planeNormal.y * v.y + _planeNormal.z * v.z) > 0;//planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
        }


        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {
            int[] indices;
            indices = _targetMesh.GetIndices(sub);

            _frontMeshData.subMeshIndices.Add(new List<int>());//subMeshが増えたことを追加してる

            _backMeshData.subMeshIndices.Add(new List<int>());


            int length = indices.Length; //先にintに入れておいたほうが処理が早いかも???
            for (int i = 0; i < length; i += 3)
            {
                int p1, p2, p3;
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];


                bool[] sides = new bool[3];

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
                    SepalateOnce(sides, new int[3] { p1, p2, p3 }, sub);
                }

            }
        }


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
                    f0 = p;
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
                    b0 = p;
                    backPoints[0] = backPoints[1] = _targetVertices[p];
                }
                else
                {
                    b1 = p;
                    backPoints[1] = _targetVertices[p];
                }
            }
        }

        float normalizedDistance0 = 0f;
        float distance = 0f;

        ////frontPoints[0]からbackPoints[0]に伸びるrayが切断面とぶつかるときの長さを取得
        //_slashPlane.Raycast(new Ray(frontPoints[0], (backPoints[0] - frontPoints[0]).normalized), out distance);
        ////0～1に変換(0だったらfrontPoints[0], 1だったらbackPoints[0], 0.5だったら中点)
        //normalizedDistance0 = distance / (backPoints[0] - frontPoints[0]).magnitude;
        normalizedDistance0 = (_planeVlue - Vector3.Dot(_planeNormal, frontPoints[0])) / (Vector3.Dot(_planeNormal, backPoints[0] - frontPoints[0]));

        //Lerpで切断によってうまれる新しい頂点の情報を生成
        Vector3 newVertex0 = Vector3.Lerp(frontPoints[0], backPoints[0], normalizedDistance0);


        //_slashPlane.Raycast(new Ray(frontPoints[1], (backPoints[1] - frontPoints[1]).normalized), out distance);
        //float normalizedDistance1 = distance / (backPoints[1] - frontPoints[1]).magnitude;
        float normalizedDistance1 = (_planeVlue - Vector3.Dot(_planeNormal, frontPoints[1])) / (Vector3.Dot(_planeNormal, backPoints[1] - frontPoints[1]));
        Vector3 newVertex1 = Vector3.Lerp(frontPoints[1], backPoints[1], normalizedDistance1);

        Vector3 cutLine = (newVertex1 - newVertex0).normalized;

        float normalizedDistance0_b = 1 - normalizedDistance0;
        float normalizedDistance1_b = 1 - normalizedDistance1;
        if (twoPointsInFront) //切断面の表側に点が2つあるか裏側に2つあるかで分ける
        {
            if (faceType == 1)//入力されてきた頂点の回り方で面を作ると面の向きが外側になるので, 回り方を変えないように打ち込んでいく
            {
                _frontMeshData.rectangleFragments[submesh].Add(
                new RectangleFragment(f1, f0, b0, new Vector3[2] { newVertex1, newVertex0 }, new float[2] { normalizedDistance1, normalizedDistance0 }, cutLine)
                );
                _backMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(b0, f1, f0, new Vector3[2] { newVertex1, newVertex0 }, new float[2] { normalizedDistance1_b, normalizedDistance0_b }, cutLine)
                    );
            }
            else
            {
                _frontMeshData.rectangleFragments[submesh].Add(
                new RectangleFragment(f0, f1, b0, new Vector3[2] { newVertex0, newVertex1 }, new float[2] { normalizedDistance0, normalizedDistance1 }, cutLine)
                );
                _backMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(b0, f0, f1, new Vector3[2] { newVertex0, newVertex1 }, new float[2] { normalizedDistance0_b, normalizedDistance1_b }, cutLine)
                    );
            }

        }
        else
        {
            if (faceType == -1)
            {
                _backMeshData.rectangleFragments[submesh].Add(
              new RectangleFragment(b1, b0, f0, new Vector3[2] { newVertex1, newVertex0 }, new float[2] { normalizedDistance1_b, normalizedDistance0_b }, cutLine)
              );
                _frontMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(f0, b1, b0, new Vector3[2] { newVertex1, newVertex0 }, new float[2] { normalizedDistance1, normalizedDistance0 }, cutLine)
                    );
            }
            else
            {
                _backMeshData.rectangleFragments[submesh].Add(
              new RectangleFragment(b0, b1, f0, new Vector3[2] { newVertex0, newVertex1 }, new float[2] { normalizedDistance0_b, normalizedDistance1_b }, cutLine)
              );
                _frontMeshData.triangleFragments[submesh].Add(
                    new TriangleFragment(f0, b0, b1, new Vector3[2] { newVertex0, newVertex1 }, new float[2] { normalizedDistance0, normalizedDistance1 }, cutLine)
                    );
            }
        }
    }

    //CutMeshOnce用の関数
    private static void SepalateOnce(bool[] sides, int[] vertexIndices, int submesh)
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
                    f0 = p;
                    //点が1つしかない側(front or back)ではelseの処理が行われないので2つの配列要素に代入(後で使う)
                    frontPoints[0] = frontPoints[1] = _targetVertices[p];
                    frontUVs[0] = frontUVs[1] = _targetUVs[p];
                    frontNormals[0] = frontNormals[1] = _targetNormals[p];
                }
                else
                {
                    f1 = p;
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
                    b0 = p;
                    backPoints[0] = backPoints[1] = _targetVertices[p];
                    backUVs[0] = backUVs[1] = _targetUVs[p];
                    backNormals[0] = backNormals[1] = _targetNormals[p];
                }
                else
                {
                    b1 = p;
                    backPoints[1] = _targetVertices[p];
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
        Vector3 newVertex1 = Vector3.Lerp(frontPoints[0], backPoints[0], normalizedDistance);
        Vector2 newUV1 = Vector2.Lerp(frontUVs[0], backUVs[0], normalizedDistance);
        Vector3 newNormal1 = Vector3.Lerp(frontNormals[0], backNormals[0], normalizedDistance);


        _slashPlane.Raycast(new Ray(frontPoints[1], (backPoints[1] - frontPoints[1]).normalized), out distance);
        normalizedDistance = distance / (backPoints[1] - frontPoints[1]).magnitude;
        Vector3 newVertex2 = Vector3.Lerp(frontPoints[1], backPoints[1], normalizedDistance);
        Vector2 newUV2 = Vector2.Lerp(frontUVs[1], backUVs[1], normalizedDistance);
        Vector3 newNormal2 = Vector3.Lerp(frontNormals[1], backNormals[1], normalizedDistance);

        if (twoPointsInFront) //切断面の表側に点が2つあるか裏側に2つあるかで分ける
        {
            if (faceType == 1)//入力されてきた頂点の回り方で面を作ると面の向きが外側になるので, 回り方を変えないように打ち込んでいく
            {
                _frontMeshData.AddTriangle(f1, f0, newVertex1, newNormal1, newUV1, submesh);
                _frontMeshData.AddTriangle(f1, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
                _backMeshData.AddTriangle(b0, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
            }
            else
            {
                _frontMeshData.AddTriangle(f0, f1, newVertex1, newNormal1, newUV1, submesh);
                _frontMeshData.AddTriangle(f1, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
                _backMeshData.AddTriangle(b0, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
            }

        }
        else
        {
            if (faceType == -1)
            {
                _backMeshData.AddTriangle(b1, b0, newVertex1, newNormal1, newUV1, submesh);
                _backMeshData.AddTriangle(b1, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
                _frontMeshData.AddTriangle(f0, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
            }
            else
            {
                _backMeshData.AddTriangle(b0, b1, newVertex1, newNormal1, newUV1, submesh);
                _backMeshData.AddTriangle(b1, newVertex2, newVertex1, new Vector3[2] { newNormal2, newNormal1 }, new Vector2[2] { newUV2, newUV1 }, submesh);
                _frontMeshData.AddTriangle(f0, newVertex1, newVertex2, new Vector3[2] { newNormal1, newNormal2 }, new Vector2[2] { newUV1, newUV2 }, submesh);
            }
        }

    }


    public class MeshData
    {
        const int maxVerticesSize = 30000;
        public List<Vector3> vertices = new List<Vector3>(maxVerticesSize);
        public Vector3[] verticesArray;
        public List<Vector3> normals = new List<Vector3>(maxVerticesSize);
        public List<Vector2> uvs = new List<Vector2>(maxVerticesSize);
        public List<int> triangles = new List<int>(maxVerticesSize * 2);
        public List<List<int>> subMeshIndices = new List<List<int>>(maxVerticesSize * 2);

        public List<List<TriangleFragment>> triangleFragments = new List<List<TriangleFragment>>(100);
        public List<List<RectangleFragment>> rectangleFragments = new List<List<RectangleFragment>>(100);

        public bool sewMesh = true;//切断後にMeshの切断面を縫い合わせるか
        public Vector3 cutNormal;   //切断面の法線


        int[] trackedArray; //_targetVerticesとverticesの対応をとっている
        int trackedVertexNum = 0; //登録された頂点の数





        const float threshold = 0.0001f;
        public void ConnectFragments()
        {
            //三角形と四角形で同一平面にあるやつをくっつける.(ここではくっつけるだけで出力するのは最後)
            for (int sub = 0; sub < subMeshIndices.Count; sub++)
            {
                List<TriangleFragment> triangleList = triangleFragments[sub];
                List<RectangleFragment> rectanglesList = rectangleFragments[sub];
                for (int i = 0; i < rectanglesList.Count; i++)
                {
                    var rectangle = rectanglesList[i];

                    for (int j = 0; j < triangleList.Count; j++)
                    {
                        var triangle = triangleList[j];

                        if (Math.Abs(Vector3.Dot(triangle.cutLine, rectangle.cutLine)) > 1 - threshold)
                        {
                            if (rectangle.lineKeys[0] == triangle.lineKeys[0])
                            {
                                rectangle.lostIndex0 = triangle.lostIndex1;
                                rectangle.parameters[0] = triangle.parameters[1];
                                rectangle.newVertices[0] = triangle.newVertices[1];
                                rectangle.lineKeys[0] = triangle.lineKeys[1];
                                triangleList.RemoveAt(j);
                                j -= 1;//リストの配列でループしてるので配列を消した場合は辻褄を合わせる

                            }
                            else if (rectangle.lineKeys[1] == triangle.lineKeys[1])
                            {
                                rectangle.lostIndex1 = triangle.lostIndex0;
                                rectangle.parameters[1] = triangle.parameters[0];
                                rectangle.newVertices[1] = triangle.newVertices[0];
                                rectangle.lineKeys[1] = triangle.lineKeys[0];
                                triangleList.RemoveAt(j);
                                j -= 1;//リストの配列でループしてるので配列を消した場合は辻褄を合わせる
                            }
                        }
                    }
                }


                //三角形同士で同一平面にあるやつをくっつける.(ここではくっつけるだけで出力するのは最後)
                for (int first = 0; first < triangleList.Count; first++)
                {
                    var fTriangle = triangleList[first];
                    for (int second = first + 1; second < triangleList.Count; second++)
                    {
                        var sTriangle = triangleList[second];
                        if (Math.Abs(Vector3.Dot(fTriangle.cutLine, sTriangle.cutLine)) > 1 - threshold)
                        {
                            if (fTriangle.lineKeys[0] == sTriangle.lineKeys[1])
                            {
                                fTriangle.lostIndex0 = sTriangle.lostIndex0;
                                fTriangle.parameters[0] = sTriangle.parameters[0];
                                fTriangle.newVertices[0] = sTriangle.newVertices[0];
                                fTriangle.lineKeys[0] = sTriangle.lineKeys[0];
                                triangleList.RemoveAt(second);
                                second -= 1;
                            }
                            else if (fTriangle.lineKeys[1] == sTriangle.lineKeys[0])
                            {
                                fTriangle.lostIndex1 = sTriangle.lostIndex1;
                                fTriangle.parameters[1] = sTriangle.parameters[1];
                                fTriangle.newVertices[1] = sTriangle.newVertices[1];
                                fTriangle.lineKeys[1] = sTriangle.lineKeys[1];

                                triangleList.RemoveAt(second);
                                second -= 1;
                            }

                        }
                    }
                }


                //四角形同士で同一平面にあるやつをくっつける.(四角形をくっつけると五角形になってしまうので一部の三角形はここで出力)
                for (int first = 0; first < rectanglesList.Count; first++)
                {
                    RectangleFragment fRectangle = rectanglesList[first];
                    for (int second = first + 1; second < rectanglesList.Count; second++)
                    {
                        RectangleFragment sRectangle = rectanglesList[second];
                        if (Math.Abs(Vector3.Dot(fRectangle.cutLine, sRectangle.cutLine)) > 1 - threshold)
                        {
                            if (fRectangle.lineKeys[0] == sRectangle.lineKeys[1])
                            {
                                AddTriangle(
                                    fRectangle.notCutIndex0,
                                    fRectangle.notCutIndex1,
                                    sRectangle.notCutIndex0,
                                    sub
                                );

                                fRectangle.notCutIndex0 = sRectangle.notCutIndex0;
                                fRectangle.parameters[0] = sRectangle.parameters[0];
                                fRectangle.newVertices[0] = sRectangle.newVertices[0];
                                fRectangle.lineKeys[0] = sRectangle.lineKeys[0];
                                rectanglesList.RemoveAt(second);
                                second -= 1;
                                goto FINDFRAGMENT3;
                            }
                            else if (fRectangle.lineKeys[0] == sRectangle.lineKeys[1])
                            {
                                AddTriangle(
                                    fRectangle.notCutIndex0,
                                    fRectangle.notCutIndex1,
                                    sRectangle.notCutIndex1,
                                    sub
                                );

                                fRectangle.notCutIndex1 = sRectangle.notCutIndex1;
                                fRectangle.parameters[1] = sRectangle.parameters[1];
                                fRectangle.newVertices[1] = sRectangle.newVertices[1];
                                fRectangle.lineKeys[1] = sRectangle.lineKeys[1];
                                rectanglesList.RemoveAt(second);
                                second -= 1;

                                goto FINDFRAGMENT3;
                            }
                        }
                    }
                FINDFRAGMENT3:;
                }

                //最後にまとめて出力


                if (sewMesh && triangleList.Count + rectanglesList.Count > 0)
                {
                    int criterionIndex;


                    if (triangleList.Count > 0)
                    {
                        TriangleFragment tri0 = triangleList[0];
                        AddTriangle(tri0, sub);
                        MakeCutSurfaceVertex(tri0, sub);
                        criterionIndex = tri0.vertexIndex0;
                        triangleList.RemoveAt(0);

                    }
                    else
                    {
                        RectangleFragment rec0 = rectanglesList[0];
                        AddTriangle(rec0, sub);
                        MakeCutSurfaceVertex(rec0, sub);
                        criterionIndex = rec0.vertexIndex1;
                        rectanglesList.RemoveAt(0);
                    }

                    bool detect = false;

                    //まずは側面のフラグメントを出力
                    for (int i = 0; i < triangleList.Count; i++)
                    {
                        TriangleFragment tri = triangleList[i];
                        AddTriangle(tri, sub);
                        MakeCutSurfaceVertex(tri, sub);//ここでsubの代わりに新しいマテリアルのindexを入れれば切断面専用のマテリアルをつけることができる
                        if (!detect && tri.vertexIndex1 == criterionIndex)
                        {
                            detect = true;
                            triangleList.RemoveAt(i);
                            i -= 1;
                        }
                    }
                    for (int j = 0; j < rectanglesList.Count; j++)
                    {
                        RectangleFragment rec = rectanglesList[j];
                        AddTriangle(rec, sub);
                        MakeCutSurfaceVertex(rec, sub);
                        if (!detect && rec.vertexIndex0 == criterionIndex)
                        {
                            detect = true;
                            rectanglesList.RemoveAt(j);
                            j -= 1;
                        }
                    }



                    //切断面を出力
                    foreach (TriangleFragment tri in triangleList)
                    {

                        Sew(tri.vertexIndex1, tri.vertexIndex0, criterionIndex, sub);
                    }
                    foreach (RectangleFragment rec in rectanglesList)
                    {
                        Sew(rec.vertexIndex0, rec.vertexIndex1, criterionIndex, sub);
                    }
                }
                else
                {
                    foreach (TriangleFragment triangle in triangleList)
                    {
                        AddTriangle(triangle, sub);
                    }

                    foreach (RectangleFragment rectangle in rectanglesList)
                    {
                        AddTriangle(rectangle, sub);
                    }
                }

            }
        }

        //三角ポリゴンの追加
        public void AddTriangle(int p1, int p2, int p3, int submeshNum)
        {
            int trackNum;
            //ここの処理はポリゴン数によっては数万回呼ばれるのでSetVertexは使わずに直に書いている(関数呼び出しにかかる時間はよくわからないけどちょっと速くなるはず)
            if ((trackNum = trackedArray[p1]) != 0)
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                trackedArray[p1] = trackedVertexNum;

                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[p1]);
                normals.Add(_targetNormals[p1]);
                uvs.Add(_targetUVs[p1]);
                trackedVertexNum++;
            }

            if ((trackNum = trackedArray[p2]) != 0)
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                trackedArray[p2] = trackedVertexNum;

                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[p2]);
                normals.Add(_targetNormals[p2]);
                uvs.Add(_targetUVs[p2]);
                trackedVertexNum++;
            }

            if ((trackNum = trackedArray[p3]) != 0)
            {
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                trackedArray[p3] = trackedVertexNum;

                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[p3]);
                normals.Add(_targetNormals[p3]);
                uvs.Add(_targetUVs[p3]);
                trackedVertexNum++;
            }
        }
        public void AddTriangle(int notCutIndex0, int notCutIndex1, Vector3 cutPoint, Vector3 normal, Vector2 uv, int submeshNum)
        {
            SetVertex(notCutIndex0, submeshNum); //1つ目の頂点登録
            SetVertex(notCutIndex1, submeshNum); //2つ目の頂点登録

            //3つ目の頂点は切断によって新規作成された頂点なので処理が異なる
            subMeshIndices[submeshNum].Add(trackedVertexNum);
            triangles.Add(trackedVertexNum);
            vertices.Add(cutPoint);
            normals.Add(normal);
            uvs.Add(uv);
            trackedVertexNum++;
        }
        public void AddTriangle(int notCutIndex, Vector3 cutPoint1, Vector3 cutPoint2, Vector3[] normals2, Vector2[] uvs2, int submeshNum)
        {
            SetVertex(notCutIndex, submeshNum);

            subMeshIndices[submeshNum].Add(trackedVertexNum);
            triangles.Add(trackedVertexNum);
            vertices.Add(cutPoint1);
            normals.Add(normals2[0]); //normal配列には新しい頂点の2つ分しか入ってない
            uvs.Add(uvs2[0]);
            trackedVertexNum++;

            subMeshIndices[submeshNum].Add(trackedVertexNum);
            triangles.Add(trackedVertexNum);
            vertices.Add(cutPoint2);
            normals.Add(normals2[1]);
            uvs.Add(uvs2[1]);
            trackedVertexNum++;

        }
        void AddTriangle(TriangleFragment tri, int submeshNum)
        {
            int p0 = tri.notCutIndex;
            int L0 = tri.lostIndex0;
            int L1 = tri.lostIndex1;



            SetVertex(p0, submeshNum);

            SetFragment(p0, L0, tri.lineKeys[0], tri.parameters[0], tri.newVertices[0], submeshNum);
            SetFragment(p0, L1, tri.lineKeys[1], tri.parameters[1], tri.newVertices[1], submeshNum);


        }
        void AddTriangle(RectangleFragment rec, int submeshNum)
        {
            int p0 = rec.notCutIndex0;
            int p1 = rec.notCutIndex1;
            int L0 = rec.lostIndex0;
            int L1 = rec.lostIndex1;

            SetVertex(p0, submeshNum);

            SetVertex(p1, submeshNum);

            SetFragment(p1, L1, rec.lineKeys[1], rec.parameters[1], rec.newVertices[1], submeshNum);
            //ここから2つ目のポリゴン
            SetFragment(p1, L1, rec.lineKeys[1], rec.parameters[1], rec.newVertices[1], submeshNum);
            SetFragment(p0, L0, rec.lineKeys[0], rec.parameters[0], rec.newVertices[0], submeshNum);

            SetVertex(p0, submeshNum);


        }

        //もともとのMeshに含まれていた頂点を新しいMeshに追加する関数. 
        //すでに登録された頂点かどうかを判断(同じ頂点が複数の三角面を構成しているときはこれがないと三角面の数だけ頂点が増えてしまう)
        void SetVertex(int index, int submeshNum)
        {
            int trackNum;
            if ((trackNum = trackedArray[index]) != 0)
            {
                //すでに登録されているときは頂点情報は追加しない
                subMeshIndices[submeshNum].Add(trackNum);
                triangles.Add(trackNum);
            }
            else
            {
                //まだ登録されてないときはtrackedArrayに登録後, 頂点情報の追加
                trackedArray[index] = trackedVertexNum;//最初に登録される頂点はindexが0なので登録されてもif文では登録されてない扱いを受けてしまうので,ClearAllで最初に0番目はつめてしまっている)(頂点の登録は三角面のindexで行われるので三角面を構成しない0番目は次回のCutでは呼ばれない)
                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                vertices.Add(_targetVertices[index]);
                normals.Add(_targetNormals[index]);
                uvs.Add(_targetUVs[index]);
                trackedVertexNum++;
            }
        }


        //このDictionaryは切断で新しくできた頂点を登録しておいて, すでに登録されていたら新規追加しないようにして頂点数を抑えるのに用いる 
        Dictionary<int, int> trackDic = new Dictionary<int, int>();
        //切断で新しく生成された頂点を新しいMeshに追加する関数
        private void SetFragment(int notCutIndex, int lostIndex, int key, float parameter, Vector3 newVertexPos, int submeshNum)
        {
            //新規作成された頂点はどの点とどの点間にあるかで識別(notCutIndex(切断面よりこっち側)-lostIndex(向こう側))
            //keyではビットシフトをして1つのint値に2つの頂点のindexのintを一意に埋め込んでいる(32bitを16bit-16bitにわけてる)
            if (trackDic.TryGetValue(key, out int index))
            {
                //すでに登録されてる頂点だったら頂点の追加はせずに三角面だけ追加
                subMeshIndices[submeshNum].Add(index);
                triangles.Add(index);
            }
            else
            {
                //登録されてないときはDictionary に登録して頂点情報追加, さらに三角面も追加
                trackDic.Add(key, trackedVertexNum);

                subMeshIndices[submeshNum].Add(trackedVertexNum);
                triangles.Add(trackedVertexNum);
                Vector3 normal = Vector3.Lerp(_targetNormals[notCutIndex], _targetNormals[lostIndex], parameter);
                Vector2 uv = Vector2.Lerp(_targetUVs[notCutIndex], _targetUVs[lostIndex], parameter);
                vertices.Add(newVertexPos);
                normals.Add(normal);
                uvs.Add(uv);

                trackedVertexNum++;
            }
        }

        //切断面を構成する頂点が増えないように登録しておく.
        //trackDicと分けている理由はCubeなど位置が同じで法線が違う頂点があったときに側面は区別したいけど切断面では1つにまとめたいから
        Dictionary<Vector3, int> cutSurfaceTrackDic = new Dictionary<Vector3, int>();
        void MakeCutSurfaceVertex(TriangleFragment tri, int submeshNum)
        {
            int index = 0;
            Vector3 v = tri.newVertices[0];
            if (cutSurfaceTrackDic.TryGetValue(v, out index))
            {
                tri.vertexIndex0 = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(v, trackedVertexNum);
                vertices.Add(tri.newVertices[0]);
                normals.Add(cutNormal);
                uvs.Add(new Vector2(v.x, v.z));//暫定的な処置
                tri.vertexIndex0 = trackedVertexNum;
                trackedVertexNum++;
            }

            v = tri.newVertices[1];
            if (cutSurfaceTrackDic.TryGetValue(v, out index))
            {
                tri.vertexIndex1 = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(v, trackedVertexNum);
                vertices.Add(tri.newVertices[1]);
                normals.Add(cutNormal);
                uvs.Add(new Vector2(v.x, v.z));//暫定的な処置
                tri.vertexIndex1 = trackedVertexNum;
                trackedVertexNum++;
            }
        }
        void MakeCutSurfaceVertex(RectangleFragment rec, int submeshNum)
        {
            int index = 0;
            Vector3 v = rec.newVertices[0];
            if (cutSurfaceTrackDic.TryGetValue(v, out index))
            {
                rec.vertexIndex0 = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(v, trackedVertexNum);
                vertices.Add(rec.newVertices[0]);
                normals.Add(cutNormal);
                uvs.Add(new Vector2(v.x, v.z));//暫定的な処置
                rec.vertexIndex0 = trackedVertexNum;
                trackedVertexNum++;
            }

            v = rec.newVertices[1];
            if (cutSurfaceTrackDic.TryGetValue(v, out index))
            {
                rec.vertexIndex1 = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(v, trackedVertexNum);
                vertices.Add(rec.newVertices[1]);
                normals.Add(cutNormal);
                uvs.Add(new Vector2(v.x, v.z));//暫定的な処置
                rec.vertexIndex1 = trackedVertexNum;
                trackedVertexNum++;
            }
        }


        //最後の縫い合わせを行う(AddTriangleとやってることはほぼ同じ). ここに入る頂点はすでに登録済みのものだけなはずなのでtrackチェックを行わない
        void Sew(int p1, int p2, int p3, int submeshNum)
        {
            subMeshIndices[submeshNum].Add(p1);
            triangles.Add(p1);
            subMeshIndices[submeshNum].Add(p2);
            triangles.Add(p2);
            subMeshIndices[submeshNum].Add(p3);
            triangles.Add(p3);
        }


        public void SetZeroVertex()
        {
            if (trackedVertexNum > 1)
            {
                vertices[0] = vertices[1];
            }
        }

        public void ClearAll()//staticなclassとして使っているので毎回中身をresetする必要あり
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            subMeshIndices.Clear();
            triangleFragments.Clear();
            rectangleFragments.Clear();

            trackedArray = new int[_targetVertices.Length];
            trackedVertexNum = 0;

            trackDic.Clear();
            cutSurfaceTrackDic.Clear();

            vertices.Add(Vector3.zero);
            normals.Add(Vector3.zero);
            uvs.Add(Vector2.zero);
            trackedVertexNum++;
        }

    }

    //三角ポリゴンを分割するとき, 三角になったほうをTriangleFragmentに, 四角になったほうをRectangleFragmentにいれている
    //これらは分割中はListにためておくだけにして, 最後にくっつけられるもの(同一平面にあるやつ)はくっつけて出力

    const int slide = 16;//32bit(intのデータ数)の半分
    public class TriangleFragment//三角ポリゴン. 
    {
        //ポリゴンの切断後に自分側にある頂点(notCut)と切り離された頂点(lost)のindex
        //時計回りの順番になっていて, この順番でtriangleに登録すると正しい方向を向く
        public int notCutIndex, lostIndex0, lostIndex1;
        public Vector3[] newVertices;//切断で新しくできた頂点
        public float[] parameters;//newVerticesがnotCutIndexとlostIndexの間でどこらへんにあるかを表す(0～1の間の値. 0だったらnotCut側)
        public Vector3 cutLine;//切断でできた線(newVertixcesをつなぐ線)
        public int vertexIndex0, vertexIndex1;//切断面を縫合わせる際, 同じ頂点はまとめて同じindexにして頂点数を抑えるのに用いる
        public int[] lineKeys;
        public TriangleFragment(int _notCutIndex, int _lostIndex0, int _lostIndex1, Vector3[] _newVertices, float[] _parameter, Vector3 _cutLine)
        {
            notCutIndex = _notCutIndex;
            lostIndex0 = _lostIndex0;
            lostIndex1 = _lostIndex1;
            newVertices = _newVertices;
            parameters = _parameter;
            cutLine = _cutLine;
            lineKeys = new int[2] { (_notCutIndex << slide) + _lostIndex0, (_notCutIndex << slide) + _lostIndex1 };
        }
    }

    public class RectangleFragment//四角ポリゴン
    {

        //頂点の順番が若干ややこしくてnotCutIndex0→notCutIndex1→lostIndex1→lostIndex0の順で時計回りになっている(こうするとnotCutとlostの数字が1同士, 0同士になって都合がいい)
        public int notCutIndex0, notCutIndex1, lostIndex0, lostIndex1;
        public Vector3[] newVertices;
        public float[] parameters;
        public Vector3 cutLine;
        public int vertexIndex0, vertexIndex1;
        public int[] lineKeys;
        public RectangleFragment(int _notCutIndex0, int _notCutIndex1, int _lostIndex, Vector3[] _newVertices, float[] _parameter, Vector3 _cutLine)
        {
            notCutIndex0 = _notCutIndex0;
            notCutIndex1 = _notCutIndex1;
            lostIndex0 = lostIndex1 = _lostIndex;
            newVertices = _newVertices;
            parameters = _parameter;
            cutLine = _cutLine;
            lineKeys = new int[2] { (_notCutIndex0 << slide) + _lostIndex, (_notCutIndex1 << slide) + _lostIndex };
        }
    }

}



*/


//これはやや速いけどコードが汚い
/*
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MeshCut2 : MonoBehaviour
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

        int verticesLength = _targetVertices.Length;
        _trackedArray = new int[verticesLength];

        Vector3 scale = targetTransform.localScale;//localscaleに合わせてPlaneに入れるnormalに補正をかける


        _slashPlane = new Plane(Vector3.Scale(scale, targetTransform.transform.InverseTransformDirection(planeNormalDirection)), targetTransform.transform.InverseTransformPoint(planeAnchorPoint));
        _planeNormal = Vector3.Scale(scale, targetTransform.transform.InverseTransformDirection(planeNormalDirection));
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        _planeVlue = Vector3.Dot(_planeNormal, anchor);

        _frontMeshData.cutNormal = -_slashPlane.normal;
        _backMeshData.cutNormal = _slashPlane.normal;//それぞれ法線を入力
        _frontMeshData.sewMesh = sewMesh;
        _backMeshData.sewMesh = sewMesh;//切断面を縫い合わせるかどうか


        _isFront = new bool[verticesLength];
        {
            verticesLength += 300;
            Vector3[] frontVertices = new Vector3[verticesLength];
            Vector3[] backVertices = new Vector3[verticesLength];
            Vector3[] frontNormals = new Vector3[verticesLength];
            Vector3[] backNormals = new Vector3[verticesLength];
            Vector2[] frontUVs = new Vector2[verticesLength];
            Vector2[] backUVs = new Vector2[verticesLength];
            int frontCount = 0;
            int backCount = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 pos = _targetVertices[i];
                Vector3 v = pos - anchor;
                //planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
                if (_isFront[i] = (_planeNormal.x * v.x + _planeNormal.y * v.y + _planeNormal.z * v.z) > 0)
                {
                    frontVertices[frontCount] = pos;
                    frontNormals[frontCount] = _targetNormals[i];
                    frontUVs[frontCount] = _targetUVs[i];
                    _trackedArray[i] = frontCount;
                    frontCount++;
                }
                else
                {
                    backVertices[backCount] = pos;
                    backNormals[backCount] = _targetNormals[i];
                    backUVs[backCount] = _targetUVs[i];
                    _trackedArray[i] = backCount;
                    backCount++;
                }
            }

            _frontMeshData.SetVertex(frontVertices, frontNormals, frontUVs, frontCount);
            _backMeshData.SetVertex(backVertices, backNormals, backUVs, backCount);
        }


        for (int sub = 0; sub < _targetMesh.subMeshCount; sub++)
        {
            int[] indices;
            indices = _targetMesh.GetIndices(sub);

            _frontMeshData.subMeshIndices.Add(new List<int>());//subMeshが増えたことを追加してる

            _backMeshData.subMeshIndices.Add(new List<int>());


            _frontMeshData.fragmentDicList.Add(new Dictionary<int, List<Fragment>>());
            _backMeshData.fragmentDicList.Add(new Dictionary<int, List<Fragment>>());

            bool side;
            int p1, p2, p3;

            for (int i = 0; i < indices.Length; i += 3)
            {
                p1 = indices[i];
                p2 = indices[i + 1];
                p3 = indices[i + 2];


                //予め計算しておいた結果を持ってくる(ここで計算すると同じ頂点にたいして何回も同じ計算をすることになるから最初にまとめてやっている(そのほうが処理時間が速かった))
                side = _isFront[p1];


                if (side == _isFront[p2] && side == _isFront[p3])//3つとも表側, 3つとも裏側のときはそのまま出力
                {
                    if (side)//表側か裏側かを識別
                    {
                        int index_of_thisMesh;

                        index_of_thisMesh = _trackedArray[p1];
                        _frontMeshData.subMeshIndices[sub].Add(index_of_thisMesh);
                        _frontMeshData.triangles.Add(index_of_thisMesh);

                        index_of_thisMesh = _trackedArray[p2];
                        _frontMeshData.subMeshIndices[sub].Add(index_of_thisMesh);
                        _frontMeshData.triangles.Add(index_of_thisMesh);

                        index_of_thisMesh = _trackedArray[p3];
                        _frontMeshData.subMeshIndices[sub].Add(index_of_thisMesh);
                        _frontMeshData.triangles.Add(index_of_thisMesh);
                    }
                    else
                    {
                        int index_of_thisMesh;

                        index_of_thisMesh = _trackedArray[p1];
                        _backMeshData.subMeshIndices[sub].Add(index_of_thisMesh);
                        _backMeshData.triangles.Add(index_of_thisMesh);

                        index_of_thisMesh = _trackedArray[p2];
                        _backMeshData.subMeshIndices[sub].Add(index_of_thisMesh);
                        _frontMeshData.triangles.Add(index_of_thisMesh);

                        index_of_thisMesh = _trackedArray[p3];
                        _frontMeshData.subMeshIndices[sub].Add(index_of_thisMesh);
                        _frontMeshData.triangles.Add(index_of_thisMesh);
                    }
                }
                else
                {
                    //三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する.
                    Sepalate(new bool[3] { side, _isFront[p2], _isFront[p3] }, new int[3] { p1, p2, p3 }, sub);
                }

            }
        }


        _frontMeshData.ConnectFragments(); //切断されたMeshの破片は最後にくっつけられるところはくっつけて出力
        _frontMeshData.SetUp();
        _backMeshData.ConnectFragments();
        _backMeshData.SetUp();

        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";
        frontMesh.vertices = _frontMeshData.verticesArray;
        frontMesh.triangles = _frontMeshData.triangles.ToArray();
        frontMesh.normals = _frontMeshData.normalsArray;
        frontMesh.uv = _frontMeshData.uvsArray;

        frontMesh.subMeshCount = _frontMeshData.subMeshIndices.Count;
        for (int i = 0; i < _frontMeshData.subMeshIndices.Count; i++)
        {
            frontMesh.SetIndices(_frontMeshData.subMeshIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh back";
        backMesh.vertices = _backMeshData.verticesArray;
        backMesh.triangles = _backMeshData.triangles.ToArray();
        backMesh.normals = _backMeshData.normalsArray;
        backMesh.uv = _backMeshData.uvsArray;

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
            targetGameObject.GetComponent<MeshCollider>().sharedMesh = meshes[0];
            fragment.AddComponent<MeshCollider>().sharedMesh = meshes[1];
        }
        if (targetGameObject.GetComponent<Rigidbody>())
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

        Dictionary<int, List<Fragment>> frontFragmentDic = _frontMeshData.fragmentDicList[submesh];
        Dictionary<int, List<Fragment>> backFragmentDic = _backMeshData.fragmentDicList[submesh];



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
        //public List<Vector3> vertices = new List<Vector3>(maxVerticesSize);
        //public List<Vector3> normals = new List<Vector3>(maxVerticesSize);
        //public List<Vector2> uvs = new List<Vector2>(maxVerticesSize);



        public Vector3[] verticesArray;
        private Vector3[] temp_verticesArray;
        public Vector3[] normalsArray;
        private Vector3[] temp_normalsArray;
        public Vector2[] uvsArray;
        private Vector2[] temp_uvsArray;


        public void SetVertex(Vector3[] _verticesArray, Vector3[] _normalsArray, Vector2[] _uvsArray, int count)
        {
            temp_verticesArray = _verticesArray;
            temp_normalsArray = _normalsArray;
            temp_uvsArray = _uvsArray;

            trackedVertexNum += count;
        }



        public List<int> triangles = new List<int>(maxVerticesSize * 2);
        public List<List<int>> subMeshIndices = new List<List<int>>(maxVerticesSize * 2);

        public List<Dictionary<int, List<Fragment>>> fragmentDicList = new List<Dictionary<int, List<Fragment>>>();


        public bool sewMesh = true;//切断後にMeshの切断面を縫い合わせるか
        public Vector3 cutNormal;   //切断面の法線



        int trackedVertexNum = 0; //登録された頂点の数



        //public void SetVertex(int index)
        //{
        //    vertices.Add(_targetVertices[index]);
        //    normals.Add(_targetNormals[index]);
        //    uvs.Add(_targetUVs[index]);

        //    _trackedArray[index] = trackedVertexNum;

        //    trackedVertexNum++;
        //}



        const float threshold = 0.0001f;
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

                        fFragment.newVertices[0].KEY_POSITION = MakeIntFromVector3(fFragment.newVertices[0].position);
                        fFragment.newVertices[1].KEY_POSITION = MakeIntFromVector3(fFragment.newVertices[1].position);
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
                    criterionIndex = firstFragment.newVertices[0].cutSurfaceIndex;
                    outPutFragments.RemoveAt(0);

                    int cutSurfaceSubmesh = 0;

                    foreach (Fragment fragment in outPutFragments)
                    {
                        AddTriangle(fragment, submesh);
                        SetCutSurfaceVertex(fragment);
                        if (fragment.newVertices[1].cutSurfaceIndex != criterionIndex)
                        {
                            Sew(fragment.newVertices[1].cutSurfaceIndex,
                                fragment.newVertices[0].cutSurfaceIndex,
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
            triangles.Add(index_of_thisMesh);

            index_of_thisMesh = _trackedArray[p2];
            subMeshIndices[submeshNum].Add(index_of_thisMesh);
            triangles.Add(index_of_thisMesh);

            index_of_thisMesh = _trackedArray[p3];
            subMeshIndices[submeshNum].Add(index_of_thisMesh);
            triangles.Add(index_of_thisMesh);

        }

        void AddTriangle(Fragment fragment, int submeshNum)
        {
            if (fragment.isRectangle)
            {
                SetTriangle(fragment.newVertices[1].leftVertexIndex, submeshNum);
                SetTriangle(fragment.newVertices[0].leftVertexIndex, submeshNum);
                SetNewVertex(fragment.newVertices[1], submeshNum);
            }

            SetTriangle(fragment.newVertices[0].leftVertexIndex, submeshNum);
            SetNewVertex(fragment.newVertices[0], submeshNum);
            SetNewVertex(fragment.newVertices[1], submeshNum);
        }


        //もともとのMeshに含まれていた頂点を新しいMeshに追加する関数. 
        //すでに登録された頂点かどうかを判断(同じ頂点が複数の三角面を構成しているときはこれがないと三角面の数だけ頂点が増えてしまう)
        void SetTriangle(int index, int submeshNum)
        {
            int index_of_thisMesh = _trackedArray[index];

            subMeshIndices[submeshNum].Add(index_of_thisMesh);
            triangles.Add(index_of_thisMesh);
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
                triangles.Add(vertexIndex);
            }
            else
            {
                vertexIndex = trackedVertexNum;
                float parameter = newVertex.dividingParameter;
                int left = newVertex.leftVertexIndex;
                int lost = newVertex.lostVertexIndex;

                trackDic.Add(newVertex.KEY_INDEX, vertexIndex);

                subMeshIndices[submeshNum].Add(vertexIndex);
                triangles.Add(vertexIndex);
                temp_normalsArray[trackedVertexNum] = Vector3.Lerp(_targetNormals[left], _targetNormals[lost], parameter);
                temp_verticesArray[trackedVertexNum] = newVertex.position;
                temp_uvsArray[trackedVertexNum] = Vector2.Lerp(_targetUVs[left], _targetUVs[lost], parameter);

                trackedVertexNum++;
            }


        }

        //切断面を構成する頂点が増えないように登録しておく.
        //trackDicと分けている理由はCubeなど位置が同じで法線が違う頂点があったときに側面は区別したいけど切断面では1つにまとめたいから
        Dictionary<int, int> cutSurfaceTrackDic = new Dictionary<int, int>();

        void SetCutSurfaceVertex(Fragment fragment)
        {
            int index = 0;
            NewVertex newVertex = fragment.newVertices[0];
            if (cutSurfaceTrackDic.TryGetValue(newVertex.KEY_POSITION, out index))
            {
                newVertex.cutSurfaceIndex = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(newVertex.KEY_POSITION, trackedVertexNum);
                temp_normalsArray[trackedVertexNum] = cutNormal;
                Vector3 position = newVertex.position;
                temp_verticesArray[trackedVertexNum] = position;
                temp_uvsArray[trackedVertexNum] = new Vector2(position.x, position.z);//暫定的な処置
                newVertex.cutSurfaceIndex = trackedVertexNum;
                trackedVertexNum++;
            }


            newVertex = fragment.newVertices[1];
            if (cutSurfaceTrackDic.TryGetValue(newVertex.KEY_POSITION, out index))
            {
                newVertex.cutSurfaceIndex = index;
            }
            else
            {
                cutSurfaceTrackDic.Add(newVertex.KEY_POSITION, trackedVertexNum);
                temp_normalsArray[trackedVertexNum] = cutNormal;
                Vector3 position = newVertex.position;
                temp_verticesArray[trackedVertexNum] = position;
                temp_uvsArray[trackedVertexNum] = new Vector2(position.x, position.z);//暫定的な処置
                newVertex.cutSurfaceIndex = trackedVertexNum;
                trackedVertexNum++;
            }
        }


        //最後の縫い合わせを行う(AddTriangleとやってることはほぼ同じ). ここに入る頂点はすでに登録済みのものだけなはずなのでtrackチェックを行わない
        void Sew(int p1, int p2, int p3, int submeshNum)
        {
            subMeshIndices[submeshNum].Add(p1);
            triangles.Add(p1);
            subMeshIndices[submeshNum].Add(p2);
            triangles.Add(p2);
            subMeshIndices[submeshNum].Add(p3);
            triangles.Add(p3);
        }


        public void SetUp()
        {
            int num = trackedVertexNum;
            verticesArray = new Vector3[num];
            normalsArray = new Vector3[num];
            uvsArray = new Vector2[num];
            Array.Copy(temp_verticesArray, verticesArray, num);
            Array.Copy(temp_normalsArray, normalsArray, num);
            Array.Copy(temp_uvsArray, uvsArray, num);
        }

        public void ClearAll()//staticなclassとして使っているので毎回中身をresetする必要あり
        {

            triangles.Clear();
            subMeshIndices.Clear();
            fragmentDicList.Clear();

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
        public Fragment(NewVertex newVertex0, NewVertex newVertex1, bool _isRectangle)
        {
            newVertices = new NewVertex[2] { newVertex0, newVertex1 };
            isRectangle = _isRectangle;
        }
    }

    public class NewVertex
    {
        public int leftVertexIndex;
        public int lostVertexIndex;
        public float dividingParameter;
        public int KEY_INDEX;
        public Vector3 position;
        public int KEY_POSITION;
        public int cutSurfaceIndex;

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


 */


