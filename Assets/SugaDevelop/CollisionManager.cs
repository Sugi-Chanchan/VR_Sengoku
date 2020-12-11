using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public delegate void SetCollisionHandler();

public class CollisionManager : MonoBehaviour
{

    

    public static event SetCollisionHandler SetCollision;

    public enum ColliderType
    {
        CuttedAndCutter,
        CuttedOnly,
        CutterOnly
    }

    private static List<ColliderInfo> cuttedAndCutter = new List<ColliderInfo>();
    private static List<ColliderInfo> cuttedOnly = new List<ColliderInfo>();
    private static List<ColliderInfo> cutterOnly = new List<ColliderInfo>();
    public static bool exist = false;

    //アニメーションより後で当たり判定を出すならLateUpdate内でこれを実行して, かつスクリプトの実行順でCollisionManagerが最後に来るようにする必要がある.
    public static void AddColliderDataList(ColliderInfo colliderData, ColliderType colliderType) //dictinaryとEnumを使ってポリゴンを追加する関数をかいた
    {

        switch (colliderType)
        {
            case ColliderType.CuttedAndCutter: cuttedAndCutter.Add(colliderData); break;
            case ColliderType.CuttedOnly: cuttedOnly.Add(colliderData); break;
            case ColliderType.CutterOnly: cutterOnly.Add(colliderData); break;
            default: cuttedAndCutter.Add(colliderData); break;
        }
    }

    private void Awake()
    {
        exist = true;
    }
    private void OnDestroy()
    {
        exist = false;
    }


    void LateUpdate()
    {

        SetCollision();

        for (int i = 0; i < cuttedAndCutter.Count; i++)
        {
            ColliderInfo cutted_cutter = cuttedAndCutter[i];
            foreach (ColliderInfo cutted in cuttedOnly)
            {
                CollisionCheck(cutted_cutter, cutted);
            }

            for (int j = i + 1; j < cuttedAndCutter.Count; j++)
            {
                CollisionCheck(cutted_cutter, cuttedAndCutter[j]);
            }
        }

        foreach (ColliderInfo cutter in cutterOnly)
        {
            foreach (ColliderInfo cutted in cuttedOnly)
            {
                CollisionCheck(cutter, cutted);
            }
            foreach (ColliderInfo cutted_cutter in cuttedAndCutter)
            {
                CollisionCheck(cutter, cutted_cutter);
            }
        }

        Clear();
    }

