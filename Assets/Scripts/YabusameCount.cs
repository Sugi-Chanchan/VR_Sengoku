using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YabusameCount : MonoBehaviour
{
    public TextMesh count;
    public static int matos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        count.text = $"{matos}/3"; 
    }
}
