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

    private bool triggerBool, triggerBool2, firstArrowFlag;

    private GameObject newArrow;

    VRTK_InteractGrab grabbingController;
    VRTK_ControllerEvents buttonController;

    // Start is called before the first frame update
    void Start()
    {
        spawnDelayTimer = 0.0f;
        triggerBool = false;
        triggerBool2 = false;
        firstArrowFlag = true;

        transform.parent.parent = GameObject.FindGameObjectWithTag("Uma").transform;

        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame

    private void OnTriggerEnter(Collider collider)
    {
        if(collider.gameObject.GetComponent<VRTK_InteractGrab>()) //Thisに触れたGameObjectのVRTK_InteractGrabを取得。おそらくプラットフォームごとに親子関係が違うので配慮している
        {
            grabbingController = collider.gameObject.GetComponent<VRTK_InteractGrab>();
        }
        else if(collider.gameObject.GetComponentInParent<VRTK_InteractGrab>())
        {
            grabbingController = collider.gameObject.GetComponentInParent<VRTK_InteractGrab>();
        }
        else //誤って矢がThisに触れてしまったときは無視
        {
            return;
        }

        if(!triggerBool && !buttonController.triggerTouched) //二つ目の条件は矢を持ちながら矢筒に触れた時を除外している
        {
            //(!newArrow.gameObject.GetComponent<VRTK_InteractableObject>().IsGrabbed() || firstArrowFlag)
            firstArrowFlag = false;

            newArrow = Instantiate(arrowPrefab);
            newArrow.name ="ArrowClone";
            newArrow.transform.parent = this.transform.parent;
            //newArrow.gameObject.GetComponent<Rigidbody>().useGravity = false;
            Destroy(newArrow.gameObject.GetComponent<Rigidbody>());

            newArrow.gameObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.006f);
            newArrow.gameObject.transform.localRotation = Quaternion.Euler(180.0f, 0.0f, 0.0f);
        }
        else //トリガー押しながら触れてしまったときはtriggerBoolはfalseであってほしいのでreturn
        {
            return;
        }

        triggerBool = true;
    }
    private void OnTriggerStay(Collider collider)
    {
        //print(CanGrab(grabbingController));
        /*
        if(CanGrab(grabbingController))
        {
        }
        */
        if(newArrow != null && newArrow.gameObject.GetComponent<VRTK_InteractableObject>().IsGrabbed() == true)
        {
            triggerBool = false;
        }
        else
        {
            //triggerBool = true;
            return;
        }
    }

    private void Update()
    {
        if(!firstArrowFlag)
        {
            if(newArrow != null && newArrow.gameObject.GetComponent<VRTK_InteractableObject>().IsGrabbed() == true) //生成した矢が掴まれたときトリガーをオンにする
            {
                triggerBool2 = true;
            }

            if(triggerBool2)
            {
                if (!buttonController.triggerTouched) //前回矢を掴んでから話したタイミング
                {
                    triggerBool2 = false;

                    if (newArrow.gameObject.transform.parent.tag == "Weapon") //矢が弓につがえられていたときは何もしない
                    {
                        return;
                    }
                    else if (newArrow.gameObject.GetComponent<Rigidbody>() != null)//これは何もしない状態で矢を離した時を想定（そのままだと矢の親が矢筒のままなので自分についてきてしまう！）
                    {
                        newArrow.gameObject.transform.parent = GameObject.FindGameObjectWithTag("Stash").transform;
                    }
                }
            }
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
    void Setup()
    {
        buttonController = GameObject.FindGameObjectWithTag("RightHand").GetComponent<VRTK_ControllerEvents>();
    }
}
