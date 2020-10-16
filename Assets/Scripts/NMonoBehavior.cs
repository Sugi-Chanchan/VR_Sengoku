using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NMonoBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        Invoke("SetUp", 0.1f); 
    }

    protected virtual void SetUp() { }
}
