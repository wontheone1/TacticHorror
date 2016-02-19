using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CameraMovementManager : MonoBehaviour {

    static Queue<List<Node>> camMovementQueue = new Queue<List<Node>>();
    static List<Node> currentCamMovePath;
    // static CameraMovementManager instance;
    static CameraMovement camMove = GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>();
    static bool CameraMoving = false;

    public static void RequestCamMove(List<Node> path)
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
