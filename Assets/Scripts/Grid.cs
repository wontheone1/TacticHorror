using System;
using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public bool DisplayGridGizmos;
    public LayerMask UnwalkableMask;
    public LayerMask WalkableMask;
    public LayerMask JumpThrouable;
    public LayerMask BlockedMask;
    public LayerMask MidFloorLayerMask;
    public LayerMask LadderEndLayerMask;
    public LayerMask BlockViewLayerMask;
    public Vector2 GridWorldSize;
    public float NodeRadius;
    public Node[,] Nodes;
    float _nodeDiameter;
    int _gridSizeX, _gridSizeY;
    Vector2 _worldBottomLeft;
    FOVRecurse fov;

    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        _nodeDiameter = NodeRadius * 2;
        _gridSizeX = Mathf.RoundToInt(GridWorldSize.x / _nodeDiameter);
        _gridSizeY = Mathf.RoundToInt(GridWorldSize.y / _nodeDiameter);
        _worldBottomLeft = Vector2.zero - (Vector2.right * GridWorldSize.x / 2) - (Vector2.up * GridWorldSize.y / 2);
        CreateGrid();
        fov = GetComponent<FOVRecurse>();
        for (int y = 0; y < GridWorldSize.y; y++)
        {
            for (int x = 0; x < GridWorldSize.x; x++)
            {
                fov.Point_Set(x, y, Nodes[x,y].BlockView);
            }
        }
    }

    public int MaxSize
    {
        get
        {
            return _gridSizeX * _gridSizeY;
        }
    }
    //creates Grid in start of the game
    void CreateGrid()
    {
        Nodes = new Node[_gridSizeX, _gridSizeY];

        bool walkable, throughable, blocked, coveredFromLeft, coveredFromRight, inMidFloor, atLadderEnd, BlockView;
        float detectionLength = 0.7f;
        for (int x = 0; x < _gridSizeX; x++)
        {
            for (int y = 0; y < _gridSizeY; y++)
            {
                Vector3 worldPoint = _worldBottomLeft + Vector2.right * (x * _nodeDiameter + NodeRadius) + Vector2.up * (y * _nodeDiameter + NodeRadius);
                walkable = (Physics2D.OverlapCircle(worldPoint, NodeRadius * detectionLength, WalkableMask)) || (Physics.CheckSphere(worldPoint, NodeRadius, WalkableMask));
                throughable = (Physics2D.OverlapCircle(worldPoint, NodeRadius * detectionLength, JumpThrouable));
                if (throughable)
                    walkable = false;
                coveredFromLeft = Physics2D.Raycast(worldPoint, Vector2.left, _nodeDiameter, BlockedMask).collider != null;
                coveredFromRight = Physics2D.Raycast(worldPoint, Vector2.right, _nodeDiameter, BlockedMask).collider != null;
                blocked = Physics2D.OverlapCircle(worldPoint, NodeRadius * detectionLength, BlockedMask);
                if (blocked)
                    coveredFromRight = coveredFromLeft = false;
                inMidFloor = Physics2D.OverlapCircle(worldPoint, NodeRadius * detectionLength, MidFloorLayerMask);
                atLadderEnd = Physics2D.OverlapCircle(worldPoint, NodeRadius * detectionLength, LadderEndLayerMask);
                Collider2D viewCollider = Physics2D.OverlapCircle(worldPoint, NodeRadius * detectionLength, LadderEndLayerMask);
                BlockView = viewCollider.tag.Equals("blockView");
                Nodes[x, y] = new Node(walkable, worldPoint, x, y, throughable, blocked, coveredFromLeft, coveredFromRight, false, inMidFloor, atLadderEnd, BlockView);
            }
        }
    }


    //gets neighbouring nodes of the given node
    public List<Node> GetNeighbours(Node node)
    {
        int checkX, checkY;
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            if (x == 0)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (y == 0)
                        continue;

                    checkY = node.GridY + y;
                    if (checkY >= 0 && checkY < _gridSizeY)
                        //if (checkMovable(node, Grid[node.GridX, checkY]) || checkMovable(Grid[node.GridX, checkY], node))
                        neighbours.Add(Nodes[node.GridX, checkY]);
                }
            }
            else
            {
                checkX = node.GridX + x;
                if (checkX >= 0 && checkX < _gridSizeX)
                    //if (checkMovable(node, Grid[checkX, node.GridY]) || checkMovable(Grid[checkX, node.GridY], node))
                    neighbours.Add(Nodes[checkX, node.GridY]);
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        Vector2 localPosition = worldPosition - _worldBottomLeft;
        float percentX = (localPosition.x) / GridWorldSize.x;
        float percentY = (localPosition.y) / GridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = (int)((_gridSizeX) * percentX);
        int y = (int)((_gridSizeY) * percentY);
        // prevent out of array range error, this way is more accurate 
        if (Math.Abs(percentX - 1) < 0.001)
            x = _gridSizeX - 1;
        if (Math.Abs(percentY - 1) < 0.001)
            y = _gridSizeY - 1;
        return Nodes[x, y];
    }

    public void ResetFcosts()
    {
        foreach (Node n in Nodes)
        {
            n.GCost = 0;
            n.HCost = 0;
        }
    }

    // ReSharper disable once UnusedMember.Local
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(GridWorldSize.x, GridWorldSize.y, 0.1f));
        if (Nodes != null && DisplayGridGizmos)
        {
            bool draw;
            foreach (Node n in Nodes)
            {
                draw = false;

                if (n.InMidOfFloor)
                {
                    Gizmos.color = Color.cyan;
                    draw = true;
                }
                else if (n.CoveredFromLeft || n.CoveredFromRight)
                {
                    Gizmos.color = Color.blue;
                    draw = true;
                }
                else if (n.OnLadder)
                {
                    Gizmos.color = Color.grey;
                    draw = true;
                }
                //else if (n.Blocked || n.Occupied)
                //{
                //    Gizmos.color = Color.red;
                //    draw = true;
                //}
                else if (n.Walkable)
                {
                    Gizmos.color = Color.white;
                    draw = true;
                }
                else if (n.JumpThroughable)
                {
                    Gizmos.color = Color.green;
                    draw = true;
                }
                if (draw)
                    Gizmos.DrawWireCube(n.WorldPosition, new Vector3(1, 1, 0f) * (_nodeDiameter - .1f));
            }
        }
    }
}