    void CollisionCheck3(ColliderInfo A, ColliderInfo B)
    {
        foreach (Polygon polygonA in A.polygons)
        {
            foreach (Polygon polygonB in B.polygons)
            {
                Vector3 _triangleVector1 = polygonA.vertices[1] - polygonA.vertices[0];
                Vector3 _triangleVector2 = polygonA.vertices[2] - polygonA.vertices[0];
                Vector3 hitPoint;

                for (int i = 0; i < 3; i++)
                {
                    if (OverrapCheck(polygonA.vertices[0], _triangleVector1, _triangleVector2, polygonB.vertices[i], polygonB.vertices[(i + 1) % 3] - polygonB.vertices[i], out hitPoint))
                    {
                        CollisionDetection(A, B, polygonA, polygonB, new Vector3[2] { hitPoint, Vector3.zero });
                        return;
                    };
                }

                _triangleVector1 = polygonB.vertices[1] - polygonB.vertices[0];
                _triangleVector2 = polygonB.vertices[2] - polygonB.vertices[0];
                for (int i = 0; i < 3; i++)
                {
                    if (OverrapCheck(polygonB.vertices[0], _triangleVector1, _triangleVector2, polygonA.vertices[i], polygonA.vertices[(i + 1) % 3] - polygonA.vertices[i], out hitPoint))
                    {
                        CollisionDetection(A, B, polygonA, polygonB, new Vector3[2] { hitPoint, Vector3.zero });
                        return;
                    };
                }
            }
        }
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


        bool CoarseCheck(Polygon polygonA, Polygon polygonB)
        {
            Vector3 normal = polygonB.normal;
            Vector3 anchor = polygonB[0];
            (float side1D_1, float side1D_2, float side2D, Vector3 side1Pos_1, Vector3 side1Pos_2, Vector3 side2Pos) AInfo;
            float d0, d1, d2;

            Vector3[] poss = polygonA.vertices;
            Vector3 pos0 = poss[0];
            Vector3 pos1 = poss[1];
            Vector3 pos2 = poss[2];
            float anc = normal.x * anchor.x + normal.y * anchor.y + normal.z * anchor.z;
            d0 = normal.x * pos0.x + normal.y * pos0.y + normal.z * pos0.z-anc;
            d1 = normal.x * pos1.x + normal.y * pos1.y + normal.z * pos1.z-anc;
            d2 = normal.x * pos2.x + normal.y * pos2.y + normal.z * pos2.z-anc;
            //d0 = Vector3.Dot(normal, pos0 - anchor);
            //d1 = Vector3.Dot(normal, pos1 - anchor);
            //d2 = Vector3.Dot(normal, pos2 - anchor);
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



            normal = polygonA.normal;
            anchor = polygonA[0];
            (float side1D_1, float side1D_2, float side2D, Vector3 side1Pos_1, Vector3 side1Pos_2, Vector3 side2Pos) BInfo;

            poss = polygonB.vertices;
            pos0 = poss[0];
            pos1 = poss[1];
            pos2 = poss[2];
            anc = normal.x * anchor.x + normal.y * anchor.y + normal.z * anchor.z;
            d0 = normal.x * pos0.x + normal.y * pos0.y + normal.z * pos0.z - anc;
            d1 = normal.x * pos1.x + normal.y * pos1.y + normal.z * pos1.z - anc;
            d2 = normal.x * pos2.x + normal.y * pos2.y + normal.z * pos2.z - anc;
            //d0 = Vector3.Dot(normal, pos0 - anchor);
            //d1 = Vector3.Dot(normal, pos1 - anchor);
            //d2 = Vector3.Dot(normal, pos2 - anchor);
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
                        return false;
                    }
                }
            }

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




    void CollisionDetection(ColliderInfo A, ColliderInfo B, Polygon polygonA, Polygon polygonB, Vector3[] hitPoints)
    {
        A.hitPoints = B.hitPoints = hitPoints;
        A.hitPolygon = polygonA;
        B.hitPolygon = polygonB;
        A.polygonCollider.OnCollision(B);
        B.polygonCollider.OnCollision(A);
    }



    //線分と三角ポリゴンが接触しているか判定. 三角形の1つの点の位置ベクトルと2つのベクトル, 線分の始点の位置ベクトルと1つの方向・大きさベクトル
    //D→ + k*e→ = A→ + s*b→ + t*c→ を解いている. F→ = D→ - A→
    public static bool OverrapCheck(Vector3 trianglePosition, Vector3 triangleVector1, Vector3 triangleVector2, Vector3 lineStartPosition, Vector3 lineVector, out Vector3 hitPoint)
    {
        hitPoint = Vector3.zero;
        //print("A :"+trianglePosition+"B :"+ (trianglePosition+triangleVector1).ToString()+"C :"+(trianglePosition+triangleVector2).ToString()+"D :"+lineStartPosition+"E :"+(lineStartPosition+lineVector).ToString());


        Vector3 F = lineStartPosition - trianglePosition;
        Vector3 b = triangleVector1;
        Vector3 c = triangleVector2;
        Vector3 e = lineVector;

        float k, t, s;

        if (Mathf.Abs(b.x) > 0.001) //係数が0のときの場合分けをしている. 非常に面倒くさかった
        {
            float b_yx = b.y / b.x;
            float b_zx = b.z / b.x;
            float Fp = b_yx * F.x - F.y;
            float ep = b_yx * e.x - e.y;
            float cp = b_yx * c.x - c.y;

            if (Mathf.Abs(cp) > 0.001) // > 0 でないのは丸め誤差を含むため
            {
                float cpp = (b_zx * c.x - c.z) / cp;

                var denom = (cpp * ep + e.z - b_zx * e.x);
                if (Mathf.Abs(denom) < 0.001)
                {
                    //print("pin"); 
                    return false;
                }
                k = -(F.z - b_zx * F.x + cpp * Fp) / denom;
                t = (Fp + ep * k) / cp;
                s = (F.x - t * c.x + k * e.x) / b.x; //例外処理をしなければここまででいい
                //print("marker");
            }
            else if (Mathf.Abs(ep) > 0.001)
            {
                k = -Fp / ep;
                float e_xp = e.x / ep;
                float e_zp = e.z / ep;
                var denom = (b_zx * c.x - c.z);
                if (Mathf.Abs(denom) < 0.001)
                {
                    //print("pin"); 
                    return false;
                }
                t = (b_zx * F.x - b_zx * e_xp * Fp + e_zp * Fp - F.z) / denom;
                s = (F.x - t * c.x + k * e.x) / b.x;
                //print("marker");
            }
            else { return false; }

        }
        else if (Mathf.Abs(c.x) > 0.001)
        {
            float c_yx = c.y / c.x;
            float Fo = c_yx * F.x - F.y;
            float eo = c_yx * e.x - e.y;


            if (Mathf.Abs(b.y) > 0.001)
            {
                float b_zy = b.z / b.y;
                float c_zx = c.z / c.x;
                var denom = (c_zx * e.x - b_zy * eo - e.z);
                if (Mathf.Abs(denom) < 0.001)
                {
                    //print("pin"); 
                    return false;
                }
                k = -(c_zx * F.x - b_zy * Fo - F.z) / denom;
                s = -(Fo + k * eo) / b.y;
                t = (F.x + k * e.x) / c.x;
                //print("marker");
            }
            else if (Mathf.Abs(b.z) > 0.001 && Mathf.Abs(eo) > 0.001)
            {
                k = -Fo / eo;
                t = (F.x + k * e.x) / c.x;
                //s = (F.z - c_zx * (F.x + k * e.x) + k * e.y) / b.z;
                s = (F.z - t * c.z + k * e.z) / b.z;
                //print("marker");
            }
            else { return false; }
        }
        else if (Mathf.Abs(e.x) > 0.001)
        {
            k = -F.x / e.x;
            float e_yx = e.y / e.x;
            float e_zx = e.z / e.x;
            if (Mathf.Abs(c.y) > 0.001)
            {
                float c_zy = c.z / c.y;

                var denom = (b.z - c_zy * b.y);
                if (Mathf.Abs(denom) < 0.001)
                {
                    //print("pin"); 
                    return false;
                }
                s = (F.z + c_zy * e_yx * F.x - c_zy * F.y - e_zx * F.x) / denom;
                t = -(e_yx * F.x - F.y + s * b.y) / c.y;
                //print("marker");
            }
            else if (Mathf.Abs(b.y) > 0.001 && Mathf.Abs(c.z) > 0.001)
            {
                s = -(e_yx * F.x - F.y) / b.y;
                t = (F.z - s * b.z + k * e.z) / c.z;
                //print("marker");
            }
            else { return false; }
        }
        else
        {

            return false;
        }

        //print("1: "+"k=" + k + ", s=" + s + ", t=" + t);
        //print("triangleSide : " + (trianglePosition + triangleVector1 * s + triangleVector2 * t).ToString());
        // print("lineSide : " + (lineStartPosition + lineVector * k).ToString());


        if (k >= 0 && k <= 1 && s >= 0 && t >= 0 && s + t <= 1)
        {
            hitPoint = lineStartPosition + lineVector * k;
            return true;
        }
        else
        {
            return false;
        }
    }

    void Clear()
    {
        cuttedAndCutter.Clear();
        cuttedOnly.Clear();
        cutterOnly.Clear();
    }

    ulong a = 0b1111111111111111111111111111111111111111111111111111111111111111;
    //static readonly ulong[] FRONTFRAG = new ulong[32] {
    //    0b00000000000000000000000000000000000000000,
    //}

}



