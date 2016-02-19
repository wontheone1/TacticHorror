using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    Vector2 target;
    public bool displayGridGizmos;
    public LayerMask UnwalkableMask;
    public LayerMask WalkableMask;
    public LayerMask JumpThrouable;
    public LayerMask BlockedMask;
    public LayerMask MidFloorLayerMask;
    public LayerMask LadderEndLayerMask;

    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Vector2 worldBottomLeft;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        worldBottomLeft = Vector2.zero - (Vector2.right * gridWorldSize.x / 2) - (Vector2.up * gridWorldSize.y / 2);
        CreateGrid();
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }
    //creates grid in start of the game
    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];

        bool walkable, throughable, blocked, coveredFromLeft, coveredFromRight, inMidFloor, atLadderEnd;
        RaycastHit2D hit;
        float detectionLength = 0.7f;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                walkable = (Physics2D.OverlapCircle(worldPoint, nodeRadius * detectionLength, WalkableMask)) || (Physics.CheckSphere(worldPoint, nodeRadius, WalkableMask));
                throughable = (Physics2D.OverlapCircle(worldPoint, nodeRadius * detectionLength, JumpThrouable));
                blocked = (Physics2D.OverlapCircle(worldPoint, nodeRadius * detectionLength, BlockedMask));
                coveredFromLeft = Physics2D.Raycast(worldPoint, Vector2.left, nodeDiameter, BlockedMask).collider != null;
                coveredFromRight = Physics2D.Raycast(worldPoint, Vector2.right, nodeDiameter, BlockedMask).collider != null;
                inMidFloor = (Physics2D.OverlapCircle(worldPoint, nodeRadius*detectionLength, MidFloorLayerMask));
                atLadderEnd = (Physics2D.OverlapCircle(worldPoint, nodeRadius * detectionLength, LadderEndLayerMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y, throughable, blocked, coveredFromLeft, coveredFromRight, false, inMidFloor, atLadderEnd);
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

                    checkY = node.gridY + y;
                    if (checkY >= 0 && checkY < gridSizeY)
                        //if (checkMovable(node, grid[node.gridX, checkY]) || checkMovable(grid[node.gridX, checkY], node))
                        neighbours.Add(grid[node.gridX, checkY]);
                }
            }
            else
            {
                checkX = node.gridX + x;
                if (checkX >= 0 && checkX < gridSizeX)
                    //if (checkMovable(node, grid[checkX, node.gridY]) || checkMovable(grid[checkX, node.gridY], node))
                    neighbours.Add(grid[checkX, node.gridY]);
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector2 worldPosition)
    {
        Vector2 localPosition = worldPosition - worldBottomLeft;
        float percentX = (localPosition.x) / gridWorldSize.x;
        float percentY = (localPosition.y) / gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);
        int x = (int)((gridSizeX) * percentX);
        int y = (int)((gridSizeY) * percentY);
        /// prevent out of array range error, this way is more accurate 
	    if (percentX == 1f)
            x = gridSizeX - 1;
        if (percentY == 1f)
            y = gridSizeY - 1;
        return grid[x, y];
    }

    public void ResetFcosts()
    {
        foreach (Node n in grid)
        {
            n.gCost = 0;
            n.hCost = 0;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWorldSize.x, gridWorldSize.y, 0.1f));
        if (grid != null && displayGridGizmos)
        {
            bool draw;
            foreach (Node n in grid)
            {
                draw = false;

                if (n.atLadderEnd)
                {
                    Gizmos.color = Color.grey;
                    draw = true;
                }
                else if (n.blocked || n.occupied)
                {
                    Gizmos.color = Color.red;
                    draw = true;
                    
                } else if (n.coveredFromLeft || n.coveredFromRight)
                {
                    Gizmos.color = Color.blue;
                    draw = true;
                }
                else if (n.jumpThroughable)
                {
                    Gizmos.color = Color.green;
                    draw = true;
                }
                else if (n.inMidOfFloor)
                {
                    Gizmos.color = Color.cyan;
                    draw = true;
                }
                else if (n.walkable)
                {
                    Gizmos.color = Color.white;
                    draw = true;
                }
                if (draw)
                    Gizmos.DrawWireCube(n.worldPosition, new Vector3(1, 1, 0f) * (nodeDiameter - .1f));
            }
        }
    }
}