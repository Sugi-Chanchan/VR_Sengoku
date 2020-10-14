using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class TriggerTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerPressed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void DoTriggerPressed(object sender, ControllerInteractionEventArgs e)
    {
        Debug.Log("Trigger Press");
    }
}
