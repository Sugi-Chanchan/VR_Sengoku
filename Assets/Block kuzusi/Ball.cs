using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 100;
    // Start is called before the first frame update
    void Start()
    {
        Rigidbody rb = this.GetComponent<Rigidbody>();

        Vector3 force = new Vector3(10.0f, 10.0f, 0.0f)*speed;

        rb.AddForce(force);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
