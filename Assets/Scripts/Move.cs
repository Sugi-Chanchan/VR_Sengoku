using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Move : MonoBehaviour
{
    [SerializeField] Transform root, parent, Vrtk;
    public GameObject uma;
    [SerializeField] private GameObject rightHand;
    public float speed, rotateSpeed;
    bool AddNewFunc = true;
    float PressedTime;
    float ReleasedTime;
    // Start is called before the first frame update
    void Start()
    {
        root = transform.root;
        parent = transform.parent;
        Vrtk = root.GetChild(0);
        //Invoke("Setup", 0.1f);
        StartCoroutine("Setup");
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

    void ResetPosition()
    {
        var xzDirection = transform.rotation.eulerAngles;
        xzDirection.x = 0;
        xzDirection.z = 0;

        Quaternion VrtkRotation = Vrtk.rotation;
        root.rotation = Quaternion.Euler(xzDirection); //rootの向きをリセット時にプレイヤーの向いている向きに合わせる
        Vrtk.rotation = VrtkRotation; //root以下のオブジェクトごと回転してしまうので補正

        uma.transform.position = transform.position - (Vector3.up * 0.5f) + root.forward;
        uma.transform.rotation = root.rotation;
        //uma.transform.position += new Vector3(0.0f, 0.0f, 1.0f);
    }

    //void Setup(){
    //    rightHand = GameObject.FindGameObjectWithTag("RightHand");
    //    if(AddNewFunc) //ブール型の変数で既にVRTK_ControllerEventsに関数を追加したかどうかの判別
    //    {
    //        /*
    //        rightHand.GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
    //        rightHand.GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerReleased);
    //        */
    //        rightHand.GetComponent<VRTK_ControllerEvents>().ButtonTwoPressed += new ControllerInteractionEventHandler(DoButtonTwoPressed);
    //        AddNewFunc = false;
    //    }
    //    Vector3 Vrtkposition = Vrtk.position;
    //    root.position = new Vector3(transform.position.x, root.position.y, transform.position.z);
    //    Vrtk.position = Vrtkposition;
    //    ResetPosition();
    //}

    IEnumerator Setup()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        if (AddNewFunc) //ブール型の変数で既にVRTK_ControllerEventsに関数を追加したかどうかの判別
        {
            ButtonManager.Set_ResetButtonDownEvent(ResetButtonPressed);
            AddNewFunc = false;
        }
        Vector3 Vrtkposition = Vrtk.position;
        root.position = new Vector3(transform.position.x, root.position.y, transform.position.z);
        Vrtk.position = Vrtkposition;
        ResetPosition();

        yield break;
    }

    /*
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
        if(PressTime >= 0.1f)
        {
            Setup();
        }
    }
    */

    private void ResetButtonPressed(object sender, ControllerInteractionEventArgs e)
    {
        StartCoroutine("Setup");
    }
}