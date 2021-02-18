using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class Arrow : MonoBehaviour
{
    public int arrowMemorySize; //いくつまでの矢がArrowStashに存在するのを許容するかを決める数
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(this.transform.parent.tag == "Stash" && this.transform.parent.childCount >= arrowMemorySize) //矢の親がArrowStashかつArrowStashが2本以上の矢を持つとき
        {
            if(this.transform.parent.GetChild(0).gameObject == this.gameObject) //このスクリプトを実行してる矢は一番古い物であるか？
            {
                Invoke("ArrowDestroy", 1);
            }
        }
    }

    void ArrowDestroy()
    {
        Destroy(this.gameObject);
    }
}
