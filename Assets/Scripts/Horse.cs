using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System;

public class Horse : MonoBehaviour
{
    public Reins leftReins,rightReins;
    protected Transform root;
    [SerializeField] _Reins _reins ;
    (VRTK_VelocityEstimator right, VRTK_VelocityEstimator left) VRTKVelEstim;
    [SerializeField] float rotateSpeed;
    const float maxSpeedLevel=3;
    [SerializeField] protected float speedLevel;
    bool bothHands=false,setupped;
    [SerializeField] AudioClip hihiiin,burururu,pakara;
    AudioSource audioSource,pakaraAudioSource;
    private void Start()
    {
        root = transform.root;
        AudioSource[] sources = GetComponents<AudioSource>();
        audioSource = sources[0];
        pakaraAudioSource = sources[1];
        pakaraAudioSource.clip = pakara;
        pakaraAudioSource.Play();
        Invoke("SetUp", 0.2f);
    }

    void SetUp()
    {
        VRTKVelEstim.left= _reins.left.GetComponent<VRTK_VelocityEstimator>();
        VRTKVelEstim.right= _reins.right.GetComponent<VRTK_VelocityEstimator>();
        
        setupped = true;
    }

    private void Update()
    {

        if (!setupped) return;

        SpeedCheck();

        if (GrabbedCheck()) //右手左手のどちらか1つでも手綱を掴んでいれば実行
        {
            var averageDisPlacement = (leftReins.Displacement+rightReins.Displacement) / 2;//右手と左手のstartPositionからのズレの平均をとる
            Rotate(averageDisPlacement.x);
            Acceleration();
            /*if (bothHands)*/ Deceleration(averageDisPlacement.z - averageDisPlacement.y);//両手で手綱を掴んでたら減速
        }

        if (speedLevel >= 2)
        {
            pakaraAudioSource.UnPause();
        }
        else
        {
            pakaraAudioSource.Pause();
        }

        Move();
    }
    
    protected virtual void Move()
    {
        var speed = SpeedFunction(speedLevel);
        var vel = root.transform.forward * Time.deltaTime * speed;
        root.position = root.position + vel;
    }

    float returnspeed = 0.02f;
    bool GrabbedCheck()//手綱を掴んでいるかの判定
    {
        bothHands = false;

        if (leftReins.IsGrabbed)
        {
            if (!rightReins.IsGrabbed)
            {
                //片手しか掴んでないときは掴んでない方の変位をもう片方に合わせる
                rightReins.FollowOtherReins(leftReins);
            }
            else
            {
                bothHands = true;
            }

            return true;
        }
        else if (rightReins.IsGrabbed)
        {
            leftReins.FollowOtherReins(rightReins);
            return true;
        }
        else
        {
            leftReins.ReturnDefaultPos();
            rightReins.ReturnDefaultPos();
        }

        return false;
    }

    void Rotate(float holizontal)
    {
        var rot = Quaternion.Euler(new Vector3(0, holizontal * rotateSpeed*Time.deltaTime, 0));
        root.rotation = rot * root.rotation;
    }

    bool accCoolTime=false;
    bool hihinCoolTime = false;
    void Acceleration ()
    {
        if (!accCoolTime&& (leftReins.IsWhipping||rightReins.IsWhipping))
        {
            speedLevel += 1;
            speedLevel = Mathf.Min(speedLevel, maxSpeedLevel);

            accCoolTime = true;
            Invoke("RemoveAccCoolTime", 0.5f);

            if (!hihinCoolTime)
            {
                hihinCoolTime = true;
                audioSource.PlayOneShot(hihiiin);
                Invoke("RemoveHihinTime", 4);
            }
        }
    }
    void RemoveHihinTime()
    {
        hihinCoolTime = false;
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

    bool deceleCooltime = false;
    void Deceleration(float backward)
    {
        if (backward < -0.82f)
        {
            speedLevel += backward * Time.deltaTime * 2;
            speedLevel = Mathf.Max(speedLevel, 0);
            //speed += backward * Time.deltaTime * 20;
            //speed = Mathf.Max(speed, 0);
            if (!deceleCooltime)
            {
                deceleCooltime = true;
                audioSource.PlayOneShot(burururu);
                Invoke("RemoveDeceleCoolTime", 3);
            }
        }
    }

    void RemoveDeceleCoolTime()
    {
        deceleCooltime = false;
    }


    void SpeedCheck()
    {
        if (speedLevel > maxSpeedLevel - 1&&!accCoolTime)
        {
            speedLevel = Mathf.Lerp(maxSpeedLevel - 1, speedLevel, 0.99f);
        }
    }

    [SerializeField] float speedFactor = 2.5f;
    protected float SpeedFunction(float speedStage)
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
    struct _Reins
    {
        public GameObject right, left;
    }
}
