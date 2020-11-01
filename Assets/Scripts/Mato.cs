using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mato : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.name == "Arrow")
        {
            collision.transform.parent = this.transform.GetChild(0);
            this.transform.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
