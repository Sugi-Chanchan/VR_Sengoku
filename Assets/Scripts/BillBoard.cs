using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillBoard : MonoBehaviour
{
    Transform player;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SetUp());
    }

    IEnumerator SetUp()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(player.position-transform.position);
    }
}
