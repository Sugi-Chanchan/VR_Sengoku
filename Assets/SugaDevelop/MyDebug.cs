using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyDebug : MonoBehaviour
{
    public static void PutPoint(string name,Vector3 pos,string text=null)
    {
        GameObject g = new GameObject(name);
        g.transform.position = pos;
        if (text != null)
        {
            g.AddComponent<MyDebugText>().text = text;
        }
    }
}

public class MyDebugText : MonoBehaviour
{
    public string text;
}