public class ColliderInfo
{
    public Polygon[] polygons;
    public PolygonCollider polygonCollider;
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
    public ColliderInfo(Polygon[] polygons, PolygonCollider collisionObjectInstance, GameObject obj)
    {
        this.polygons = polygons;
        polygonCollider = collisionObjectInstance;
        collisionObject = obj;
    }
    public ColliderInfo() { }

    public ColliderInfo Set(Polygon[] polygons, PolygonCollider collisionObjectInstance, GameObject obj)
    {
        this.polygons = polygons;
        polygonCollider = collisionObjectInstance;
        collisionObject = obj;
        return this;
    }

}

public class Polygon
{
    public Vector3[] vertices;
    public Vector3 normal;
    public int COLLISION_CHECK;
    public int COLLISION_CHECK_SUB;
    public Vector3 this[int index]
    {
        get
        {
            if (index < 0 && index > 2)
            {
                Debug.LogError("index must be from 0 to 2");
            }
            return vertices[index];
        }
    }
    public Polygon() { vertices = new Vector3[3]; }
    public Polygon(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        vertices = new Vector3[3] { vertex1, vertex2, vertex3 };
        MakeNormal();
    }
    public void Set(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        vertices[0] = vertex1;
        vertices[1] = vertex2;
        vertices[2] = vertex3;
        MakeNormal();
    }

