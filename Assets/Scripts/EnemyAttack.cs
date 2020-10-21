﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{

    [SerializeField] int damage;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            print("Player Hit");
            other.GetComponent<HP>().Damage(damage);
        }
    }
}
