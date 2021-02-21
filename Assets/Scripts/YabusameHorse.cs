using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YabusameHorse : Horse
{
    public GameObject Target;
    bool horseTarget = false;

    protected override void Move()
    {
        var vel = Vector3.zero;
        var speed = SpeedFunction(speedLevel);

        if (!horseTarget)
        {
            vel = root.transform.forward * Time.deltaTime * speed;
        }
        else
        {
            vel = (Target.transform.position - this.transform.position).normalized * Time.deltaTime * speed;
        }
        root.position = root.position + vel;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Start")
        {
            horseTarget = true;
        }
        if (other.gameObject.name == "Target")
        {
            horseTarget = false;
        }
    }
}
