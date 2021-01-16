using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KibaManager : GameManager
{
    public GameObject blackOutPrefab;
    CameraFilter cameraFilter;
    [SerializeField] Shader filter;

    void Start()
    {

    }

    protected override void Setup()
    {
        SetBodyCollider();
        SetGameOverCamera();
    }


    void SetBodyCollider()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerBody body = player.AddComponent<PlayerBody>();
        Transform start = new GameObject("Start").transform;
        Transform end = new GameObject("End").transform;
        start.parent = player.transform;
        end.parent = player.transform;
        start.localPosition = Vector3.zero;
        end.localPosition = -Vector3.up;
        body.SetCollider(start, end);
    }

    void SetGameOverCamera()
    {
        switch (ButtonManager.Device)
        {
            case Device.Oculus:
                cameraFilter = GameObject.Find("CenterEyeAnchor").AddComponent<CameraFilter>();
                break;
            case Device.SteamVR:
                cameraFilter = GameObject.Find("Camera (eye)").AddComponent<CameraFilter>();
                break;
            default: Debug.LogError("error has happend!"); break;

        }

        cameraFilter.filter = new Material(this.filter);
    }

    public override void GameOver()
    {
        cameraFilter.GameOver();
    }

}
