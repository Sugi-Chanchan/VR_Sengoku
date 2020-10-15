using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Reins : MonoBehaviour
{
    
    VRTK_InteractableObject interactableObject;
    Vector3 startPosition;
    Transform root;
    [SerializeField] float rotateSpeed,speed,maxSpeed;
    [SerializeField]VRTK_VelocityEstimator leftHand;

    // Start is called before the first frame update
    void Start()
    {
        root = transform.root;
        Invoke("SetUp", 0.2f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (interactableObject.IsGrabbed())
        {
            Grabbed();
        }
        else
        {
            Idle();
        }

        var vel = root.transform.forward * Time.deltaTime * speed;
        root.position = root.position + vel;
    }

    void SetUp()
    {
        interactableObject = GetComponent<VRTK_InteractableObject>();
        startPosition = transform.localPosition;
        //leftHand = GameObject.FindGameObjectWithTag("LeftHand").transform.parent.GetComponent<VRTK_VelocityEstimator>();
        leftHand = this.gameObject.AddComponent<VRTK_VelocityEstimator>();
    }

    void Grabbed()
    {
        var reinsPosition = transform.localPosition - startPosition;

        Rotate(reinsPosition.x);
        Deceleration(-reinsPosition.y);
        Acceleration();
    }

   


    void Idle()
    {
        transform.localPosition = startPosition;
    }

    void Rotate(float reinPosX)
    {

        var rot = Quaternion.Euler(new Vector3(0,reinPosX*rotateSpeed, 0));
        root.rotation = rot * root.rotation;
    }

    void Deceleration(float reinPosZ)
    {
        if (reinPosZ < -0.27f)
        {
            speed += reinPosZ * Time.deltaTime*20;
            speed = (speed < 0) ? 0 : speed;
        }
        print(reinPosZ);
    }

    bool accCoolTime;
    void Acceleration()
    {
        if (!accCoolTime)
        {

            float acc = leftHand.GetAccelerationEstimate().y;
            if (acc < -100)
            {
                speed += 5;
                speed = (speed > maxSpeed) ? maxSpeed : speed;
                accCoolTime = true;
                Invoke("RemoveCoolTime", 0.5f);
            }
        }
    }

    void RemoveCoolTime()
    {
        accCoolTime = false;
    }
}
