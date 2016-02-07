using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statemachine : MonoBehaviour
{
    private GameController gameController;
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

    //// Use this for initialization
    void Awake()
    {
        gameController = GetComponent<GameController>();
    }

    // Use this for initialization
    void Start()
    {
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

    public void implementCurrentState()
    {
        Vector2[] camMovePath;
        switch (curState)
        {
            case State.player:
                //players turn
                //change active unit to player Unit
                camMovePath = new Vector2[2];
                camMovePath[0] = Camera.main.gameObject.transform.position;
                gameController.ActiveUnits = gameController.playerUnits;
                gameController.ActiveUnit = gameController.playerUnits[0];
                camMovePath[1] = gameController.ActiveUnit.transform.position;
                CameraMovementManager.RequestCamMove(camMovePath);
                replenishActionPoints();
                break;

            case State.enemy:
                //enemys turn
                //change active unit to enemyunit
                camMovePath = new Vector2[2];
                camMovePath[0] = Camera.main.gameObject.transform.position;
                gameController.ActiveUnits = gameController.enemyUnits;
                gameController.ActiveUnit = gameController.enemyUnits[0];
                camMovePath[1] = gameController.ActiveUnit.transform.position;
                CameraMovementManager.RequestCamMove(camMovePath);
                replenishActionPoints();
                break;

            case Statemachine.State.lose:
                //game is lost
                break;

            case Statemachine.State.win:
                //game is won, lol
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
        }
    }
}
