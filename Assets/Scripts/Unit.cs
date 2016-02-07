using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    float speed = 6f; // speed for animation
    Vector2[] path;
    int targetIndex;
    bool succesful = false;
    Vector2 originalClickPos;
    protected int MAX_ACTION_POINT; //movement point + other action
    protected int MAX_HP; // health point
    protected int MAX_AP; // attack point
    protected int MAX_MP; // mana
    protected int attackRange; //attack Range
    protected int actionPoint; //movement point + other action
    protected int hp; // health point
    protected int ap; // attack point
    protected int mp; // mana
    protected Grid grid;
    Pathfinding pathfinding;
    GameController gameController;
    Unit targetUnit = null;
    private int movementCostToDestination;
    public Unit TargetUnit
    {
        get { return targetUnit; }
    }

    protected virtual void Awake()
    {
        grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
        pathfinding = GameObject.FindWithTag("MainCamera").GetComponent<Pathfinding>();
        gameController = GameObject.FindWithTag("MainCamera").GetComponent<GameController>();
    }

    void Start()
    {
    }

    public void RequestPath(Vector2 target)
    {
        if(isMovementPossible() && GameController.unitMoving == false)
            PathRequestManager.RequestPath(transform.position, target, actionPoint, OnPathFound);
    }

    public void setAttackTarget(Unit _targetUnit)
    {
        Node thisUnitNode = getCurrentNode();
        Node targetUnitNode = _targetUnit.getCurrentNode();
        if (pathfinding.GetDistance(thisUnitNode, targetUnitNode) <= attackRange)
            targetUnit = _targetUnit;
        else
            Debug.Log("the unit is out of attack range");
    }

    public void unsetAttackTarget()
    {
        targetUnit = null;
    }

    public void attackTarget()
    {
        if (targetUnit != null)
        {
            targetUnit.takeDamage(ap);
        }
    }

    public void takeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
            gameController.KillUnit(this);
    }

    public Node getCurrentNode()
    {
        return grid.NodeFromWorldPoint(transform.position);
    }

    public bool isMovementPossible()
    {
        return actionPoint > 10;
    }

    public void OnPathFound(Vector2[] newPath, bool pathSuccessful, int movementCost)
    {
        if (pathSuccessful)
        {
            //mark path succesful
            succesful = true;
            path = newPath;
            movementCostToDestination = movementCost;
        }
    }

    //movement script to move unit when "move button" is clicked, succesful boolean tests for succesful path before moving
    public Vector2[] startMoving()
    {
        if (succesful && path != null)
        {
            StopCoroutine("FollowPath");
            succesful = false;
            StartCoroutine("FollowPath");
            actionPoint -= movementCostToDestination;
            return path;
        }
        return null;
    }

    public void replenishActionPoint()
    {
        actionPoint = MAX_ACTION_POINT;
    }

    IEnumerator FollowPath()
    {
        GameController.unitMoving = true;
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                if (transform.position == currentWaypoint)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        /// When finished moving, clear up 
                        targetIndex = 0;
                        path = new Vector2[0];
                        break;
                    }
                    currentWaypoint = path[targetIndex];
                }
                transform.position = Vector2.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
                yield return null;
            }
            GameController.unitMoving = false;
        }
    }

    //delete units path, used before switching units and switching turn, function is called from Grid-script
    public void deletePath()
    {
        path = null;
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], new Vector3(0.25f, 0.25f, 0.25f));

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}
