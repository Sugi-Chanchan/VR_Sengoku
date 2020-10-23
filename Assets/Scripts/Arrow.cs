using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Arrow : MonoBehaviour
{
    VRTK_InteractableObject interactableObject;
    VRTK_InteractGrab interactGrab;
    [SerializeField] Transform arrowParent, arrow, bowstring;

    [SerializeField]private GameObject rightHand;

    private GameObject grabbedObject;

    [SerializeField] Rigidbody arrowRigidbody;

    [SerializeField] public bool load, triggerBool;
    // Start is called before the first frame update
    void Start()
    {
        Setup();
        //Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if(interactGrab.IsGrabButtonPressed() && interactGrab.GetGrabbedObject() != null && interactGrab.GetGrabbedObject().transform.GetChild(0).CompareTag("Arrow"))
        {
            arrowParent = interactGrab.GetGrabbedObject().transform;
            arrow = arrowParent.GetChild(0);
            //print(arrowParent.name);
            //print(arrow.parent.name);

            if((bowstring.position - arrowParent.position).sqrMagnitude < 0.05f)
            {
                arrow.parent = bowstring.parent;
                //interactGrab.ForceRelease(true);
                ArrowUnderBowMain();
                print(load);
            }
        }
    }

    void Setup(){
        load = false;
        //triggerBool = false;
        
        rightHand = GameObject.FindGameObjectWithTag("RightHand");
        bowstring = GameObject.Find("Bowstring").transform; //子にJapanese_bowを持つSphereのtransformを持ってくる

        /*
        rightHand.GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
        rightHand.GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
        */

        //interactableObject = arrow.parent.GetComponent<VRTK_InteractableObject>();
        interactGrab = rightHand.GetComponent<VRTK_InteractGrab>();
    }

    void ArrowUnderRightHand() //矢がRightHandの下にある場合にちょうどいい場所に移動させる関数
    {
        arrow.localPosition = new Vector3(-0.0138f, -0.0066f, -0.0007f);
        arrow.localRotation = Quaternion.Euler(-180.0f, -3.752991f, -210.0f);
    }

    void ArrowUnderBowMain() //矢がBowMainの下にある場合にちょうどいい場所に移動させる関数
    {
        arrow.localPosition = new Vector3(-0.004f, -0.03f, -0.017f);
        arrow.localRotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
        
        load = true;
    }

    public bool GetLoadBool(){
        return load;
    }

    /*
    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        triggerBool = true;
    }
    private void DoTriggerReleased(object sender, ControllerInteractionEventArgs e)
    {
        triggerBool = false;
    }
    */
}
