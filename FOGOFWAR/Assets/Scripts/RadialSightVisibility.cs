using UnityEngine;
using System.Collections;

public class FogOfWarVisibility : MonoBehaviour
{
    private bool founded = false;

    // Use this for initialization
    void Start()
    {

    }

    void Update()
    {
        if (founded)
        {
            GetComponent<Renderer>().enabled = true;
        }
        else
        {
            GetComponent<Renderer>().enabled = false;
        }

        founded = false;
    }

    void Observed()
    {
        Debug.Log("Founded", gameObject);
        founded = true;
    }
}