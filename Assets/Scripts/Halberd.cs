using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd : MonoBehaviour
{

    [SerializeField] Transform hand;
    Rigidbody rig;
    Quaternion criteriaRotQuat;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rig.MovePosition(hand.position);
        rig.MoveRotation(hand.rotation);
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
}
