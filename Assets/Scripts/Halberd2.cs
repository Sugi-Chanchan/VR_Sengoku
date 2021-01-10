using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Halberd2 : StickColliderDynamic
{

    private void Start()
    {
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        while (ButtonManager.Device == Device.Unknown) { yield return null; }
        while (!GameObject.FindGameObjectWithTag("WeaponPositionRight")) { yield return null; }

        GameObject weaponPosition = GameObject.FindGameObjectWithTag("WeaponPositionRight");
        transform.parent = weaponPosition.transform.parent;
        transform.position = weaponPosition.transform.position;
        transform.rotation = weaponPosition.transform.rotation;
    }

    public override void OnCollision(CollisionInfo collisionInfo)
    {

    }
}
