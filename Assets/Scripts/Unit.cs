using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    /// <summary>
    /// store FMOD events
    /// </summary>
    //[FMODUnity.EventRef]
    protected string dieEvent = "event:/Characters/Soldier/soldier_up_down_ladder";
    protected string walkEvent;
    protected string ladderUpdownEvent;
    protected string getHitEvent;
    protected string attackEvent;

    public static int UNIT_COUNT = 0;
    public Rigidbody2D rb;
    public string name;
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
    private bool unitMoving = false;
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
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
    }

    //delete units path, used before switching units and switching turn, function is called from Grid-script
    public void deletePath()
    {
        if(!unitMoving)
            path = null;
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
            FMODUnity.RuntimeManager.PlayOneShot(attackEvent);
            actionPoint = 0;
        }
    }

    public void takeDamage(int damage)
    {
        if (damage > 0)
        {
            hp -= damage;
            FMODUnity.RuntimeManager.PlayOneShot(getHitEvent);
            if (hp <= 0)
                gameController.KillUnit(this);
        }
    }

    public virtual void die()
    {
        gameController.TextBoxManager.EventHandler(name, "die");
        FMODUnity.RuntimeManager.PlayOneShot(dieEvent, transform.position);
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
            Debug.Log("started moving");
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
        // rb.isKinematic = true;
        unitMoving = true;
        GameController.unitMoving = true;
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                if (Vector3.Distance(transform.position,currentWaypoint) < 0.10)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        /// When finished moving, clear up 
                        Debug.Log("Finished moving.");
                        targetIndex = 0;
                        path = new Vector2[0];
                        break;
                    }
                    currentWaypoint = path[targetIndex];
                }

                transform.position = Vector2.MoveTowards(transform.position,
                       currentWaypoint, speed * Time.deltaTime);
                FMODUnity.RuntimeManager.PlayOneShot(walkEvent);
                yield return null;
            }
            GameController.unitMoving = false;
            Debug.Log("here");
            unitMoving = false;
            // rb.isKinematic = false;
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        
        if (coll.relativeVelocity.magnitude > 2.5)
        {
            Debug.Log("collision enter");
            Vector2 currentVelocity = rb.velocity = Vector2.zero;
            rb.isKinematic = true;
            path = new Vector2[2];
            path[0] = transform.position;
            path[1] = grid.NodeFromWorldPoint(transform.position).worldPosition;
            succesful = true;
            startMoving();
        }
    }

    void OnCollisionExit2D(Collision2D coll)
    {
        Debug.Log("collisionexit rel vel " + coll.relativeVelocity);
        rb.velocity = new Vector2(0, 3.5f);
        rb.isKinematic = false; 
    }

    void setActive()
    {
        //rb.
    }

    void setInactive()
    {
        
    }

    public bool hasPath()
    {
        if (path != null)
            return path.Length > 0;
        else
            return false;
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
