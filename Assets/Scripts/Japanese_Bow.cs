using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Japanese_Bow : MonoBehaviour
{
    private const float maxBowstring = 1.174f; //1.174は弦をギリギリまで引っ張った時のbowからbowstringまでの距離。弓の大きさなどを変化させた場合は具体的な値を計測する必要がある
    [SerializeField] int followBowstringInt = 70; //自分の手にどのくらい弦が追従するか
    private Transform weaponPositionLeft;
    private Transform bow, bowstring;
    private Vector3 firstBowToBowstring, bowToBowstring;
    float firstDrowABowDistance, drowABowDistance, tem, per;
    private Vector3 firstBowstringPosition;
    private Animator bowAnimator;
    //Quaternion criteriaRotQuat;
    // Start is called before the first frame update
    void Start()
    {
        bowAnimator = GetComponent <Animator> (); //Thisの持つアニメーションコントローラーを持ってくる

        bow = this.transform.Find("bow"); //子にJapanese_bowを持つSphereのtransformを持ってくる
        bowstring = this.transform.Find("bowstring"); //弦に対応するSphereのtransformを持ってくる
        firstBowstringPosition = bowstring.localPosition; //初期のbowstringの位置を取得
        firstBowToBowstring = bow.localPosition - bowstring.localPosition; //bowからbowstringまでの三次元ベクトル
        firstDrowABowDistance = firstBowToBowstring.sqrMagnitude; //初期のbowからbowstringまでの距離
        Invoke("Setup", 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        bowToBowstring = bow.localPosition - bowstring.localPosition; //bowからbowstringまでの三次元ベクトル
        drowABowDistance = bowToBowstring.sqrMagnitude; //具体的なbowからbowstringまでの距離
        tem = maxBowstring - firstDrowABowDistance;
        per = (drowABowDistance - firstDrowABowDistance) / (tem/100); //初期状態から弦をどのくらい引いたか。per%となる
        //print((bowstring.localPosition - firstBowstringPosition).sqrMagnitude);

        if((bowstring.localPosition - firstBowstringPosition).sqrMagnitude < 0.001f)
        {
            bowAnimator.SetBool("Drow", false);
        }
        else
        {
            bowAnimator.SetBool("Drow", true);

            //transform.Find("Cube").rotation = Quaternion.FromToRotation(Vector3.forward, -bowToBowstring);

            if(per <= 100)
            {
                bowAnimator.Play("Japanese_Bow", 0, per/followBowstringInt);
            }
            else if(per > 100)
            {
                bowAnimator.Play("Japanese_Bow", 0, 100);
            }
        }
        //print(drowABowDistance);1
    }

    void Setup(){
        weaponPositionLeft = GameObject.FindGameObjectWithTag("WeaponPositionLeft").transform;
        transform.parent = weaponPositionLeft;
        transform.localPosition = Vector3.zero;
    }
}
