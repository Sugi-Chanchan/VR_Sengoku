using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd : MonoBehaviour
{

    Transform weaponPosition;
    Rigidbody rig;
    Quaternion criteriaRotQuat;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rig.MovePosition(weaponPosition.position);
        rig.MoveRotation(weaponPosition.rotation);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Enemy>())
        {
            var dir = (collision.transform.position - transform.position).normalized;
            dir.y = 0.2f;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir * 20, ForceMode.Impulse);
        }
    }

    void Setup(){
        weaponPosition = GameObject.FindGameObjectWithTag("Weapon").transform;
    }
}
