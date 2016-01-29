using UnityEngine;
using System.Collections;

public class RadialSight : MonoBehaviour
{
    public float radius = 10.0f;
    public LayerMask layermask = -1;


	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        foreach(Collider col in Physics.OverlapSphere(transform.position,radius,layermask))
        {
            col.SendMessage("Founded", SendMessageOptions.DontRequireReceiver);

        }
	
	}
}
