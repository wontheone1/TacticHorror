using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    private Statemachine statemachine;
    
    Unit activeUnit;
    List<Unit> activeUnits;
    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;

    Vector3 mousePosition;
    bool unitSelected = false;
    Vector3 originalClickPos;
    bool disableRayCast = false;

    // getters and setters
    public List<Unit> ActiveUnits
    {
        get { return activeUnits; }
        set { activeUnits = value; }
    }
    public Unit ActiveUnit
    {
        get { return activeUnit; }
        set { activeUnit = value; }
    }

    // Use this for initialization
    void Awake()
    {
        statemachine = GetComponent<Statemachine>();
    }

    public void endTurn()
    {
        disableRayCast = true;
        statemachine.endTurn();
    }

    // Update is called once per frame
    void Update()
    {
        activities();
    }

    //this is the old update function, now it takes list of player or enemies to work with, depending on the turn
    public void activities()
    {
        /// change activeUnit using 'tab' key
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            selectNextUnit();
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
                Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D[] col = Physics2D.OverlapPointAll(v);
                if (col.Length > 0)
                {
                    foreach (Collider2D c in col)
                    {
                        for (int i = 0; i < activeUnits.Count; i++)
                        {
                            if (activeUnits[i].transform == (c.transform))
                            {
                                Debug.Log("Unit selected " + c.transform.name);
                                activeUnit.deletePath();
                                activeUnit = activeUnits[i];
                                unitSelected = true;
                            }
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
                mousePosition = Input.mousePosition;
                mousePosition.z = -transform.position.z;
                mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
                activeUnit.RequestPath(mousePosition);
            }
        }
        unitSelected = false;
        //set raycast usable again
        disableRayCast = false;
    } // end of activities ( update function, which is ran every frame )

    /// <summary>
    /// select next unit in the array. 
    /// Returns true when there is an available unit, false when there is no available unit.
    /// </summary>
    public bool selectNextUnit()
    {
        int currentUnitIndex = activeUnits.IndexOf(activeUnit);
        /// if unit is last one in the list change to the first unit.
        if (currentUnitIndex == activeUnits.Count - 1)
        {
            if (activeUnits[0].isMovementPossibe())
            {
                activeUnit = activeUnits[0];
                return true;
            }
            else
                currentUnitIndex = 1;
        }
        for (int i = (currentUnitIndex + 1); i < activeUnits.Count; i++)
        {
            if (activeUnits[i].isMovementPossibe())
            {
                activeUnit = activeUnits[i];
                return true;
            }
        }
        return false;
    }

    //function which move-button calls, this function disables raycasting and moves active unit
    public void moveUnit()
    {
        disableRayCast = true;
        activeUnit.startMoving();
        // select next unit automatically, if no unit is available, end turn;
        if (!selectNextUnit())
            endTurn();
    }
}
