using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

// ReSharper disable once CheckNamespace
public class GameController : MonoBehaviour
{
    private TextBoxManager _textBoxManager;
    private Button _endButton;
    private Grid _grid;
    private Node _clickedNode;
    private Statemachine _statemachine;
    public Text DebugText;
    private Unit _activeUnit;
    private List<Unit> _activeUnits;
    private List<Unit> _opponentUnits;
    public List<Unit> PlayerUnits;
    public List<Unit> EnemyUnits;
    public static bool UnitMoving = false;
    private Vector3 _mousePosition;
    private bool _unitSelected;
    private Vector3 _originalClickPos;
    private bool _disableRayCast;

    // getters and setters
    public List<Unit> ActiveUnits
    {
        get { return _activeUnits; }
        set { _activeUnits = value; }
    }

    public List<Unit> OpponentUnits
    {
        get { return _opponentUnits; }
        set { _opponentUnits = value; }
    }

    public Unit ActiveUnit
    {
        get { return _activeUnit; }
        set { _activeUnit = value; }
    }

    public TextBoxManager TextBoxManager
    {
        get
        {
            return _textBoxManager;
        }
    }
    
    // ReSharper disable once UnusedMember.Local
    void Awake()
    {
        try
        {
            DebugText = GameObject.Find("Debug Text").GetComponent<Text>();
        }
        catch (Exception e) { Debug.Log(e);}
        _statemachine = GetComponent<Statemachine>();
        _grid = GetComponent<Grid>();
        _endButton = GameObject.Find("EndTurnButton").GetComponent<Button>();
        _endButton.onClick.AddListener(EndTurn);
        _textBoxManager = GetComponent<TextBoxManager>();
        for (int i = 0;
            i < GameObject.Find("Players").transform.childCount;
            i++)
        {
            PlayerUnits.Add(GameObject.Find("Players").transform.GetChild(i).GetComponent<Unit>());
        }

        for (int i = 0;
            i < GameObject.Find("Enemies").transform.childCount;
            i++)
        {
            EnemyUnits.Add(GameObject.Find("Enemies").transform.GetChild(i).GetComponent<Unit>());
        }
    }

    public void EndTurn()
    {
        _disableRayCast = true;
        ClearPaths();
        _statemachine.EndTurn();
    }

    public void ClearPaths()
    {
        foreach(Unit unit in ActiveUnits)
        {
            unit.DeletePath();
        }
    }
    
    // ReSharper disable once UnusedMember.Local
    void Update()
    {
        if (!UnitMoving && !CameraMovement.CameraIsMoving && _activeUnit != null)
            Activities();
    }

    public void Activities()
    {
        // change _activeUnit using 'tab' key
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            SelectNextUnit();
        }

