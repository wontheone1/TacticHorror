using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
public class CameraMovementManager : MonoBehaviour {
    private static readonly Queue<List<Node>> CamMovementQueue = new Queue<List<Node>>();
    private static List<Node> _currentCamMovePath;
    // static CameraMovementManager instance;
    private static readonly CameraMovement CamMove = GameObject.FindWithTag("MainCamera").GetComponent<CameraMovement>();
    private static bool _cameraMoving;

    public static void RequestCamMove(List<Node> path)
    {
        if (path != null)
        {
            CamMovementQueue.Enqueue(path);
            MoveNext();
        }
    }

    static void MoveNext()
    {
        if (!_cameraMoving && CamMovementQueue.Count > 0)
        {
            _currentCamMovePath = CamMovementQueue.Dequeue();
            _cameraMoving = true;
            CamMove.MoveTo(_currentCamMovePath);
        }
    }

    public static void FinishedMoving()
    {
        _cameraMoving = false;
        MoveNext();
    }
}
