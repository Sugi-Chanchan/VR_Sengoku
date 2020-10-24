using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    public static List<Polygon> playerBody, playerWeapon, enemyBody, enemyWeapon;
    
    void LateUpdate()
    {




        Clear();
    }

    void CollisionCheck(Polygon A, Polygon B)
    {

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

    Polygon(Vector3 vertex1,Vector3 vertex2,Vector3 vertex3,CollisionObject collisionObjectInstance)
    {
        vertex = new Vector3[] { vertex1, vertex2, vertex3 };
        collisionObject = collisionObjectInstance;
    }
}


public abstract class CollisionObject : MonoBehaviour
{
    /// <summary>
    /// 1つ前のフレームでの位置を更新
    /// </summary>
    protected abstract void SetVertexOfBeforeFrame();
    /// <summary>
    /// 衝突判定をとってほしいフレームでCollisionManagerにPolygonをおくる, Update内で使うこと(LateUpdateはだめ)
    /// </summary>
    protected abstract void SendPolygon();
    /// <summary>
    /// 衝突が検知されたらこれが呼ばれる
    /// </summary>
    public abstract void OnCollision();
}
