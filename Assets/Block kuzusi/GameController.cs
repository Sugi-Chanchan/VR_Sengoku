using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject ClearUI;
    public GameObject GameOverUI;
    public int count;
    public int count1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        count = GameObject.FindGameObjectsWithTag("Block").Length;
        count1 = GameObject.FindGameObjectsWithTag("Player").Length;
    
        if(count == 0)
        {
            ClearUI.SetActive(true);
        }

        if(count1 == 0 && count != 0)
        {
            GameOverUI.SetActive(true);
        }
    }
}
