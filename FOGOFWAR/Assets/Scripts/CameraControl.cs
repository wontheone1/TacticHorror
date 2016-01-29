using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    public float moveSensitivityX = 1.0f;
    public float moveSensitivityY = 1.0f;
    public bool updateZoomSensitivity = true;
    public float orthoZoomSpeed = 0.05f;
    public float minZoom = 1.0f;
    public float maxZoom = 20.0f;
    public bool invertMoveX = false;
    public bool invertMoveY = false;

    public float inertiaDuration = 1.0f;

    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;

    }

    void Update()
    {
        if (updateZoomSensitivity)
        {
            moveSensitivityX = _camera.orthographicSize / 5.0f;
            moveSensitivityY = _camera.orthographicSize / 5.0f;
        }

        Touch[] touches = Input.touches;

        if (touches.Length > 0)
        {
            //Single touch (move)
            if (touches.Length == 1)
            {
                if (touches[0].phase == TouchPhase.Began)
                {
                    Vector2 delta = touches[0].deltaPosition;

                    float positionX = delta.x * moveSensitivityX * Time.deltaTime;
                    positionX = invertMoveX ? positionX : positionX * -1;

                    float positionY = delta.y * moveSensitivityY * Time.deltaTime;
                    positionY = invertMoveY ? positionY : positionY * -1;

                    _camera.transform.position += new Vector3(positionX, positionY, 0);

                }
            }


            //Double touch (zoom)
            if (touches.Length == 2)
            {

                Touch touchOne = touches[0];
                Touch touchTwo = touches[1];

                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;
                Vector2 touchTwoPrevPos = touchTwo.position - touchTwo.deltaPosition;

                float prevTouchDeltaMag = (touchOnePrevPos - touchTwoPrevPos).magnitude;
                float touchDeltaMag = (touchOne.position - touchTwo.position).magnitude;

                float deltaMagDiff = prevTouchDeltaMag - touchDeltaMag;


                _camera.orthographicSize += deltaMagDiff * orthoZoomSpeed;
                _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, minZoom, maxZoom) - 0.001f;


            }
        }
    }
}

    