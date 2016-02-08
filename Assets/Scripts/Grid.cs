using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    Vector2 target;
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public LayerMask walkableMask;
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

    void Update()
    {
        
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


        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                bool walkable = (Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.70f, walkableMask)) || (Physics.CheckSphere(worldPoint, nodeRadius, walkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }
    //gets neighbouring nodes of the given node
    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
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

    public void resetFcosts()
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
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                if(n.walkable)
                    Gizmos.DrawWireCube(n.worldPosition, new Vector3(1, 1, 0f) * (nodeDiameter - .1f));
            }
        }
    }
}