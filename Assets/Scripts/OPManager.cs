using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OPManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        Vector3 pos = player.transform.position + player.transform.forward;
        pos.y = 0;
        this.transform.position = pos;

        Vector3 dir = player.transform.position - this.transform.position;
        dir.y = 0;
        this.transform.rotation = Quaternion.LookRotation(dir);
    }
}
