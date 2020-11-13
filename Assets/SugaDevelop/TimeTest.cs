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
        int loop = 100;
        int num = 100000;
        Set();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        if (turn == 0)
        {
            for (int j = 0; j < loop; j++)
            {

                int[] array = new int[num];
                int[] array2 = new int[num];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i;
                }
                for (int i = 0; i < array2.Length; i++)
                {
                    array2[i] = array[i];
                }
            }

        }
        else
        {

            for (int j = 0; j < loop; j++)
            {
                unsafe
                {

                    int[] array = new int[num];
                    int[] array2 = new int[num];
                    fixed (int* p1 = &array[0])
                    {

                        for (int i = 0; i < array.Length; i++)
                        {
                            p1[i] = i;
                        }
                        fixed(int* p2 = &array2[0])
                        {
                            for (int i = 0; i < array2.Length; i++)
                            {
                                p2[i] = p1[i];
                            }
                        }
                    }
                    
                    
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
    }

    
    public void T()
    {
        unsafe
        {

            float x = 1.0f / 3.0f;
            float y = 1.0f / 6.0f;
            float z = 0.1f;



            int* a = (int*)&x;
            print(*a);
        }
    }


}



