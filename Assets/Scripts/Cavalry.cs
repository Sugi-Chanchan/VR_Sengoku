using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cavalry : Enemy
{
    Transform player;
    [SerializeField] Animator anim;
    [SerializeField]bool attack = false;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("SetUp", 1);
        
    }

    // Update is called once per frame
    void Update()
    {
        var dir = player.position - transform.position;
        if (Vector3.Dot(-dir.normalized,player.forward)>0.7f||dir.sqrMagnitude<900)
        {

            transform.rotation = Quaternion.LookRotation(dir);
            transform.position = Vector3.MoveTowards(transform.position, player.position, 1 * Time.deltaTime);

            if (dir.sqrMagnitude < 10&&!attack)
            {
                attack = true;          
                StartCoroutine("Attack");
            }
        }


        
    }

    void SetUp()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    IEnumerator Attack()
    {
        int r = (Random.value > 0.5f) ? 1 : 2;
        anim.SetInteger("AttackNumber", r);
        yield return new WaitForSeconds(1);
        anim.SetInteger("AttackNumber", 0);
        yield return new WaitForSeconds(1);
        attack = false;
    }
}
