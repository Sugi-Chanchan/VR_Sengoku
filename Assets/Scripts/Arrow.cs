using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Arrow : MonoBehaviour
{
    VRTK_InteractableObject interactableObject;
    private Transform arrow, bowstring;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(interactableObject.IsGrabbed())
        {
            print("aaa");
        }
        */
        if((bowstring.position - arrow.position).sqrMagnitude < 0.05f)
        {
            arrow.parent = bowstring.parent;
            ArrowUnderBowMain();
        }
    }

    void Setup(){
        arrow = GameObject.Find("Arrow").transform; //矢のtransformを取得
        bowstring = GameObject.Find("Bowstring").transform; //子にJapanese_bowを持つSphereのtransformを持ってくる
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
    }
}
