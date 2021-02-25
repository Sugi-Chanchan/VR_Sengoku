using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cavalry : Enemy
{
    public Transform player;
    public Animator anim;
    [SerializeField] bool attack = false;
    public GameObject halberd;
    public float speed;
    bool setupped;
    AudioSource audioSource;
    public AudioClip voice, sound;
    KibaManager kibaManager;
    // Start is called before the first frame update
    void Start()
    {
        //anim.SetFloat("AttackSpeed", 1.0f);
        Invoke("SetUp", 1);
        //halberd.Prevented += this.Prevented;
        kibaManager = GameManager.Instance as KibaManager;
    }

    // Update is called once per frame
    void Update()
    {
        kibaManager.clear = false;
        var pos = transform.position;
        pos.y = kibaManager.GetHeadHeight()-0.5f;
        transform.position = pos;

        if (!setupped) return;

        var dir = player.position - transform.position;
        dir.y = 0;

        var nowdir = transform.forward;

        dir = Vector3.Slerp(nowdir, dir, 0.1f);
        //if (Vector3.Dot(-dir.normalized,player.forward)>0.7f||dir.sqrMagnitude<900)
        //{

            transform.rotation = Quaternion.LookRotation(dir);
            
            transform.position +=dir.normalized*Time.deltaTime*speed ;

            if (dir.sqrMagnitude < 100&&!attack)
            {
                attack = true;          
                StartCoroutine("Attack");
            }
        //}


        
    }

    void SetUp()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        setupped = true;
        audioSource = GetComponent<AudioSource>();
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

    public void WeaponCutted()
    {
        anim.SetBool("WeaponCutted", true);
        audioSource.PlayOneShot(sound);
        audioSource.PlayOneShot(voice);

        speed = 0;
    }
}
