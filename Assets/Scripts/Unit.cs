﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
public class Unit : MonoBehaviour
{
    public static int UnitCount = 0;
    public string Unitname;
    public bool IsDead;
    /// <summary>
    /// store FMOD events
    /// </summary>
    //[FMODUnity.EventRef]
    protected string LadderUpdownEvent, WalkEvent, DieEvent, GetHitEvent, AttackEvent, JumpEvent;
    // private GameObject _projectile;
    private Rigidbody2D _rb;
    //Animator
    private AnimatorStateInfo _stateInfo;
    private Animator _unitAnim;
    private const float WalkingSpeed = 4f;
    private const float ClimbingSpeed = 2.5f;
    private const float PrejumpLandingSpeed = 0.7f;
    private const float JumpSpeed = 5f;
    private float _speed; // _speed for animation
    private List<Node> _path;
    private bool _succesful;
    protected int MaxActionPoint; //movement point + other action
    protected int MaxHp; // health point
    protected int MaxAp; // attack point
    protected int MaxMp; // mana
    protected int AttackRange; //attack Range
    protected int ActionPoint; //movement point + other action
    protected int Hp; // health point
    protected int Ap; // attack point
    protected int Mp; // mana
    protected Grid Grid;
    private Node _currentWayPoint;
    private Pathfinding _pathfinding;
    private GameController _gameController;
    private bool _unitMoving, _underJumpPhysics;
    private Unit _targetUnit;
    private int _movementCostToDestination;
    private Vector3 _rightScale, _leftScale;

    public Unit TargetUnit
    {
        get { return _targetUnit; }
    }

    /// <summary>
    /// string Hash for animators... (optimization)
    /// </summary>

    private readonly int _attackHash = Animator.StringToHash("attack");
    private readonly int _jumpHash = Animator.StringToHash("jump");
    private readonly int _landHash = Animator.StringToHash("land");
    private readonly int _killedHash = Animator.StringToHash("killed");
    private readonly int _undercoverHash = Animator.StringToHash("undercover");
    private readonly int _isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int _goUpLadderHash = Animator.StringToHash("goUpLadder");
    private readonly int _goOutLadderHash = Animator.StringToHash("goOutLadder");
    private readonly int _turnStateHash = Animator.StringToHash("turn");
    private readonly int _turnBackStateHash = Animator.StringToHash("turnBack");
    private readonly int _climbStateHash = Animator.StringToHash("climb");
    private readonly int _walkStateHash = Animator.StringToHash("walk");
    private readonly int _idleStateHash = Animator.StringToHash("idle");
    private readonly int _dieStateHash = Animator.StringToHash("die");
    private readonly int _crouchStateHash = Animator.StringToHash("crouch");
    private readonly int _preJumpStateHash = Animator.StringToHash("preJump");
    private readonly int _midJumpStateHash = Animator.StringToHash("midJump");
    private readonly int _landingStateHash = Animator.StringToHash("landing");
    private readonly int _attackingStateHash = Animator.StringToHash("attacking");

    protected virtual void Awake()
    {
        // _projectile = GameObject.FindWithTag("global");
        _unitAnim = GetComponent<Animator>();
        Grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
        _pathfinding = GameObject.FindWithTag("MainCamera").GetComponent<Pathfinding>();
        _gameController = GameObject.FindWithTag("MainCamera").GetComponent<GameController>();
        _leftScale = _rightScale = transform.localScale;
        _leftScale.x *= -1;
        _rb = GetComponent<Rigidbody2D>();
    }

    protected virtual void Initialize()
    {
    }

    protected virtual void Update()
    {
    }

    //delete units _path, used before switching units and switching turn, function is called from Grid-script
    public void DeletePath()
    {
        if (!_unitMoving)
            _path = null;
    }

    public void RequestPath(Vector2 target)
    {
        if (IsMovementPossible() && GameController.UnitMoving == false)
            PathRequestManager.RequestPath(transform.position, target, ActionPoint, OnPathFound);
    }

    public void SetAttackTarget(Unit targetUnit)
    {
        Node thisUnitNode = GetCurrentNode();
        Node targetUnitNode = targetUnit.GetCurrentNode();
        if (_pathfinding.GetDistance(thisUnitNode, targetUnitNode) <= AttackRange 
            && (targetUnit.GetCurrentNode().GridY == GetCurrentNode().GridY))
            _targetUnit = targetUnit;
        else
            Debug.Log("the unit is out of attack range");
    }

    public void UnsetAttackTarget()
    {
        _targetUnit = null;
    }

    public void AttackTarget()
    {
        if (_targetUnit != null)
        {
            _unitAnim.SetTrigger(_attackHash);
            StartCoroutine("AttackAnimation");
            FMODUnity.RuntimeManager.PlayOneShot(AttackEvent);
            ActionPoint = 0;
        }
    }

