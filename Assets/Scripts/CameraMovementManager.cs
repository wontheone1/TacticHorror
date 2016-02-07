using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CameraMovementManager : MonoBehaviour {

    Queue<Vector2[]> camMovementQueue = new Queue<Vector2[]>();
    Vector2[] currentCamMovePath;
    static CameraMovementManager instance;
    public CameraMovement camMove;
    bool CameraMoving = false;

    void Awake()
    {
        instance = this;
    }

    public static void RequestCamMove(Vector2[] path)
    {
        Debug.Log("requested cam move");
        if (path != null)
        {
            instance.camMovementQueue.Enqueue(path);
            instance.moveNext();
        }
    }

    void moveNext()
    {
        if (!instance.CameraMoving && instance.camMovementQueue.Count > 0)
        {
            instance.currentCamMovePath = instance.camMovementQueue.Dequeue();
            instance.CameraMoving = true;
            instance.camMove.moveTo(instance.currentCamMovePath);
        }
    }

    public static void FinishedMoving()
    {
        instance.CameraMoving = false;
        instance.moveNext();
    }

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
