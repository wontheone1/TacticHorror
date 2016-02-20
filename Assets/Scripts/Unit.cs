using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
public class Unit : MonoBehaviour
{
    /// <summary>
    /// store FMOD events
    /// </summary>
    //[FMODUnity.EventRef]
    protected string LadderUpdownEvent, WalkEvent, DieEvent, GetHitEvent, AttackEvent, JumpEvent;

    //Animator
    private AnimatorStateInfo _stateInfo;
    private Animator _unitAnim;
    public static int UnitCount = 0;
    public string Unitname;
    private const float WalkingSpeed = 4f;
    private const float ClimbingSpeed = 2.5f;
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
    private bool _unitMoving;
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

    protected virtual void Awake()
    {
        _unitAnim = GetComponent<Animator>();
        Grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
        _pathfinding = GameObject.FindWithTag("MainCamera").GetComponent<Pathfinding>();
        _gameController = GameObject.FindWithTag("MainCamera").GetComponent<GameController>();
        _leftScale = _rightScale = transform.localScale;
        _leftScale.x *= -1;
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
        if (_pathfinding.GetDistance(thisUnitNode, targetUnitNode) <= AttackRange)
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
            _targetUnit.TakeDamage(Ap);
            FMODUnity.RuntimeManager.PlayOneShot(AttackEvent);
            ActionPoint = 0;
        }
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
        ActionPoint = MaxActionPoint;
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
                    DetectJumpOrLandCondition(_unitAnim.GetCurrentAnimatorStateInfo(0));
                    DecideSpeedAccordingToAnimationState(_unitAnim.GetCurrentAnimatorStateInfo(0));
                    _stateInfo = _unitAnim.GetCurrentAnimatorStateInfo(0);
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
            _unitAnim.SetBool(_isWalkingHash, false);
        }
    }

    public void DetectJumpOrLandCondition(AnimatorStateInfo state)
    {
        
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

    private void DecideSpeedAccordingToAnimationState(AnimatorStateInfo state)
    {
        if (state.shortNameHash == _turnStateHash ||
            state.shortNameHash == _turnBackStateHash ||
            state.shortNameHash == _idleStateHash)
        {
            _speed = 0f;
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
