using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mato : MonoBehaviour
{
    AudioSource targetHitSound;

    // Start is called before the first frame update
    void Start()
    {
        targetHitSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.name == "Arrow" && this.transform.GetChild(0).tag == "Mato")
        {
            targetHitSound.Play();
            this.transform.GetComponent<Rigidbody>().useGravity = true;

            if (this.transform.parent.name == "Return")
            {
                print("dsaasdadads");
                Invoke("ChangeSceneOP", 0.5f);
            }
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        if(this.transform.GetChild(0).tag == "Respawn")
        {
            StartCoroutine("wait");

            targetHitSound.Play();

            collision.transform.parent = this.transform.GetChild(0); //この3行で当たった矢を静止
            collision.transform.GetComponent<Rigidbody>().useGravity = false;
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.transform.GetComponent<Rigidbody>().velocity = Vector3.zero;

            GameObject[] Matos = GameObject.FindGameObjectsWithTag("Mato");

            for (int i = 0; i < Matos.Length; i++)
            {
                Matos[i].transform.parent.GetComponent<Rigidbody>().useGravity = false;
                Matos[i].transform.parent.localPosition = new Vector3(0, 1.8993f, 0);
                Matos[i].transform.parent.localRotation = Quaternion.Euler(new Vector3(-45.0f, -90.0f, 90.0f));
            }
        }
    }

    IEnumerator wait()
    {
        yield return new WaitForSeconds(0.1f);
    }

    void ChangeSceneOP()
    {
        SceneManager.LoadScene("OP画面");
    }
}
