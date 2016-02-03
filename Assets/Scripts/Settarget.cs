using UnityEngine;
using System.Collections;
public class Settarget : MonoBehaviour
{
	Vector3 newPosition;
	void Start () {
		newPosition = transform.position;
	}
	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				newPosition = hit.point;
				newPosition.z = -1;
				transform.position = newPosition;
			}
		}
	}
}
