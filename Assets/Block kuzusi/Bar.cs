using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bar : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform BarTrans = GameObject.Find("Bar").transform;
        Vector3 pos = BarTrans.position;

        if(Input.GetKey(KeyCode.RightArrow))
        {
            if(pos.x < 20)
            {
                this.transform.Translate(0.5f, 0f, 0f);
            }
        }
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            if(pos.x > -20)
            {
                this.transform.Translate(-0.5f, 0f, 0f);
            }
        }
    }
}
