using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Move : MonoBehaviour
{
    [SerializeField]Transform root,parent,VRTK;
    public float startposition;
    public GameObject uma;
    public float speed,rotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        root = transform.root;
        parent = transform.parent;
        VRTK = root.GetChild(0);
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //var vel = root.transform.forward*Time.deltaTime*speed;
        //var rot = Quaternion.Euler(new Vector3(0,(parent.localPosition.x - startposition)*rotateSpeed, 0));
        //root.position = root.position + vel;
        //root.rotation = rot * root.rotation;

        //root.RotateAround(uma.transform.position, Vector3.up, (parent.localPosition.x - startposition)*rotateSpeed);
        //uma.transform.rotation = rot * uma.transform.rotation;
    }

    void ResetPosition(){
        uma.transform.position = transform.position - Vector3.up;
    }

    void Setup(){
        startposition = parent.localPosition.x;
       // ResetPosition();
        Vector3 VRTKposition = VRTK.position;
        root.position = new Vector3(transform.position.x, root.position.y, transform.position.z);
        VRTK.position = VRTKposition;
    }
}
