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
    VRTK.VRTK_VelocityEstimator leftHand;

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
        leftHand = GameObject.FindGameObjectWithTag("LeftHand").transform.parent.GetComponent<VRTK_VelocityEstimator>();
    }

    void Grabbed()
    {
        var reinsPosition = transform.localPosition - startPosition;

        if (false) { //腕を振ったかを判定
        }
        Rotate(reinsPosition.x);
        Deceleration(reinsPosition.z);
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
            speed += reinPosZ * Time.deltaTime;
            speed = (speed < 0) ? 0 : speed;
        }
    }
    
    void Acceleration()
    {

    }
}
