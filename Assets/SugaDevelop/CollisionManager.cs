using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public delegate void SetCollisionHandler();

public class CollisionManager : MonoBehaviour
{

    public static event SetCollisionHandler SetCollision;

    public enum ColliderType//AND演算をして0にならない組み合わせでは衝突判定を行わない
    {
        CuttedAndCutter = 0b00,
        CuttedOnly = 0b01,
        CutterOnly = 0b10
    }

    private static UnsafeList<ColliderInfo> colliderInfoList = new UnsafeList<ColliderInfo>(20);//毎フレームここにColliderのデータが集まる

    //アニメーションより後で当たり判定を出すならLateUpdate内でこれを実行して, かつスクリプトの実行順でCollisionManagerが最後に来るようにする必要がある.
    public static void AddColliderDataList(ColliderInfo colliderData) //dictinaryとEnumを使ってポリゴンを追加する関数をかいた
    {
        colliderInfoList.Add(colliderData);
    }

    enum SortAxis { X_Axis = 0, Y_Axis = 1, Z_Axis = 2 }
    [Tooltip("Colliderをソートする軸を決める. 接触していないCollider同士が重ならないような軸を選ぶとよい")]
    [SerializeField] private SortAxis _sortAxis = SortAxis.X_Axis;
    static int sortAxis, axis1, axis2;
    private void Awake()
    {
        sortAxis = (int)_sortAxis;
        axis1 = (sortAxis + 1) % 3;
        axis2 = (sortAxis + 2) % 3;
    }

    void LateUpdate()
    {

        //Colliderが1つもなければreturn
        if (SetCollision == null) return;

        SetCollision();//PolygonColliderの各インスタンスのSendColliderInfoを呼びだす.ここでcolliderInfoListにColliderデータが集まる

        ColliderInfo[] colliderInfoArray = colliderInfoList.unsafe_array;//Listから配列を取り出す
        int arrayLength = colliderInfoList.unsafe_count;
        ArraySort(colliderInfoArray, 0, arrayLength - 1);//sortAxisに沿ってboundy_minの座標が小さい順に並べかえる

        for (int i = 0; i < arrayLength; i++)
        {
            ColliderInfo first = colliderInfoArray[i];
            Vector3 fistMaxBoundy = first.boundy_max;
            float firstMaxBoundySortAxis = fistMaxBoundy[sortAxis];
            Vector3 firstMinBoundy = first.boundy_min;

            int TYPE_FIRST = first.COLLIDER_TYPE;
            for (int j = i + 1; j < arrayLength; j++)
            {
                ColliderInfo second = colliderInfoArray[j];
                if (firstMaxBoundySortAxis < second.boundy_min[sortAxis]) { break; }//小さい順に並んでいるのでこれ以降の要素とは衝突しないことが保証されているのでbreak
                if ((TYPE_FIRST & second.COLLIDER_TYPE) != 0) { continue; }//cuttedOnly同士, cutterOnly同士は衝突判定を行わない
                if (fistMaxBoundy[axis1] < second.boundy_min[axis1] || firstMinBoundy[axis1] > second.boundy_max[axis1]) { continue; }
                if (fistMaxBoundy[axis2] < second.boundy_min[axis2] || firstMinBoundy[axis2] > second.boundy_max[axis2]) { continue; }
                CollisionCheck(first, second);

            }
        }

        colliderInfoList.Clear();


    }



