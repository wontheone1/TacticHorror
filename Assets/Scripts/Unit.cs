using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    /// <summary>
    /// store FMOD events
    /// </summary>
    //[FMODUnity.EventRef]
    protected string ladderUpdownEvent, walkEvent, dieEvent, getHitEvent, attackEvent, jumpEvent;

    //Animator
    private AnimatorStateInfo stateInfo;
    private Animator unitAnim;
    public static int UNIT_COUNT = 0;
    public string unitname;
    private float walkingSpeed = 4f, climbingSpeed = 2.5f;
    float speed = 0f; // speed for animation
    List<Node> path;
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
    private Node currentWayPoint;
    Pathfinding pathfinding;
    GameController gameController;
    private bool unitMoving, startedWalking, startedClimbing, startedFinishingWalk, startedFinishingClimbing = false;
    Unit targetUnit = null;
    private int movementCostToDestination;
    public Unit TargetUnit
    {
        get { return targetUnit; }
    }

    /// <summary>
    /// string Hash for animators... (optimization)
    /// </summary>
    /// 
    //private int isMovingHash = Animator.StringToHash("isMoving");
    private int isWalkingHash = Animator.StringToHash("isWalking");
    private int startWalkHash = Animator.StringToHash("startWalk");
    private int stopWalkHash = Animator.StringToHash("stopWalk");
    private int goUpLadderHash = Animator.StringToHash("goUpLadder");
    private int goOutLadderHash = Animator.StringToHash("goOutLadder");
    private int turnStateHash = Animator.StringToHash("turn");
    private int turnBackStateHash = Animator.StringToHash("turnBack");
    private int climbStateHash = Animator.StringToHash("climb");
    private int walkStateHash = Animator.StringToHash("walk");
    private int idleStateHash = Animator.StringToHash("idle");
    private BoxCollider2D collider;
    Vector3 rightScale;
    Vector3 leftScale;

    protected virtual void Awake()
    {
        Debug.Log("climb: " +climbStateHash);
        Debug.Log("idle: " +idleStateHash);
        Debug.Log("turn: " +turnStateHash);
        Debug.Log("turnBack: " +turnBackStateHash);
        Debug.Log("walk: "+walkStateHash);
        collider = GetComponent<BoxCollider2D>();
        unitAnim = GetComponent<Animator>();
        grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
        pathfinding = GameObject.FindWithTag("MainCamera").GetComponent<Pathfinding>();
        gameController = GameObject.FindWithTag("MainCamera").GetComponent<GameController>();
        leftScale = rightScale = transform.localScale;
        leftScale.x *= -1;
    }

    protected virtual void Update()
    {
    }

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

    public void SetAttackTarget(Unit _targetUnit)
    {
        Node thisUnitNode = GetCurrentNode();
        Node targetUnitNode = _targetUnit.GetCurrentNode();
        if (pathfinding.GetDistance(thisUnitNode, targetUnitNode) <= attackRange)
            targetUnit = _targetUnit;
        else
            Debug.Log("the unit is out of attack range");
    }

    public void UnsetAttackTarget()
    {
        targetUnit = null;
    }

    public void AttackTarget()
    {
        if (targetUnit != null)
        {
            targetUnit.TakeDamage(ap);
            FMODUnity.RuntimeManager.PlayOneShot(attackEvent);
            actionPoint = 0;
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage > 0)
        {
            hp -= damage;
            FMODUnity.RuntimeManager.PlayOneShot(getHitEvent);
            if (hp <= 0)
                gameController.KillUnit(this);
        }
    }

    public virtual void Die()
    {
        gameController.TextBoxManager.EventHandler(unitname, "Die");
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

    public void OnPathFound(List<Node> newPath, bool pathSuccessful, int movementCost)
    {
        if (pathSuccessful)
        {
            //mark path succesful
            succesful = true;
            path = newPath;
            if (path.Count > 0)
            {
                Debug.Log("face");
                DecideFaceDirection(path[0]);
            }
            movementCostToDestination = movementCost;
        }
    }

    public void DecideFaceDirection(Node faceTo)
    {
        if (faceTo.worldPosition.x < transform.position.x)
        {
            transform.localScale = leftScale; /// face left
        }
        else
        {
            transform.localScale = rightScale; /// face right
        }
    }

    //movement script to move unit when "move button" is clicked, succesful boolean tests for succesful path before moving
    public List<Node> StartMoving()
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

    public void ReplenishActionPoint()
    {
        actionPoint = MAX_ACTION_POINT;
    }

    IEnumerator FollowPath()
    {
        GameController.unitMoving = unitMoving = true;
        if (path.Count > 0)
        {
            Debug.Log(path.Count);
            foreach (Node n in path)
            {
                currentWayPoint = n;
                DecideFaceDirection(currentWayPoint);
                DecideWalkingOrClimb(currentWayPoint);
                do
                {
                    Debug.Log(stateInfo.shortNameHash);
                    stateInfo = unitAnim.GetCurrentAnimatorStateInfo(0);
                    yield return null;
                } while (stateInfo.shortNameHash != climbStateHash
                && stateInfo.shortNameHash != walkStateHash);

                // while (Math.Abs(transform.position.x - currentWayPoint.worldPosition.x) > 0.1
                //     || Math.Abs(transform.position.y - currentWayPoint.worldPosition.y) > 0.1)
                while (Vector2.Distance(transform.position, currentWayPoint.worldPosition) > 0.1)
                {
                    DecideSpeedAccordingToAnimationState(unitAnim.GetCurrentAnimatorStateInfo(0));
                    Debug.Log(currentWayPoint.worldPosition);
                    stateInfo = unitAnim.GetCurrentAnimatorStateInfo(0);
                    Debug.Log(stateInfo.shortNameHash);
                    transform.position = Vector2.MoveTowards(transform.position,
                       currentWayPoint.worldPosition, speed * Time.deltaTime);

                    yield return null;
                    //transform.position = Vector2.MoveTowards(transform.position,
                    //       SetDestinationByAnimatorState(unitAnim.GetCurrentAnimatorStateInfo(0),
                    //       currentWayPoint), speed * Time.deltaTime);
                    FMODUnity.RuntimeManager.PlayOneShot(walkEvent);
                    //if (CheckDestinationReachedByAnimatorState(unitAnim.GetCurrentAnimatorStateInfo(0), currentWayPoint))
                    //    break;
                }
                FinishWalkingOrCliming(unitAnim.GetCurrentAnimatorStateInfo(0));
                // yield return new WaitForSeconds(1);
                do
                {
                    stateInfo = unitAnim.GetCurrentAnimatorStateInfo(0);
                    Debug.Log(stateInfo.shortNameHash);
                    yield return null;
                } while (stateInfo.shortNameHash == climbStateHash 
                || stateInfo.shortNameHash == walkStateHash);
            }
            /// When finished moving, clear up 
            path = new List<Node>();
            GameController.unitMoving = unitMoving = false;
            // unitAnim.SetBool(isMovingHash, unitMoving);
            unitAnim.SetBool(isWalkingHash, false);
            // startedWalking = false;
            Debug.Log("Moving finished");
        }
    }

    public bool IsHorizontalMovement(Node currentWaypoint)
    {
        return (currentWayPoint.gridY == GetCurrentNode().gridY);
    }

    public bool IsClimbing(Node currentWayPoint)
    {
        return (currentWayPoint.gridY != GetCurrentNode().gridY) && !GetCurrentNode().jumpThroughable;
    }

    private void DecideWalkingOrClimb(Node currentWayPoint)
    {
        if (IsHorizontalMovement(currentWayPoint))
        {
            unitAnim.SetBool(isWalkingHash, true);
        }
        else if (IsClimbing(currentWayPoint))
        {
            unitAnim.SetTrigger(goUpLadderHash);
        }
        DecideSpeedAccordingToAnimationState(unitAnim.GetCurrentAnimatorStateInfo(0));
    }

    private void FinishWalkingOrCliming(AnimatorStateInfo state)
    {
        if (state.shortNameHash == climbStateHash)
        {
            unitAnim.SetTrigger(goOutLadderHash);
        }
        else if (state.shortNameHash == walkStateHash)
        {
            unitAnim.SetBool(isWalkingHash, false);
        }
        DecideSpeedAccordingToAnimationState(unitAnim.GetCurrentAnimatorStateInfo(0));
    }

    private void DecideSpeedAccordingToAnimationState(AnimatorStateInfo state)
    {
        if (state.shortNameHash == turnStateHash ||
            state.shortNameHash == turnBackStateHash ||
            state.shortNameHash == idleStateHash)
        {
            speed = 0f;
        }
        else if (state.shortNameHash == climbStateHash)
        {
            speed = climbingSpeed;
        }
        else if (state.shortNameHash == walkStateHash)
        {
            speed = walkingSpeed;
        }
    }



    //private bool CheckDestinationReachedByAnimatorState(AnimatorStateInfo state, Node currentWayPoint)
    //{
    //    if (state.shortNameHash == climbStateHash)
    //    {
    //        return Math.Abs(transform.position.y - currentWayPoint.worldPosition.y) < 0.05f;
    //    }
    //    return Math.Abs(transform.position.x - currentWayPoint.worldPosition.x) < 0.05f;
    //}

    //private Vector2 SetDestinationByAnimatorState(AnimatorStateInfo state, Node currentWayPoint)
    //{
    //    if (state.shortNameHash == climbStateHash)
    //    {
    //        return new Vector2(transform.position.x, currentWayPoint.worldPosition.y);
    //    }
    //    return new Vector2(currentWayPoint.worldPosition.x, transform.position.y);
    //}



    public bool IsFinishedClimbing(Node currentWaypoint)
    {
        return startedClimbing && GetCurrentNode().atLadderEnd;
        // return startedClimbing && GetCurrentNode().atLadderEnd && (Vector2.Distance(GetCurrentNode().worldPosition, transform.position) < 0.1);
    }


    public bool IsFinishedWalking(Node currentWaypoint)
    {
        return startedWalking && (currentWaypoint.gridX == GetCurrentNode().gridX);
    }

    public bool HasPath()
    {
        if (path != null)
            return path.Count > 0;
        else
            return false;
    }
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Count; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(path[i].worldPosition, new Vector3(0.2f, 0.2f, 0.2f));
                Gizmos.color = Color.green;
                if (currentWayPoint != null)
                    Gizmos.DrawCube(currentWayPoint.worldPosition, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
    }
}
