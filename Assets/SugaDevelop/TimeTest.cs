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

        list.Add(-1);
        list.Add(2);
        list.Add(10);
        list.Add(-10);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        list.Add(2);
        string s = "";
        foreach (int i in list)
        {
            s += i + ",";
        }
        print(s);


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
    int num = 1000;


    int[] array = new int[] { 5, 1, 9, 10, 4, 23, 4, 1, 6, 3, 0, 456, 2, 4, 7, 0, 2, 4, 11 ,8264,6,5,34,16,0,46,13465,1,2,3,-4,-10,5,-94,51,3,6,4,3,65,2,34,52,2,35,2,5,2,334,5,23,4,25,23,45,23,52,35,32,5,23,452,35,23,53,23,5,25,3,5,6,3,747,334,734,2,-20,9183,-4212089};
        UnsafeList<int> list = new UnsafeList<int>(5);
    void Set()
    {
        //ArraySort(array, 0, array.Length - 1);
        //PrintArray();
        
    }

    public void Test()
    {
        Set();


        int loop = 100000;
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        if (turn == 0)
        {
            for (int j = 0; j < loop; j++)
            {

                //ここから

                int sum = 0;
                foreach(int i in list)
                {
                    sum += i;
                }


                //ここまで


            }
            
        }
        else
        {
            for (int j = 0; j < loop; j++)
            {


                //ここから


                int sum = 0;
                int[] array = list.unsafe_array;
                for(int i = 0; i < list.Count; i++)
                {
                    sum += array[i];
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

        int k;
        {
            int f = start, b = end;
            while (f <= b)
            {
                while (f <= end && array[f] < pivot) { f++; }
                while (b >= start && array[b] >= pivot) { b--; }
                if (f > b) break;
                int tmp = array[f];
                array[f] = array[b];
                array[b] = tmp;
                f++; b--;
            }
            k = f;
        }
        //int k = partition(array, start, end, pivot);
        ArraySort(array, start, k - 1);
        ArraySort(array, k, end);
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