    private void AttackDone()
    {
        if (_targetUnit != null)
        {
            _targetUnit.TakeDamage(Ap);
        }
    }

    private IEnumerator AttackAnimation()
    {
        
        bool projectileHit = false;
        _stateInfo = _unitAnim.GetCurrentAnimatorStateInfo(0);
        GameObject currentProjectile = Instantiate((GameObject)Resources.Load("projectile")
            , transform.FindChild("spawnPosition").position
            , Quaternion.identity) as GameObject;
        if (currentProjectile != null)
        {
            currentProjectile.SetActive(true);
            currentProjectile.GetComponent<Renderer>().sortingLayerName = "foreground";
            while (true)
            {
                if (currentProjectile.transform.position != _targetUnit.transform.position)
                {
                    currentProjectile.transform.position =
                        Vector3.MoveTowards(currentProjectile.transform.position, _targetUnit.transform.position, 6*Time.deltaTime);
                }
                else
                {
                    Destroy(currentProjectile);
                    projectileHit = true;
                }
                if ((_stateInfo.shortNameHash != _attackingStateHash) && projectileHit)
                    break;
                yield return null;
            }
        }
        AttackDone();
    }

    public void TakeDamage(int damage)
    {
        if (damage > 0)
        {
            Hp -= damage;
            FMODUnity.RuntimeManager.PlayOneShot(GetHitEvent);
            if (Hp <= 0)
                _gameController.KillUnit(this);
        }
    }

    public virtual void Die()
    {
        _unitAnim.SetTrigger(_killedHash);
        _gameController.TextBoxManager.EventHandler(Unitname, "Die");
        FMODUnity.RuntimeManager.PlayOneShot(DieEvent, transform.position);
    }

    public Node GetCurrentNode()
    {
        return Grid.NodeFromWorldPoint(transform.position);
    }

    public bool IsMovementPossible()
    {
        return ActionPoint > 10;
    }

    public void OnPathFound(List<Node> newPath, bool pathSuccessful, int movementCost)
    {
        if (pathSuccessful)
        {
            //mark _path _succesful
            _succesful = true;
            _path = newPath;
            if (_path.Count > 0)
            {
                DecideFaceDirection(_path[0]);
            }
            _movementCostToDestination = movementCost;
        }
    }

    public void DecideFaceDirection(Node faceTo)
    {
        transform.localScale = faceTo.WorldPosition.x < transform.position.x ? _leftScale : _rightScale;
    }

    public void FlipFaceDirection()
    {
        transform.localScale = transform.localPosition == _leftScale ? _rightScale : _leftScale;
    }

    /// move unit when method is called by gameController, _succesful boolean check before moving
    public List<Node> StartMoving()
    {
        if (_succesful && _path != null)
        {
            StopCoroutine("FollowPath");
            _succesful = false;
            StartCoroutine("FollowPath");
            ActionPoint -= _movementCostToDestination;
            return _path;
        }
        return null;
    }

    public void ReplenishActionPoint()
    {
        ActionPoint = IsDead ? 0 : MaxActionPoint;
    }

    // ReSharper disable once UnusedMember.Local
    private IEnumerator FollowPath()
    {
        GameController.UnitMoving = _unitMoving = true;
        if (_path.Count > 0)
        {
            foreach (Node n in _path)
            {
                _currentWayPoint = n;
                DecideFaceDirection(_currentWayPoint);
                DecideWalkingOrClimb(_currentWayPoint);
                do
                {
                    _stateInfo = _unitAnim.GetCurrentAnimatorStateInfo(0);
                    yield return null;
                } while (_stateInfo.shortNameHash != _climbStateHash
                && _stateInfo.shortNameHash != _walkStateHash);
                while (Vector2.Distance(transform.position, _currentWayPoint.WorldPosition) > 0.1)
                {
                    _stateInfo = _unitAnim.GetCurrentAnimatorStateInfo(0);
                    DetectJumpOrLandCondition(_stateInfo);
                    DecideSpeedAccordingToAnimationState(_stateInfo);
                    transform.position = Vector2.MoveTowards(transform.position,
                       _currentWayPoint.WorldPosition, _speed * Time.deltaTime);
                    yield return null;
                    FMODUnity.RuntimeManager.PlayOneShot(WalkEvent);
                }
                FinishWalkingOrCliming(_unitAnim.GetCurrentAnimatorStateInfo(0));
                do
                {
                    _stateInfo = _unitAnim.GetCurrentAnimatorStateInfo(0);
                    yield return null;
                } while (_stateInfo.shortNameHash == _climbStateHash
                || _stateInfo.shortNameHash == _walkStateHash);
            }
            // When finished moving, clear up 
            _path = new List<Node>();
            GameController.UnitMoving = _unitMoving = false;
            DecideCrouchOrStanding();
            _unitAnim.SetBool(_isWalkingHash, false);
        }
    }