        // Select units only when user is not moving the camera
		if (Input.GetMouseButtonDown(0) && !_disableRayCast)
        {
            _originalClickPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0) && _originalClickPos != null && !_disableRayCast)
        {
            if (Vector3.Distance(Input.mousePosition, _originalClickPos) < 0.05)
            {
                SelectUnitByClick();
            }
        }

        // Move units only when user is not moving the camera
        // store path only if raycast is not disabled
        if (Input.GetMouseButtonUp(0) && _originalClickPos != null && !_disableRayCast)
        {
            if (Vector3.Distance(Input.mousePosition, _originalClickPos) < 0.05 && !_unitSelected)
            {
                FindPathForUnit();
            }
        }
        _unitSelected = false;
        //set raycast usable again
        _disableRayCast = false;
    }

    /// <summary>
    /// select next unit in the array. 
    /// Returns true when there is an available unit, false when there is no available unit.
    /// </summary>
    public bool SelectNextUnit()
    {
        List<Node> camMovePath = new List<Node>();
        int currentUnitIndex = _activeUnits.IndexOf(_activeUnit);
        camMovePath.Add(_grid.NodeFromWorldPoint(Camera.main.gameObject.transform.position));
        for (int i = 0; i < _activeUnits.Count; i++)
        {
            
            if (_activeUnits[(currentUnitIndex + 1 + i)%_activeUnits.Count].IsMovementPossible())
            {
                _activeUnit = _activeUnits[(currentUnitIndex + 1 + i) % _activeUnits.Count];
                camMovePath.Add(_grid.NodeFromWorldPoint(_activeUnit.transform.position));
                CameraMovementManager.RequestCamMove(camMovePath);
                return true;
            }
        }
        return false;
    }

    private void SelectUnitByClick()
    {
        Vector2 v = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] col = Physics2D.OverlapPointAll(v);
        if (col.Length <= 0) return;
        foreach (Collider2D c in col)
        {
            // if an opponent unit is clicked, select it as target
            // if there is a target, attack it
            foreach (Unit opponent in _opponentUnits)
            {
                if (opponent.transform != c.transform) continue;
                _unitSelected = true;
                _activeUnit.DeletePath();

                if (_activeUnit.TargetUnit == c.gameObject.GetComponent<Unit>())
                {
                    // attack target will result in select next available unit or end turn
                    _activeUnit.AttackTarget();
                    DebugText.text = "Attacked: " + _activeUnit.TargetUnit.Unitname;
                    return;
                }
                _activeUnit.SetAttackTarget(opponent);

                if (_activeUnit.TargetUnit == null) continue;
                DebugText.text = "Target: " + _activeUnit.TargetUnit.Unitname;
                return;
            }

            // if no opponent was clicked, unset target
            _activeUnit.UnsetAttackTarget();

            List<Node> camMovePath = new List<Node>
            {
                _grid.NodeFromWorldPoint(Camera.main.gameObject.transform.position)
            };
            // select clicked unit
            Collider2D c1 = c;
            foreach (Unit activeUnit in _activeUnits.Where(activeUnit => activeUnit.transform == c1.transform))
            {
                if (activeUnit.IsMovementPossible())
                {
                    _unitSelected = true;
                    _activeUnit.DeletePath();
                    _activeUnit = activeUnit;
                    camMovePath.Add(_grid.NodeFromWorldPoint(_activeUnit.transform.position));
                    CameraMovementManager.RequestCamMove(camMovePath);
                    DebugText.text = _activeUnit.name;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// if clicked the same tile, move Unit there, otherwise find new path for active unit
    /// </summary>
    private void FindPathForUnit()
    {
        _mousePosition = Input.mousePosition;
        _mousePosition.z = -transform.position.z;
        _mousePosition = Camera.main.ScreenToWorldPoint(_mousePosition);
        if (_clickedNode != null && _activeUnit.HasPath())
        {
            if (_clickedNode.WorldPosition == _grid.NodeFromWorldPoint(_mousePosition).WorldPosition)
            {
                MoveUnit();
                return;
            }
                 
        }
        _clickedNode = _grid.NodeFromWorldPoint(_mousePosition);
        _activeUnit.RequestPath(_mousePosition);
    }

    public void KillUnit(Unit unit)
    {
        DebugText.text = unit.Unitname + " was killed.";
        _opponentUnits.Remove(unit);
        PlayerUnits.Remove(unit);
        EnemyUnits.Remove(unit);
        unit.Die();
        // Destroy(unit.gameObject);

        if (PlayerUnits.Count == 0)
            _statemachine.LoseGame();
        else if (EnemyUnits.Count == 0)
            _statemachine.WinGame();
    }

    //function which move-button calls, this function disables raycasting and moves active unit
    public void MoveUnit()
    {
        StartCoroutine(MoveUnitCoroutine());
    }

    private IEnumerator MoveUnitCoroutine()
    {
        _disableRayCast = true;
        Vector3 cameraPositionToGo;
        _activeUnit.StartMoving();
        while (UnitMoving)
        {
            cameraPositionToGo = _activeUnit.transform.position;
            cameraPositionToGo.z = Camera.main.transform.position.z;
            Camera.main.transform.position = cameraPositionToGo;
            yield return null;
        }
        // select next unit automatically, if no unit is available, end turn;
        if (!_activeUnit.IsMovementPossible())
            if (!SelectNextUnit())
                EndTurn();
    }
}
