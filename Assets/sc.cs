using UnityEngine;
using System.Collections;

public class sc : MonoBehaviour {


    void Awake()
    {
        gameObject.GetComponent<MeshRenderer>().sortingLayerName = "UI Layer";
        gameObject.GetComponent<MeshRenderer>().sortingOrder = 0;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
