using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KibaGameManager : GameManager
{
    public GameObject blackOutPrefab;

    private void Start()
    {
        GameOver();
    }

    public override void GameOver()
    {
        var blackout = Instantiate(blackOutPrefab);
        blackout.transform.parent = _player.transform;
        blackout.GetComponent<MeshRenderer>().material.SetFloat("StartTime", Time.time);
    }
}
