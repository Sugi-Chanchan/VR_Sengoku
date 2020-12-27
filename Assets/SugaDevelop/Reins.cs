using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System;

public class Reins : MonoBehaviour
{
    const float returnspeed=0.02f;

    Vector3 startPosition;

    private VRTK_InteractableObject interactableObject;
    private VRTK_VelocityEstimator velocityEstimator;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.localPosition;
        interactableObject = GetComponent<VRTK_InteractableObject>();
        velocityEstimator = GetComponent<VRTK_VelocityEstimator>();
    }

    const int num_previous = 3;
    Vector3 velocity;
    Vector3[] previousVelocity = new Vector3[num_previous];
    int count = 0;

    private void Update()
    {
        previousVelocity[count++ % num_previous] = velocity;
        velocity= velocityEstimator.GetVelocityEstimate();
    }

    public bool IsGrabbed
    {
        get { return interactableObject.IsGrabbed(); }
    }

    public Vector3 Displacement
    {
        get { return transform.localPosition - startPosition; }
    }

    public void ReturnDefaultPos()
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, startPosition, returnspeed);
    }

    public void FollowOtherReins(Reins other)
    {
        Vector3 displacement = other.Displacement;
        transform.localPosition = startPosition + Vector3.MoveTowards(transform.localPosition - startPosition, displacement, returnspeed);
    }


    
    public bool IsWhipping
    {
        get
        {
            float whippingValue = ForwardWhippingValue + VerticalWhippingValue;
            return whippingValue > 3;
        }
    }

    //前後方向の動きをとる
    float ForwardWhippingValue
    {
        get
        {
            bool whipping = false;
            float velocityZ = velocity.z;
            if (velocityZ > -0.3) { return 0; }
            for (int i = 0; i < previousVelocity.Length; i++)
            {
                if (previousVelocity[i].z > 1) { whipping = true; }
            }

            if (whipping)
            {
                float forwardWhip = 0;
                for (int i = 0; i < previousVelocity.Length; i++)
                {
                    if (i == 0) { forwardWhip = velocityZ - previousVelocity[0].z; continue; }

                    forwardWhip = Math.Min(forwardWhip, velocityZ - previousVelocity[i].z);
                }
                return -forwardWhip;
            }
            return 0;
        }
    }

    float VerticalWhippingValue
    {
        get
        {
            bool whipping = false;
            float velocityY = velocity.y;
            if (velocityY < 0.3) { return 0; }
            for (int i = 0; i < previousVelocity.Length; i++)
            {
                if (previousVelocity[i].y < -1) { whipping = true; }
            }

            if (whipping)
            {
                float verticalWhip = 0;
                for (int i = 0; i < previousVelocity.Length; i++)
                {
                    if (i == 0) { verticalWhip = velocityY - previousVelocity[0].y; continue; }

                    verticalWhip = Math.Max(verticalWhip, velocityY - previousVelocity[i].y);
                }
                return verticalWhip;
            }

            return 0;
        }
    }

    int debugCount;
    string debugString1, debugString2, debugString3, debugString4,debugString5;
    public bool debug;
    void DebugLog()
    {
        if (!debug) { return; }
        float y = velocity.y;
        debugString1 += y + "\n";
        debugString2 += velocity.z + "\n";
        float yWhip = VerticalWhippingValue;
        debugString3 += yWhip + "\n";
        float zWhip = ForwardWhippingValue;
        debugString4 += zWhip + "\n";
        debugString5 += (yWhip + zWhip) + "\n";
        if (debugCount++ > 100)
        {
            System.IO.StreamWriter sw = new System.IO.StreamWriter("../YData.txt", false);
            sw.WriteLine(debugString1);
            sw.Flush();
            sw.Close();

            System.IO.StreamWriter sw2 = new System.IO.StreamWriter("../ZData.txt", false);
            sw2.WriteLine(debugString2);
            sw2.Flush();
            sw2.Close();

            System.IO.StreamWriter sw3 = new System.IO.StreamWriter("../YWhipData.txt", false);
            sw3.WriteLine(debugString3);
            sw3.Flush();
            sw3.Close();

            System.IO.StreamWriter sw4 = new System.IO.StreamWriter("../ZWhipData.txt", false);
            sw4.WriteLine(debugString4);
            sw4.Flush();
            sw4.Close();

            System.IO.StreamWriter sw5 = new System.IO.StreamWriter("../WhipData.txt", false);
            sw5.WriteLine(debugString5);
            sw5.Flush();
            sw5.Close();

            debugCount = 0;
        }
    }
}
