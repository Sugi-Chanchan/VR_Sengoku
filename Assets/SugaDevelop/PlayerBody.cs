using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class PlayerBody : StickColliderDynamic
{
    int hp = 10;
    bool hitted = false;
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
    }

    private async void WaitForAsynic(int seconds,Action action)
    {
        await Task.Delay(TimeSpan.FromSeconds(seconds));
        action();
    }
    
}