    static Vector3 intersectionA0, intersectionA1, intersectionB0, intersectionB1;
    static Vector3[] hitPoints = new Vector3[2];
    const float threshold = 0.000000001f;
    void CollisionCheck(ColliderInfo A, ColliderInfo B)
    {

        foreach (Polygon polygonA in A.polygons)
        {
            foreach (Polygon polygonB in B.polygons)
            {
                if (CoarseCheck(polygonA, polygonB))
                {
                    float A0x = intersectionA0.x;
                    float A1x = intersectionA1.x;
                    float B0x;
                    float B1x;
                    if ((A0x - A1x) * (A0x - A1x) < threshold)
                    {
                        A0x = intersectionA0.y;
                        A1x = intersectionA1.y;
                        if ((A0x - A1x) * (A0x - A1x) < threshold)
                        {
                            A0x = intersectionA0.z;
                            A1x = intersectionA1.z;
                            B0x = intersectionB0.z;
                            B1x = intersectionB1.z;
                        }
                        else
                        {
                            B0x = intersectionB0.y;
                            B1x = intersectionB1.y;
                        }
                    }
                    else
                    {
                        B0x = intersectionB0.x;
                        B1x = intersectionB1.x;
                    }

                    if (A0x < A1x)
                    {
                        if (A1x < B1x)
                        {
                            if (A1x < B0x)
                            {
                                continue;
                            }
                            else
                            {
                                if (A0x > B0x)
                                {
                                    hitPoints[0] = intersectionA0;
                                    hitPoints[1] = intersectionA1;
                                }
                                else
                                {
                                    hitPoints[0] = intersectionB0;
                                    hitPoints[1] = intersectionA1;
                                }
                            }
                        }
                        else
                        {
                            if (A1x < B0x)
                            {
                                if (A0x > B1x)
                                {
                                    hitPoints[0] = intersectionA0;
                                    hitPoints[1] = intersectionA1;
                                }
                                else
                                {
                                    hitPoints[0] = intersectionB1;
                                    hitPoints[1] = intersectionA1;
                                }
                            }
                            else
                            {
                                if (B0x < A0x)
                                {
                                    if (B1x < A0x) { continue; }
                                    else
                                    {
                                        hitPoints[0] = intersectionB1;
                                        hitPoints[1] = intersectionA0;
                                    }
                                }
                                else
                                {
                                    if (B1x < A0x)
                                    {
                                        hitPoints[0] = intersectionB0;
                                        hitPoints[1] = intersectionA0;
                                    }
                                    else
                                    {
                                        hitPoints[0] = intersectionB0;
                                        hitPoints[1] = intersectionB1;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (A0x < B1x)
                        {
                            if (A0x < B0x)
                            {
                                continue;
                            }
                            else
                            {
                                if (A1x > B0x)
                                {
                                    hitPoints[0] = intersectionA0;
                                    hitPoints[1] = intersectionA1;
                                }
                                else
                                {
                                    hitPoints[0] = intersectionB0;
                                    hitPoints[1] = intersectionA0;
                                }
                            }
                        }
                        else
                        {
                            if (A0x < B0x)
                            {
                                if (A1x > B1x)
                                {
                                    hitPoints[0] = intersectionA0;
                                    hitPoints[1] = intersectionA1;
                                }
                                else
                                {
                                    hitPoints[0] = intersectionB1;
                                    hitPoints[1] = intersectionA0;
                                }
                            }
                            else
                            {
                                if (B0x < A1x)
                                {
                                    if (B1x < A1x) { continue; }
                                    else
                                    {
                                        hitPoints[0] = intersectionB1;
                                        hitPoints[1] = intersectionA1;
                                    }
                                }
                                else
                                {
                                    if (B1x < A1x)
                                    {
                                        hitPoints[0] = intersectionB0;
                                        hitPoints[1] = intersectionA1;
                                    }
                                    else
                                    {
                                        hitPoints[0] = intersectionB0;
                                        hitPoints[1] = intersectionB1;
                                    }
                                }
                            }
                        }
                    }

                    CollisionDetection(A, B, polygonA, polygonB, hitPoints);
                    return;
                }
            }
        }


        bool CoarseCheck(Polygon polygonA, Polygon polygonB)//アバウトな判定(これを突破した後に最後の判定がある)
        {

            //相手の平面に対して頂点3つとも同じ側にあるときは絶対に衝突していないのでそれをチェック(Mollerの衝突判定) //http://marupeke296.com/COL_3D_No21_TriTri.html

            Vector3 normal = polygonA.normal;
            Vector3 anchor = polygonA[0];
            (float side1D_1, float side1D_2, float side2D, Vector3 side1Pos_1, Vector3 side1Pos_2, Vector3 side2Pos) BInfo;

            Vector3 pos0 = polygonB.vertex0;
            Vector3 pos1 = polygonB.vertex1;
            Vector3 pos2 = polygonB.vertex2;

            float d0, d1, d2;
            float anc = normal.x * anchor.x + normal.y * anchor.y + normal.z * anchor.z;
            d0 = normal.x * pos0.x + normal.y * pos0.y + normal.z * pos0.z - anc;
            d1 = normal.x * pos1.x + normal.y * pos1.y + normal.z * pos1.z - anc;
            d2 = normal.x * pos2.x + normal.y * pos2.y + normal.z * pos2.z - anc;
            if (d0 > 0)
            {
                if (d1 > 0)
                {
                    if (d2 > 0)
                    {
                        return false;//3つとも同じ側にあるので衝突しない
                    }
                    else
                    {
                        BInfo = (d0, d1, -d2, pos0, pos1, pos2);
                    }
                }
                else
                {
                    if (d2 > 0)
                    {
                        BInfo = (d0, d2, -d1, pos0, pos2, pos1);
                    }
                    else
                    {
                        BInfo = (-d1, -d2, d0, pos1, pos2, pos0);
                    }
                }
            }
            else
            {
                if (d1 > 0)
                {
                    if (d2 > 0)
                    {
                        BInfo = (d1, d2, -d0, pos1, pos2, pos0);
                    }
                    else
                    {
                        BInfo = (-d0, -d2, d1, pos0, pos2, pos1);
                    }
                }
                else
                {
                    if (d2 > 0)
                    {
                        BInfo = (-d0, -d1, d2, pos0, pos1, pos2);
                    }
                    else
                    {
                        return false;//3つとも同じ側にあるので衝突しない
                    }
                }
            }


            normal = polygonB.normal;
            anchor = polygonB[0];
            (float side1D_1, float side1D_2, float side2D, Vector3 side1Pos_1, Vector3 side1Pos_2, Vector3 side2Pos) AInfo;


            pos0 = polygonA.vertex0;
            pos1 = polygonA.vertex1;
            pos2 = polygonA.vertex2;
            anc = normal.x * anchor.x + normal.y * anchor.y + normal.z * anchor.z;
            d0 = normal.x * pos0.x + normal.y * pos0.y + normal.z * pos0.z - anc;
            d1 = normal.x * pos1.x + normal.y * pos1.y + normal.z * pos1.z - anc;
            d2 = normal.x * pos2.x + normal.y * pos2.y + normal.z * pos2.z - anc;
            if (d0 > 0)
            {
                if (d1 > 0)
                {
                    if (d2 > 0)
                    {
                        return false;
                    }
                    else
                    {
                        AInfo = (d0, d1, -d2, pos0, pos1, pos2);
                    }
                }
                else
                {
                    if (d2 > 0)
                    {
                        AInfo = (d0, d2, -d1, pos0, pos2, pos1);
                    }
                    else
                    {
                        AInfo = (-d1, -d2, d0, pos1, pos2, pos0);
                    }
                }
            }
            else
            {
                if (d1 > 0)
                {
                    if (d2 > 0)
                    {
                        AInfo = (d1, d2, -d0, pos1, pos2, pos0);
                    }
                    else
                    {
                        AInfo = (-d0, -d2, d1, pos0, pos2, pos1);
                    }
                }
                else
                {
                    if (d2 > 0)
                    {
                        AInfo = (-d0, -d1, d2, pos0, pos1, pos2);
                    }
                    else
                    {
                        return false;
                    }
                }
            }



            //Mollerの衝突判定法を利用してそれぞれのポリゴンと相手の平面との交点を計算

            float dNormalized = AInfo.side1D_1 / (AInfo.side1D_1 + AInfo.side2D);
            intersectionA0 = (1 - dNormalized) * AInfo.side1Pos_1 + dNormalized * AInfo.side2Pos;
            dNormalized = AInfo.side1D_2 / (AInfo.side1D_2 + AInfo.side2D);
            intersectionA1 = (1 - dNormalized) * AInfo.side1Pos_2 + dNormalized * AInfo.side2Pos;

            dNormalized = BInfo.side1D_1 / (BInfo.side1D_1 + BInfo.side2D);
            intersectionB0 = (1 - dNormalized) * BInfo.side1Pos_1 + dNormalized * BInfo.side2Pos;
            dNormalized = BInfo.side1D_2 / (BInfo.side1D_2 + BInfo.side2D);
            intersectionB1 = (1 - dNormalized) * BInfo.side1Pos_2 + dNormalized * BInfo.side2Pos;

            return true;
        }
    }



    void CollisionDetection(ColliderInfo A, ColliderInfo B, Polygon polygonA, Polygon polygonB, Vector3[] hitPoints)//衝突を見つけたとき
    {
        CollisionInfo Ainfo = new CollisionInfo();
        CollisionInfo Binfo = new CollisionInfo();
        Ainfo.hitPoints = Binfo.hitPoints = hitPoints;
        Ainfo.hitPolygon = polygonA.Copy;
        Binfo.hitPolygon = polygonB.Copy;
        Ainfo.collisionObject = A.collisionObject;
        Binfo.collisionObject = B.collisionObject;
        A.OnCollision(Binfo);
        B.OnCollision(Ainfo);
    }

    //クイックソート(これ考えた人めっちゃ頭いい) //http://www.ics.kagoshima-u.ac.jp/~fuchida/edu/algorithm/sort-algorithm/quick-sort.html
    private void ArraySort(ColliderInfo[] infoArray, int start, int end)
    {
        if (start == end) return;
        float pivot;
        {
            int index = start + 1;
            float startValue = infoArray[start].boundy_min[sortAxis];
            while (index <= end && startValue == infoArray[index].boundy_min[sortAxis]) { index++; }
            if (index > end) return;
            pivot = (startValue >= infoArray[index].boundy_min[sortAxis]) ? startValue : infoArray[index].boundy_min[sortAxis];
        }

        int k;
        {
            int f = start, b = end;
            while (f <= b)
            {
                while (f <= end && infoArray[f].boundy_min[sortAxis] < pivot) { f++; }
                while (b >= start && infoArray[b].boundy_min[sortAxis] >= pivot) { b--; }
                if (f > b) break;
                ColliderInfo tmp = infoArray[f];
                infoArray[f] = infoArray[b];
                infoArray[b] = tmp;
                f++; b--;
            }
            k = f;
        }

        ArraySort(infoArray, start, k - 1);
        ArraySort(infoArray, k, end);

    }

}



public class ColliderInfo
{
    private PolygonCollider polygonCollider;
    public Polygon[] polygons;
    public GameObject collisionObject;
    public Vector3 boundy_max, boundy_min;
    public int COLLIDER_TYPE;

    public ColliderInfo(Polygon[] polygons, PolygonCollider polygonCollider,int _COLLIDER_TYPE, GameObject obj)
    {
        this.polygons = polygons;
        this.polygonCollider = polygonCollider;
        COLLIDER_TYPE = _COLLIDER_TYPE;
        collisionObject = obj;
    }
    public ColliderInfo() { }

    public ColliderInfo Set(Polygon[] polygons, PolygonCollider collisionObjectInstance,int _COLLIDER_TYPE, GameObject obj)
    {
        this.polygons = polygons;
        polygonCollider = collisionObjectInstance;
        COLLIDER_TYPE = _COLLIDER_TYPE;
        collisionObject = obj;
        return this;
    }

    public void SetBoundy((Vector3 min, Vector3 max) set)
    {
        boundy_min = set.min;
        boundy_max = set.max;
    }

    public void OnCollision(CollisionInfo info)
    {
        if (polygonCollider == null) { Debug.LogError("polygonCollider isn't setted in ColliderInfo!"); return; }
        polygonCollider.OnCollision(info);
    }

}

public class CollisionInfo
{
    public GameObject collisionObject;
    public Polygon hitPolygon;
    private Vector3[] hitpoints;
    public Vector3[] hitPoints
    {
        get
        {
            return new Vector3[2] { hitpoints[0], hitpoints[1] };
        }
        set
        {
            hitpoints = value;
        }
    }
}

public class Polygon
{
    public Vector3 vertex0, vertex1, vertex2;
    public Vector3 normal;
    public Vector3 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0: return vertex0;
                case 1: return vertex1;
                case 2: return vertex2;
                default: Debug.LogError("index must be from 0 to 2"); return vertex0;
            }
        }
    }
    public Polygon() { }
    public Polygon(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2)
    {
        vertex0 = _vertex0;
        vertex1 = _vertex1;
        vertex2 = _vertex2;
        MakeNormal();
    }
    public void Set(Vector3 _vertex0, Vector3 _vertex1, Vector3 _vertex2)
    {
        vertex0 = _vertex0;
        vertex1 = _vertex1;
        vertex2 = _vertex2;
        MakeNormal();
    }

    private void MakeNormal()
    {
        normal = new Vector3(
           vertex0.z * (vertex2.y - vertex1.y) + vertex1.z * (vertex0.y - vertex2.y) + vertex2.z * (vertex1.y - vertex0.y),
           vertex0.x * (vertex2.z - vertex1.z) + vertex1.x * (vertex0.z - vertex2.z) + vertex2.x * (vertex1.z - vertex0.z),
           vertex0.y * (vertex2.x - vertex1.x) + vertex1.y * (vertex0.x - vertex2.x) + vertex2.y * (vertex1.x - vertex0.x)
           );
        float magnitude = normal.magnitude;
        if (magnitude != 0)
        {
            normal.x /= magnitude;
            normal.y /= magnitude;
            normal.z /= magnitude;
        }
        else
        {
            normal = new Vector3(vertex0.y - vertex1.y, vertex1.x - vertex0.x, 0);
            magnitude = normal.magnitude;
            if (magnitude != 0)
            {
                normal.x /= magnitude;
                normal.y /= magnitude;
                normal.z /= magnitude;
            }
            else
            {
                normal = new Vector3(vertex2.y - vertex1.y, vertex1.x - vertex2.x, 0);
                magnitude = normal.magnitude;
                if (magnitude != 0)
                {
                    normal.x /= magnitude;
                    normal.y /= magnitude;
                    normal.z /= magnitude;
                }
                else
                {
                    normal = new Vector3(0, 0, 1);
                }
            }
        }
        //Debug.Log(normal);
    }

    public Polygon Copy { get { return (Polygon)MemberwiseClone(); } }
}

public abstract class PolygonCollider : MonoBehaviour //CollisionManagerで衝突を検知するためにはこれを継承する必要がある
{
    // 自分のコライダーの種類を定義する
    [SerializeField] protected CollisionManager.ColliderType colliderType = CollisionManager.ColliderType.CuttedAndCutter;
    int COLLIDER_TYPE;
    
