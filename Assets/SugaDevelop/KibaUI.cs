using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KibaUI : MonoBehaviour
{
    static KibaUI kibaUI;

    public static KibaUI ui
    {
        get
        {
            return kibaUI; 
        }
    }


    private void Awake()
    {
        kibaUI = this;
    }

    public Text hpText;
    // Start is called before the first frame update
    void Start()
    {

    }


    public void SetHp(int hp)
    {
        hpText.text = "HP:" + hp;
    }

}
