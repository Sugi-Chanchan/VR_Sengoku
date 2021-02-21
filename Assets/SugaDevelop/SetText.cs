using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetText : MonoBehaviour
{
    [SerializeField] string OculusText,ViveText;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    IEnumerator SetUp()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        GetComponent<Text>().text = (ButtonManager.Device == Device.Oculus) ? OculusText : ViveText;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
