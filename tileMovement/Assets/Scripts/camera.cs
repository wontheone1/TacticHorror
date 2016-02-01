using UnityEngine;
using System.Collections;

public class camera : MonoBehaviour
{
    //
    // VARIABLES
    //

    public float turnSpeed = 4.0f;      // Speed of camera turning when mouse moves in along an axis
    public float panSpeed;       /// Speed of the camera when being panned ! adjusted with camera y position value !
    public float zoomSpeed = 1000.0f;      // Speed of the camera going back and forth

    private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
    private bool isPanning;     // Is the camera being panned?
    private bool isZooming;     // Is the camera zooming?

    //
    // UPDATE
    //

    void Update()
    {

        // Get the left mouse button
        if (Input.GetMouseButtonDown(0))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isPanning = true;
        }

        // Get the middle mouse button
        if (Input.GetMouseButtonDown(2))
        {
            // Get mouse origin
            mouseOrigin = Input.mousePosition;
            isZooming = true;
        }

        // Disable movements on button release
        if (!Input.GetMouseButton(0)) isPanning = false;
        if (!Input.GetMouseButton(2)) isZooming = false;


        // Move the camera on it's XY plane
        if (isPanning)
        {
            // camera movement speed adjustment according to current zoom level
            panSpeed = 1.75f * transform.position.y;
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
            Vector3 move = new Vector3(pos.x * -panSpeed, pos.y * -panSpeed, 0);
            transform.Translate(move, Space.Self);
            // prevent camera from keep moving unless there is further mouse movement
            mouseOrigin = Input.mousePosition;
        }

        // Move the camera linearly along Z axis
        if (isZooming)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            Vector3 move = pos.y * zoomSpeed * 8 * transform.forward;

            /// When zooming in, prevent camera from getting too close to the ground(preventing it from go past the ground.)
            if (transform.position.y + move.y < 10)
            {
                transform.position = new Vector3(transform.position.x + move.x, 10, transform.position.z + move.z);
            }
            else
            {
                transform.Translate(move, Space.World);
            }
            // prevent camera from keep moving unless there is further mouse movement
            mouseOrigin = Input.mousePosition;
        }
    }
}