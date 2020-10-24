using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    public static List<Polygon> playerBody, playerWeapon, enemyBody, enemyWeapon;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {




        Clear();
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
    public abstract void SendPolygon();
    public abstract void OnCollision();
}
