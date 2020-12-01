using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

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
    int num = 100000;


    public class TempA
    {
        public int num;
        public TempA other;
    }
    TempA[] arrayA;
    void Set()
    {
        arrayA = new TempA[num];
        for (int i = 0; i < num; i++)
        {
            arrayA[i] = new TempA();
        }
    }

    public void Test()
    {
        Set();


        int loop = 100;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        if (turn == 0)
        {
            for (int j = 0; j < loop; j++)
            {

                //ここから

                bool b;
                for(int i = 0; i < num; i++)
                {
                    b = arrayA[i].other != null;
                }




                //ここまで


            }

        }
        else
        {
            for (int j = 0; j < loop; j++)
            {


                //ここから


                bool b;
                for (int i = 0; i < num; i++)
                {
                    b = arrayA[i].num==1000;
                }




                //ここまで

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






    struct Astruct
    {
        public bool b;
        public int a;
        public Astruct(bool _b, int _a)
        {
            b = _b;
            a = _a;
        }
    }


}



