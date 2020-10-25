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

    private static List<Polygon> playerBody = new List<Polygon>();
    private static List<Polygon> playerWeapon = new List<Polygon>();
    private static List<Polygon> enemyBody = new List<Polygon>();
    private static List<Polygon> enemyWeapon = new List<Polygon>();

    private static readonly Dictionary<ColliderType, List<Polygon>> PolygonTypeDic = new Dictionary<ColliderType, List<Polygon>>
    {
        {ColliderType.PlayerBody,playerBody },
        {ColliderType.PlayerWeapon,playerWeapon },
        {ColliderType.EnemyBoby,enemyBody },
        {ColliderType.EnemyWeapon,enemyWeapon }
    };

    public static void AddPolygonList(Polygon polygon, ColliderType colliderType) //dictinaryとEnumを使ってポリゴンを追加する関数をかいた
    {
        PolygonTypeDic[colliderType].Add(polygon);
    }

    void LateUpdate()
    {
        //ポリゴン同士が重なっているか判定して重なっていたらOncollisionを実行
        foreach (Polygon pWeapon in playerWeapon)
        {
            foreach (Polygon eWeapon in enemyWeapon)
            {
                CollisionCheck(pWeapon, eWeapon);
            }

            foreach (Polygon eBody in enemyBody)
            {
                CollisionCheck(pWeapon, eBody);
            }
        }


        foreach (Polygon pBody in playerBody)
        {
            foreach (Polygon eWeapon in enemyWeapon)
            {
                CollisionCheck(pBody, eWeapon);
            }
        }


        Clear();
    }

    void CollisionCheck(Polygon A, Polygon B)
    {
        Vector3 _triangleVector1 = A.vertex[1] - A.vertex[0];
        Vector3 _triangleVector2 = A.vertex[2] - A.vertex[0];
        for (int i = 0; i < 3; i++)
        {
            if (OverrapCheck(A.vertex[0], _triangleVector1, _triangleVector2, B.vertex[i], B.vertex[(i + 1) % 3]- B.vertex[i] ))
            {
                CollisionDetection(A, B);
                return;
            };
        }

        _triangleVector1 = B.vertex[1] - B.vertex[0];
        _triangleVector2 = B.vertex[2] - B.vertex[0];
        for (int i = 0; i < 3; i++)
        {
            if (OverrapCheck(B.vertex[0], _triangleVector1, _triangleVector2, A.vertex[i], A.vertex[(i + 1) % 3]- A.vertex[i] ))
            {
                CollisionDetection(A, B);
                return;
            };
        }
    }

    void CollisionDetection(Polygon A, Polygon B)
    {
        A.collisionObject.OnCollision();
        B.collisionObject.OnCollision();
    }



    //線分と三角ポリゴンが接触しているか判定. 三角形の1つの点の位置ベクトルと2つのベクトル, 線分の始点の位置ベクトルと1つの方向・大きさベクトル
    //D→ + k*e→ = A→ + s*b→ + t*c→ を解いている. F→ = D→ - A→
    public static bool OverrapCheck(Vector3 trianglePosition, Vector3 triangleVector1, Vector3 triangleVector2, Vector3 lineStartPosition, Vector3 lineVector)
    {

        //print("A :"+trianglePosition+"B :"+ (trianglePosition+triangleVector1).ToString()+"C :"+(trianglePosition+triangleVector2).ToString()+"D :"+lineStartPosition+"E :"+(lineStartPosition+lineVector).ToString());


        Vector3 F = lineStartPosition - trianglePosition;
        Vector3 b = triangleVector1;
        Vector3 c = triangleVector2;
        Vector3 e = lineVector;

        float k, t, s;

        if (Mathf.Abs(b.x) >0.001) //係数が0のときの場合分けをしている. 非常に面倒くさかった
        {
            float b_yx = b.y / b.x;
            float b_zx = b.z / b.x;
            float Fp = b_yx * F.x - F.y;
            float ep = b_yx * e.x - e.y;
            float cp = b_yx * c.x - c.y;
            
            if (Mathf.Abs(cp) >0.001)
            {
                float cpp = (b_zx * c.x - c.z) / cp;

                var denom = (cpp * ep + e.z - b_zx * e.x);
                if (Mathf.Abs(denom) < 0.001) {
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
                if (Mathf.Abs(denom) < 0.001) {
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
                if (Mathf.Abs(denom) < 0.001) {
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
                if (Mathf.Abs(denom) < 0.001) {
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
        else {
            
            return false;
        }

       // print("k=" + k + ", s=" + s + ", t=" + t);
       //print("triangleSide : " + (trianglePosition + triangleVector1 * s + triangleVector2 * t).ToString());
      // print("lineSide : " + (lineStartPosition + lineVector * k).ToString());


        if (k >= 0 && k <= 1 && s >= 0 && t >= 0 && s + t <= 1)
        {
            print("hit!");
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

public class Polygon
{
    public Vector3[] vertex;
    public CollisionObject collisionObject;

    public Polygon(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, CollisionObject collisionObjectInstance)
    {
        vertex = new Vector3[] { vertex1, vertex2, vertex3 };
        collisionObject = collisionObjectInstance;
    }
}


public abstract class CollisionObject : MonoBehaviour
{
    private List<Transform[]> linesOfTransform = new List<Transform[]>();
    private List<Vector3[]> linePosInBeforeFrame = new List<Vector3[]>();
    protected abstract CollisionManager.ColliderType ColliderType { get; }

    /// <summary>
    /// 当たり判定を検出する線分を追加する
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    protected void AddHitLine(Transform start, Transform end)
    {
        linesOfTransform.Add(new Transform[2] { start, end });
        linePosInBeforeFrame.Add(new Vector3[2]);
    }

    /// <summary>
    /// 衝突判定をとってほしいフレームでCollisionManagerにPolygonをおくる, Update内で使うこと(LateUpdateはだめ)
    /// </summary>
    protected void CheckCollision()
    {
        for (int i = 0; i < linesOfTransform.Count; i++)
        {
            Polygon A = new Polygon(linePosInBeforeFrame[i][0], linePosInBeforeFrame[i][1], linesOfTransform[i][0].position, this);
            Polygon B = new Polygon(linesOfTransform[i][0].position, linesOfTransform[i][1].position, linePosInBeforeFrame[i][1], this);

            CollisionManager.AddPolygonList(A, ColliderType);
            CollisionManager.AddPolygonList(B, ColliderType);

        }

        //print(this.name +"  before: "+ linePosInBeforeFrame[0][0]+" , "+linePosInBeforeFrame[0][1]+",  now : "+linesOfTransform[0][0].position+" , "+linesOfTransform[0][1].position);


        //最後に今の当たり判定線の位置情報を過去の位置情報リストに加える
        for (int i = 0; i < linesOfTransform.Count; i++) { linePosInBeforeFrame[i] = new Vector3[2] { linesOfTransform[i][0].position, linesOfTransform[i][1].position }; }

    }
    /// <summary>
    /// 衝突が検知されたらこれが呼ばれる
    /// </summary>
    public abstract void OnCollision();

    /// <summary>
    /// Startでよんでくれ. AddHitLineを実行してくれ.
    /// </summary>
    protected abstract void SetCollider(); 
}

