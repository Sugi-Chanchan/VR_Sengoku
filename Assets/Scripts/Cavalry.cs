using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cavalry : Enemy
{
    Transform player;
    [SerializeField] Animator anim;
    [SerializeField]bool attack = false;
    [SerializeField] EnemyWeapon halberd;
    [SerializeField] float speed;
    bool setupped;
    // Start is called before the first frame update
    void Start()
    {
        //anim.SetFloat("AttackSpeed", 1.0f);
        //Invoke("SetUp", 1);
        //halberd.Prevented += this.Prevented;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!setupped) return;

        var dir = player.position - transform.position;
        dir.y = 0;
        if (Vector3.Dot(-dir.normalized,player.forward)>0.7f||dir.sqrMagnitude<900)
        {

            transform.rotation = Quaternion.LookRotation(dir);
            
            transform.position +=dir.normalized*Time.deltaTime*speed ;

            if (dir.sqrMagnitude < 100&&!attack)
            {
                attack = true;          
                StartCoroutine("Attack");
            }
        }


        
    }

    void SetUp()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        setupped = true;
    }

    void Prevented(object sender,EventArgs e)
    {
        anim.SetTrigger("Prevented");
        anim.SetFloat("AttackSpeed", 0.0f);
        //anim.Play("Prevented");
    }
    


    IEnumerator Attack()
    {
        int r = (UnityEngine.Random.value > 0.5f) ? 1 : 2;
        anim.SetInteger("AttackNumber", 2);
        yield return new WaitForSeconds(3);
        anim.SetInteger("AttackNumber", 0);
        yield return new WaitForSeconds(1);
        attack = false;
        anim.SetFloat("AttackSpeed", 1.0f);
    }
}