    private void DecideCrouchOrStanding()
    {
        _unitAnim.SetBool(_isWalkingHash, false);
        if (GetCurrentNode().CoveredFromLeft)
        {
            transform.localScale = _rightScale; 
            _unitAnim.SetBool(_undercoverHash, true);
        }
        else if (GetCurrentNode().CoveredFromRight)
        {
            transform.localScale = _leftScale;
            _unitAnim.SetBool(_undercoverHash, true);
        }
        else
        {
            _unitAnim.SetBool(_undercoverHash, false);
        }
    }

    public void DetectJumpOrLandCondition(AnimatorStateInfo state)
    {
        if (GetCurrentNode().JumpThroughable && !GetCurrentNode().Walkable &&
            state.shortNameHash == _walkStateHash)
        {
            // FlipFaceDirection();
            _unitAnim.SetTrigger(_jumpHash);
        } else if (GetCurrentNode().Walkable && state.shortNameHash == _midJumpStateHash)
        {
            // FlipFaceDirection();
            _unitAnim.SetTrigger(_landHash);
            UnapplyJumpPhysics();
        }
        DecideSpeedAccordingToAnimationState(_stateInfo);
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "ground")
        {
            _unitAnim.SetTrigger(_landHash);
            UnapplyJumpPhysics();
            DecideSpeedAccordingToAnimationState(_stateInfo);
        }
    }

    public bool IsHorizontalMovement(Node currentWaypoint)
    {
        return (_currentWayPoint.GridY == GetCurrentNode().GridY);
    }

    public bool IsClimbing(Node currentWayPoint)
    {
        return (currentWayPoint.GridY != GetCurrentNode().GridY) && !GetCurrentNode().JumpThroughable;
    }

    private void DecideWalkingOrClimb(Node currentWayPoint)
    {
        if (IsHorizontalMovement(currentWayPoint))
        {
            _unitAnim.SetBool(_isWalkingHash, true);
        }
        else if (IsClimbing(currentWayPoint))
        {
            _unitAnim.SetTrigger(_goUpLadderHash);
        }
        DecideSpeedAccordingToAnimationState(_unitAnim.GetCurrentAnimatorStateInfo(0));
    }

    private void FinishWalkingOrCliming(AnimatorStateInfo state)
    {
        if (state.shortNameHash == _climbStateHash)
        {
            _unitAnim.SetTrigger(_goOutLadderHash);
        }
        else if (state.shortNameHash == _walkStateHash)
        {
            _unitAnim.SetBool(_isWalkingHash, false);
        }
        DecideSpeedAccordingToAnimationState(_unitAnim.GetCurrentAnimatorStateInfo(0));
    }

    private void ApplyJumpPhysics()
    {
        if (!_underJumpPhysics)
        {
            _underJumpPhysics = true;
            _rb.isKinematic = false;
            _rb.gravityScale = 1;
            _rb.velocity = new Vector2(0, 4.5f);
        }
    }

    private void UnapplyJumpPhysics()
    {
        
        if (_underJumpPhysics)
        {
            Debug.Log("jump unapply");
            _underJumpPhysics = false;
            _rb.isKinematic = true;
            _rb.gravityScale = 0;
        }
    }

    private void DecideSpeedAccordingToAnimationState(AnimatorStateInfo state)
    {
        if (state.shortNameHash == _turnStateHash || state.shortNameHash == _turnBackStateHash ||
            state.shortNameHash == _idleStateHash || state.shortNameHash == _dieStateHash ||
            state.shortNameHash == _crouchStateHash)
        {
            _speed = 0f;
        }
        else if (state.shortNameHash == _preJumpStateHash || state.shortNameHash == _landingStateHash)
        {
            _speed = PrejumpLandingSpeed;
            UnapplyJumpPhysics();
        }
        else if (state.shortNameHash == _midJumpStateHash)
        {
            _speed = JumpSpeed;
            ApplyJumpPhysics();
        }
        else if (state.shortNameHash == _climbStateHash)
        {
            _speed = ClimbingSpeed;
        }
        else if (state.shortNameHash == _walkStateHash)
        {
            _speed = WalkingSpeed;
        }
    }

    public bool HasPath()
    {
        if (_path != null)
            return _path.Count > 0;
        else
            return false;
    }

    public void OnDrawGizmos()
    {
        if (_path != null)
        {
            foreach (Node n in _path)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(n.WorldPosition, new Vector3(0.2f, 0.2f, 0.2f));
                Gizmos.color = Color.green;
                if (_currentWayPoint != null)
                    Gizmos.DrawCube(_currentWayPoint.WorldPosition, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
    }
}
