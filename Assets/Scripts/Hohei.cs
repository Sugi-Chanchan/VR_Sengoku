using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hohei : Enemy
{
    Transform player;
    [SerializeField] float speed;
    [SerializeField] Vector3 vel;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        vel = GetComponent<Rigidbody>().velocity;

        var pos = transform.position;
        pos.y = 0;
        var pPos = player.position;
        pPos.y = 0;
        transform.rotation = Quaternion.LookRotation(pPos-pos);
        transform.Translate((pPos - pos).normalized * speed * Time.deltaTime,Space.World);
    }
}
