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
    int[] array = new int[] { 5, 1, 9, 10, 4, 23, 4, 1, 6, 3, 0, 456, 2, 4, 7, 0, 2, 4, 11 ,8264,6,5,34,16,0,46,13465,1,2,3,4,51,3};
    void Set()
    {
        ArraySort(array, 0, array.Length - 1);


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


                int a;
                for (int i = 0; i < num; i++)
                {
                    a = i << 5;
                }



                //ここまで


            }

        }
        else
        {
            for (int j = 0; j < loop; j++)
            {


                //ここから


                ulong a;
                var n = (ulong)num;
                for (ulong i = 0; i < n; i++)
                {
                    a = i << 5;
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

    void ArraySort(int[] array, int start, int end)
    {
        PrintArray();
        if (start == end) return;
        //c++;
        //if (c > 100) { return; }
        int pivot;
        {
            pivot = start + 1;
            int startValue = array[start];
            while (pivot <= end && startValue == array[pivot]) { pivot++; }
            if (pivot > end) return;
            pivot = (startValue >= array[pivot])? startValue : array[pivot];
        }



        int k = partition(array, start, end, pivot);
        ArraySort(array, start, k - 1);
        ArraySort(array, k, end);
    }
    int c = 0;
    int partition(int[] array, int start, int end, int value)
    {

        int f = start, b = end;
        while (f <= b)
        {
            while (f <= end && array[f] < value) { f++; }
            while (b >= start && array[b] >= value) { b--;}
            if (f > b) break;
            int tmp = array[f];
            array[f] = array[b];
            array[b] = tmp;
            f++; b--;
        }
        return f;
    }



    void PrintArray()
    {
        string s = "";
        foreach (int num in array)
        {
            s += num + ",";
        }
        print(s);
    }
}



