using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : StickColliderDynamic
{
    private Vector3 prePos;
    private Vector3 velocity;
    [SerializeField] Transform sentan;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CalculateVelocity()
    {
        velocity = (sentan.position - prePos) / Time.deltaTime;
        prePos = sentan.position;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }


    public override void OnCollision(CollisionInfo collisionInfo)
    {
        
    }
}
