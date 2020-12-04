using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Naginata : MonoBehaviour
{
    Transform weaponPositionRight;
    Transform naginataMainTransform;
    Quaternion criteriaRotQuat;
    // Start is called before the first frame update
    void Start()
    {
        naginataMainTransform = this.transform.parent;
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rig.MovePosition(weaponPositionRight.position);
        //rig.MoveRotation(weaponPositionRight.rotation);
    }

    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Enemy>())
        {
            var dir = (collision.transform.position - transform.position).normalized;
            dir.y = 0.2f;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir * 20, ForceMode.Impulse);
        }
    }
    */

    void Setup()
    {
        //weaponPositionRight = GameObject.FindGameObjectWithTag("WeaponPositionRight").transform;
        naginataMainTransform.parent = GameObject.FindGameObjectWithTag("WeaponPositionRight").transform;
    }
}
