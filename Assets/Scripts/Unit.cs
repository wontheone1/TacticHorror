using UnityEngine;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
public class Unit : MonoBehaviour
{
    /// <summary>
    /// store FMOD events
    /// </summary>
    //[FMODUnity.EventRef]
    public string LadderUpdownEvent, WalkEvent, DieEvent, GetHitEvent, AttackEvent, JumpEvent;

    //Animation
    public readonly float WalkingSpeed = 4.5f;
    public readonly float ClimbingSpeed = 3f;
    public readonly float PrejumpSpeed = 1f;
    public readonly float MidJumpLandingSpeed = 5f;
    public readonly float LandingSpeed = 1f;

    // general variables
    // public static int UnitCount = 0;
    public string Unitname;
    public bool IsDead;
    public Unit TargetUnit;
    public bool UnitMoving;
    public List<Node> Path;
    public Node CurrentWayPoint;
    public bool Succesful;
    public float Speed; // _speed for animation
    public int MovementCostToDestination;
    protected string projectileName = "rock";
    private Vector3 _rightScale, _leftScale;

    // Stats 
    protected int _maxActionPoint; //movement point + other action
    protected int _maxHp; // health point
    protected int _attackRange; //attack Range
    public int ActionPoint; //movement point + other action
    public int Hp; // health point
    protected int _ap; // attack point

    // other objects
    protected Grid Grid;
    public HealthBar HealthBar { get; set; }
    public GreenBar GreenBar { get; set; }
    private UnitController _unitController;

    protected int MaxActionPoint
    {
        get { return _maxActionPoint; }
    }

    public int MaxHp
    {
        get { return _maxHp; }
    }

    public int AttackRange
    {
        get { return _attackRange; }
    }

    public Vector3 RightScale
    {
        get { return _rightScale; }
    }

    public Vector3 LeftScale
    {
        get { return _leftScale; }
    }

    public int Ap
    {
        get { return _ap; }
    }

    public UnitController UnitController
    {
        get { return _unitController; }
    }

    public string ProjectileName
    {
        get { return projectileName; }
    }
    

    /// <summary>
    /// string Hash for animators... (optimization)
    /// </summary>
    public readonly int AttackHash = Animator.StringToHash("attack");
    public readonly int JumpHash = Animator.StringToHash("jump");
    public readonly int LandHash = Animator.StringToHash("land");
    public readonly int KilledHash = Animator.StringToHash("killed");
    public readonly int UndercoverHash = Animator.StringToHash("undercover");
    public readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    public readonly int GoUpLadderHash = Animator.StringToHash("goUpLadder");
    public readonly int GoOutLadderHash = Animator.StringToHash("goOutLadder");
    public readonly int TurnStateHash = Animator.StringToHash("turn");
    public readonly int TurnBackStateHash = Animator.StringToHash("turnBack");
    public readonly int ClimbStateHash = Animator.StringToHash("climb");
    public readonly int WalkStateHash = Animator.StringToHash("walk");
    public readonly int IdleStateHash = Animator.StringToHash("idle");
    public readonly int DieStateHash = Animator.StringToHash("die");
    public readonly int CrouchStateHash = Animator.StringToHash("crouch");
    public readonly int PreJumpStateHash = Animator.StringToHash("preJump");
    public readonly int MidJumpStateHash = Animator.StringToHash("midJump");
    public readonly int LandingStateHash = Animator.StringToHash("landing");
    // private readonly int _attackingStateHash = Animator.StringToHash("attacking");
    public readonly int FollowAttackStateHash = Animator.StringToHash("followAttack");

    protected virtual void Awake()
    {
        _unitController = gameObject.AddComponent<UnitController>();
        _unitController.Unit = this;
        _unitController.UnitAnim = this.GetComponent<Animator>();
        Grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
        _leftScale = _rightScale = transform.localScale;
        _leftScale.x *= -1;
    }

    protected virtual void Start()
    {
        if (HealthBar != null)
            HealthBar.FillBar(Hp);
        UnitController.UpdateGreenBar();
    }
    //protected virtual void Update()
    //{
    //}

    //delete units _path, used before switching units and switching turn, function is called from Grid-script
    public void DeletePath()
    {
        if (!UnitMoving)
            Path = null;
    }

    public void RequestPath(Vector2 target)
    {
        UnitController.RequestPath(target);
    }

    public void SetAttackTarget(Unit targetUnit)
    {
        _unitController.SetAttackTarget(targetUnit);
    }

    public void AttackTarget()
    {
        _unitController.AttackTarget();
    }

    public void TakeDamage(int damage)
    {
        _unitController.TakeDamage(damage);
    }

    public virtual void Die()
    {
        _unitController.Die();
    }

    public Node GetCurrentNode()
    {
        return Grid.NodeFromWorldPoint(transform.position);
    }

    public bool IsMovementPossible()
    {
        return ActionPoint > 10;
    }

    public List<Node> StartMoving()
    {
        return _unitController.StartMoving();
    }

    public void ReplenishActionPoint()
    {
        ActionPoint = IsDead ? 0 : _maxActionPoint;
        UnitController.UpdateGreenBar();
    }

    public bool HasPath()
    {
        if (Path != null)
            return Path.Count > 0;
        else
            return false;
    }

    public void OnDrawGizmos()
    {
        if (Path != null)
        {
            foreach (Node n in Path)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(n.WorldPosition, new Vector3(0.2f, 0.2f, 0.2f));
                Gizmos.color = Color.green;
                if (CurrentWayPoint != null)
                    Gizmos.DrawCube(CurrentWayPoint.WorldPosition, new Vector3(0.3f, 0.3f, 0.3f));
            }
        }
    }
}