    private void MakeNormal()
    {
        var vertex0 = vertices[0];
        var vertex1 = vertices[1];
        var vertex2 = vertices[2];
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
}

public abstract class PolygonCollider : MonoBehaviour
{
    public bool enableCollision = true;
    // 自分のコライダーの種類を定義する
    [SerializeField] protected CollisionManager.ColliderType colliderType = CollisionManager.ColliderType.CuttedAndCutter;


    protected virtual void OnEnable()
    {
        CollisionManager.SetCollision += SendCollisionData;
    }
    protected virtual void OnDisable()
    {
        CollisionManager.SetCollision -= SendCollisionData;

    }

    /// <summary>
    /// 衝突の判定に使うポリゴン(Vector3が3つ分の三角形)の配列を作成
    /// </summary>
    /// <returns></returns>
    protected abstract Polygon[] SetPolygons();
    protected virtual void CollisionManagerUpdate() { }

    /// <summary>
    /// 衝突が検知されたらこれが呼ばれる
    /// </summary>
    public abstract void OnCollision(ColliderInfo colliderInfo);

    ColliderInfo inputData = new ColliderInfo();
    protected virtual void SendCollisionData()
    {
        
        if (enableCollision)
        {
            CollisionManager.AddColliderDataList(inputData.Set(SetPolygons(), this, this.gameObject), colliderType);
        }
    }
}

public abstract class StickCollider : PolygonCollider
{
    public Transform start, end;
    private Vector3 prePos_start, prePos_end;

    protected void SetStartPos(Vector3 pos)
    {
        start.position = pos;
    }
    protected void SetEndPos(Vector3 pos)
    {
        end.position = pos;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        prePos_start = start.position;
        prePos_end = end.position;
    }



    private Polygon[] polygons = new Polygon[2];
    protected override Polygon[] SetPolygons()
    {
        Polygon A = new Polygon(prePos_start, prePos_end, start.position);
        Polygon B = new Polygon(start.position, end.position, prePos_end);
        polygons[0] = A;
        polygons[1] = B;

        return polygons;
    }

    protected override void SendCollisionData()
    {
        base.SendCollisionData();
        prePos_start = start.position;
        prePos_end = end.position;
    }

}


