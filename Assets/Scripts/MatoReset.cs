using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatoReset : MonoBehaviour
{
    GameObject []mato;
    public GameObject matoPrefab;

    private GameObject newMato;

    // Start is called before the first frame update
    void Start()
    {
        mato = GameObject.FindGameObjectsWithTag("Mato");

    }

    private void OntriggerEnter(Collider collider)
    {
        for(int i = 0; i < mato.Length; i++)
        {
            Vector3 matoPosition;

            matoPosition = mato[i].transform.position;

            Destroy(mato[i]);

            newMato = Instantiate(matoPrefab);
            newMato.transform.position = matoPosition;
            newMato.name = "mato";
        }
    }
}
