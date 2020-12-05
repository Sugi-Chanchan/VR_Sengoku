using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    public enum ColliderType
    {
        PlayerBody,
        PlayerWeapon,
        EnemyBoby,
        EnemyWeapon
    }

    private static List<ColliderInputData> playerBody = new List<ColliderInputData>();
    private static List<ColliderInputData> playerWeapon = new List<ColliderInputData>();
    private static List<ColliderInputData> enemyBody = new List<ColliderInputData>();
    private static List<ColliderInputData> enemyWeapon = new List<ColliderInputData>();

    private static readonly Dictionary<ColliderType, List<ColliderInputData>> PolygonTypeDic = new Dictionary<ColliderType, List<ColliderInputData>>
    {
        {ColliderType.PlayerBody,playerBody },
        {ColliderType.PlayerWeapon,playerWeapon },
        {ColliderType.EnemyBoby,enemyBody },
        {ColliderType.EnemyWeapon,enemyWeapon }
    };

    //アニメーションより後で当たり判定を出すならLateUpdate内でこれを実行して, かつスクリプトの実行順でCollisionManagerが最後に来るようにする必要がある.
    public static void AddColliderDataList(ColliderInputData colliderData, ColliderType colliderType) //dictinaryとEnumを使ってポリゴンを追加する関数をかいた
    {
        PolygonTypeDic[colliderType].Add(colliderData);
    }

    void LateUpdate()
    {
        //ポリゴン同士が重なっているか判定して重なっていたらOncollisionを実行
        foreach (ColliderInputData pWeapon in playerWeapon)
        {
            foreach (ColliderInputData eWeapon in enemyWeapon)
            {

                CollisionCheck(pWeapon, eWeapon);
            }

            foreach (ColliderInputData eBody in enemyBody)
            {

                CollisionCheck(pWeapon, eBody);
            }
        }


        foreach (ColliderInputData pBody in playerBody)
        {
            foreach (ColliderInputData eWeapon in enemyWeapon)
            {

                CollisionCheck(pBody, eWeapon);
            }
        }


        Clear();
    }

    void CollisionCheck(ColliderInputData A, ColliderInputData B)
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
                        CollisionDetection(A, B, polygonA, polygonB, hitPoint);
                        return;
                    };
                }

                _triangleVector1 = polygonB.vertices[1] - polygonB.vertices[0];
                _triangleVector2 = polygonB.vertices[2] - polygonB.vertices[0];
                for (int i = 0; i < 3; i++)
                {
                    if (OverrapCheck(polygonB.vertices[0], _triangleVector1, _triangleVector2, polygonA.vertices[i], polygonA.vertices[(i + 1) % 3] - polygonA.vertices[i], out hitPoint))
                    {
                        CollisionDetection(A, B, polygonA, polygonB, hitPoint);
                        return;
                    };
                }
            }
        }
    }

    void CollisionDetection(ColliderInputData A, ColliderInputData B, Polygon polygonA, Polygon polygonB, Vector3 hitPoint)
    {
        CollisionInfo collisionInfoA = new CollisionInfo(polygonB, hitPoint, B.collisionObject.ColliderType);
        CollisionInfo collisionInfoB = new CollisionInfo(polygonA, hitPoint, A.collisionObject.ColliderType);
        A.collisionObject.OnCollision(collisionInfoA);
        B.collisionObject.OnCollision(collisionInfoB);
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
        playerBody.Clear();
        playerWeapon.Clear();
        enemyBody.Clear();
        enemyWeapon.Clear();
    }

}

public class ColliderInputData
{
    public Polygon[] polygons;
    public CollisionObject collisionObject;

    public ColliderInputData(Polygon[] polygons, CollisionObject collisionObjectInstance)
    {
        this.polygons = polygons;
        collisionObject = collisionObjectInstance;
    }
    public ColliderInputData() { }

    public ColliderInputData SetNew(Polygon[] polygons, CollisionObject collisionObjectInstance)
    {
        this.polygons = polygons;
        collisionObject = collisionObjectInstance;
        return this;
    }

}

public class Polygon
{
    public Vector3[] vertices;
    public Polygon(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        vertices = new Vector3[3] { vertex1, vertex2, vertex3 };
    }
    public void SetNew(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        vertices = new Vector3[3] { vertex1, vertex2, vertex3 };
    }
}

public class CollisionInfo
{
    public Polygon polygon;
    public Vector3 hitPoint;
    public CollisionManager.ColliderType colliderType;

    public CollisionInfo(Polygon _polygon, Vector3 _hitPoint, CollisionManager.ColliderType _colliderType)
    {
        polygon = _polygon;
        hitPoint = _hitPoint;
        colliderType = _colliderType;
    }
}




public abstract class CollisionObject : MonoBehaviour
{
    // 自分のコライダーの種類は必ず定義する
    public abstract CollisionManager.ColliderType ColliderType { get; }

    /// <summary>
    /// 衝突が検知されたらこれが呼ばれる
    /// </summary>
    public abstract void OnCollision(CollisionInfo collisionInfo);

    ColliderInputData inputData = new ColliderInputData();
    protected void SetCollisionPlane(Polygon[] polygons)
    {
        CollisionManager.AddColliderDataList(inputData.SetNew(polygons, this), ColliderType);
    }
}

public abstract class SwordCollider : CollisionObject
{
    public Transform start, end;
    private Vector3 prePos_start, prePos_end;
    public bool enableCollision = true;


    protected virtual void Update()
    {
        if (enableCollision)
        {
            SetCollision();
        }
        prePos_start = start.position;
        prePos_end = end.position;
    }

    private Polygon[] polygons = new Polygon[2];
    private void SetCollision()
    {

        Polygon A = new Polygon(prePos_start, prePos_end, start.position);
        Polygon B = new Polygon(start.position, end.position, prePos_end);
        polygons[0] = A;
        polygons[1] = B;

        SetCollisionPlane(polygons);
    }

}