    /// <summary>
    /// 衝突が検知されたらこれが呼ばれる
    /// </summary>
    public abstract void OnCollision(CollisionInfo collisionInfo);

    /// <summary>
    /// 衝突の判定に使うポリゴン(Vector3が3つで構成される三角形)の配列を作成
    /// </summary>
    /// <returns></returns>
    protected abstract Polygon[] SetPolygons();

    private bool _enableCollision = true;
    public bool enableCollision
    {
        get { return _enableCollision; }
        set
        {
            if (value != _enableCollision)
            {
                _enableCollision = value;
                if (value)
                {
                    _OnEnableCollision();
                }
                else
                {
                    _OnDisableCollision();
                }
            }
        }
    }

    protected virtual void OnEnable() { _OnEnableCollision(); }
    protected virtual void OnDisable() { _OnDisableCollision(); }
    protected virtual void OnEnableCollision() { }
    protected virtual void OnDisableCollision() { }

    private void _OnEnableCollision()
    {
        if (_enableCollision && enabled && gameObject.activeInHierarchy)
        {
            COLLIDER_TYPE = (int)colliderType;
            CollisionManager.SetCollision += SendCollisionData;
            OnEnableCollision();
        }
    }
    private void _OnDisableCollision()
    {
        CollisionManager.SetCollision -= SendCollisionData;
        OnDisableCollision();
    }


