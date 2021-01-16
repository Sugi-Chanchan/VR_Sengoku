using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PlayerBody : StickColliderDynamic
{
    int hp = 1;
    bool hitted = false;

    private void Start()
    {
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

        if (hp <= 0)
        {
            GameManager.Instance.GameOver();
        }
    }

    private async void WaitForAsynic(int seconds,Action action)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        action();
    }
    
}
