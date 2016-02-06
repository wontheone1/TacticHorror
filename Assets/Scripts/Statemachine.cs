using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statemachine : MonoBehaviour {

    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;
    Unit activeUnit;
    List<Unit> activeUnits;
    bool disableRayCast = false;
    Vector3 mousePosition;
    bool unitSelected = false;
    Vector3 originalClickPos;

    // instantiating enum state machine for easy and understandable usage
    public enum State
    {
        player,
        enemy,
        win,
        lose
    };
    State curState = State.player;
    public State CurState
    {
        get { return curState; }
    }

    //this function is called when end turn -button is pressed, this function disables raycasting also
    public void endTurn()
    {
        disableRayCast = true;
        //if current state is player, switch to enemy state
        if (curState == State.player)
        {
            //switch current state to enemy
            curState = State.enemy;
        }

        //works same way as switching from player to enemy
        else if (curState == State.enemy)
        {
            curState = State.player;
        }
        implementCurrentState();
    }

    public void implementCurrentState()
    {
        switch (curState)
        {
            case State.player:
                //players turn
                //change active unit to player Unit
                activeUnits = playerUnits;
                activeUnit = playerUnits[0];
                for (int i = 0; i < enemyUnits.Count; i++)
                {
                    enemyUnits[i].deletePath();
                }
                break;

            case State.enemy:
                //enemys turn
                //change active unit to enemyunit
                activeUnits = enemyUnits;
                activeUnit = enemyUnits[0];
                //delete paths from every player unit
                for (int i = 0; i < playerUnits.Count; i++)
                {
                    playerUnits[i].deletePath();
                }
                break;

            case Statemachine.State.lose:
                //game is lost
                break;

            case Statemachine.State.win:
                //game is won, lol
                break;
        }
    }
    //// Use this for initialization
    void Awake ()
    {
        implementCurrentState();
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
            int currentUnitIndex = activeUnits.IndexOf(activeUnit);
            /// if unit is last one in the list change to the first unit.
            if (currentUnitIndex == activeUnits.Count - 1)
                activeUnit = activeUnits[0];
            else
                activeUnit = activeUnits[currentUnitIndex + 1];
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
                Vector3 mousePosition = Input.mousePosition;
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

                if (hit.collider != null)
                {
                    Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
                }


                Vector2 v = Camera.main.ScreenToWorldPoint(mousePosition);

                Collider2D[] col = Physics2D.OverlapPointAll(v);

                if (col.Length > 0)
                {
                    Debug.Log("collider is not 0");
                    foreach (Collider2D c in col)
                    {
                        Debug.Log("c:" + c);
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

    //function which move-button calls, this function disables raycasting and moves active unit
    public void moveUnit()
    {
        disableRayCast = true;
        activeUnit.startMoving();
    }
}
