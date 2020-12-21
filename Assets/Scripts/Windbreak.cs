using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windbreak : MonoBehaviour
{
    [SerializeField] Rigidbody weaponRigidbody;
    [SerializeField] AudioSource weaponAudio;
    [SerializeField] AudioClip windbreakSound;
    float speed;

    void Start()
    {
    }

    void Update()
    {
        speed = weaponRigidbody.velocity.magnitude;
        //print(speed);
        if(speed > 0.1)
        {
            print("うおお");
            //audio.PlayOneShot(windbreakSound);
            weaponAudio.PlayOneShot(windbreakSound);
        }
    }
}
