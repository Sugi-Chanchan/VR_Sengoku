using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reins : MonoBehaviour
{
    Vector3 displacement;
    Vector3 startPosisiopn;
    // Start is called before the first frame update
    void SetUp()
    {
        startPosisiopn = transform.localPosition;
    }
}
