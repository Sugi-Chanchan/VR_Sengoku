using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Quiver : MonoBehaviour
{
    //public Transform quiverSpawn;
    public GameObject arrowPrefab;
    public float spawnDelay = 1.0f;
    private float spawnDelayTimer = 0.0f;

    private bool triggerBool;

    private GameObject newArrow;

    VRTK_InteractGrab grabbingController;

    // Start is called before the first frame update
    void Start()
    {
        spawnDelayTimer = 0.0f;
        triggerBool = false;
    }

    // Update is called once per frame

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.GetComponent<VRTK_InteractGrab>()) //Thisに触れたGameObjectのVRTK_InteractGrabを取得。おそらくプラットフォームごとに親子関係が違うので配慮している
        {
            grabbingController = collider.gameObject.GetComponent<VRTK_InteractGrab>();
        }
        else
        {
            grabbingController = collider.gameObject.GetComponentInParent<VRTK_InteractGrab>();
        }

        if(triggerBool == false)
        {
            newArrow = Instantiate(arrowPrefab);
            newArrow.name ="ArrowClone";
            newArrow.transform.parent = this.transform.parent;
            //newArrow.gameObject.GetComponent<Rigidbody>().useGravity = false;
            Destroy(newArrow.gameObject.GetComponent<Rigidbody>());

            newArrow.gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.006f);
            newArrow.gameObject.transform.localRotation = Quaternion.Euler(180.0f, 0.0f, 0.0f);
        }
        triggerBool = true;
    }
    private void OnTriggerStay(Collider collider)
    {
        //print(CanGrab(grabbingController));
        if(CanGrab(grabbingController))
        {
        }
        if(newArrow.gameObject.GetComponent<VRTK_InteractableObject>().IsGrabbed() == true)
        {
            triggerBool = false;
        }
        else
        {
            //triggerBool = true;
            return;
        }
    }

    /*
    private bool OnTriggerExit(Collider collider)
    {
        //return triggerBool = false;
        return true;
    }
    */

    private bool CanGrab(VRTK_InteractGrab grabbingController)
    {
        return(grabbingController && grabbingController.GetGrabbedObject() == null && grabbingController.IsGrabButtonPressed());
    }

    /*
    private bool NoArrowNothced(GameObject controller)
    {
        if(VRTK_DeviceFinder.IsControllerRightHand(controller))
        {
            GameObject controllerLeftHand = VRTK_DeviceFinder.GetControllerLeftHand(true);
            bow = controllerLeftHand.GetComponentInChildren<BowAim>();
            if (bow == null)
            {
                bow = VRTK_DeviceFinder.GetModelAliasController(controllerLeftHand).GetComponentInChildren<BowAim>();
            }
        }
        return (bow == null || !bow.HasArrow());
    }
    */
}
