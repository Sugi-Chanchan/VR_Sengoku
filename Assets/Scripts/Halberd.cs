using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd : MonoBehaviour
{

    Transform weaponPositionRight;
    Rigidbody rig;
    Quaternion criteriaRotQuat;
    bool setup = false;
    [SerializeField] Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (setup)
        {
            rig.MovePosition(weaponPositionRight.position);
            rig.MoveRotation(weaponPositionRight.rotation);
        }
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

    void Setup()
    {
        weaponPositionRight = GameObject.FindGameObjectWithTag("WeaponPositionRight").transform;
        setup = true;
    }
}
