using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System;


public enum Device
{
    Unknown,
    Oculus,
    SteamVR
}

public class ButtonManager : MonoBehaviour
{


    public static Device Device { get; private set; }
    static VRTK_ControllerEvents leftHand, rightHand;

    private void Awake()
    {
        Device = Device.Unknown;
    }
    void Start()
    {
        StartCoroutine("CheckDvice");
    }


    IEnumerator CheckDvice()
    {
        int count = 0;
        while (!GameObject.Find("Oculus")&&!GameObject.Find("SteamVR"))
        {
                count++;
                if (count > 200)
                {
                    Debug.LogError("haven't found any device");
                    yield return new WaitForSeconds(1);
                }
            
            yield return null;

        }
        if (GameObject.Find("SteamVR"))
        {
            SetSteamVR();
            yield break;
        }
        if (GameObject.Find("Oculus"))
        {
            SetOculus();
            yield break;
        }
        yield break;
    }


    void SetOculus()
    {
        var right = GameObject.FindGameObjectWithTag("RightHand");
        var left = GameObject.FindGameObjectWithTag("LeftHand");
        rightHand = right.GetComponent<VRTK_ControllerEvents>();
        leftHand = left.GetComponent<VRTK_ControllerEvents>();


        var rCon = right.GetComponent<VRTK_InteractGrab>();
        var lCon = left.GetComponent<VRTK_InteractGrab>();
        rCon.grabButton = VRTK_ControllerEvents.ButtonAlias.GripPress;
        lCon.grabButton = VRTK_ControllerEvents.ButtonAlias.GripPress;

        Device = Device.Oculus;
    }

    void SetSteamVR()
    {
        var right = GameObject.FindGameObjectWithTag("RightHand");
        var left = GameObject.FindGameObjectWithTag("LeftHand");
        rightHand = right.GetComponent<VRTK_ControllerEvents>();
        leftHand = left.GetComponent<VRTK_ControllerEvents>();

        var rCon = right.GetComponent<VRTK_InteractGrab>();
        var lCon = left.GetComponent<VRTK_InteractGrab>();
        rCon.grabButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;
        lCon.grabButton = VRTK_ControllerEvents.ButtonAlias.TriggerPress;

        Device = Device.SteamVR;
    }


    public static void Set_RightGrabButtonDownEvent(Action<object, ControllerInteractionEventArgs> action)
    {
        switch (Device)
        {
            case Device.Oculus:
                rightHand.GripPressed += new ControllerInteractionEventHandler(action);
                break;
            case Device.SteamVR:
                rightHand.TriggerPressed += new ControllerInteractionEventHandler(action);
                break;
            default: Debug.LogError("device is not found"); break;
        }

    }

    public static void Set_LeftGrabButtonDownEvent(Action<object, ControllerInteractionEventArgs> action)
    {
        switch (Device)
        {
            case Device.Oculus:
                leftHand.GripPressed += new ControllerInteractionEventHandler(action);
                break;
            case Device.SteamVR:
                leftHand.TriggerPressed += new ControllerInteractionEventHandler(action);
                break;
            default: Debug.LogError("device is not found"); break;
        }

    }

    public static void Set_ResetButtonDownEvent(Action<object, ControllerInteractionEventArgs> action)
    {
        switch (Device)
        {
            case Device.Oculus:
                leftHand.StartMenuPressed += new ControllerInteractionEventHandler(action);
                break;
            case Device.SteamVR:
                rightHand.ButtonTwoPressed += new ControllerInteractionEventHandler(action);
                break;
            default: Debug.LogError("device is not found"); break;
        }

    }

    public static void Set_UseButtonDownEvent(Action<object, ControllerInteractionEventArgs> action)
    {
        switch (Device)
        {
            case Device.Oculus:
                rightHand.ButtonTwoPressed += new ControllerInteractionEventHandler(action);
                break;
            case Device.SteamVR:
                rightHand.GripPressed += new ControllerInteractionEventHandler(action);
                break;
            default: Debug.LogError("device is not found"); break;
        }
    }
}
