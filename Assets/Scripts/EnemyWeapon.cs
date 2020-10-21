﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyWeapon : MonoBehaviour
{
    public EventHandler Prevented;
    public Transform weaponPosition;
    Rigidbody rig;
    private void Start()
    {
        transform.parent = null;
        rig=GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rig.MovePosition(weaponPosition.position);
        rig.MoveRotation(weaponPosition.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Prevented(this, EventArgs.Empty);

        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Prevented(this, EventArgs.Empty);

        }
    }
}
