using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour {

	Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
	PathRequest currentPathRequest;

	static PathRequestManager instance;
	Pathfinding pathfinding;

	bool isProcessingPath;

	void Awake() {
		instance = this;
		pathfinding = GetComponent<Pathfinding>();
	}

	public static void RequestPath(Vector2 pathStart, Vector2 pathEnd, int actionPoint, Action<List<Node>, bool, int> callback) {
        PathRequest newRequest = new PathRequest(pathStart,pathEnd, actionPoint, callback);
		instance.pathRequestQueue.Enqueue(newRequest);
		instance.TryProcessNext();
	}

	void TryProcessNext() {
		if (!isProcessingPath && pathRequestQueue.Count > 0) {
			currentPathRequest = pathRequestQueue.Dequeue();
			isProcessingPath = true;
			pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd, currentPathRequest.actionPoint);
		}
	}

	public void FinishedProcessingPath(List<Node> path, bool success, int movementCost) {
		currentPathRequest.callback(path,success, movementCost);
		isProcessingPath = false;
		TryProcessNext();
	}

	struct PathRequest {
		public Vector2 pathStart;
		public Vector2 pathEnd;
	    public int actionPoint;
		public Action<List<Node>, bool, int> callback;

		public PathRequest(Vector2 _start, Vector2 _end, int _actionPoint, Action<List<Node>, bool, int> _callback) {
			pathStart = _start;
			pathEnd = _end;
		    actionPoint = _actionPoint;
			callback = _callback;
		}

	}
}
