using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using System.IO;
using System;

public class DebugReins : MonoBehaviour
{

    private VRTK_VelocityEstimator estim;
    Vector3 preAcc, prepreAcc, preprepreAcc, preVel, prepreVel, preprepreVel;

    // Start is called before the first frame update
    void Start()
    {
        estim = GetComponent<VRTK_VelocityEstimator>();
    }
    float preP1, prepreP1;
    string s, avs, sparameter1, sparameter2, sparameter3, sparameter4, sparameter5;
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
        Vector3 averageAcc = (momentAcceleration + preAcc + prepreAcc + preprepreAcc) / 3;

        float moment = momentAcceleration.y;
        float average = averageAcc.y;
        s += moment + "\n";
        avs += average + "\n";

        //s += (momentAcceleration.y * momentAcceleration.y + momentAcceleration.z * momentAcceleration.z - momentAcceleration.x * momentAcceleration.x) + "\n";
        //avs += (averageAcc.y * averageAcc.y + averageAcc.z * averageAcc.z - averageAcc.x * averageAcc.x) + "\n";
        Vector3 vel = estim.GetVelocityEstimate();
        var averageVel = (vel + preVel) / 2;

        float parameter1 = vel.y;
        sparameter1 += parameter1 + "\n";


        //float parameter2 = Math.Abs(momentAcceleration.y) + Math.Abs(momentAcceleration.z)- Math.Abs(momentAcceleration.x);
        float parameter2 = vel.z;
        sparameter2 += parameter2 + "\n";

        float ychange;
        if (vel.y > 0.3f && (preVel.y < -1 || prepreVel.y < -1 || preprepreVel.y < -1))
        {
            ychange = vel.y - preVel.y;
            ychange = Math.Max(ychange, vel.y - prepreVel.y);
            ychange = Math.Max(ychange, vel.y - preprepreVel.y);
        }
        else
        {
            ychange = 0;
        }


        float zchange;
        if (vel.z < -0.3f && (preVel.z > 1 || prepreVel.z > 1 || preprepreVel.z > 1))
        {
            zchange = vel.z - preVel.z;
            zchange = Math.Min(zchange, vel.z - prepreVel.z);
            zchange = Math.Min(zchange, vel.z - preprepreVel.z);
        }
        else
        {
            zchange = 0;
        }


        
        float parameter3 = ychange;
        sparameter3 += parameter3 + "\n";


        float parameter4 = zchange;
        sparameter4 += parameter4 + "\n";


        float parameter5 = ychange - zchange;
        sparameter5 += parameter5 + "\n";

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

            StreamWriter sw_p4 = new StreamWriter("../Parameter4Data.txt", false);
            sw_p4.WriteLine(sparameter4);
            sw_p4.Flush();
            sw_p4.Close();

            StreamWriter sw_p5 = new StreamWriter("../Parameter5Data.txt", false);
            sw_p5.WriteLine(sparameter5);
            sw_p5.Flush();
            sw_p5.Close();

            count = 0;
        }
        preprepreVel = prepreVel;
        prepreVel = preVel;
        preVel = vel;

        preprepreAcc = prepreAcc;
        prepreAcc = preAcc;
        preAcc = momentAcceleration;

    }
}
