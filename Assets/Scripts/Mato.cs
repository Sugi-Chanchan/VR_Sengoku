using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mato : MonoBehaviour
{
    AudioSource targetHitSound;

    // Start is called before the first frame update
    void Start()
    {
        targetHitSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.name == "Arrow")
        {
            targetHitSound.Play();
            collision.transform.parent = this.transform.GetChild(0);
            this.transform.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
