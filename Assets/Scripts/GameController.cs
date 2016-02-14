﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private TextBoxManager textBoxManager;
    private Button endButton;
    private Grid grid;
    private Node clickedNode = null;
    private Statemachine statemachine;
    public Text debugText;
    Unit activeUnit;
    List<Unit> activeUnits;
    List<Unit> opponentUnits;
    public List<Unit> playerUnits;
    public List<Unit> enemyUnits;
    public static bool unitMoving = false;
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

    public List<Unit> OpponentUnits
    {
        get { return opponentUnits; }
        set { opponentUnits = value; }
    }

    public Unit ActiveUnit
    {
        get { return activeUnit; }
        set { activeUnit = value; }
    }

    public TextBoxManager TextBoxManager
    {
        get
        {
            return textBoxManager;
        }
    }

    // Use this for initialization
    void Awake()
    {
        try
        {
            debugText = GameObject.Find("Debug Text").GetComponent<Text>();
            Debug.Log(debugText);
        }
        catch (Exception e) { }
        statemachine = GetComponent<Statemachine>();
        grid = GetComponent<Grid>();
        endButton = GameObject.Find("EndTurn").GetComponent<Button>();
        endButton.onClick.AddListener(delegate {endTurn();});
        textBoxManager = GetComponent<TextBoxManager>();
    }

    public void endTurn()
    {
        disableRayCast = true;
        clearPaths();
        statemachine.endTurn();
    }

    public void clearPaths()
    {
        foreach(var unit in ActiveUnits)
        {
            unit.deletePath();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!unitMoving && !CameraMovement.cameraIsMoving && activeUnit != null)
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
                selectUnitByClick();
            }
        }

        /// Move units only when user is not moving the camera
        // store path only if raycast is not disabled
        if (Input.GetMouseButtonUp(0) && originalClickPos != null && !disableRayCast)
        {
            if (Vector3.Distance(Input.mousePosition, originalClickPos) < 0.05 && !unitSelected)
            {
                findPathForUnit();
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
        Vector2[] camMovePath = new Vector2[2];
        int currentUnitIndex = activeUnits.IndexOf(activeUnit);
        camMovePath[0] = Camera.main.gameObject.transform.position;
        /// if unit is last one in the list change to the first unit.
        if (currentUnitIndex == activeUnits.Count - 1)
        {
            if (activeUnits[0].isMovementPossible())
            {   
                activeUnit = activeUnits[0];
                camMovePath[1] = activeUnit.transform.position;
                CameraMovementManager.RequestCamMove(camMovePath);
                return true;
            }
            else
                currentUnitIndex = 1;
        }
        for (int i = (currentUnitIndex + 1); i < activeUnits.Count; i++)
        {
            if (activeUnits[i].isMovementPossible())
            {
                activeUnit = activeUnits[i];
                camMovePath[1] = activeUnit.transform.position;
                CameraMovementManager.RequestCamMove(camMovePath);
                return true;
            }
        }
        return false;
    }

    private void selectUnitByClick()
    {
        Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] col = Physics2D.OverlapPointAll(v);
        if (col.Length > 0)
        {
            foreach (Collider2D c in col)
            {
                /// if an opponent unit is clicked, select it as target
                /// if there is a target, attack it
                for (int i = 0; i < opponentUnits.Count; i++)
                {
                    if (opponentUnits[i].transform == c.transform)
                    {
                        unitSelected = true;
                        activeUnit.deletePath();
                        if (activeUnit.TargetUnit == c.gameObject.GetComponent<Unit>())
                        {
                            activeUnit.attackTarget();
                            debugText.text = "Attacked: " + activeUnit.TargetUnit.name;
                            if (!selectNextUnit())
                                endTurn();
                            return;
                        }
                        activeUnit.setAttackTarget(opponentUnits[i]);
                        if (activeUnit.TargetUnit != null)
                        {
                            debugText.text = "Target: " + activeUnit.TargetUnit.name;
                            return;
                        }
                    }
                }

                /// if no opponent was clicked, unset target
                activeUnit.unsetAttackTarget();

                Vector2[] camMovePath = new Vector2[2];
                camMovePath[0] = Camera.main.gameObject.transform.position;
                /// select clicked unit
                for (int i = 0; i < activeUnits.Count; i++)
                {
                    if (activeUnits[i].transform == c.transform)
                    {
                        unitSelected = true;
                        activeUnit.deletePath();
                        activeUnit = activeUnits[i];
                        camMovePath[1] = activeUnit.transform.position;
                        CameraMovementManager.RequestCamMove(camMovePath);
                        debugText.text = activeUnit.name;
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// if clicked the same tile, move Unit there, otherwise find new path for active unit
    /// </summary>
    private void findPathForUnit()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = -transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        if (clickedNode != null && activeUnit.hasPath())
        {
            if (clickedNode.worldPosition == grid.NodeFromWorldPoint(mousePosition).worldPosition)
            {
                moveUnit();
                return;
            }
                 
        }
        clickedNode = grid.NodeFromWorldPoint(mousePosition);
        activeUnit.RequestPath(mousePosition);
    }

    public void KillUnit(Unit unit)
    {
        debugText.text = unit.unitname + " was killed.";
        opponentUnits.Remove(unit);
        playerUnits.Remove(unit);
        enemyUnits.Remove(unit);
        unit.die();
        Destroy(unit.gameObject);

        if (playerUnits.Count == 0)
            statemachine.loseGame();
        else if (enemyUnits.Count == 0)
            statemachine.winGame();
    }

    //function which move-button calls, this function disables raycasting and moves active unit
    public void moveUnit()
    {
        StartCoroutine(moveUnitCoroutine());
    }

    private IEnumerator moveUnitCoroutine()
    {
        disableRayCast = true;
        CameraMovementManager.RequestCamMove(activeUnit.startMoving());
        while (unitMoving || CameraMovement.cameraIsMoving)
        {
            yield return null;
        }
        // select next unit automatically, if no unit is available, end turn;
        if (!activeUnit.isMovementPossible())
            if (!selectNextUnit())
                endTurn();
    }
}
