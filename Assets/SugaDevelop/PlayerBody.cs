using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PlayerBody : MultiSticksColliderDynamic
{
    int hp = 1;
    bool hitted = false;
    bool lose = false;

    private void Start()
    {
        colliderType = CollisionManager.ColliderType.CuttedOnly;

        SetCollider(new StickLocalPos[3] {
            new StickLocalPos(Vector3.down*0.5f,Vector3.up*0.1f),
            new StickLocalPos(Vector3.left*0.3f,Vector3.right*0.3f),
            new StickLocalPos(Vector3.back*0.3f,Vector3.forward*0.3f)});

        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        while (ButtonManager.Device == Device.Unknown)
        {
            yield return null;
        }

        KibaUI.ui.SetHp(hp);
    }


    public override void OnCollision(CollisionInfo collisionInfo)
    {
        if (collisionInfo.collisionObject.tag == "EnemyWeapon")
        {
            if (hitted) { return; }
            hitted = true;
            hp -= 1;
            KibaUI.ui.SetHp(hp);
            WaitForAsynic(2, () => hitted = false);
        }

        if (hp <= 0&&!lose)
        {
            GameManager.Instance.GameOver();
            lose = true;
        }
    }

    private async void WaitForAsynic(int seconds,Action action)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        action();
    }
    
    public void GamePlay(int hp)
    {
        this.hp = hp;
        KibaUI.ui.SetHp(hp);
        lose = false;
    }

}
