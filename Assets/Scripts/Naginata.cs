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
        StartCoroutine(Setup());
    }

    // Update is called once per frame
    /*
    void FixedUpdate()
    {
        rig.MovePosition(weaponPositionRight.position);
        rig.MoveRotation(weaponPositionRight.rotation);
    }
    */

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

    IEnumerator Setup()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        //weaponPositionRight = GameObject.FindGameObjectWithTag("WeaponPositionRight").transform;
        naginataMainTransform.parent = GameObject.FindGameObjectWithTag("WeaponPositionRight").transform;
        naginataMainTransform.localPosition = new Vector3(-0.002f, 0, 0);
        naginataMainTransform.localRotation = Quaternion.Euler(new Vector3(0, 0, -94.4f));
    }
}
