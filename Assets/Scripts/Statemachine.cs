using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using System.Linq;


public class Statemachine : MonoBehaviour
{
    public GameObject[] ObjectsHiddenBeforeGameStarts;
    public Text StateText;
    private GameController _gameController;
    private CameraMovement _cameraMovement;
    private FOVRecurse _fov;
    private readonly string winEvent = "event:/Music/victory";
    private readonly string loseEvent = "event:/Music/defeat";
    private Grid _grid;
    // instantiating enum state machine for easy and understandable usage
    public enum State
    {
        SceneStart,
        Player,
        Enemy,
        Win,
        Lose
    }

    State _curState = State.SceneStart;

    public State CurState
    {
        get { return _curState; }
    }
    
    // ReSharper disable once UnusedMember.Local
    private void Awake()
    {
        ObjectsHiddenBeforeGameStarts = new[] {GameObject.Find("EndTurn")};
        try
        {
            StateText = GameObject.Find("Game State Text").GetComponent<Text>();
        }
        catch (Exception)
        {
            // ignored
        }
        
        _cameraMovement = GetComponent<CameraMovement>();
        _gameController = GetComponent<GameController>();
        foreach (GameObject variable in ObjectsHiddenBeforeGameStarts)
        {
            try
            {
                variable.SetActive(false);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        _grid = GetComponent<Grid>();
        _fov = GetComponent<FOVRecurse>();
    }
    
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        // ImplementCurrentState();
    }

    public void StartGame()
    {
        _curState = State.Player;
        foreach (var unit in _gameController.EnemyUnits)
        {
            unit.UnitController.DecideCrouchOrStanding();
            unit.GetCurrentNode().Occupied = true;
        }
        foreach (var unit in _gameController.PlayerUnits)
        {
            unit.UnitController.DecideCrouchOrStanding();
            unit.GetCurrentNode().Occupied = true;
        }
        try
        {
            foreach (GameObject variable in ObjectsHiddenBeforeGameStarts)
            {
                variable.SetActive(true);
            }
        }
        catch (Exception)
        {
            // ignored
        }
        _cameraMovement.CameraDisabled = false;
        ImplementCurrentState();
    }

    //this function is called when end turn -button is pressed, this function disables raycasting also
    public void EndTurn()
    {
        //if current state is Player, switch to Enemy state
        if (_curState == State.Player)
        {
            //switch current state to Enemy
            _curState = State.Enemy;
        }

        //works same way as switching from Player to Enemy
        else if (_curState == State.Enemy)
        {
            _curState = State.Player;
        }
        ImplementCurrentState();
        _gameController.ShowSelectionUI();
        _gameController.ShowTilesInMovementRange();
    }

    public void WinGame()
    {
        _curState = State.Win;
        ImplementCurrentState();
    }

    public void LoseGame()
    {
        _curState = State.Lose;
        ImplementCurrentState();
    }

    public void ImplementCurrentState()
    {
        
        switch (_curState)
        {
            case State.SceneStart:
                _cameraMovement.CameraDisabled = true;
                
                break;

		case State.Player:
                //players turn
                //change active unit to Player Unit
				StateText.text = "Player turn";
				SetActiveUnitsOpponentUnits (_gameController.PlayerUnits, _gameController.EnemyUnits);
				_grid.DrawFOW ();
                _gameController.ShowTilesInMovementRange();
                CameraToCurrentActiveUnit();
                ReplenishActionPoints();
                UnsetTargetUnits();
                break;

            case State.Enemy:
                //enemys turn
                //change active unit to enemyunit
                StateText.text = "Enemy turn";
                SetActiveUnitsOpponentUnits(_gameController.EnemyUnits, _gameController.PlayerUnits);
                _grid.DrawFOW();
                _gameController.ShowTilesInMovementRange();
                CameraToCurrentActiveUnit();
                ReplenishActionPoints();
                UnsetTargetUnits();
                break;

            case State.Lose:
                //game is lost
                StateText.text = "You Lost!";
                FMODUnity.RuntimeManager.PlayOneShot(loseEvent);
                break;

            case State.Win:
                //game is won, lol
                StateText.text = "You Won!";
                FMODUnity.RuntimeManager.PlayOneShot(winEvent);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetActiveUnitsOpponentUnits(List<Unit> playerUnits, List<Unit> enemyUnits)
    {
        _gameController.ActiveUnits = playerUnits;
        _gameController.ActiveUnit = playerUnits[0];
        _gameController.OpponentUnits = enemyUnits;
        _fov.SetActiveUnits();
    }

    void CameraToCurrentActiveUnit()
    {
        List<Node> camMovePath;
        camMovePath = new List<Node> { _grid.NodeFromWorldPoint(Camera.main.gameObject.transform.position) };
        camMovePath.Add(_grid.NodeFromWorldPoint(_gameController.ActiveUnit.transform.position));
        CameraMovementManager.RequestCamMove(camMovePath);
    }

    /// <summary>
    /// When end turn, replenish action points of other units
    /// </summary>
    public void ReplenishActionPoints()
    {
        foreach (var unit in _gameController.ActiveUnits)
        {
            unit.ReplenishActionPoint();
            unit.DeletePath();
        }
    }
    public void UnsetTargetUnits()
    {
        foreach (var unit in _gameController.ActiveUnits)
        {
            unit.UnitController.UnsetAttackTarget();
        }
    }
}
