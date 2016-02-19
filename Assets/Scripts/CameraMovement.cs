using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraMovement : MonoBehaviour
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
    public static bool cameraIsMoving = false;
    private bool cameraDisabled = false;
    private Camera cam;

    public bool CameraDisabled
    {
        get { return cameraDisabled; }
        set { cameraDisabled = value; }
    }

    void Awake()
    {
        gridWorldSize = GetComponent<Grid>().gridWorldSize;
        cam = Camera.main;
    }


    void Update()
    {
        if (!cameraDisabled)
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
            if (isPanning && !cameraIsMoving)
            {
                // camera movement speed adjustment according to current zoom level
                panSpeed = -3f*cam.orthographicSize;
                Vector3 pos = cam.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
                Vector3 move = new Vector3(pos.x*-panSpeed, pos.y*-panSpeed, 0);
                /// When panning, prevent camera from going too far from playing area
                Vector3 finalCameraPostion = transform.position - move;
                /// When it goes beyond right edge of the world
                if (finalCameraPostion.x > (gridWorldSize.x + transform.position.z*0.6)/2)
                {
                    finalCameraPostion.x = (float) (gridWorldSize.x + transform.position.z*0.6)/2;
                }
                /// When it goes beyond left edge of the world
                if (finalCameraPostion.x < (-gridWorldSize.x - transform.position.z*0.6)/2)
                {
                    finalCameraPostion.x = (float) (-gridWorldSize.x - transform.position.z*0.6)/2;
                }
                /// When it goes beyond upper edge of the world
                if (finalCameraPostion.y > (gridWorldSize.y + transform.position.z*0.6)/2)
                {
                    finalCameraPostion.y = (float) (gridWorldSize.y + transform.position.z*0.6)/2;
                }
                /// When it goes beyond lower edge of the world
                if (finalCameraPostion.y < (-gridWorldSize.y - transform.position.z*0.6)/2)
                {
                    finalCameraPostion.y = (float) (-gridWorldSize.y - transform.position.z*0.6)/2;
                }
                // prevent camera from keep moving unless there is further mouse movement
                transform.position = finalCameraPostion;
                mouseOrigin = Input.mousePosition;
            }

            // Move the camera linearly along Z axis
            if (isZooming)
            {
                float move = cam.ScreenToViewportPoint(Input.mousePosition - mouseOrigin).y;

                float zoom = move*zoomSpeed*8;

                /// When zooming in, prevent camera from getting too close to the ground(preventing it from go past the ground.)
                if (cam.orthographicSize - zoom < 3)
                {
                    cam.orthographicSize = 3;
                }
                /// prevent camera from zooming out too much
                else if (cam.orthographicSize - zoom > 12)
                {
                    cam.orthographicSize = 12;
                }
                else
                {
                    cam.orthographicSize = cam.orthographicSize - zoom;
                }
                // prevent camera from keep moving unless there is further mouse movement
                mouseOrigin = Input.mousePosition;
            }
        }
    }

    public void moveTo(List<Node> path)
    {
        StartCoroutine(moveCoroutine(path));
    }

    IEnumerator moveCoroutine(List<Node> path)
    {
        cameraIsMoving = true;
        if (path != null && path.Count > 0)
        {
            Vector3 targetPos;
            transform.position = new Vector3(path[0].worldPosition.x, path[0].worldPosition.y, transform.position.z);
            foreach (Node n in path)
            {
                targetPos = n.worldPosition;
                targetPos.z = transform.position.z;
                panSpeed = Time.deltaTime * cam.orthographicSize * 0.12f *
                        Vector2.Distance(transform.position, targetPos) + 0.05f;
                while ((transform.position) != targetPos)
                {
                    // camera movement speed adjustment according to current zoom level
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, panSpeed);
                    yield return null;
                }
            }
            cameraIsMoving = false;
            CameraMovementManager.FinishedMoving();
        }
    }
}