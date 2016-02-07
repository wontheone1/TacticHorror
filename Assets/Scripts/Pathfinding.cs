using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using Debug = UnityEngine.Debug;

public class Pathfinding : MonoBehaviour {
	

	PathRequestManager requestManager;
	Grid grid;

	void Awake() {
		requestManager = GetComponent<PathRequestManager>();
		grid = GetComponent<Grid>();
	}
	

	public void StartFindPath(Vector2 startPos, Vector2 targetPos, int actionPoint) {
        StartCoroutine(FindPath(startPos,targetPos, actionPoint));
	}

	IEnumerator FindPath(Vector2 startPos, Vector2 targetPos, int actionPoint) {
		Stopwatch sw = new Stopwatch();
		sw.Start();
		Vector2[] waypoints = new Vector2[0];
		bool pathSuccess = false;
		Node startNode = grid.NodeFromWorldPoint(startPos);
		Node targetNode = grid.NodeFromWorldPoint(targetPos);
		if (startNode.walkable && targetNode.walkable) {
			Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
			HashSet<Node> closedSet = new HashSet<Node>();
			openSet.Add(startNode);
            Node currentNode;
            while (openSet.Count > 0) {
                currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);
				if (currentNode == targetNode) {
					sw.Stop();
					pathSuccess = true;
					break;
				}
				foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
					if (!neighbour.walkable || closedSet.Contains(neighbour)) {
						continue;
					}
					int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
					if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
						neighbour.gCost = newMovementCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetNode);
						neighbour.parent = currentNode;
						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
						else
						    openSet.UpdateItem(neighbour);
					}
				}
			}
		}
		yield return null;
		if (pathSuccess) {
			waypoints = RetracePath(startNode,targetNode);
		}
	    if (targetNode.FCost > actionPoint)
	        StartFindPath(startPos, targetNode.parent.worldPosition, actionPoint);
	    else
	    {
	        requestManager.FinishedProcessingPath(waypoints, pathSuccess, targetNode.FCost);
            grid.resetFcosts();
        }
	}

	Vector2[] RetracePath(Node startNode, Node endNode) {
		List<Node> path = new List<Node>();
		Node currentNode = endNode;
		while (currentNode != startNode) {
			path.Add(currentNode);
			currentNode = currentNode.parent;
		}
        path.Add(startNode);

        Vector2[] waypoints;
        /// simplify path only when path has more than one node
        if (path.Count > 1)
	    {
	        waypoints = SimplifyPath(path);
	        Array.Reverse(waypoints);
	    }
	    else
	    {
            waypoints = new Vector2[]{path[0].worldPosition};
        }
	    return waypoints;

	}

	Vector2[] SimplifyPath(List<Node> path) {
		HashSet<Vector2> waypoints = new HashSet<Vector2>();
		Vector2 directionOld = new Vector2(path[0].gridX - path[1].gridX, path[0].gridY - path[1].gridY);
        waypoints.Add(path[0].worldPosition); /// add first path in case path.Count is just 1
        for (int i = 1; i < path.Count; i ++) {
			Vector2 directionNew = new Vector2(path[i-1].gridX - path[i].gridX,path[i-1].gridY - path[i].gridY);
			if (directionNew != directionOld) {
                waypoints.Add(path[i - 1].worldPosition);
                waypoints.Add(path[i].worldPosition);
			}
			directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

	public int GetDistance(Node nodeA, Node nodeB) {
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (dstX > dstY)
			return 14*dstY + 10* (dstX-dstY);
		return 14*dstX + 10 * (dstY-dstX);
	}
}
