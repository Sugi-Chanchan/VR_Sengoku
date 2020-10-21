using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Japanese_Bow : MonoBehaviour
{
    VRTK_InteractableObject interactableObject;

    private float maxBowstring = 1.174f; //1.174は弦をギリギリまで引っ張った時のbowからbowstringまでの距離。弓の大きさなどを変化させた場合は具体的な値を計測する必要がある
    [SerializeField] int followBowstringInt = 70; //自分の手にどのくらい弦が追従するか
    [SerializeField] float arrowPower = 70; //矢が発射される強さ
    private Transform weaponPositionLeft;
    private Transform bow, bowstring;
    private Transform arrow;
    private Rigidbody arrowRigidbody;
    private bool flagTrigger;
    private Vector3 firstBowToBowstring, bowToBowstring;
    private float firstDrowABowDistance, drowABowDistance, tem, per;
    private Vector3 firstBowstringPosition;
    private Animator bowAnimator;
    //Quaternion criteriaRotQuat;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        bowAnimator.SetBool("Shoot", false);

        per = perCulc(bow, bowstring, tem, firstDrowABowDistance); //perCulc関数はどのくらい弦を引いたか計算する関数

        if(interactableObject.IsGrabbed())
        {
            bowAnimator.SetBool("Drow", true);

            bowAnimator.Play("drowAnimation", 0, Mathf.Clamp(per/followBowstringInt, 0.0f, 0.99f));
            flagTrigger = true;
        }
        else if(flagTrigger)
        {
            bowAnimator.SetBool("Drow", false);

            bowstring.localPosition = firstBowstringPosition; //bowstringの位置を初期状態に

            arrowProcess(); //矢の親を変更したり、発射したりする関数

            flagTrigger = false;
        }
    }

    void Setup(){
        bowAnimator = GetComponent <Animator> (); //Thisの持つアニメーションコントローラーを持ってくる

        bow = GameObject.Find("Bow").transform; //子にJapanese_bowを持つSphereのtransformを持ってくる
        bowstring = GameObject.Find("Bowstring").transform; //弦に対応するSphereのtransformを持ってくる
        interactableObject = bowstring.GetComponent<VRTK_InteractableObject>();

        firstBowstringPosition = bowstring.localPosition; //初期のbowstringの位置を取得
        firstBowToBowstring = bow.localPosition - bowstring.localPosition; //bowからbowstringまでの三次元ベクトル
        firstDrowABowDistance = firstBowToBowstring.sqrMagnitude; //初期のbowからbowstringまでの距離
        tem = maxBowstring - firstDrowABowDistance; //弓の弾く値の範囲

        arrow = GameObject.Find("Arrow").transform; //矢のtransformを取得
        flagTrigger = false; //Triggerが押されたかどうかのフラグ

        weaponPositionLeft = GameObject.FindGameObjectWithTag("WeaponPositionLeft").transform; //左手のGameObjectのtransformを取得
        transform.parent = weaponPositionLeft; //Thisの親を左手に変更
        transform.localPosition = Vector3.zero;
    }

    private float perCulc(Transform x, Transform y, float xYRange, float firstDistance)
    {
        Vector3 yToX;
        float yToXDistance, re;

        yToX = x.localPosition - y.localPosition;
        yToXDistance = yToX.sqrMagnitude;
        re = (yToXDistance - firstDistance) / (xYRange/100);

        return re;
    }

    void arrowProcess()
    {
        arrow.parent = GameObject.FindGameObjectWithTag("Stash").transform;
        arrowRigidbody = arrow.gameObject.AddComponent <Rigidbody> ();
        arrowRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        arrowRigidbody.AddForce(transform.right*per*arrowPower);
    }
}
