using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reins : MonoBehaviour
{

    public static float followSpeed =0.02f;
    public Transform debugCube;
    private Vector3 follow;
    // Start is called before the first frame update
    void Start()
    {
        debugCube.parent = this.transform.parent;
        follow = this.transform.position;
    }


    // Update is called once per frame
    void Update()
    {
        follow = Vector3.MoveTowards(follow, transform.localPosition, followSpeed);
        debugCube.localPosition = follow;
    }
}
