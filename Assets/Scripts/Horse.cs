using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Horse : MonoBehaviour
{
    Transform root;
    public GameObject[] reins = new GameObject[2];
    VRTK_InteractableObject[] interactableObjects = new VRTK_InteractableObject[2];
    Vector3[] startpostions = new Vector3[2];
    Vector3[] displacement = new Vector3[2];
    VRTK_VelocityEstimator[] VRTKVelEstim = new VRTK_VelocityEstimator[2];
    [SerializeField]float rotateSpeed, speed, maxSpeed;
    bool bothHands=false;

    private void Start()
    {
        root = transform.root;
        Invoke("SetUp", 0.2f);
    }

    void SetUp()
    {
        for (int i = 0; i < 2; i++)
        {
            VRTKVelEstim[i] = reins[i].GetComponent<VRTK_VelocityEstimator>();
            startpostions[i] = reins[i].transform.localPosition;
            interactableObjects[i] = reins[i].GetComponent<VRTK_InteractableObject>();
        }
    }

    private void FixedUpdate()
    {
        if (GrabbedCheck())
        {
            var averageDisPlacement = ((reins[0].transform.localPosition - startpostions[0]) + (reins[1].transform.localPosition - startpostions[1])) / 2;
            Rotate(averageDisPlacement.x);
            Acceleration();
            if (bothHands) Deceleration(averageDisPlacement.z - averageDisPlacement.y);
        }

        var vel = root.transform.forward * Time.deltaTime * speed;
        root.position = root.position + vel;
    }

    float returnspeed = 0.02f;
    bool GrabbedCheck()
    {
        bothHands = false;

        if (interactableObjects[0].IsGrabbed())
        {
            if (!interactableObjects[1].IsGrabbed())
            {
                displacement[0] = displacement[1] = reins[0].transform.localPosition - startpostions[0];
                reins[1].transform.localPosition = startpostions[1]+ Vector3.MoveTowards(reins[1].transform.localPosition-startpostions[1],displacement[1],returnspeed);
            }
            else
            {
                bothHands = true;
            }

            return true;
        }
        else if (interactableObjects[1].IsGrabbed())
        {
            displacement[0] = displacement[1] = reins[1].transform.localPosition - startpostions[1];
            reins[0].transform.localPosition =startpostions[0]+ Vector3.MoveTowards(reins[0].transform.localPosition-startpostions[0], displacement[0], returnspeed);
            return true;
        }
        else
        {
            for(int i = 0; i < 2; i++)
            {
                displacement[i] = Vector3.zero;
                reins[i].transform.localPosition = Vector3.MoveTowards(reins[i].transform.localPosition, startpostions[i], returnspeed);
            }
        }

        return false;
    }

    void Rotate(float holizontal)
    {
        var rot = Quaternion.Euler(new Vector3(0, holizontal * rotateSpeed, 0));
        root.rotation = rot * root.rotation;
    }

    bool accCoolTime=false;
    void Acceleration ()
    {
        float yacc = Mathf.Min(VRTKVelEstim[0].GetAccelerationEstimate().y, VRTKVelEstim[1].GetAccelerationEstimate().y);
        if (yacc < -200&&!accCoolTime)
        {
            speed += 5;
            speed = Mathf.Max(speed, maxSpeed);
            accCoolTime = true;
            Invoke("RemoveAccCoolTime", 0.5f);
        }
    }

    void Deceleration(float backward)
    {
        if (backward < -0.82f)
        {
            speed += backward * Time.deltaTime * 20;
            speed = Mathf.Max(speed, 0);
        }
        //print(backward);
    }

    void RemoveAccCoolTime()
    {
        accCoolTime = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Enemy>())
        {
            var dir=(collision.transform.position - transform.position).normalized;
            dir.y = 0.2f;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir*20, ForceMode.Impulse);
        }
    }
}
