using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugiAnimTest : MonoBehaviour
{
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Return()
    {
        anim.SetFloat("Speed",0.0f);
        anim.SetTrigger("Return");
    }
}
