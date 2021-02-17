using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoonVanish : MonoBehaviour
{

    [SerializeField] int time =3;
    // Start is called before the first frame update
    void Start()
    {
        Invoke("Destroy", time);
    }

    void Destroy()
    {
        Destroy(this.gameObject);
    }
    
}
