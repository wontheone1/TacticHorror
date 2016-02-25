using UnityEngine;
public class Node : IHeapItem<Node>
{


    public bool Walkable, JumpThroughable, Blocked,
        CoveredFromRight, CoveredFromLeft, Occupied, 
        InMidOfFloor, OnLadder, ToJumpTo;
    public Vector2 WorldPosition;
    public int GridX;
    public int GridY;
    public int GCost;
    public int HCost;
    public Node Parent;

    public Node(bool walkable, Vector2 worldPos, int gridX, int gridY,
        bool throughable, bool blocked, bool coveredFromLeft,
        bool coveredFromRight, bool occupied, bool inMidOfFloor, bool onLadder)
    {
        Walkable = walkable;
        WorldPosition = worldPos;
        GridX = gridX;
        GridY = gridY;
        JumpThroughable = throughable;
        Blocked = blocked;
        CoveredFromLeft = coveredFromLeft;
        CoveredFromRight = coveredFromRight;
        Occupied = occupied;
        InMidOfFloor = inMidOfFloor;
        OnLadder = onLadder;
    }

    public int FCost
    {
        get
        {
            return GCost + HCost;
        }
    }

    public int HeapIndex { get; set; }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);
        if (compare == 0)
        {
            compare = HCost.CompareTo(nodeToCompare.HCost);
        }
        return -compare;
    }
}
