using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public List<Unit> playerUnits;
    Unit activeUnit;
    bool unitSelected = false;
    Vector3 mouserPositon;
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Vector2 originalClickPos;
    Vector2 worldBottomLeft;
    Camera camera;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        worldBottomLeft = Vector2.zero - (Vector2.right * gridWorldSize.x / 2) - (Vector2.up * gridWorldSize.y / 2);
        CreateGrid();
        activeUnit = playerUnits[0];
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        /// change activeUnit using 'tab' key
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            int currentUnitIndex = playerUnits.IndexOf(activeUnit);
            /// if unit is last one in the list change to the first unit.
            if (currentUnitIndex == playerUnits.Count - 1)
                activeUnit = playerUnits[0];
            else
                activeUnit = playerUnits[currentUnitIndex + 1];
        }

        /// Select units only when user is not moving the camera
        if (Input.GetMouseButtonDown(0))
        {
            originalClickPos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(0) && originalClickPos != null)
        {
            if (Vector2.Distance(Input.mousePosition, originalClickPos) < 0.05)
            {
                Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
                RaycastHit2D hit = Physics2D.Raycast(rayPos, Vector2.zero, 0f);
                Debug.Log("Hit : " + hit);
                if (hit)
                {
                    Debug.Log("I hit : " + hit);
                    for (int i = 0; i < playerUnits.Count; i++)
                    {
                        if (playerUnits[i].transform == (hit.transform))
                        {
                            Debug.Log("Unit selected " + hit.transform.name);
                            activeUnit = playerUnits[i];
                            unitSelected = true;
                        }
                    }
                }
            }
        }

        /// Move units only when user is not moving the camera
        if (Input.GetMouseButtonUp(0) && originalClickPos != null)
        {
            if (Vector3.Distance(Input.mousePosition, originalClickPos) < 0.05 && !unitSelected)
            {
                mouserPositon = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                mouserPositon.z = -transform.position.z;
                mouserPositon = Camera.main.ScreenToWorldPoint(mouserPositon);
                activeUnit.RequestPath(mouserPositon);
            }
        }
        unitSelected = false;
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];


        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector2.right * (x * nodeDiameter + nodeRadius) + Vector2.up * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWorldSize.x, gridWorldSize.y, 0.1f));
        if (grid != null && displayGridGizmos)
        {
            foreach (Node n in grid)
            {
                Gizmos.color = (n.walkable) ? Color.white : Color.red;
                Gizmos.DrawWireCube(n.worldPosition, new Vector3(1,1,0.01f) * (nodeDiameter - .1f));
            }
        }
    }
}