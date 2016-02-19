using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using Debug = UnityEngine.Debug;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    Grid grid;

    void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
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
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);
        if (startNode.walkable && targetNode.walkable && !targetNode.inMidOfFloor)
        {
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
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
                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if ((!neighbour.walkable && !neighbour.jumpThroughable) || closedSet.Contains(neighbour))
                        continue;
                    if (!neighbour.walkable && (currentNode.gridY < neighbour.gridY))
                        continue;

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
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
        if (pathSuccess)
        {
            path = RetracePath(startNode, targetNode);
        }
        if (targetNode.FCost > actionPoint)
            StartFindPath(startPos, targetNode.parent.worldPosition, actionPoint);
        else
        {
            requestManager.FinishedProcessingPath(path, pathSuccess, targetNode.FCost);
            grid.ResetFcosts();
        }
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    public int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
        return 10 * (dstX + dstY);
    }
}
