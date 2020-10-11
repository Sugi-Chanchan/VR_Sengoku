using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hose : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Enemy>())
        {
            var dir=(collision.transform.position - transform.position).normalized;
            dir.y = 0.2f;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir*20, ForceMode.Impulse);
        }
    }
}
