using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBox : MonoBehaviour
{
    Vector3 pos;
    // Start is called before the first frame update
    void Start()
    {
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = pos;
    }

    public void Red()
    {
        GetComponent<Renderer>().material.color = Color.red;
        Invoke("White", 1);
    }

    public void White()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
}
