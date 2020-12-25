using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System.IO;
using System;

public class Reins : MonoBehaviour
{

    private VRTK_VelocityEstimator estim;
    Vector3 preAcc, prepreAcc,preprepreAcc;
    
    // Start is called before the first frame update
    void Start()
    {
        estim = GetComponent<VRTK_VelocityEstimator>();
    }
    float preP1, prepreP1;
    string s, avs,sparameter1,sparameter2,sparameter3;
    int count;
    bool write = true;
    // Update is called once per frame
    void Update()
    {
        Check();
    }

    int counter;
    void Check()
    {
        Vector3 momentAcceleration = estim.GetAccelerationEstimate();
        Vector3 averageAcc = (momentAcceleration + preAcc+prepreAcc+preprepreAcc) / 3;

        float moment = momentAcceleration.y;
        float average = averageAcc.y;
        s +=  moment+ "\n";
        avs +=  average+ "\n";

        //s += (momentAcceleration.y * momentAcceleration.y + momentAcceleration.z * momentAcceleration.z - momentAcceleration.x * momentAcceleration.x) + "\n";
        //avs += (averageAcc.y * averageAcc.y + averageAcc.z * averageAcc.z - averageAcc.x * averageAcc.x) + "\n";

        float parameter1 = (momentAcceleration.magnitude+preAcc.magnitude+prepreAcc.magnitude+preprepreAcc.magnitude)/4;
        sparameter1 += parameter1 + "\n";

        float parameter2 = Math.Abs(momentAcceleration.y) + Math.Abs(momentAcceleration.z);
        sparameter2 += parameter2 + "\n";

        float parameter3 = momentAcceleration.magnitude;
        sparameter3 += parameter3 + "\n";

        if (count++ > 100)
        {
            StreamWriter sw = new StreamWriter("../RawData.txt", false);
            sw.WriteLine(s);
            sw.Flush();
            sw.Close();
            StreamWriter sw_av = new StreamWriter("../AverageData.txt", false);
            sw_av.WriteLine(avs);
            sw_av.Flush();
            sw_av.Close();

            StreamWriter sw_p1 = new StreamWriter("../Parameter1Data.txt", false);
            sw_p1.WriteLine(sparameter1);
            sw_p1.Flush();
            sw_p1.Close();

            StreamWriter sw_p2 = new StreamWriter("../Parameter2Data.txt", false);
            sw_p2.WriteLine(sparameter2);
            sw_p2.Flush();
            sw_p2.Close();

            StreamWriter sw_p3 = new StreamWriter("../Parameter3Data.txt", false);
            sw_p3.WriteLine(sparameter3);
            sw_p3.Flush();
            sw_p3.Close();

        }

        preprepreAcc = prepreAcc;
        prepreAcc = preAcc;
        preAcc = momentAcceleration;

    }
}
