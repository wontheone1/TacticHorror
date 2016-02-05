using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public List<Unit> playerUnits;
	public List<Unit> enemyUnits;
    Unit activeUnit;
    bool unitSelected = false;
    Vector2 target;
    Vector3 mousePosition;
    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;
    Vector2 originalClickPos;
    Vector2 worldBottomLeft;
	bool disableRayCast = false;

	// instantiating enum state machine for easy and understandable usage
	public enum State{player,enemy,win,lose}
	public State curState = State.player;

    void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        worldBottomLeft = Vector2.zero - (Vector2.right * gridWorldSize.x / 2) - (Vector2.up * gridWorldSize.y / 2);
        CreateGrid();
        activeUnit = playerUnits[0];
    }

    void Update()
	{
		switch (curState) {

		case State.player:
			//players turn
			//send playerunits as usable units to activities
			activities(playerUnits);
			break;

		case State.enemy:
			//enemys turn
			//send enemyunits as usable units to activities
			activities(enemyUnits);
			break;

		case State.lose:
			//game is lost


			break;

		case State.win:
			//game is won, lol


			break;
		}
	}

	//this is the old update function, now it takes list of player or enemies to work with, depending on the turn
	void activities(List<Unit> units){
        /// change activeUnit using 'tab' key
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            int currentUnitIndex = units.IndexOf(activeUnit);
            /// if unit is last one in the list change to the first unit.
            if (currentUnitIndex == units.Count - 1)
                activeUnit = units[0];
            else
                activeUnit = units[currentUnitIndex + 1];
        }

        /// Select units only when user is not moving the camera
		if (Input.GetMouseButtonDown(0) && !disableRayCast)
        {
            originalClickPos = Input.mousePosition;
        }
		if (Input.GetMouseButtonUp(0) && originalClickPos != null && !disableRayCast)
        {
            if (Vector3.Distance(Input.mousePosition, originalClickPos) < 0.05)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    for (int i = 0; i < units.Count; i++)
                    {
                        if (units[i].transform == (hit.transform))
                        {
							activeUnit.deletePath();
                            Debug.Log("Unit selected " + hit.transform.name);
                            activeUnit = units[i];
							target = activeUnit.transform.position;
                            unitSelected = true;
                        }
                    }
                }
            }
        }

        /// Move units only when user is not moving the camera
		// store path only if raycast is not disabled
		if (Input.GetMouseButtonUp(0) && originalClickPos != null && !disableRayCast)
        {
            if (Vector3.Distance(Input.mousePosition, originalClickPos) < 0.05 && !unitSelected)
            {
                mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                mousePosition.z = -transform.position.z;
                mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                activeUnit.RequestPath(mousePosition);
            }
        }
        unitSelected = false;
		//set raycast usable again
		disableRayCast = false;
	} // end of activities ( update function, which is ran every frame )

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
                bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
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
	//function which move-button calls, this function disables raycasting and moves active unit
	public void moveUnit(){
		disableRayCast = true;
		activeUnit.startMoving ();
	}

	//this function is called when end turn -button is pressed, this function disables raycasting also
	public void endTurn(){
		disableRayCast = true;

		//if current state is player, switch to enemy state
		if (curState == State.player) {
			//switch current state to enemy
			curState = State.enemy;
			//change active unit to enemyunit
			activeUnit = enemyUnits[0];
			//delete paths from every player unit
			for (int i = 0; i < playerUnits.Count; i++) {
				playerUnits [i].deletePath ();
			}
		}

		//works same way as switching from player to enemy
		else if (curState == State.enemy) {
			curState = State.player;
			activeUnit = playerUnits[0];
			for (int i = 0; i < enemyUnits.Count; i++) {
				enemyUnits [i].deletePath ();
			}
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
                Gizmos.DrawWireCube(n.worldPosition, new Vector3(1,1,0f) * (nodeDiameter - .1f));
            }
        }
    }
}