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
    // instantiating enum state machine for easy and understandable usage
    public enum State
    {
        sceneStart,
        player,
        enemy,
        win,
        lose
    };
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
            catch (Exception){}
        }
    }

    // Use this for initialization
    void Start()
    {
        implementCurrentState();
    }

    public void startGame()
    {
        curState = State.player;
        try
        {
            foreach (GameObject VARIABLE in objectsHiddenBeforeGameStarts)
            {
                VARIABLE.SetActive(true);
            }
        }
        catch (Exception) { }
        cameraMovement.CameraDisabled = false;
        implementCurrentState();
    }

    //this function is called when end turn -button is pressed, this function disables raycasting also
    public void endTurn()
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
        implementCurrentState();
    }

    public void winGame()
    {
        curState = State.win;
        implementCurrentState();
    }

    public void loseGame()
    {
        curState = State.lose;
        implementCurrentState();
    }

    public void implementCurrentState()
    {
        Vector2[] camMovePath;
        switch (curState)
        {
            case State.sceneStart:
                cameraMovement.CameraDisabled = true;
                break;

            case State.player:
                //players turn
                //change active unit to player Unit
                StateText.text = "Player turn";
                camMovePath = new Vector2[2];
                camMovePath[0] = Camera.main.gameObject.transform.position;
                gameController.ActiveUnits = gameController.playerUnits;
                gameController.ActiveUnit = gameController.playerUnits[0];
                gameController.OpponentUnits = gameController.enemyUnits;
                camMovePath[1] = gameController.ActiveUnit.transform.position;
                CameraMovementManager.RequestCamMove(camMovePath);
                replenishActionPoints();
                break;

            case State.enemy:
                //enemys turn
                //change active unit to enemyunit
                StateText.text = "Enemy turn";
                camMovePath = new Vector2[2];
                camMovePath[0] = Camera.main.gameObject.transform.position;
                gameController.ActiveUnits = gameController.enemyUnits;
                gameController.ActiveUnit = gameController.enemyUnits[0];
                gameController.OpponentUnits = gameController.playerUnits;
                camMovePath[1] = gameController.ActiveUnit.transform.position;
                CameraMovementManager.RequestCamMove(camMovePath);
                replenishActionPoints();
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
    public void replenishActionPoints()
    {
        foreach (var unit in gameController.ActiveUnits)
        {
            unit.replenishActionPoint();
            unit.deletePath();
        }
    }
}
