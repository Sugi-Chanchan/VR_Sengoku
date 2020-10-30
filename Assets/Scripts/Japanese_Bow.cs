using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Japanese_Bow : MonoBehaviour
{
    VRTK_InteractableObject interactableObject;
    VRTK_InteractGrab interactGrab;

    [SerializeField] SkinnedMeshRenderer arrowSkin, japaneseBowSkin;
    int drowArrow, drowBow;

    private float maxBowstring = 1.174f; //1.174は弦をギリギリまで引っ張った時のbowからbowstringまでの距離。弓の大きさなどを変化させた場合は具体的な値を計測する必要がある
    [SerializeField] float followBowstringFloat; //自分の手にどのくらい弦が追従するか、初期0.8
    [SerializeField] float arrowPower; //矢が発射される強さ、初期100
    private Transform weaponPositionLeft;
    private Transform bow, bowstring;
    [SerializeField] Transform arrowParent, arrow;

    private GameObject rightHand;
    private Rigidbody arrowRigidbody;
    private bool flagTrigger, setupped;
    private Vector3 firstBowToBowstring, bowToBowstring;
    private float firstDrowABowDistance, drowABowDistance, tem, per;
    private Vector3 firstBowstringPosition;

    public bool load;

    //public Arrow arrowScript;
    //private Animator bowAnimator;
    //Quaternion criteriaRotQuat;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        if(!setupped) return;

        //print(interactGrab.IsGrabButtonPressed());
        //print(interactGrab.GetGrabbedObject() != null);
        //print(interactGrab.GetGrabbedObject().transform.GetChild(0).CompareTag("Arrow"));
        if(interactGrab.IsGrabButtonPressed() && interactGrab.GetGrabbedObject() != null && interactGrab.GetGrabbedObject().transform.GetChild(0).CompareTag("Arrow"))
        {          
            arrowParent = interactGrab.GetGrabbedObject().transform; //矢のtransformを取得
            print(arrowParent.name);
            print("aaa1");
            arrow = arrowParent.GetChild(0);
            
            arrowSkin = arrow.GetComponent<SkinnedMeshRenderer>();
            drowArrow = arrowSkin.sharedMesh.GetBlendShapeIndex("Key 1");

            if((bowstring.position - arrowParent.position).sqrMagnitude < 0.01f)
            {
                arrow.parent = bowstring.parent;

                ArrowUnderBowMain();
            }
        }

        per = perCulc(bow, bowstring, tem, firstDrowABowDistance); //perCulc関数はどのくらい弦を引いたか計算する関数

        if(interactableObject.IsGrabbed()) //条件：InteractableObjectが掴まれたとき
        {
            japaneseBowSkin.SetBlendShapeWeight(drowBow, Mathf.Clamp(per/followBowstringFloat, 0.0f, 100.0f));

            if(load)
            {
                arrowSkin.SetBlendShapeWeight(drowArrow, Mathf.Clamp(per/followBowstringFloat, 0.0f, 100.0f));
            }
            flagTrigger = true;
        }
        else if(flagTrigger)
        {
            japaneseBowSkin.SetBlendShapeWeight(drowBow, 0.0f);

            bowstring.localPosition = firstBowstringPosition; //bowstringの位置を初期状態に

            if(load)
            {
                arrowSkin.SetBlendShapeWeight(drowArrow, 0.0f);
                print("Released");
                arrowProcess(); //矢の親を変更したり、発射したりする関数
            }

            flagTrigger = false;
        }
    }

    void Setup(){
        load = false; //矢をつがえたかどうかフラグ
        flagTrigger = false; //Triggerが押されたかどうかのフラグ

        rightHand = GameObject.FindGameObjectWithTag("RightHand");
        interactGrab = rightHand.GetComponent<VRTK_InteractGrab>();

        bow = GameObject.Find("Bow").transform; //子にJapanese_bowを持つSphereのtransformを持ってくる
        bowstring = GameObject.Find("Bowstring").transform; //弦に対応するSphereのtransformを持ってくる
        interactableObject = bowstring.GetComponent<VRTK_InteractableObject>();

        firstBowstringPosition = bowstring.localPosition; //初期のbowstringの位置を取得
        firstBowToBowstring = bow.localPosition - bowstring.localPosition; //bowからbowstringまでの三次元ベクトル
        firstDrowABowDistance = firstBowToBowstring.sqrMagnitude; //初期のbowからbowstringまでの距離
        tem = maxBowstring - firstDrowABowDistance; //弓の弾く値の範囲

        japaneseBowSkin = transform.GetChild(0).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        drowBow = japaneseBowSkin.sharedMesh.GetBlendShapeIndex("Key 1");

        weaponPositionLeft = GameObject.FindGameObjectWithTag("WeaponPositionLeft").transform; //左手のGameObjectのtransformを取得
        transform.parent = weaponPositionLeft; //Thisの親を左手に変更
        transform.localPosition = Vector3.zero;

        setupped = true;
    }

    private float perCulc(Transform x, Transform y, float xYRange, float firstDistance)
    {
        float yToXDistance, re;

        yToXDistance = (x.localPosition - y.localPosition).sqrMagnitude;
        re = (yToXDistance - firstDistance) / (xYRange/100);

        return re;
    }

    void arrowProcess()
    {
        load = false;
        print("shoot");
        arrow.parent = GameObject.Find("ArrowStash").transform;
        if(!(arrowRigidbody = arrow.gameObject.GetComponent <Rigidbody> ()))
        {
            arrowRigidbody = arrow.gameObject.AddComponent <Rigidbody> ();
        }
        arrowRigidbody.useGravity = true;
        //arrowRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        arrowRigidbody.AddForce(transform.right*per*arrowPower);
    }

    void ArrowUnderBowMain() //矢がBowMainの下にある場合にちょうどいい場所に移動させる関数
    {
        arrow.localPosition = new Vector3(-0.004f, 0.0134f, 0.021f);
        arrow.localRotation = Quaternion.Euler(-90.0f, 0.0f, 0.0f);
        
        load = true;
    }
}