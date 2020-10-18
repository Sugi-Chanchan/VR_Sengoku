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

    [SerializeField] float walkDistance = 30.0f;
    [SerializeField] float attackDistance = 5.0f;

    private CharacterController characterController;
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent <CharacterController> ();
        animator = GetComponent <Animator> ();
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
            animator.SetInteger("Distance", 5);
            transform.rotation = Quaternion.LookRotation(pPos-pos);
            transform.Translate((pPos - pos).normalized * speed * 2 * Time.deltaTime,Space.World);
        }
        
        if(distance > attackDistance && distance < walkDistance){ //プレイヤーまでの距離が近いとき
            animator.SetInteger("Distance", 3);
            transform.rotation = Quaternion.LookRotation(pPos-pos);
            transform.Translate((pPos - pos).normalized * speed * Time.deltaTime,Space.World);
        }

        if(distance < attackDistance){ //プレイヤーが攻撃範囲にいるとき
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
