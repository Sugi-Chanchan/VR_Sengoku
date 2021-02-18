using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Level
{
    Easy,
    Normal,
    Hard
}

public class KibaManager : GameManager
{

    enum State
    {
        Preparing,
        Playing,
        End
    }
    State state;

    
    public GameObject[] cavalries ;
    PlayerBody playerBody;
    [SerializeField] GameObject playerBodyObject;
    public GameObject blackOutPrefab;
    CameraFilter cameraFilter;
    [SerializeField] Shader filter;
    GameObject player;

    public AudioClip playBGM, clearBGM, gameOverBGM;
    AudioSource audioSource;

    public GameObject tatamiesPrefab;
    GameObject tatamies;

    protected override void Start()
    {
        state = State.Preparing;
        base.Start();
    }

    protected override void Setup()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        SetBodyCollider();
        SetAudioListener();
        SetGameOverCamera();
        audioSource = GetComponent<AudioSource>();

        tatamies = Instantiate(tatamiesPrefab);
    }

    private void Update()
    {
        ClearCheck();
    }

    void SetBodyCollider()
    {
        var gameobject = Instantiate(playerBodyObject);
        gameobject.transform.parent = player.transform;
        gameobject.transform.localPosition = Vector3.zero;
        playerBody= gameobject.GetComponent<PlayerBody>();

    }

    void SetAudioListener()
    {
        player.AddComponent<AudioListener>();
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
        audioSource.clip = gameOverBGM;
        audioSource.Play();
        DestroyTatami();

        GameObject[] tatamiiii = GameObject.FindGameObjectsWithTag("Tatami");
        for(int i = 0; i < tatamiiii.Length; i++)
        {
            Destroy(tatamiiii[i]);
        }

        tatamies = Instantiate(tatamiesPrefab);

        state = State.End;
    }

    public override void Clear()
    {
        audioSource.clip = clearBGM;
        audioSource.Play();
        DestroyTatami();
        tatamies = Instantiate(tatamiesPrefab);
        state = State.End;
    }


    public void GameStart(Level level,GameObject cuttedTatami)
    {
        if (state == State.Playing) { return; }
        DestroyEnemy();

        cameraFilter.GameStart();
        int hp = 1;
        switch (level)
        {
            case Level.Easy: PutCavarly(1,1,3,3,4); break;
            case Level.Normal: PutCavarly(3, 1, 3, 3, 4); break;
            case Level.Hard: PutCavarly(3, 1, 3, 3, 4); break;
            default: Debug.LogError("level is wrong"); break;
        }

        playerBody.GamePlay(hp);
        cuttedTatami.transform.parent.parent.parent = null;
        Destroy(tatamies);

        audioSource.clip = playBGM;
        audioSource.Play();

        clear = clearOnce = false;

        state = State.Playing;
    }


    void PutCavarly(int number,int speed_min,int speed_max,float length_min,float length_max)
    {
        for(int i = 0; i < number; i++)
        {
            foreach(GameObject cavalry in cavalries)
            {
                float r = Random.Range(10, 100);
                float theta = Random.Range(0, 2 * Mathf.PI);
                var x = r * Mathf.Sin(theta);
                var z = r * Mathf.Cos(theta);

                Vector3 pos = new Vector3(x, 1, z);

                var gameobject = Instantiate(cavalry);
                gameobject.transform.position = pos;
                Cavalry cavalryClass = gameobject.GetComponent<Cavalry>();
                cavalryClass.speed = Random.Range(speed_min, speed_max);
                var temp = cavalryClass.halberd.transform.localScale;
                temp.y = Random.Range(length_min, length_max);
                cavalryClass.halberd.transform.localScale = temp;
            }
        }
    }
 
    void DestroyEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for(int i = 0; i < enemies.Length; i++)
        {
            Destroy(enemies[i]);
        }
    }

    void DestroyTatami()
    {
        GameObject[] tatami = GameObject.FindGameObjectsWithTag("Tatami");
        for (int i = 0; i < tatami.Length; i++)
        {
            Destroy(tatami[i]);
        }
    }

    public bool clear,clearOnce;
    void ClearCheck()
    {
        if (state !=State.Playing) return;
        if (clear&&!clearOnce)
        {
            Clear();
            clearOnce = true;
        }

        clear = true;
    }

    private float headHeight;
    public float GetHeadHeight()
    {
        return headHeight;
    }

    public void SetHeadHeight(float height)
    {
        headHeight = height;
    }

}
