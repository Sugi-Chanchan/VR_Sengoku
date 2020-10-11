using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    Transform root,parent;
    public float speed,rotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        root = transform.root;
        parent = transform.parent;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var vel = root.transform.forward*Time.deltaTime*speed;
        var rot = Quaternion.Euler(new Vector3(0,parent.localPosition.x*rotateSpeed, 0));
        root.position = root.position + vel;
        root.rotation = rot * root.rotation;
    }
}
