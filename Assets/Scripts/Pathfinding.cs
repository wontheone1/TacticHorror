using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    private PathRequestManager _requestManager;
    private Grid _grid;

    // ReSharper disable once UnusedMember.Local
    private void Awake()
    {
        _requestManager = GetComponent<PathRequestManager>();
        _grid = GetComponent<Grid>();
    }


    public void StartFindPath(Vector2 startPos, Vector2 targetPos, int actionPoint)
    {
        StartCoroutine(FindPath(startPos, targetPos, actionPoint));
    }

    IEnumerator FindPath(Vector2 startPos, Vector2 targetPos, int actionPoint)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        List<Node> path = new List<Node>();
        bool pathSuccess = false;
        Node startNode = _grid.NodeFromWorldPoint(startPos);
        Node targetNode = _grid.NodeFromWorldPoint(targetPos);
        if (startNode.Walkable && targetNode.Walkable && !targetNode.InMidOfFloor)
        {
            Heap<Node> openSet = new Heap<Node>(_grid.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();
            openSet.Add(startNode);
            Node currentNode;
            while (openSet.Count > 0)
            {
                currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);
                if (currentNode == targetNode)
                {
                    sw.Stop();
                    pathSuccess = true;
                    break;
                }
                foreach (Node neighbour in _grid.GetNeighbours(currentNode))
                {
                    if ((!neighbour.Walkable && !neighbour.JumpThroughable) || closedSet.Contains(neighbour))
                        continue;
                    if (!neighbour.Walkable && (currentNode.GridY < neighbour.GridY))
                        continue;
                    if (neighbour.JumpThroughable && currentNode.InMidOfFloor)
                        continue;

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;
                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            path = RetracePath(startNode, targetNode);
        }
        if (targetNode.FCost > actionPoint)
            StartFindPath(startPos, targetNode.Parent.WorldPosition, actionPoint);
        else
        {
            _requestManager.FinishedProcessingPath(path, pathSuccess, targetNode.FCost);
            _grid.ResetFcosts();
        }
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }
        path.Add(startNode);
        path.Reverse();
        if (path.Count > 1)
        {
            path = SimplifyPath(path);
        }
        return path;
    }

    List<Node> SimplifyPath(List<Node> path)
    {
        List<Node> waypoints = new List<Node>();
        Vector2 directionOld = new Vector2(path[0].GridX - path[1].GridX, path[0].GridY - path[1].GridY);
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);
            if ((directionNew != directionOld && !path[i].JumpThroughable)
                || (path[i-1].Walkable && path[i].JumpThroughable))
            {
                if (!waypoints.Contains(path[i - 1]))
                    waypoints.Add(path[i - 1]);
            } else if ((path[i - 1].JumpThroughable && path[i].Walkable))
            {
                path[i].ToJumpTo = true;
                if (!waypoints.Contains(path[i]))
                    waypoints.Add(path[i]);
            }
            directionOld = directionNew;
        }
        if (!waypoints.Contains(path[path.Count - 1]))
            waypoints.Add(path[path.Count - 1]);
        return waypoints;
    }

    public int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);
        return 10 * (dstX + dstY);
    }
}
