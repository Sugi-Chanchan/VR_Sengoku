using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SugiTest1 : MonoBehaviour
{
    Dictionary<int,List<int>> dic=new Dictionary<int, List<int>>();
    List<int> list;
    List<string> list2 = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        //list = Abstract_Virtual.list;
        dic.Add(1, Abstract_Virtual.list);
    }

    // Update is called once per frame
    void Update()
    {
        dic[1].Add(3);
        //list.Add(3);
    }
}
