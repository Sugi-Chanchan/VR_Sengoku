using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : Enemy
{
    Transform player;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("SetUp", 1);
    }

    // Update is called once per frame
    void Update()
    {
        var dir = player.position - transform.position;
        if (Vector3.Dot(-dir.normalized,player.forward)>0.7f||dir.magnitude<30)
        {

            transform.rotation = Quaternion.LookRotation(dir);
            transform.position = Vector3.MoveTowards(transform.position, player.position, 3 * Time.deltaTime);
        }
        
    }

    void SetUp()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
