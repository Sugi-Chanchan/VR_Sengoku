using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Horse : MonoBehaviour
{
    Transform root;
    public GameObject[] reins = new GameObject[2];
    VRTK_InteractableObject[] interactableObjects = new VRTK_InteractableObject[2];
    Vector3[] startpostions = new Vector3[2];
    Vector3[] displacement = new Vector3[2];
    VRTK_VelocityEstimator[] VRTKVelEstim = new VRTK_VelocityEstimator[2];
    [SerializeField] float rotateSpeed;
    const float maxSpeedLevel=3;
    [SerializeField]float speedLevel;
    bool bothHands=false;

    private void Start()
    {
        root = transform.root;
        Invoke("SetUp", 0.2f);
    }

    void SetUp()
    {
        for (int i = 0; i < 2; i++)
        {
            VRTKVelEstim[i] = reins[i].GetComponent<VRTK_VelocityEstimator>();
            startpostions[i] = reins[i].transform.localPosition;
            interactableObjects[i] = reins[i].GetComponent<VRTK_InteractableObject>();
        }
    }

    private void FixedUpdate()
    {
        SpeedCheck();

        if (GrabbedCheck()) //右手左手のどちらか1つでも手綱を掴んでいれば実行
        {
            var averageDisPlacement = ((reins[0].transform.localPosition - startpostions[0]) + (reins[1].transform.localPosition - startpostions[1])) / 2;//右手と左手の平均をとる
            Rotate(averageDisPlacement.x);
            Acceleration();
            if (bothHands) Deceleration(averageDisPlacement.z - averageDisPlacement.y);//両手で手綱を掴んでたら減速
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
                displacement[0] = displacement[1] = reins[0].transform.localPosition - startpostions[0];
                reins[1].transform.localPosition = startpostions[1]+ Vector3.MoveTowards(reins[1].transform.localPosition-startpostions[1],displacement[1],returnspeed);
            }
            else
            {
                bothHands = true;
            }

            return true;
        }
        else if (interactableObjects[1].IsGrabbed())
        {
            displacement[0] = displacement[1] = reins[1].transform.localPosition - startpostions[1];
            reins[0].transform.localPosition =startpostions[0]+ Vector3.MoveTowards(reins[0].transform.localPosition-startpostions[0], displacement[0], returnspeed);
            return true;
        }
        else
        {
            for(int i = 0; i < 2; i++)
            {
                displacement[i] = Vector3.zero;
                reins[i].transform.localPosition = Vector3.MoveTowards(reins[i].transform.localPosition, startpostions[i], returnspeed);
            }
        }

        return false;
    }

    void Rotate(float holizontal)
    {
        var rot = Quaternion.Euler(new Vector3(0, holizontal * rotateSpeed, 0));
        root.rotation = rot * root.rotation;
    }

    bool accCoolTime=false;
    void Acceleration ()
    {
        float yacc = Mathf.Min(VRTKVelEstim[0].GetAccelerationEstimate().y, VRTKVelEstim[1].GetAccelerationEstimate().y); //両手の加速度の小さい方を取得
        if (yacc < -200&&!accCoolTime)
        {
            speedLevel += 1;
            speedLevel = Mathf.Min(speedLevel, maxSpeedLevel);

            accCoolTime = true;
            Invoke("RemoveAccCoolTime", 0.5f);
            
        }
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

    float SpeedFunction(float speedStage)
    {
        float s = speedStage;
        return 2.5f * s * s;
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
}
