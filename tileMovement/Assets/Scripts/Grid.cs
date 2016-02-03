using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public List<Unit> playerUnits;
    Unit activeUnit;
    Vector3 target;
	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public Vector3 gridWorldSize;
	public float nodeRadius;
	Node[,] grid;
	float nodeDiameter;
	int gridSizeX, gridSizeY;
    Vector3 originalClickPos;
    Vector3 worldBottomLeft;

    void Awake() {
		nodeDiameter = nodeRadius*2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x/nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.z/nodeDiameter);
        worldBottomLeft = Vector3.zero - (Vector3.right * gridWorldSize.x / 2) - (Vector3.forward * gridWorldSize.z / 2);
        CreateGrid();
	    activeUnit = playerUnits[0];
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
		if (Input.GetMouseButtonDown(0)) 
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray,out hit))
			{
				for (int i = 0; i < playerUnits.Count; i++) {
					if (playerUnits [i].transform == (hit.transform)) {
						Debug.Log ("Unit selected");
						activeUnit = playerUnits [i];
					}
				}
				Debug.Log ("Target name: " + hit.transform.name);
				//Destroy(GameObject.Find(hit.collider.gameObject.name));

			}
		}
        
        if (Input.GetMouseButtonDown(0))
        {
            originalClickPos = transform.position;
        }
        if (Input.GetMouseButtonUp(0) && originalClickPos != null)
        {
            if (transform.position == originalClickPos)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.point;
                }
                activeUnit.RequestPath(target);
            }
        }
    }

    public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid() {
		grid = new Node[gridSizeX,gridSizeY];
		

		for (int x = 0; x < gridSizeX; x ++) {
			for (int y = 0; y < gridSizeY; y ++) {
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint,nodeRadius,unwalkableMask));
				grid[x,y] = new Node(walkable,worldPoint, x,y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbours.Add(grid[checkX,checkY]);
				}
			}
		}

		return neighbours;
	}
	

	public Node NodeFromWorldPoint(Vector3 worldPosition) {
        Vector3 localPosition = worldPosition - worldBottomLeft;

        float percentX = (localPosition.x) / gridWorldSize.x;
        float percentY = (localPosition.z) / gridWorldSize.z;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

        return grid[x, y];
    }
	
	void OnDrawGizmos() {
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(gridWorldSize.x, 1, gridWorldSize.z));
		if (grid != null && displayGridGizmos) {
			foreach (Node n in grid) {
				Gizmos.color = (n.walkable)?Color.white:Color.red;
				Gizmos.DrawWireCube(n.worldPosition, Vector3.one * (nodeDiameter-.1f));
			}
		}
	}
}