    ColliderInfo inputData = new ColliderInfo();
    private Polygon[] polygons;
    protected virtual void SendCollisionData()
    {
        polygons = SetPolygons();
        inputData.Set(polygons, this, COLLIDER_TYPE,this.gameObject);
        inputData.SetBoundy(CalculateBoundy());
        CollisionManager.AddColliderDataList(inputData);
    }

    protected virtual (Vector3 boundy_min, Vector3 boundy_max) CalculateBoundy()//ポリゴンが完全にはいる箱の大きさを計算.(処理を軽くしたければoverride)
    {
        Vector3 min = polygons[0].vertex0;
        Vector3 max = polygons[0].vertex0;
        foreach (Polygon poly in polygons)
        {
            min = Vector3.Min(min, poly.vertex0);
            min = Vector3.Min(min, poly.vertex1);
            min = Vector3.Min(min, poly.vertex2);
            max = Vector3.Max(max, poly.vertex0);
            max = Vector3.Max(max, poly.vertex1);
            max = Vector3.Max(max, poly.vertex2);
        }
        return (min, max);
    }
}

public abstract class StickColliderDynamic : PolygonCollider //棒の運動を扱うにはこれを継承すると便利
{
    [SerializeField] Transform start, end;
    private Vector3 prePos_start, prePos_end;

