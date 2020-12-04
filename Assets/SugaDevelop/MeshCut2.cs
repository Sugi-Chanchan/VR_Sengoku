using System;
using System.Collections.Generic;
using UnityEngine;


public class MeshCut2 : MonoBehaviour
{
    static Mesh _targetMesh;
    static Vector3[] _targetVertices;
    static Vector3[] _targetNormals;
    static Vector2[] _targetUVs;   //この3つはめっちゃ大事でこれ書かないと10倍くらい重くなる(for文中で使うから参照渡しだとやばい)

    //平面の方程式はn・r=h(nは法線,rは位置ベクトル,hはconst(=_planeValue))
    static Vector3 _planeNormal;
    static float _planeValue;

    static UnsafeList<bool> _isFront_List = new UnsafeList<bool>(SIZE);
    static UnsafeList<int> _trackedArray_List = new UnsafeList<int>(SIZE);

    static bool[] _isFront;//頂点が切断面に対して表にあるか裏にあるか
    static int[] _trackedArray;//切断後のMeshでの切断前の頂点の番号

    static bool _makeCutSurface;

    static Dictionary<int, (int, int)> newVertexDic = new Dictionary<int, (int, int)>(101);


    static FragmentList fragmentList = new FragmentList();
    static RoopFragmentCollection roopCollection = new RoopFragmentCollection();


    //UnsafeListはListの中身の配列を引きずり出して直接書き換えるために自作したクラス. 高速だけど安全性が低い
    const int SIZE = 200;
    static UnsafeList<Vector3> _frontVertices = new UnsafeList<Vector3>(SIZE);//想定されるモデルの頂点数分の領域を予め空けておく
    static UnsafeList<Vector3> _backVertices = new UnsafeList<Vector3>(SIZE);
    static UnsafeList<Vector3> _frontNormals = new UnsafeList<Vector3>(SIZE);
    static UnsafeList<Vector3> _backNormals = new UnsafeList<Vector3>(SIZE);
    static UnsafeList<Vector2> _frontUVs = new UnsafeList<Vector2>(SIZE);
    static UnsafeList<Vector2> _backUVs = new UnsafeList<Vector2>(SIZE);

    static UnsafeList<UnsafeList<int>> _frontSubmeshIndices = new UnsafeList<UnsafeList<int>>(SIZE * 3);
    static UnsafeList<UnsafeList<int>> _backSubmeshIndices = new UnsafeList<UnsafeList<int>>(SIZE * 3);

