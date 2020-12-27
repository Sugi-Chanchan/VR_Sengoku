using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System;

public class Horse : MonoBehaviour
{
    Transform root;
    [SerializeField] Reins reins ;
    VRTK_InteractableObject[] interactableObjects = new VRTK_InteractableObject[2];
    (Vector3 right ,Vector3 left) startpostions;
    (Vector3 right,Vector3 left) displacement ;
    (VRTK_VelocityEstimator right, VRTK_VelocityEstimator left) VRTKVelEstim;
    [SerializeField] float rotateSpeed;
    const float maxSpeedLevel=3;
    [SerializeField]float speedLevel;
    bool bothHands=false,setupped;
    
    private void Start()
    {
        root = transform.root;
        Invoke("SetUp", 0.2f);
    }

    void SetUp()
    {
        VRTKVelEstim.left= reins.left.GetComponent<VRTK_VelocityEstimator>();
        VRTKVelEstim.right= reins.right.GetComponent<VRTK_VelocityEstimator>();

        startpostions.left = reins.left.transform.localPosition;
        interactableObjects[0] = reins.left.GetComponent<VRTK_InteractableObject>();
        startpostions.right = reins.right.transform.localPosition;
        interactableObjects[1] = reins.right.GetComponent<VRTK_InteractableObject>();
        setupped = true;
    }

    private void Update()
    {

        if (!setupped) return;

        SpeedCheck();

        if (GrabbedCheck()) //右手左手のどちらか1つでも手綱を掴んでいれば実行
        {
            var averageDisPlacement = ((reins.left.transform.localPosition - startpostions.left) + (reins.right.transform.localPosition - startpostions.right)) / 2;//右手と左手の平均をとる
            Rotate(averageDisPlacement.x);
            Acceleration();
            /*if (bothHands)*/ Deceleration(averageDisPlacement.z - averageDisPlacement.y);//両手で手綱を掴んでたら減速
        }

        var speed = SpeedFunction(speedLevel);
        var vel = root.transform.forward * Time.deltaTime * speed;
        root.position = root.position + vel;
    }

    float returnspeed = 0.02f;
    bool GrabbedCheck()//手綱を掴んでいるかの判定
    {
        bothHands = false;

        if (interactableObjects[0].IsGrabbed())
        {
            if (!interactableObjects[1].IsGrabbed())
            {
                //片手しか掴んでないときは掴んでない方の変位をもう片方に合わせる
                displacement.left = displacement.right = reins.left.transform.localPosition - startpostions.left;
                reins.right.transform.localPosition = startpostions.right+ Vector3.MoveTowards(reins.right.transform.localPosition-startpostions.right,displacement.right,returnspeed);
            }
            else
            {
                bothHands = true;
            }

            return true;
        }
        else if (interactableObjects[1].IsGrabbed())
        {
            displacement.left = displacement.right = reins.right.transform.localPosition - startpostions.right;
            reins.left.transform.localPosition =startpostions.left+ Vector3.MoveTowards(reins.left.transform.localPosition-startpostions.left, displacement.left, returnspeed);
            return true;
        }
        else
        {
            displacement.left = displacement.right= Vector3.zero;
            reins.right.transform.localPosition = Vector3.MoveTowards(reins.right.transform.localPosition, startpostions.right, returnspeed);
            reins.left.transform.localPosition = Vector3.MoveTowards(reins.left.transform.localPosition, startpostions.left, returnspeed);
        }

        return false;
    }

    void Rotate(float holizontal)
    {
        var rot = Quaternion.Euler(new Vector3(0, holizontal * rotateSpeed*Time.deltaTime, 0));
        root.rotation = rot * root.rotation;
    }

    bool accCoolTime=false;
    void Acceleration ()
    {


        if (!accCoolTime&& AccelerationCheck())
        {
            speedLevel += 1;
            speedLevel = Mathf.Min(speedLevel, maxSpeedLevel);

            accCoolTime = true;
            Invoke("RemoveAccCoolTime", 0.5f);
            
        }
    }

    const int preVelocitySize = 3;
    Vector3[] preVelocity = new Vector3[preVelocitySize];
    int count=0;
    bool AccelerationCheck()
    {

        //Vector3 velocity=

        Vector3 leftAcc =VRTKVelEstim.left.GetAccelerationEstimate();
        Vector3 rightAcc =VRTKVelEstim.right.GetAccelerationEstimate();
        float leftValue = Math.Abs(leftAcc.y) + Math.Abs(leftAcc.z) - Math.Abs(leftAcc.x);
        float rightValue = Math.Abs(rightAcc.y) + Math.Abs(rightAcc.z) - Math.Abs(rightAcc.x);



        //preVelocity[count++%preVelocitySize]=

        return (Math.Max(leftValue,rightValue)) > 200;
    }

    void RemoveAccCoolTime()
    {
        accCoolTime = false;
    }

    void Deceleration(float backward)
    {
        if (backward < -0.82f)
        {
            speedLevel += backward * Time.deltaTime * 2;
            speedLevel = Mathf.Max(speedLevel, 0);
            //speed += backward * Time.deltaTime * 20;
            //speed = Mathf.Max(speed, 0);
        }
    }

    void SpeedCheck()
    {
        if (speedLevel > maxSpeedLevel - 1&&!accCoolTime)
        {
            speedLevel = Mathf.Lerp(maxSpeedLevel - 1, speedLevel, 0.99f);
        }
    }

    [SerializeField] float speedFactor = 2.5f;
    float SpeedFunction(float speedStage)
    {
        float s = speedStage;
        return speedFactor * s * s;
    }

    

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Enemy>())
        {
            var dir=(collision.transform.position - transform.position).normalized;
            dir.y = 0.2f;
            collision.gameObject.GetComponent<Rigidbody>().AddForce(dir*20, ForceMode.Impulse);
        }
    }

  [System.Serializable]
    struct Reins
    {
        public GameObject right, left;
    }
}
