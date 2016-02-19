using UnityEngine;
using System.Collections;
using FMOD;

public class Node : IHeapItem<Node>
{


    public bool walkable, jumpThroughable, blocked,
        coveredFromRight, coveredFromLeft, occupied, inMidOfFloor, atLadderEnd;
    public Vector2 worldPosition;
    public int gridX;
    public int gridY;
    public int gCost;
    public int hCost;
    public Node parent;
    int heapIndex;

    public Node(bool _walkable, Vector2 _worldPos, int _gridX, int _gridY,
        bool throughable, bool _blocked, bool _coveredFromLeft,
        bool _coveredFromRight, bool _occupied, bool _inMidOfFloor, bool _atLadderEnd)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        jumpThroughable = throughable;
        blocked = _blocked;
        coveredFromLeft = _coveredFromLeft;
        coveredFromRight = _coveredFromRight;
        occupied = _occupied;
        inMidOfFloor = _inMidOfFloor;
        atLadderEnd = _atLadderEnd;
    }

    public int FCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }
}