    /// <summary>
    /// <para>gameObjectを切断して2つのMeshにして返します.1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です.</para>
    /// <para>何度も切るようなオブジェクトでも頂点数が増えないように処理をしてあるほか, 簡単な物体なら切断面を縫い合わせることもできます</para>
    /// </summary>
    /// <param name="target">切断対象のgameObject</param>
    /// <param name="planeAnchorPoint">切断面上の1点</param>
    /// <param name="planeNormalDirection">切断面の法線</param>
    /// <param name="makeCutSurface">切断後にMeshを縫い合わせるか否か(切断面が2つ以上できるときはfalseにしてシェーダーのCull Frontで切断面を表示するようにする)</param>
    /// <returns></returns>
    public static Mesh[] CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface, Material cutSurfaceMaterial = null)
    {


        //初期化
        {
            _targetMesh = targetMesh; //Mesh情報取得
            //for文で_targetMeshから呼ぶのは非常に重くなるのでここで配列に格納してfor文ではここから渡す(Mesh.verticesなどは参照ではなくて毎回コピーしたものを返してるっぽい)
            _targetVertices = _targetMesh.vertices;
            _targetNormals = _targetMesh.normals;
            _targetUVs = _targetMesh.uv;


            int verticesLength = _targetVertices.Length;
            _makeCutSurface = makeCutSurface;

            _trackedArray_List.Clear(verticesLength);//Listのサイズを確保_trackedArray_Listはここで配列のサイズを整えるためだけに使用
            _trackedArray = _trackedArray_List.unsafe_array;//中身の配列を割り当て
            _isFront_List.Clear(verticesLength);
            _isFront = _isFront_List.unsafe_array;
            newVertexDic.Clear();
            roopCollection.Clear();
            fragmentList.Clear();

            _frontVertices.Clear(verticesLength); //List.Clear()とほぼ同じ挙動
            _frontNormals.Clear(verticesLength);
            _frontUVs.Clear(verticesLength);
            _frontSubmeshIndices.Clear(2);

            _backVertices.Clear(verticesLength);
            _backNormals.Clear(verticesLength);
            _backUVs.Clear(verticesLength);
            _backSubmeshIndices.Clear(2);

            Vector3 scale = targetTransform.localScale;//localscaleに合わせてPlaneに入れるnormalに補正をかける
            _planeNormal = Vector3.Scale(scale, targetTransform.transform.InverseTransformDirection(planeNormalDirection));
        }



        //最初に頂点の情報だけを入力していく

        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        _planeValue = Vector3.Dot(_planeNormal, anchor);
        {
            //UnsafeListから中身の配列を取り出す(配列の要素数はverticesLengthなので要素数を超えたアクセスは発生しない)
            //List.Addよりもちょっと早い
            Vector3[] frontVertices_array = _frontVertices.unsafe_array;
            Vector3[] backVertices_array = _backVertices.unsafe_array;
            Vector3[] frontNormals_array = _frontNormals.unsafe_array;
            Vector3[] backNormals_array = _backNormals.unsafe_array;
            Vector2[] frontUVs_array = _frontUVs.unsafe_array;
            Vector2[] backUVs_array = _backUVs.unsafe_array;

            float pnx = _planeNormal.x;
            float pny = _planeNormal.y;
            float pnz = _planeNormal.z;

            float ancx = anchor.x;
            float ancy = anchor.y;
            float ancz = anchor.z;

            int frontCount = 0;
            int backCount = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 pos = _targetVertices[i];
                //planeの表側にあるか裏側にあるかを判定.(たぶん表だったらtrue)
                if (_isFront[i] = (pnx * (pos.x - ancx) + pny * (pos.y - ancy) + pnz * (pos.z - ancz)) > 0)
                {
                    //頂点情報を入力
                    frontVertices_array[frontCount] = pos;
                    frontNormals_array[frontCount] = _targetNormals[i];
                    frontUVs_array[frontCount] = _targetUVs[i];
                    //もとのMeshのn番目の頂点が新しいMeshで何番目になるのかを記録
                    _trackedArray[i] = frontCount++;
                }
                else
                {
                    backVertices_array[backCount] = pos;
                    backNormals_array[backCount] = _targetNormals[i];
                    backUVs_array[backCount] = _targetUVs[i];

                    _trackedArray[i] = backCount++;
                }
            }

            //配列に入れた要素数と同じだけcountをすすめる
            _frontVertices.unsafe_count = frontCount;
            _frontNormals.unsafe_count = frontCount;
            _frontUVs.unsafe_count = frontCount;
            _backVertices.unsafe_count = backCount;
            _backNormals.unsafe_count = backCount;
            _backUVs.unsafe_count = backCount;
        }

        //次に, 三角ポリゴンの情報を追加していく
        int submeshCount = _targetMesh.subMeshCount;

        for (int sub = 0; sub < submeshCount; sub++)
        {

            int[] indices = _targetMesh.GetIndices(sub);



            int indicesLength = indices.Length;
            _frontSubmeshIndices.AddOnlyCount();
            _frontSubmeshIndices.Top = _frontSubmeshIndices.Top?.Clear(indicesLength) ?? new UnsafeList<int>(indicesLength);
            _backSubmeshIndices.AddOnlyCount();
            _backSubmeshIndices.Top = _backSubmeshIndices.Top?.Clear(indicesLength) ?? new UnsafeList<int>(indicesLength);


            //リストから配列を引き出す
            UnsafeList<int> frontIndices = _frontSubmeshIndices[sub];
            int[] frontIndices_array = frontIndices.unsafe_array;
            int frontIndicesCount = 0;
            UnsafeList<int> backIndices = _backSubmeshIndices[sub];
            int[] backIndices_array = backIndices.unsafe_array;
            int backIndicesCount = 0;

            //ポリゴンの情報は頂点3つで1セットなので3つ飛ばしでループ
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
                    //indicesは切断前のMeshの頂点番号が入っているので_trackedArrayを通すことで新しいMeshでの番号に変えている
                    frontIndices_array[frontIndicesCount++] = _trackedArray[p1];
                    frontIndices_array[frontIndicesCount++] = _trackedArray[p2];
                    frontIndices_array[frontIndicesCount++] = _trackedArray[p3];
                }
                else if (!side1 && !side2 && !side3)
                {
                    backIndices_array[backIndicesCount++] = _trackedArray[p1];
                    backIndices_array[backIndicesCount++] = _trackedArray[p2];
                    backIndices_array[backIndicesCount++] = _trackedArray[p3];
                }
                else  //三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する.
                {
                    //Sepalate内で三角面の追加が行われることがあるのでここでUnsafeListのカウントをすすめておく
                    frontIndices.unsafe_count = frontIndicesCount;
                    backIndices.unsafe_count = backIndicesCount;
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { p1, p2, p3 }, sub);
                    frontIndicesCount = frontIndices.unsafe_count;
                    backIndicesCount = backIndices.unsafe_count;
                }

            }
            //最後にUnsafeListのカウントを進めておく
            frontIndices.unsafe_count = frontIndicesCount;
            backIndices.unsafe_count = backIndicesCount;
        }


        fragmentList.MakeTriangle();//切断されたポリゴンはここでそれぞれのMeshに追加される

        if (makeCutSurface)
        {
            if (cutSurfaceMaterial == null)//特に切断面のマテリアル指定がなければ0番のマテリアルを当てる
            {
                roopCollection.MakeCutSurface(0);//切断面を縫い合わせる
            }
            else
            {
                MeshRenderer renderer;
                if (renderer = targetTransform.GetComponent<MeshRenderer>())
                {
                    Material[] mats = renderer.materials;
                    int matLength = mats.Length;
                    if (mats[matLength - 1]?.name == cutSurfaceMaterial.name)//すでに切断マテリアルが追加されているときはそれを使うので追加しない
                    {
                        roopCollection.MakeCutSurface(matLength - 1);
                    }
                    else
                    {
                        Material[] newMats = new Material[matLength + 1];
                        mats.CopyTo(newMats, 0);
                        newMats[matLength] = cutSurfaceMaterial;


                        renderer.materials = newMats;
                        renderer.materials[matLength].name = cutSurfaceMaterial.name;//prefabのインスタンス化などで名前が変わってしまうことを防ぐ


                        _frontSubmeshIndices.Add(new UnsafeList<int>(20));//submeshが増えるのでリスト追加
                        _backSubmeshIndices.Add(new UnsafeList<int>(20));
                        roopCollection.MakeCutSurface(matLength);//マテリアル追加
                    }
                }
                else
                {
                    Debug.LogError("plese set MeshRenderer in target object");
                    roopCollection.MakeCutSurface(0);
                }
            }

        }

        //2つのMeshを新規に作ってそれぞれに情報を追加して出力
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        //unity2019.4以降ならこっちを使うだけで3～4割速くなる(unity2019.2以前は対応していない.2019.3は知らない)
        //int fcount = _frontVertices.unsafe_count;//unity2019.4以降
        //frontMesh.SetVertices(_frontVertices.unsafe_array, 0, fcount);//unity2019.4以降
        //frontMesh.SetNormals(_frontNormals.unsafe_array, 0, fcount);//unity2019.4以降
        //frontMesh.SetUVs(0, _frontUVs.unsafe_array, 0, fcount);//unity2019.4以降
        frontMesh.vertices = _frontVertices.ToArray();//unity2019.2以前
        frontMesh.normals = _frontNormals.ToArray();//unity2019.2以前
        frontMesh.uv = _frontUVs.ToArray();//unity2019.2以前



        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for (int i = 0; i < _frontSubmeshIndices.Count; i++)
        {
            frontMesh.SetIndices(_frontSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);//unity2019.2以前
            //frontMesh.SetIndices(_frontSubmeshIndices[i].unsafe_array, 0, _frontSubmeshIndices[i].unsafe_count, MeshTopology.Triangles, i, false);//unity2019.4以降
        }


        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh back";
        //int bcount = _backVertices.unsafe_count;//unity2019.4以降
        //backMesh.SetVertices(_backVertices.unsafe_array, 0, bcount);//unity2019.4以降
        //backMesh.SetNormals(_backNormals.unsafe_array, 0, bcount);//unity2019.4以降
        //backMesh.SetUVs(0, _backUVs.unsafe_array, 0, bcount);//unity2019.4以降
        backMesh.vertices = _backVertices.ToArray();//unity2019.2以前
        backMesh.normals = _backNormals.ToArray();//unity2019.2以前
        backMesh.uv = _backUVs.ToArray();//unity2019.2以前

        backMesh.subMeshCount = _backSubmeshIndices.Count;
        for (int i = 0; i < _backSubmeshIndices.Count; i++)
        {
            backMesh.SetIndices(_backSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);//unity2019.2以前
            //backMesh.SetIndices(_backSubmeshIndices[i].unsafe_array, 0, _backSubmeshIndices[i].unsafe_count, MeshTopology.Triangles, i, false);//unity2019.4以降
        }

        //Destroy(_targetMesh); //明示的に消してあげることでガベコレの処理が軽くなるかも?


        return new Mesh[2] { frontMesh, backMesh };
    }

    /// <summary>
    /// Meshを切断します. 
    /// 配列の1番目の要素には入力したGameObjectが入っています
    /// </summary>
    /// <param name="targetGameObject">切断されるGameObject</param>
    /// <param name="planeAnchorPoint">切断平面上のどこか1点</param>
    /// <param name="planeNormalDirection">切断平面の法線</param>
    /// <param name="makeCutSurface">切断面を作るかどうか</param>
    /// <returns></returns>
    public static GameObject[] CutMesh(GameObject targetGameObject, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface)
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

        Mesh[] meshes = CutMesh(mesh, transform, planeAnchorPoint, planeNormalDirection, makeCutSurface);

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



    //ポリゴンを切断
    //ポリゴンは切断面の表側と裏側に分割される.
    //このとき三角ポリゴンを表面から見て, なおかつ切断面の表側にある頂点が下に来るように見て,
    //三角形の左側の辺を形成する点をf0,b0, 右側にある辺を作る点をf1,b1とする.(fは表側にある点でbは裏側)(頂点は3つなので被りが存在する)
    //ここでポリゴンの向きを決めておくと後々とても便利
    //以降左側にあるものは0,右側にあるものは1をつけて扱う(例外はあるかも)
    //(ひょっとすると実際の向きは逆かもしれないけどvertexIndicesと同じまわり方で出力してるので逆でも問題はない.ここでは3点が時計回りで並んでいると仮定して全部の)
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        int f0 = 0, f1 = 0, b0 = 0, b1 = 0; //頂点のindex番号を格納するのに使用
        bool twoPointsInFrontSide;//どちらがに頂点が2つあるか

        //ポリゴンの向きを揃える
        if (sides[0])
        {
            if (sides[1])
            {
                f0 = vertexIndices[1];
                f1 = vertexIndices[0];
                b0 = b1 = vertexIndices[2];
                twoPointsInFrontSide = true;
            }
            else
            {
                if (sides[2])
                {
                    f0 = vertexIndices[0];
                    f1 = vertexIndices[2];
                    b0 = b1 = vertexIndices[1];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    f0 = f1 = vertexIndices[0];
                    b0 = vertexIndices[1];
                    b1 = vertexIndices[2];
                    twoPointsInFrontSide = false;
                }
            }
        }
        else
        {
            if (sides[1])
            {
                if (sides[2])
                {
                    f0 = vertexIndices[2];
                    f1 = vertexIndices[1];
                    b0 = b1 = vertexIndices[0];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    f0 = f1 = vertexIndices[1];
                    b0 = vertexIndices[2];
                    b1 = vertexIndices[0];
                    twoPointsInFrontSide = false;
                }
            }
            else
            {
                f0 = f1 = vertexIndices[2];
                b0 = vertexIndices[0];
                b1 = vertexIndices[1];
                twoPointsInFrontSide = false;
            }
        }

        //切断前のポリゴンの頂点の座標を取得(そのうち2つはかぶってる)
        Vector3 frontPoint0, frontPoint1, backPoint0, backPoint1;
        if (twoPointsInFrontSide)
        {
            frontPoint0 = _targetVertices[f0];
            frontPoint1 = _targetVertices[f1];
            backPoint0 = backPoint1 = _targetVertices[b0];
        }
        else
        {
            frontPoint0 = frontPoint1 = _targetVertices[f0];
            backPoint0 = _targetVertices[b0];
            backPoint1 = _targetVertices[b1];
        }

        //ベクトル[backPoint0 - frontPoint0]を何倍したら切断平面に到達するかは以下の式で表される
        //平面の式: dot(r,n)=A ,Aは定数,nは法線, 
        //今回    r =frontPoint0+k*(backPoint0 - frontPoint0), (0 ≦ k ≦ 1)
        //これは, 新しくできる頂点が2つの頂点を何対何に内分してできるのかを意味している
        float dividingParameter0 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint0)) / (Vector3.Dot(_planeNormal, backPoint0 - frontPoint0));
        //Lerpで切断によってうまれる新しい頂点の座標を生成
        Vector3 newVertexPos0 = Vector3.Lerp(frontPoint0, backPoint0, dividingParameter0);


        float dividingParameter1 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint1)) / (Vector3.Dot(_planeNormal, backPoint1 - frontPoint1));
        Vector3 newVertexPos1 = Vector3.Lerp(frontPoint1, backPoint1, dividingParameter1);

        //新しい頂点の生成, ここではNormalとUVは計算せず後から計算できるように頂点のindex(_trackedArray[f0], _trackedArray[b0],)と内分点の情報(dividingParameter0)を持っておく
        NewVertex vertex0 = fragmentList.MakeVertex(_trackedArray[f0], _trackedArray[b0], dividingParameter0, newVertexPos0);
        NewVertex vertex1 = fragmentList.MakeVertex(_trackedArray[f1], _trackedArray[b1], dividingParameter1, newVertexPos1);


        //切断でできる辺(これが同じポリゴンは結合することで頂点数の増加を抑えられる)
        Vector3 cutLine = (newVertexPos1 - newVertexPos0).normalized;
        int KEY_CUTLINE = MakeIntFromVector3_ErrorCut(cutLine);//Vector3だと処理が重そうなのでintにしておく, ついでに丸め誤差を切り落とす

        //切断情報を含んだFragmentクラス
        Fragment fragment = fragmentList.MakeFragment(vertex0, vertex1, twoPointsInFrontSide, KEY_CUTLINE, submesh);
        //Listに追加してListの中で同一平面のFragmentは結合とかする
        fragmentList.Add(fragment, KEY_CUTLINE, submesh);

    }

    class RoopFragment
    {
        public RoopFragment next; //右隣のやつ
        public Vector3 rightPosition;//右側の座標(左側の座標は左隣のやつがもってる)
        public RoopFragment(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
        }
        public RoopFragment SetNew(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
            return this;
        }
    }
    class RooP
    {
        public RoopFragment start, end; //startが左端, endが右端
        //public int KEY_LEFT, KEY_RIGHT;
        public Vector3 startPos, endPos;
        public int count;
        public Vector3 center;
        public RooP(RoopFragment _left, RoopFragment _right, Vector3 _startPos, Vector3 _endPos, Vector3 rightPos)
        {
            start = _left;
            end = _right;
            startPos = _startPos;
            endPos = _endPos;
            count = 1;
            center = rightPos;
        }
    }

    public class RoopFragmentCollection
    {
        const int listSize = 31;
        List<RooP>[] leftLists = new List<RooP>[listSize];
        List<RooP>[] rightLists = new List<RooP>[listSize];
        UnsafeList<RoopFragment> roopFragments = new UnsafeList<RoopFragment>(100);

        public RoopFragmentCollection()
        {
            for (int i = 0; i < listSize; i++)
            {
                leftLists[i] = new List<RooP>(5);
                rightLists[i] = new List<RooP>(5);
            }
        }

        public void Add(Vector3 left, Vector3 right)
        {


            int KEY_LEFT = MakeIntFromVector3(left); //Vector3からintへ
            int KEY_RIGHT = MakeIntFromVector3(right);


            RoopFragment target;
            roopFragments.AddOnlyCount();
            roopFragments.Top = roopFragments.Top?.SetNew(right) ?? new RoopFragment(right);
            target = roopFragments.Top;

            //Dictionaryとにた処理
            int leftIndex = KEY_LEFT % listSize;//自分の左手の座標が格納されているindex 
            int rightIndex = KEY_RIGHT % listSize;//右手

            //自分の左手とくっつくのは相手の右手なので右手Listの中から自分の左手indexの場所を探す
            var rList = rightLists[leftIndex];
            RooP roop1 = null;
            bool find1 = false;
            int rcount = rList.Count;
            for (int i = 0; i < rcount; i++)
            {
                RooP temp = rList[i];
                if (temp.endPos == left)
                {
                    //roopの右手をtargetの右手に変える(roopは左端と右端の情報だけを持っている)
                    temp.end.next = target;
                    temp.end = target;
                    temp.endPos = right;
                    roop1 = temp;
                    //roopをリストから外す(あとで右手Listの自分の右手indexの場所に移すため)
                    rList.RemoveAt(i);
                    find1 = true;
                    break;
                }
            }
            var lList = leftLists[rightIndex];
            RooP roop2 = null;
            bool find2 = false;
            int lcount = lList.Count;
            for (int j = 0; j < lcount; j++)
            {
                roop2 = lList[j];
                if (right == roop2.startPos)
                {
                    if (roop1 == roop2)
                    {
                        //print("make roop");
                        roop1.count++;
                        roop1.center += right;
                        return;
                    }//roop1==roop2のとき, roopが完成したのでreturn

                    target.next = roop2.start;
                    roop2.start = target;
                    roop2.startPos = left;
                    lList.RemoveAt(j);
                    find2 = true;
                    break;
                }
            }

            if (find1)
            {
                if (find2)//2つのroopがくっついたとき
                {
                    roop1.end = roop2.end;
                    roop1.endPos = roop2.endPos;
                    roop1.count += roop2.count + 1;
                    roop1.center += roop2.center + right;
                    int key = MakeIntFromVector3(roop2.endPos) % listSize;
                    for (int i = 0; i < rightLists[key].Count; i++)
                    {
                        if (roop2 == rightLists[key][i])
                        {
                            rightLists[key][i] = roop1;
                        }
                    }

                }
                else//自分の左手とroopの右手がくっついたとき, 右手リストの自分の右手indexにroopをついか
                {
                    roop1.count++;
                    roop1.center += right;
                    rightLists[rightIndex].Add(roop1);
                }
            }
            else
            {
                if (find2)
                {
                    roop2.count++;
                    roop2.center += right;
                    leftLists[leftIndex].Add(roop2);
                }
                else//どこにもくっつかなかったとき, roopを作成, 追加
                {
                    RooP newRoop = new RooP(target, target, left, right, right);
                    rightLists[rightIndex].Add(newRoop);
                    leftLists[leftIndex].Add(newRoop);
                }
            }
        }


        public void MakeCutSurface(int submesh)
        {

            foreach (List<RooP> list in leftLists)
            {
                foreach (RooP rf in list)
                {
                    //roopFragmentのnextをたどっていくことでroopを一周できる

                    MakeVertex(rf.center / rf.count, out int center_f, out int center_b);

                    RoopFragment nowFragment = rf.start;

                    MakeVertex(nowFragment.rightPosition, out int first_f, out int first_b);
                    int previous_f = first_f;
                    int previous_b = first_b;

                    int count = 0;
                    while (nowFragment.next != null)
                    {
                        nowFragment = nowFragment.next;


                        MakeVertex(nowFragment.rightPosition, out int index_f, out int index_b);

                        _frontSubmeshIndices[submesh].Add(center_f);
                        _frontSubmeshIndices[submesh].Add(index_f);
                        _frontSubmeshIndices[submesh].Add(previous_f);

                        _backSubmeshIndices[submesh].Add(center_b);
                        _backSubmeshIndices[submesh].Add(previous_b);
                        _backSubmeshIndices[submesh].Add(index_b);

                        previous_f = index_f;
                        previous_b = index_b;


                        if (count > 1000) //何かあったときのための安全装置(while文こわい)
                        {
                            Debug.LogError("Something is wrong?");
                            break;
                        }
                        count++;
                    }
                    _frontSubmeshIndices[submesh].Add(center_f);
                    _frontSubmeshIndices[submesh].Add(first_f);
                    _frontSubmeshIndices[submesh].Add(previous_f);

                    _backSubmeshIndices[submesh].Add(center_b);
                    _backSubmeshIndices[submesh].Add(previous_b);
                    _backSubmeshIndices[submesh].Add(first_b);
                }
            }

            void MakeVertex(Vector3 vertexPos, out int findex, out int bindex)
            {
                findex = _frontVertices.Count;
                bindex = _backVertices.Count;
                Vector2 uv = new Vector2(vertexPos.x, vertexPos.z);//切断面のUVは適当
                _frontVertices.Add(vertexPos);
                _frontNormals.Add(-_planeNormal);
                _frontUVs.Add(uv);

                _backVertices.Add(vertexPos);
                _backNormals.Add(_planeNormal);
                _backUVs.Add(uv);

            }
        }

        public void Clear()
        {
            for (int i = 0; i < listSize; i++)
            {
                leftLists[i].Clear();
                rightLists[i].Clear();
            }
        }
    }

    public class Fragment
    {
        public NewVertex vertex0, vertex1;
        public bool twoPointsInFrontSide, twoPointsInBackSide;
        public int KEY_CUTLINE;
        public int submesh;//submesh番号(どのマテリアルを当てるか)

        public Fragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            twoPointsInFrontSide = _twoPointsInFrontSide;
            twoPointsInBackSide = !_twoPointsInFrontSide;
            KEY_CUTLINE = _KEY_CUTLINE;
            submesh = _submesh;
        }

        public Fragment SetNew(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            twoPointsInFrontSide = _twoPointsInFrontSide;
            twoPointsInBackSide = !_twoPointsInFrontSide;
            KEY_CUTLINE = _KEY_CUTLINE;
            submesh = _submesh;
            return this;
        }

        public void AddTriangle()
        {
            (int findex0, int bindex0) = vertex0.GetIndex(); //Vertexの中で新しく生成された頂点を登録してその番号だけを返している
            (int findex1, int bindex1) = vertex1.GetIndex();

            if (twoPointsInFrontSide)//表側に2こ頂点があるときはポリゴン数が一個増える. 裏側も同様
            {
                _frontSubmeshIndices[submesh].Add(vertex1.frontsideindex_of_frontMesh);
                _frontSubmeshIndices[submesh].Add(vertex0.frontsideindex_of_frontMesh);
                _frontSubmeshIndices[submesh].Add(findex1);
            }

            _frontSubmeshIndices[submesh].Add(vertex0.frontsideindex_of_frontMesh);
            _frontSubmeshIndices[submesh].Add(findex0);
            _frontSubmeshIndices[submesh].Add(findex1);

            if (twoPointsInBackSide)
            {
                _backSubmeshIndices[submesh].Add(vertex0.backsideindex_of_backMash);
                _backSubmeshIndices[submesh].Add(vertex1.backsideindex_of_backMash);
                _backSubmeshIndices[submesh].Add(bindex0);
            }
            _backSubmeshIndices[submesh].Add(vertex1.backsideindex_of_backMash);
            _backSubmeshIndices[submesh].Add(bindex1);
            _backSubmeshIndices[submesh].Add(bindex0);

            if (_makeCutSurface)
            {
                roopCollection.Add(vertex0.position, vertex1.position);//切断平面を形成する準備
            }
        }
    }

    //新しい頂点のNormalとUVは最後に生成するので, もともとある頂点をどの比で混ぜるかをdividingParameterが持っている
    public class NewVertex
    {
        public int frontsideindex_of_frontMesh; //frontVertices,frontNormals,frontUVsでの頂点の番号
        public int backsideindex_of_backMash;
        public float dividingParameter;
        public int KEY_VERTEX;
        public Vector3 position;

        public NewVertex(int front, int back, float parameter, Vector3 vertexPosition)
        {
            frontsideindex_of_frontMesh = front;
            backsideindex_of_backMash = back;
            KEY_VERTEX = (front << 16) | back;
            dividingParameter = parameter;
            position = vertexPosition;
        }

        public NewVertex SetNew(int front, int back, float parameter, Vector3 vertexPosition)
        {
            frontsideindex_of_frontMesh = front;
            backsideindex_of_backMash = back;
            KEY_VERTEX = (front << 16) | back;
            dividingParameter = parameter;
            position = vertexPosition;
            return this;
        }

        public (int findex, int bindex) GetIndex()
        {
            //法線とUVの情報はここで生成する
            Vector3 frontNormal, backNormal;
            Vector2 frontUV, backUV;

            frontNormal = _frontNormals[frontsideindex_of_frontMesh];
            frontUV = _frontUVs[frontsideindex_of_frontMesh];

            backNormal = _backNormals[backsideindex_of_backMash];
            backUV = _backUVs[backsideindex_of_backMash];



            Vector3 newNormal = Vector3.Lerp(frontNormal, backNormal, dividingParameter);
            Vector2 newUV = Vector2.Lerp(frontUV, backUV, dividingParameter);

            int findex, bindex;
            (int, int) trackNumPair;
            //同じ2つの点の間に生成される頂点は1つにまとめたいのでDictionaryを使う
            if (newVertexDic.TryGetValue(KEY_VERTEX, out trackNumPair))
            {
                findex = trackNumPair.Item1;//新しい頂点が表側のMeshで何番目か
                bindex = trackNumPair.Item2;
            }
            else
            {

                findex = _frontVertices.Count;
                _frontVertices.Add(position);
                _frontNormals.Add(newNormal);
                _frontUVs.Add(newUV);

                bindex = _backVertices.Count;
                _backVertices.Add(position);
                _backNormals.Add(newNormal);
                _backUVs.Add(newUV);

                newVertexDic.Add(KEY_VERTEX, (findex, bindex));

            }

            return (findex, bindex);
        }
    }

    public class FragmentList
    {
        const int listSize = 71;
        List<Fragment>[] fragmentLists = new List<Fragment>[listSize];//複数のListに分散させることで検索速度を上げている(Dictionaryを参考にした)
        UnsafeList<NewVertex> vertexRepository = new UnsafeList<NewVertex>(200);
        UnsafeList<Fragment> fragmentRepository = new UnsafeList<Fragment>(100);
        public FragmentList()
        {
            for (int i = 0; i < listSize; i++)
            {
                fragmentLists[i] = new List<Fragment>(10);
            }
        }
        public void Add(Fragment fragment, int KEY, int submesh)
        {

            //基本的な仕組みはDictionaryと同じ
            int listIndex = KEY % listSize;
            List<Fragment> flist = fragmentLists[listIndex];//同じ切断辺を持つFragmentは同じ場所に格納される(別のFragmentが入っていないわけではない)
            bool connect = false;
            //格納されているFragmentからくっつけられるやつを探す
            for (int i = flist.Count - 1; i >= 0; i--)
            {
                Fragment compareFragment = flist[i];
                if (fragment.KEY_CUTLINE == compareFragment.KEY_CUTLINE)//同じ切断辺をもつか判断
                {
                    Fragment left, right;
                    if (fragment.vertex0.KEY_VERTEX == compareFragment.vertex1.KEY_VERTEX)//fragmentがcompareFragmentに右側からくっつく場合
                    {
                        right = fragment;
                        left = compareFragment;
                    }
                    else if (fragment.vertex1.KEY_VERTEX == compareFragment.vertex0.KEY_VERTEX)//左側からくっつく場合
                    {
                        left = fragment;
                        right = compareFragment;
                    }
                    else
                    {
                        continue;//どっちでもないときは次のループへ
                    }

                    //同じ側に頂点が2つあるもの同士をくっつけるときは一部を出力しないといけない(Fragmentは2辺分の情報しかもてないので)
                    if (left.twoPointsInFrontSide && right.twoPointsInFrontSide)
                    {

                        _frontSubmeshIndices[submesh].Add(right.vertex1.frontsideindex_of_frontMesh);

                        _frontSubmeshIndices[submesh].Add(right.vertex0.frontsideindex_of_frontMesh);

                        _frontSubmeshIndices[submesh].Add(left.vertex0.frontsideindex_of_frontMesh);
                    }
                    else
                    {
                        bool twoside = left.twoPointsInFrontSide || right.twoPointsInFrontSide;
                        left.twoPointsInFrontSide = right.twoPointsInFrontSide = twoside;//どっちかが表側に2頂点もってたなら新しくできるやつも同じく2頂点もつ

                    }
                    if (left.twoPointsInBackSide && right.twoPointsInBackSide)
                    {

                        _backSubmeshIndices[submesh].Add(right.vertex0.backsideindex_of_backMash);

                        _backSubmeshIndices[submesh].Add(right.vertex1.backsideindex_of_backMash);

                        _backSubmeshIndices[submesh].Add(left.vertex0.backsideindex_of_backMash);
                    }
                    else
                    {
                        bool twoside = left.twoPointsInBackSide || right.twoPointsInBackSide;
                        left.twoPointsInBackSide = right.twoPointsInBackSide = twoside;
                    }

                    //結合を行う
                    //Fragmentがより広くなるように頂点情報を変える
                    left.vertex1 = right.vertex1;
                    right.vertex0 = left.vertex0;

                    //connectがtrueになっているということは2つのFragmentのあいだに新しいやつがはまって3つが1つになったということ
                    //connect==trueのとき, rightもleftもListにすでに登録されてるやつなのでどっちかを消してやる
                    if (connect)
                    {
                        flist.Remove(right);
                        break;
                    }

                    fragment = compareFragment;
                    connect = true;
                }
            }

            if (!connect)
            {
                flist.Add(fragment);
            }
        }


        public void MakeTriangle()
        {
            int sum = 0;
            foreach (List<Fragment> list in fragmentLists)
            {
                foreach (Fragment f in list)
                {
                    f.AddTriangle();
                    sum++;
                }
            }
        }

        public void Clear()
        {
            foreach (List<Fragment> f in fragmentLists)
            {
                f.Clear();

            }
            vertexRepository.Clear(200);
            fragmentRepository.Clear(100);
        }

        public NewVertex MakeVertex(int front, int back, float parameter, Vector3 vertexPosition)
        {
            vertexRepository.AddOnlyCount();
            vertexRepository.Top = vertexRepository.Top?.SetNew(front, back, parameter, vertexPosition) ?? new NewVertex(front, back, parameter, vertexPosition);
            return vertexRepository.Top;
        }

        public Fragment MakeFragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {

            fragmentRepository.AddOnlyCount();
            fragmentRepository.Top = fragmentRepository.Top?.SetNew(_vertex0, _vertex1, _twoPointsInFrontSide, _KEY_CUTLINE, _submesh) ?? new Fragment(_vertex0, _vertex1, _twoPointsInFrontSide, _KEY_CUTLINE, _submesh);
            return fragmentRepository.Top;
        }
    }


    class UnsafeList<T> //List.Add()が遅いのでListから配列を引きずり出して直接入力するために使用. 結構速い
    {
        public T[] unsafe_array;
        int capacity;
        public int unsafe_count;
        public int Count { get { return unsafe_count; } }//unsafe_countと違って安全
        public int Capacity { get { return capacity; } }//内部ではcapacityを使ったほうが速い

        public UnsafeList(int cap)
        {
            if (cap <= 0) { cap = 1; }
            unsafe_array = new T[cap];
            capacity = cap;
            unsafe_count = 0;
        }
        public T this[int index]
        {
            get
            {
                if (index >= unsafe_count) { Debug.LogError("index is out of range!!"); }
                return unsafe_array[index];
            }
            set
            {
                if (index >= unsafe_count) { Debug.LogError("index is out of range!!"); }
                unsafe_array[index] = value;
            }
        }

        public void SetCount_Unsafe(int _count)
        {
            if (capacity < _count)
            {
                var temp = new T[_count];
                Array.Copy(unsafe_array, temp, _count);
                unsafe_array = temp;
                capacity = _count;
            }
            unsafe_count = _count;
        }


        public void Add(T value)
        {
            if (capacity == unsafe_count)
            {
                capacity = (capacity) * 2;
                var temp = new T[capacity];
                Array.Copy(unsafe_array, temp, unsafe_count);
                unsafe_array = temp;
            }
            unsafe_array[unsafe_count++] = value;
        }

        public UnsafeList<T> Clear(int _minCapacity)//初期化と同時に拡張
        {
            if (capacity < _minCapacity)
            {
                var temp = new T[_minCapacity];
                unsafe_array = temp;
                capacity = _minCapacity;
            }
            unsafe_count = 0;
            return this;
        }

        public T[] ToArray()
        {
            var output = new T[unsafe_count];
            Array.Copy(unsafe_array, output, unsafe_count);
            return output;
        }

        public List<T> ToList()
        {
            var output = new T[unsafe_count];
            Array.Copy(unsafe_array, output, unsafe_count);
            return new List<T>(output);
        }

        //UnsafeListは雑な作りなのでClear()してもカウントを0にするだけで内部配列は変化しない
        //Listに参照型を入れる場合, 新しい要素を入れるよりもとからあるものを使いまわしたほうがGCが減って早くなるはず
        //Add_TryReuse()を使うことで要素数を1つ増やしてそれがnullかどうかを判断できる(nullじゃなかったらそのまま再利用)
        /// <summary>
        /// 要素を1つ増やして中身がnullでなければtrue
        /// </summary>
        /// <returns></returns>
        //public bool Add_TryReuse()
        //{
        //    if (capacity == unsafe_count)
        //    {
        //        capacity = (capacity) * 2;
        //        var temp = new T[capacity];
        //        Array.Copy(unsafe_array, temp, unsafe_count);
        //        unsafe_array = temp;
        //    }
        //    return (unsafe_array[unsafe_count++] != null);
        //}

        public void AddOnlyCount()
        {
            if (capacity == unsafe_count)
            {
                capacity = (capacity) * 2;
                var temp = new T[capacity];
                Array.Copy(unsafe_array, temp, unsafe_count);
                unsafe_array = temp;
            }
            unsafe_count++;
        }

        public T Top //listの先頭を返す
        {
            get
            {
                return unsafe_array[unsafe_count - 1];
            }
            set
            {
                unsafe_array[unsafe_count - 1] = value;
            }

        }
    }

    const int filter = 0x000003FF;
    const int amp = 1 << 18;//丸め誤差を落とすためにやや低めの倍率がかかっている
    public static int MakeIntFromVector3(Vector3 vec)
    {

        int cutLineX = ((int)(vec.x * amp) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }

    const int amp2 = 1 << 10;//丸め誤差を落とすためにやや低めの倍率がかかっている
    public static int MakeIntFromVector3_ErrorCut(Vector3 vec)
    {
        int cutLineX = ((int)(vec.x * amp2) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp2) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp2) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }
}

