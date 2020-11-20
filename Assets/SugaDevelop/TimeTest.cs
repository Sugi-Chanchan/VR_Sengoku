using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        




        Invoke("Test", 1);
    }

    private void Update()
    {

        // print(list.Count);
    }
    long[] time = new long[2];
    long[] count = new long[2];
    int turn = 0;
    int[] win = new int[2];
    public void Test()
    {
        Set();


        int loop = 100;
        int num = 100000;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        if (turn == 0)
        {
            for (int j = 0; j < loop; j++)
            {

                int a = 0;
                int b = 0;
                for (int i = 0; i < num; i++)
                {
                    a=-i;
                }
                for (int i = 0; i < num; i++)
                {
                    b = a * a;
                }
            }

        }
        else
        {

            for (int j = 0; j < loop; j++)
            {

                int a = 0;
                int b = 0;
                for (int i = 0; i < num; i++)
                {
                    a = -i;
                }
                for (int i = 0; i < num; i++)
                {
                    b = System.Math.Abs(a);
                }
            }

        }

        sw.Stop();
        time[turn] += sw.ElapsedMilliseconds;
        count[turn]++;
        float av = (float)time[turn] / (float)count[turn];

        print(av + "ms, turn: " + ((turn == 0) ? "F" : "S"));
        if (count[1] == 5)
        {
            if (time[0] < time[1])
            {
                win[0]++;
            }
            else if (time[0] > time[1])
            {
                win[1]++;
            }

            print("Win F:S=" + win[0] + ":" + win[1]);

            count = new long[2] { 0, 0 };
            time = new long[2] { 0, 0 };
        }
        turn = (turn + 1) % 2;
        Invoke("Test", 4f);
    }

  

    void Set()
    {
        unsafe
        {
            int filter = -0x00000001;
            float f = -0.33f;
            //int i= (int)(f*1024)<<15;
            int i = -2;
            int ii = i | filter;
            int iii = i & filter;
            print(ii);
            print(iii);
        }

    }

    
    public void T()
    {

    }


}



