using UnityEngine;
using System.Collections;

public class camera : MonoBehaviour
{
    //
    // VARIABLES
    //

    public float turnSpeed = 4.0f;      // Speed of camera turning when mouse moves in along an axis
    public float panSpeed;       /// Speed of the camera when being panned ! adjusted with camera y position value !
    public float zoomSpeed = 4.0f;      // Speed of the camera going back and forth

    private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
    private bool isPanning;     // Is the camera being panned?
    private bool isZooming;     // Is the camera zooming?
    Vector3 gridWorldSize;
    //
    // UPDATE
    //

    void Awake()
    {
        gridWorldSize = GetComponent<Grid>().gridWorldSize;
    }

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
            panSpeed = 1.75f * transform.position.z;
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
            Vector3 move = new Vector3(pos.x * -panSpeed, pos.y * -panSpeed, 0);
            /// When panning, prevent camera from going too far from playing area
            Vector3 finalCameraPostion = transform.position - move;
            /// When it goes beyond right edge of the world
            if (finalCameraPostion.x > (gridWorldSize.x + transform.position.z * 0.6) / 2 )
            {
                finalCameraPostion.x = (float) (gridWorldSize.x + transform.position.z * 0.6) / 2;
            }
            /// When it goes beyond left edge of the world
            if (finalCameraPostion.x < (-gridWorldSize.x - transform.position.z * 0.6) / 2 )
            {
                finalCameraPostion.x = (float) (-gridWorldSize.x - transform.position.z * 0.6) / 2 ;
            }
            /// When it goes beyond upper edge of the world
            if (finalCameraPostion.y > (gridWorldSize.y + transform.position.z * 0.6) / 2)
            {
                finalCameraPostion.y = (float) (gridWorldSize.y + transform.position.z * 0.6) / 2 ;
            }
            /// When it goes beyond lower edge of the world
            if (finalCameraPostion.y < (-gridWorldSize.y - transform.position.z * 0.6) / 2)
            {
                finalCameraPostion.y = (float) (-gridWorldSize.y - transform.position.z * 0.6) / 2 ;
            }
            // prevent camera from keep moving unless there is further mouse movement
            transform.position = finalCameraPostion;
            mouseOrigin = Input.mousePosition;
        }

        // Move the camera linearly along Z axis
        if (isZooming)
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

            Vector3 move = pos.y * zoomSpeed * 8 * transform.forward;

            /// When zooming in, prevent camera from getting too close to the ground(preventing it from go past the ground.)
            if (transform.position.z + move.z > -5)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -5);
            }
            /// prevent camera from zooming out too much
            else if (transform.position.z + move.z < -20)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, -20);
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