using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHalberd : MonoBehaviour
{

    public float speed;
    Rigidbody rig;
    bool rotate = false;
    float sum;
    // Start is called before the first frame update
    void Start()
    {
        rotate = true;
        rig = GetComponent<Rigidbody>(); 
        
    }

    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {
            //rig.MoveRotation(Quaternion.Euler(0, 0, speed)*rig.rotation);
            transform.rotation = Quaternion.Euler(0, 0, speed) * transform.rotation;
            sum += speed;
        }
        if (sum >= 720)
        {
            sum = 0;
            StopRotate();
        }
    }

    void OnRotate()
    {
        rotate = true;
    }
    void StopRotate()
    {
        rotate = false;
        Invoke("OnRotate", 2);
    }
}
