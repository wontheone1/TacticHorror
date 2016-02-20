using UnityEngine;
public class Settarget : MonoBehaviour
{
    private Vector3 _newPosition;

    // ReSharper disable once UnusedMember.Local
    private void Start () {
		_newPosition = transform.position;
	}

    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once ArrangeTypeMemberModifiers
    private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out hit))
			{
				_newPosition = hit.point;
				transform.position = _newPosition;
			}
		}
	}
}
