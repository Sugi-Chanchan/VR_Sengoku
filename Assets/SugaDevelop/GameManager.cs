using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameManager : MonoBehaviour
{
    protected GameObject _player, _lefthand, _righthand;

    protected virtual void Start()
    {
        Instance = this;
        StartCoroutine(_Setup());
    }
    
    IEnumerator _Setup()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        _player = GameObject.FindGameObjectWithTag("Player");
        _lefthand = GameObject.FindGameObjectWithTag("LeftHand");
        _righthand = GameObject.FindGameObjectWithTag("RightHand");
        Setup();
    }
    protected virtual void Setup() { }
    

    public static GameManager Instance { get; protected set; }



    public virtual void GameOver() { }
    public virtual void Clear() { }
    public virtual void Pause() { }
    public virtual void RePlay() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public virtual void Damage() { }
}
