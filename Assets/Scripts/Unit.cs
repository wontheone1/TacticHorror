using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    /// <summary>
    /// store FMOD events
    /// </summary>
    //[FMODUnity.EventRef]
    protected string ladderUpdownEvent, walkEvent, dieEvent, getHitEvent, attackEvent, jumpEvent;


    //Animator
    private Animator unitAnim;
    public static int UNIT_COUNT = 0;
    public string unitname;
    private float walkingSpeed = 4f, climbingSpeed = 0.8f;
    float speed = 4f; // speed for animation
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

    /// <summary>
    /// string Hash for animators... (optimization)
    /// </summary>
    private int isMovingHash = Animator.StringToHash("isMoving");
    private int goUpLadderHash = Animator.StringToHash("goUpLadder");
    private int goOutLadderHash = Animator.StringToHash("goOutLadder");
    private int turnStateHash = Animator.StringToHash("turn");
    private int climbStateHash = Animator.StringToHash("climb");
    private BoxCollider2D collider;
    Vector3 rightScale;
    Vector3 leftScale;

    protected virtual void Awake()
    {
        Debug.Log(turnStateHash);
        Debug.Log(climbStateHash);
        collider = GetComponent<BoxCollider2D>();
        unitAnim = GetComponent<Animator>();
        grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
        pathfinding = GameObject.FindWithTag("MainCamera").GetComponent<Pathfinding>();
        gameController = GameObject.FindWithTag("MainCamera").GetComponent<GameController>();
        leftScale = rightScale = transform.localScale;
        leftScale.x *= -1;
    }

    //protected virtual void Update()
    //{
    //}

    //delete units path, used before switching units and switching turn, function is called from Grid-script
    public void deletePath()
    {
        if (!unitMoving)
            path = null;
    }

    public void RequestPath(Vector2 target)
    {
        if (IsMovementPossible() && GameController.unitMoving == false)
            PathRequestManager.RequestPath(transform.position, target, actionPoint, OnPathFound);
    }

    public void setAttackTarget(Unit _targetUnit)
    {
        Node thisUnitNode = GetCurrentNode();
        Node targetUnitNode = _targetUnit.GetCurrentNode();
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
        gameController.TextBoxManager.EventHandler(unitname, "die");
        FMODUnity.RuntimeManager.PlayOneShot(dieEvent, transform.position);
    }

    public Node GetCurrentNode()
    {
        return grid.NodeFromWorldPoint(transform.position);
    }

    public bool IsMovementPossible()
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
            if (path.Length > 1)
            {
                DecideFaceDirection(path[1]);
            }
            movementCostToDestination = movementCost;
        }
    }

    public void DecideFaceDirection(Vector3 faceTo)
    {
        if (faceTo.x < transform.position.x)
        {
            FaceLeft();
        }
        else
        {
            FaceRight();
        }
    }

    public void FaceLeft()
    {
        transform.localScale = leftScale;
    }

    public void FaceRight()
    {
        transform.localScale = rightScale;
    }

    //movement script to move unit when "move button" is clicked, succesful boolean tests for succesful path before moving
    public Vector2[] startMoving()
    {
        if (succesful && path != null)
        {
            StopCoroutine("FollowPath");
            succesful = false;
            unitAnim.SetBool(isMovingHash, true);
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
        GameController.unitMoving = unitMoving = true;
        if (path.Length > 0)
        {
            Vector3 currentWaypoint = path[0];
            while (true)
            {
                DecideFaceDirection(currentWaypoint);
                if (isClimbing(currentWaypoint))
                {
                    AnimatorStateInfo stateInfo = unitAnim.GetCurrentAnimatorStateInfo(0);
                    Debug.Log(stateInfo.shortNameHash);

                    if (stateInfo.shortNameHash != turnStateHash && stateInfo.shortNameHash != climbStateHash)
                    {
                        unitAnim.SetTrigger(goUpLadderHash);
                        speed = climbingSpeed;
                    }
                }
                else
                {
                    unitAnim.SetTrigger(goOutLadderHash);
                    speed = walkingSpeed;
                }
                if (Vector3.Distance(transform.position, currentWaypoint) < 0.10)
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
                //

                transform.position = Vector2.MoveTowards(transform.position,
                       currentWaypoint, speed * Time.deltaTime);
                FMODUnity.RuntimeManager.PlayOneShot(walkEvent);
                yield return null;
            }
            GameController.unitMoving = unitMoving = false;
            unitAnim.SetBool(isMovingHash, false);
            unitAnim.SetTrigger(goOutLadderHash);
            speed = walkingSpeed;
        }
    }

    public bool isClimbing(Vector3 currentWaypoint)
    {
        return grid.NodeFromWorldPoint(currentWaypoint).gridY != GetCurrentNode().gridY;
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
                Gizmos.color = Color.white;
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
