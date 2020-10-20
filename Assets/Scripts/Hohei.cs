using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hohei : Enemy
{
    Transform player;
    [SerializeField] float speed;
    [SerializeField] Vector3 vel;

    private Vector3 posToPPos; //敵(This)からプレイヤーに向かうベクトル
    private float distance; //敵(This)からプレイヤーまでの距離

    [SerializeField] float walkDistance = 30.0f; //敵(This)が歩き出す範囲の半径
    [SerializeField] float attackDistance = 10.0f; //敵(This)が攻撃し始める範囲の半径

    private CharacterController characterController;
    private Animator animator;
    public BoxCollider weaponCollider;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent <CharacterController> ();
        animator = GetComponent <Animator> ();
        weaponCollider.enabled = false;
        Invoke("FindPlayer", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null) return;

        vel = GetComponent<Rigidbody>().velocity;

        var pos = transform.position;
        pos.y = 0;
        var pPos = player.position;
        pPos.y = 0;

        posToPPos = pPos - pos;
        distance = posToPPos.sqrMagnitude; //敵(This)からプレイヤーまでの距離

        if(distance >= walkDistance){ //プレイヤーまでの距離が遠いとき
            weaponCollider.enabled = false;
            animator.SetInteger("Distance", 5);
            transform.rotation = Quaternion.LookRotation(pPos-pos);
            transform.Translate((pPos - pos).normalized * speed * 2 * Time.deltaTime,Space.World);
        }
        
        if(distance > attackDistance && distance < walkDistance){ //プレイヤーまでの距離が近いとき
            weaponCollider.enabled = false;
            animator.SetInteger("Distance", 3);
            transform.rotation = Quaternion.LookRotation(pPos-pos);
            transform.Translate((pPos - pos).normalized * speed * Time.deltaTime,Space.World);
        }

        if(distance < attackDistance){ //プレイヤーが攻撃範囲にいるとき
            weaponCollider.enabled = true;
            animator.SetInteger("Distance", 1);
            transform.rotation = Quaternion.LookRotation(pPos-pos);
            //transform.Translate((pPos - pos).normalized * speed * Time.deltaTime,Space.World);
        }
    }

    void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
