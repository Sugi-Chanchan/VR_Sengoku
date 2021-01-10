using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KibaManager : MonoBehaviour
{
    
    void Start()
    {
        StartCoroutine(SetBodyCollider());
    }

    IEnumerator SetBodyCollider()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerBody body= player.AddComponent<PlayerBody>();
        Transform start = new GameObject("Start").transform;
        Transform end = new GameObject("End").transform;
        start.parent = player.transform;
        end.parent = player.transform;
        start.localPosition = Vector3.zero;
        end.localPosition = -Vector3.up;
        body.SetCollider(start, end);
    }
}
