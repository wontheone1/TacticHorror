using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour {
    private readonly Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    private PathRequest _currentPathRequest;

    private static PathRequestManager _instance;
    private Pathfinding _pathfinding;

    private bool _isProcessingPath;

    // ReSharper disable once UnusedMember.Local
	void Awake() {
		_instance = this;
		_pathfinding = GetComponent<Pathfinding>();
	}

	public static void RequestPath(Vector2 pathStart, Vector2 pathEnd, int actionPoint, Action<List<Node>, bool, int> callback) {
        PathRequest newRequest = new PathRequest(pathStart,pathEnd, actionPoint, callback);
		_instance._pathRequestQueue.Enqueue(newRequest);
		_instance.TryProcessNext();
	}

    private void TryProcessNext() {
		if (!_isProcessingPath && _pathRequestQueue.Count > 0) {
			_currentPathRequest = _pathRequestQueue.Dequeue();
			_isProcessingPath = true;
			_pathfinding.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd, _currentPathRequest.ActionPoint);
		}
	}

	public void FinishedProcessingPath(List<Node> path, bool success, int movementCost) {
		_currentPathRequest.Callback(path,success, movementCost);
		_isProcessingPath = false;
		TryProcessNext();
	}

	struct PathRequest {
		public readonly Vector2 PathStart;
		public readonly Vector2 PathEnd;
	    public readonly int ActionPoint;
		public readonly Action<List<Node>, bool, int> Callback;

		public PathRequest(Vector2 start, Vector2 end, int actionPoint, Action<List<Node>, bool, int> callback) {
			PathStart = start;
			PathEnd = end;
		    ActionPoint = actionPoint;
			Callback = callback;
		}

	}
}
