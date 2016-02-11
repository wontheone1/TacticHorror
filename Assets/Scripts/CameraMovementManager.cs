using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CameraMovementManager : MonoBehaviour {

    static Queue<Vector2[]> camMovementQueue = new Queue<Vector2[]>();
    static Vector2[] currentCamMovePath;
    // static CameraMovementManager instance;
    static CameraMovement camMove = GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>();
    static bool CameraMoving = false;

    public static void RequestCamMove(Vector2[] path)
    {
        if (path != null)
        {
            camMovementQueue.Enqueue(path);
            moveNext();
        }
    }

    static void moveNext()
    {
        if (!CameraMoving && camMovementQueue.Count > 0)
        {
            currentCamMovePath = camMovementQueue.Dequeue();
            CameraMoving = true;
            camMove.moveTo(currentCamMovePath);
        }
    }

    public static void FinishedMoving()
    {
        CameraMoving = false;
        moveNext();
    }
}
