using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
public class CameraMovement : MonoBehaviour
{
    //
    // VARIABLES
    //
    public float TurnSpeed = 4.0f;      // Speed of camera turning when mouse moves in along an axis
    public float PanSpeed;       /// Speed of the camera when being panned ! adjusted with camera y position value !
    public float ZoomSpeed = 4.0f;      // Speed of the camera going back and forth
    private Vector3 _mouseOrigin;    // Position of cursor when mouse dragging starts
    private bool _isPanning;     // Is the camera being panned?
    private bool _isZooming;     // Is the camera zooming?
    private Vector3 _gridWorldSize;
    public static bool CameraIsMoving;
    private bool _cameraDisabled;
    private Camera _cam;

    public bool CameraDisabled
    {
        get { return _cameraDisabled; }
        set { _cameraDisabled = value; }
    }

    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        _gridWorldSize = GetComponent<Grid>().GridWorldSize;
        _cam = Camera.main;
    }


    // ReSharper disable once UnusedMember.Local
    void Update()
    {
        if (!_cameraDisabled)
        {
            // Get the left mouse button
            if (Input.GetMouseButtonDown(0))
            {
                // Get mouse origin
                _mouseOrigin = Input.mousePosition;
                _isPanning = true;
            }

            // Get the middle mouse button
            if (Input.GetMouseButtonDown(2))
            {
                // Get mouse origin
                _mouseOrigin = Input.mousePosition;
                _isZooming = true;
            }

            // Disable movements on button release
            if (!Input.GetMouseButton(0)) _isPanning = false;
            if (!Input.GetMouseButton(2)) _isZooming = false;


            // Move the camera on it's XY plane
            if (_isPanning && !CameraIsMoving)
            {
                // camera movement speed adjustment according to current zoom level
                PanSpeed = -3f*_cam.orthographicSize;
                Vector3 pos = _cam.ScreenToViewportPoint(Input.mousePosition - _mouseOrigin);
                Vector3 move = new Vector3(pos.x*-PanSpeed, pos.y*-PanSpeed, 0);
                // When panning, prevent camera from going too far from playing area
                Vector3 finalCameraPostion = transform.position - move;
                // When it goes beyond right edge of the world
                if (finalCameraPostion.x > (_gridWorldSize.x + transform.position.z*0.6)/2)
                {
                    finalCameraPostion.x = (float) (_gridWorldSize.x + transform.position.z*0.6)/2;
                }
                // When it goes beyond left edge of the world
                if (finalCameraPostion.x < (-_gridWorldSize.x - transform.position.z*0.6)/2)
                {
                    finalCameraPostion.x = (float) (-_gridWorldSize.x - transform.position.z*0.6)/2;
                }
                // When it goes beyond upper edge of the world
                if (finalCameraPostion.y > (_gridWorldSize.y + transform.position.z*0.6)/2)
                {
                    finalCameraPostion.y = (float) (_gridWorldSize.y + transform.position.z*0.6)/2;
                }
                // When it goes beyond lower edge of the world
                if (finalCameraPostion.y < (-_gridWorldSize.y - transform.position.z*0.6)/2)
                {
                    finalCameraPostion.y = (float) (-_gridWorldSize.y - transform.position.z*0.6)/2;
                }
                // prevent camera from keep moving unless there is further mouse movement
                transform.position = finalCameraPostion;
                _mouseOrigin = Input.mousePosition;
            }

            // Move the camera linearly along Z axis
            if (_isZooming)
            {
                float move = _cam.ScreenToViewportPoint(Input.mousePosition - _mouseOrigin).y;

                float zoom = move*ZoomSpeed*8;

                // When zooming in, prevent camera from getting too close to the ground(preventing it from go past the ground.)
                if (_cam.orthographicSize - zoom < 3)
                {
                    _cam.orthographicSize = 3;
                }
                // prevent camera from zooming out too much
                else if (_cam.orthographicSize - zoom > 12)
                {
                    _cam.orthographicSize = 12;
                }
                else
                {
                    _cam.orthographicSize = _cam.orthographicSize - zoom;
                }
                // prevent camera from keep moving unless there is further mouse movement
                _mouseOrigin = Input.mousePosition;
            }
        }
    }

    public void MoveTo(List<Node> path)
    {
        StartCoroutine(MoveCoroutine(path));
    }

    IEnumerator MoveCoroutine(List<Node> path)
    {
        CameraIsMoving = true;
        if (path != null && path.Count > 0)
        {
            Vector3 targetPos;
            transform.position = new Vector3(path[0].WorldPosition.x, path[0].WorldPosition.y, transform.position.z);
            foreach (Node n in path)
            {
                targetPos = n.WorldPosition;
                targetPos.z = transform.position.z;
                PanSpeed = Time.deltaTime * _cam.orthographicSize * 0.12f *
                        Vector2.Distance(transform.position, targetPos) + 0.05f;
                while ((transform.position) != targetPos)
                {
                    // camera movement speed adjustment according to current zoom level
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, PanSpeed);
                    yield return null;
                }
            }
            CameraIsMoving = false;
            CameraMovementManager.FinishedMoving();
        }
    }
}