    public void SetCollider(Transform start,Transform end)
    {
        this.start = start;
        this.end = end;
    }

    public Vector3 startPos
    {
        get { return start.position; }
        set { if (start != null) { start.position = value; } else { Debug.LogError("null!"); } }
    }
    public Vector3 endPos
    {
        get { return end.position; }
        set { if (end != null) { end.position = value; } else { Debug.LogError("null!"); } }
    }

    protected override void OnEnableCollision()
    {
        prePos_start = start.position;
        prePos_end = end.position;
    }


    private Polygon[] polygons = new Polygon[2];
    Polygon polygon1 = new Polygon();
    Polygon polygon2 = new Polygon();
    protected override Polygon[] SetPolygons()
    {
        polygon1.Set(prePos_start, prePos_end, start.position);
        polygon2.Set(start.position, end.position, prePos_end);
        polygons[0] = polygon1;
        polygons[1] = polygon2;
        return polygons;
    }

    protected override (Vector3 boundy_min, Vector3 boundy_max) CalculateBoundy()
    {
        Vector3 ps = prePos_start;
        Vector3 pe = prePos_end;
        Vector3 startpos = start.position;
        Vector3 endpos = end.position;

        float minTemp1, minTemp2, maxTemp1, maxTemp2;
        if (ps.x > pe.x)
        {
            minTemp1 = pe.x;
            maxTemp1 = ps.x;
        }
        else
        {
            minTemp1 = ps.x;
            maxTemp1 = pe.x;
        }
        if (startpos.x > endpos.x)
        {
            minTemp2 = endpos.x;
            maxTemp2 = startpos.x;
        }
        else
        {
            minTemp2 = startpos.x;
            maxTemp2 = endpos.x;
        }
        float minx = (minTemp1 > minTemp2) ? minTemp2 : minTemp1;
        float maxx = (maxTemp1 > maxTemp2) ? maxTemp1 : maxTemp2;

        if (ps.y > pe.y)
        {
            minTemp1 = pe.y;
            maxTemp1 = ps.y;
        }
        else
        {
            minTemp1 = ps.y;
            maxTemp1 = pe.y;
        }
        if (startpos.y > endpos.y)
        {
            minTemp2 = endpos.y;
            maxTemp2 = startpos.y;
        }
        else
        {
            minTemp2 = startpos.y;
            maxTemp2 = endpos.y;
        }
        float miny = (minTemp1 > minTemp2) ? minTemp2 : minTemp1;
        float maxy = (maxTemp1 > maxTemp2) ? maxTemp1 : maxTemp2;
        if (ps.z > pe.z)
        {
            minTemp1 = pe.z;
            maxTemp1 = ps.z;
        }
        else
        {
            minTemp1 = ps.z;
            maxTemp1 = pe.z;
        }
        if (startpos.z > endpos.z)
        {
            minTemp2 = endpos.z;
            maxTemp2 = startpos.z;
        }
        else
        {
            minTemp2 = startpos.z;
            maxTemp2 = endpos.z;
        }
        float minz = (minTemp1 > minTemp2) ? minTemp2 : minTemp1;
        float maxz = (maxTemp1 > maxTemp2) ? maxTemp1 : maxTemp2;

        return (new Vector3(minx, miny, minz), new Vector3(maxx, maxy, maxz));
    }

    protected override void SendCollisionData()
    {
        base.SendCollisionData();
        prePos_start = start.position;
        prePos_end = end.position;
    }
}


