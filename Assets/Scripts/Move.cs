using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Move : MonoBehaviour
{
    [SerializeField]Transform root,parent,Vrtk;
    public float startposition;
    public GameObject uma;
    [SerializeField]private GameObject rightHand;
    public float speed,rotateSpeed;
    int i = 0;
    float PressedTime;
    float ReleasedTime;
    // Start is called before the first frame update
    void Start()
    {
        root = transform.root;
        parent = transform.parent;
        Vrtk = root.GetChild(0);
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
        rightHand = GameObject.FindGameObjectWithTag("Right_Hand");
        if(i == 0)
        {
            rightHand.GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
            rightHand.GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
            i = 1;
        }
        startposition = parent.localPosition.x;
        ResetPosition();
        Vector3 Vrtkposition = Vrtk.position;
        root.position = new Vector3(transform.position.x, root.position.y, transform.position.z);
        Vrtk.position = Vrtkposition;
    }
    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        PressedTime = Time.time;
        Debug.Log("Trigger Press");
    }

    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        ReleasedTime = Time.time;
        Debug.Log("Trigger Released");

        float PressTime = ReleasedTime - PressedTime;
        if(PressTime >= 3.0f)
        {
            Setup();
        }
    }
}
