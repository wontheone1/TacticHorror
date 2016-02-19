using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Statemachine : MonoBehaviour
{
    public GameObject[] objectsHiddenBeforeGameStarts;
    public Text StateText;
    private GameController gameController;
    private CameraMovement cameraMovement;
    private string winEvent = "event:/Music/victory";
    private string loseEvent = "event:/Music/defeat";
    private Grid grid;
    // instantiating enum state machine for easy and understandable usage
    public enum State
    {
        sceneStart,
        player,
        enemy,
        win,
        lose
    }

    State curState = State.sceneStart;
    public State CurState
    {
        get { return curState; }
    }

    //// Use this for initialization
    void Awake()
    {
        objectsHiddenBeforeGameStarts = new[] {GameObject.Find("EndTurn")};
        try
        {
            StateText = GameObject.Find("Game State Text").GetComponent<Text>();
        } catch (Exception e) { };
        cameraMovement = GetComponent<CameraMovement>();
        gameController = GetComponent<GameController>();
        foreach (GameObject VARIABLE in objectsHiddenBeforeGameStarts)
        {
            try
            {
                VARIABLE.SetActive(false);
            }
            catch (Exception)
            {
                // ignored
            }
        }
        grid = GetComponent<Grid>();
    }

    // Use this for initialization
    void Start()
    {
        ImplementCurrentState();
    }

    public void StartGame()
    {
        curState = State.player;
        try
        {
            foreach (GameObject variable in objectsHiddenBeforeGameStarts)
            {
                variable.SetActive(true);
            }
        }
        catch (Exception)
        {
            // ignored
        }
        cameraMovement.CameraDisabled = false;
        ImplementCurrentState();
    }

    //this function is called when end turn -button is pressed, this function disables raycasting also
    public void EndTurn()
    {
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
        ImplementCurrentState();
    }

    public void WinGame()
    {
        curState = State.win;
        ImplementCurrentState();
    }

    public void LoseGame()
    {
        curState = State.lose;
        ImplementCurrentState();
    }

    public void ImplementCurrentState()
    {
        List<Node> camMovePath;
        switch (curState)
        {
            case State.sceneStart:
                cameraMovement.CameraDisabled = true;
                break;

            case State.player:
                //players turn
                //change active unit to player Unit
                StateText.text = "Player turn";
                camMovePath = new List<Node>();
                camMovePath.Add(grid.NodeFromWorldPoint(Camera.main.gameObject.transform.position));
                gameController.ActiveUnits = gameController.playerUnits;
                gameController.ActiveUnit = gameController.playerUnits[0];
                gameController.OpponentUnits = gameController.enemyUnits;
                camMovePath.Add(grid.NodeFromWorldPoint(gameController.ActiveUnit.transform.position));
                CameraMovementManager.RequestCamMove(camMovePath);
                ReplenishActionPoints();
                break;

            case State.enemy:
                //enemys turn
                //change active unit to enemyunit
                StateText.text = "Enemy turn";
                camMovePath = new List<Node>();
                camMovePath.Add(grid.NodeFromWorldPoint(Camera.main.gameObject.transform.position));
                gameController.ActiveUnits = gameController.enemyUnits;
                gameController.ActiveUnit = gameController.enemyUnits[0];
                gameController.OpponentUnits = gameController.playerUnits;
                camMovePath.Add(grid.NodeFromWorldPoint(gameController.ActiveUnit.transform.position));
                CameraMovementManager.RequestCamMove(camMovePath);
                ReplenishActionPoints();
                break;

            case Statemachine.State.lose:
                //game is lost
                StateText.text = "You Lost!";
                FMODUnity.RuntimeManager.PlayOneShot(loseEvent);
                break;

            case Statemachine.State.win:
                //game is won, lol
                StateText.text = "You Won!";
                FMODUnity.RuntimeManager.PlayOneShot(winEvent);
                break;
        }
    }

    /// <summary>
    /// When end turn, replenish action points of other units
    /// </summary>
    public void ReplenishActionPoints()
    {
        foreach (var unit in gameController.ActiveUnits)
        {
            unit.ReplenishActionPoint();
            unit.deletePath();
        }
    }
}
