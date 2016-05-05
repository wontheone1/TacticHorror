using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class UnitController : MonoBehaviour
{
    private AnimatorStateInfo _stateInfo;
    public Unit Unit;
    public Animator UnitAnim;
    private Pathfinding _pathfinding;
    private GameController _gameController;
    private Grid _grid;
	public Text HitText;
	public Text HitChanceText;

    protected virtual void Awake()
    {
		HitText = GameObject.Find("HitText").GetComponent<Text>();
		HitChanceText = GameObject.Find("HitChanceText").GetComponent<Text>();
		HitText.text = "";
		HitChanceText.text = "";
        _pathfinding = GameObject.FindWithTag("MainCamera").GetComponent<Pathfinding>();
        _gameController = GameObject.FindWithTag("MainCamera").GetComponent<GameController>();
        _grid = GameObject.FindWithTag("MainCamera").GetComponent<Grid>();
    }

    public void RequestPath(Vector2 target)
    {
        if (Unit.IsMovementPossible() && GameController.UnitMoving == false)
            PathRequestManager.RequestPath(transform.position, target, Unit.ActionPoint, OnPathFound);
        UnsetAttackTarget();
    }

    public void OnPathFound(List<Node> newPath, bool pathSuccessful, int movementCost)
    {
        if (pathSuccessful)
        {
            //mark _path _succesful
            Unit.Succesful = true;
            Unit.Path = newPath;
            if (Unit.Path.Count > 0)
            {
                DecideFaceDirection(Unit.Path[0]);
            }
            Unit.MovementCostToDestination = movementCost;
        }
    }

    /// <summary>
    /// if a unit is in attack range and on the same floor, set TargetUnit 
    /// </summary>
    /// <param name="targetUnit"></param>
    public void SetAttackTarget(Unit targetUnit)
    {
        Node thisUnitNode = Unit.GetCurrentNode();
        Node targetUnitNode = targetUnit.GetCurrentNode();
        if (_pathfinding.GetDistance(thisUnitNode, targetUnitNode) <= Unit.AttackRange
            && (targetUnit.GetCurrentNode().GridY == Unit.GetCurrentNode().GridY))
        {
            Unit.TargetUnit = targetUnit;
            DecideFaceDirection(Unit.TargetUnit.GetCurrentNode());
			missChanceCalculation ();
        }
        else
        {
            _gameController.DebugText.text = "The unit is out of range.";
            UnsetAttackTarget();
        }
    }

    public void UnsetAttackTarget()
    {
        Unit.TargetUnit = null;
		HitChanceText.text = "";
    }

    /// <summary>
    /// start AttackAnimation Coroutine 
    /// </summary>
    public void AttackTarget()
    {
        if (Unit.TargetUnit != null && Unit.ActionPoint != 0)
        {
            GameController.UnitMoving = Unit.UnitMoving = true;
            StartCoroutine("AttackAnimation");
            FMODUnity.RuntimeManager.PlayOneShot(Unit.AttackEvent);
            Unit.ActionPoint = 0;
            UpdateGreenBar();
        }
    }

    /// <summary>
    /// when attack is done, apply damage to the TargetUnit and select next unit or end turn
    /// </summary>
    private void AttackDone()
    {
        if (Unit.TargetUnit != null)
        {
			if (missChanceCalculation ()) {
				Unit.TargetUnit.TakeDamage (randomizeDamage ());
			} 
			else {
				HitChanceText.text = "";
				HitText.text = "Attack missed";
				StartCoroutine (RemoveText (HitText));

			}
        }
        GameController.UnitMoving = Unit.UnitMoving = false;
        if (!_gameController.SelectNextUnit())
            _gameController.EndTurn();
    }
	IEnumerator RemoveText(Text text){
		yield return new WaitForSeconds (2);
		text.text = "";
	}


	private int randomizeDamage(){
		System.Random rnd = new System.Random ();
		int rngDamage = rnd.Next(Unit.ApMin, Unit.ApMax);
		HitChanceText.text = "";
		HitText.text = "Attack damage was "+rngDamage;
		StartCoroutine (RemoveText (HitText));

		Debug.Log("Randomized damage was: " + rngDamage);
		return rngDamage;
	}





	private bool missChanceCalculation(){

		int attackingDistance = _pathfinding.GetDistance(Unit.GetCurrentNode(), Unit.TargetUnit.GetCurrentNode());
		Debug.Log ("target distance: " + attackingDistance);
		int chanceToHit = 100 - attackingDistance;

		//checking if target is in cover
		if ((Unit.TargetUnit.inCover == true) && !((Unit.transform.localScale == Unit.RightScale && Unit.TargetUnit.transform.localScale == Unit.TargetUnit.RightScale) || 
			(Unit.transform.localScale == Unit.LeftScale && Unit.TargetUnit.transform.localScale == Unit.TargetUnit.LeftScale))) {
			chanceToHit = chanceToHit / 2;
			Debug.Log ("target is in cover");
		}
		HitChanceText.text = "chance to hit: " + chanceToHit;

		//randomizing a number between 0 and 100, attack hits if chanceToHit is higher than randomized number
		System.Random rnd = new System.Random ();
		int rngChance = rnd.Next (0, 100);
		Debug.Log ("rnd chance: " + rngChance);

		if (chanceToHit >= rngChance) {
			return true;
		}
		else {
			Debug.Log ("Attack missed");
			return false;
		}

	}

    // ReSharper disable once UnusedMember.Local
    /// <summary>
    /// summon projectile when throwing animation is done, call AttackDone() when projectile reached TargetUnit
    /// </summary>
    /// <returns></returns>
    private IEnumerator AttackAnimation()
    {
        UnitAnim.SetTrigger(Unit.AttackHash);
        bool projectileHit = false;
        while (true)
        {
            // wait until attack animation ends and follow_attack state starts
            if (UnitAnim.GetCurrentAnimatorStateInfo(0).shortNameHash == Unit.FollowAttackStateHash)
                break;
            yield return null;
        }
        GameObject currentProjectile = Instantiate((GameObject)Resources.Load(Unit.ProjectileName)
            , Unit.transform.FindChild("spawnPosition").position
            , Quaternion.identity) as GameObject;
        if (currentProjectile != null)
        {
            Vector3 moveTo;
            // move currentProjectile to the TargetUnit every frame until it hits 
            while (true)
            {
                if (currentProjectile.transform.position != Unit.TargetUnit.transform.position)
                {
                    currentProjectile.transform.position =
                        Vector3.MoveTowards(currentProjectile.transform.position, Unit.TargetUnit.transform.position, 6 * Time.deltaTime);
                    // Camera follows currentProjectile
                    moveTo = new Vector3(currentProjectile.transform.position.x, currentProjectile.transform.position.y,
                        Camera.main.transform.position.z);
                    Camera.main.transform.position = moveTo;
                }
                else
                {
                    Destroy(currentProjectile);
                    projectileHit = true;
                }
                if (projectileHit)
                    break;
                yield return null;
            }
        }
        AttackDone();
    }

    // apply damage and kill it when Hp <= 0
    public void TakeDamage(int damage)
    {
        if (damage > 0)
        {
            Unit.Hp -= damage;
            Mathf.Clamp(Unit.Hp, 0, Unit.MaxHp);
            if (Unit.HealthBar != null)
                Unit.HealthBar.FillBar(Unit.Hp);
            FMODUnity.RuntimeManager.PlayOneShot(Unit.GetHitEvent);
            UnitAnim.SetTrigger(Unit.GetHitHash);
            if (Unit.Hp <= 0)
                _gameController.KillUnit(Unit);
        }
    }

    // trigger die animation and call Dialogue event handler
    public virtual void Die()
    {
        Unit.GetCurrentNode().Occupied = false;
        UnitAnim.SetTrigger(Unit.KilledHash);
        Unit.GetComponent<BoxCollider2D>().enabled = false;
        _gameController.TextBoxManager.EventHandler(Unit.Unitname, "Die");
        FMODUnity.RuntimeManager.PlayOneShot(Unit.DieEvent);
    }

    public List<Node> StartMoving()
    {
        if (Unit.Succesful && Unit.Path != null)
        {
            Unit.Succesful = false;
            StartCoroutine("FollowPath");
            Unit.ActionPoint -= Unit.MovementCostToDestination;
            UpdateGreenBar();
            return Unit.Path;
        }
        return null;
    }


    // ReSharper disable once UnusedMember.Local
    /// <summary>
    /// move unit every frame and decide the animation according to the situation
    /// </summary>
    private IEnumerator FollowPath()
    {
        GameController.UnitMoving = Unit.UnitMoving = true;
        // Free the tile from Occupied state
        Unit.GetCurrentNode().Occupied = false;
        // ReSharper disable once ForCanBeConvertedToForeach
        for (int i = 0; i < Unit.Path.Count; i++)
        {
            Unit.CurrentWayPoint = Unit.Path[i];
            DecideFaceDirection(Unit.CurrentWayPoint);
            DecideWalkingOrClimbOrJump(Unit.CurrentWayPoint);
            Vector3 moveToward;
            while (true)
            {
                // if turn, or turnBack state, wait until the state finishes without moving
                do
                {
                    _stateInfo = UnitAnim.GetCurrentAnimatorStateInfo(0);
                    yield return null;
                } while (_stateInfo.shortNameHash == Unit.TurnStateHash
                && _stateInfo.shortNameHash == Unit.TurnBackStateHash);

                _stateInfo = UnitAnim.GetCurrentAnimatorStateInfo(0);
                DecideSpeedAccordingToAnimationState(_stateInfo);
                // if walkState, move only in x direction
                if (_stateInfo.shortNameHash == Unit.WalkStateHash)
                {
                    moveToward = new Vector2(Unit.CurrentWayPoint.WorldPosition.x, Unit.transform.position.y);
                    while (Unit.transform.position != moveToward)
                    {
                        Unit.transform.position = Vector2.MoveTowards(Unit.transform.position,
                   moveToward, Unit.Speed * Time.deltaTime);
                        yield return null;
                    }
                    break;
                }
                // if climbState, move only in y direction
                if (_stateInfo.shortNameHash == Unit.ClimbStateHash)
                {
                    moveToward = new Vector2(Unit.transform.position.x, Unit.CurrentWayPoint.WorldPosition.y);
                    while (Unit.transform.position != moveToward)
                    {
                        Unit.transform.position = Vector2.MoveTowards(Unit.transform.position,
                   moveToward, Unit.Speed * Time.deltaTime);
                        yield return null;
                    }
                    break;
                }
                // if JumpFalling or JumpRising move to the landing point while applying custom gravity effect
                if (_stateInfo.shortNameHash == Unit.JumpFallingStateHash ||
                    _stateInfo.shortNameHash == Unit.JumpRisingStateHash)
                {
                    moveToward = new Vector2(Unit.CurrentWayPoint.WorldPosition.x, Unit.transform.position.y);
                    float yVelocity = 1.5f;
                    while ((Math.Abs(Unit.transform.position.x - Unit.CurrentWayPoint.WorldPosition.x) > 0.1f)
                        || (Math.Abs(Unit.transform.position.y - Unit.CurrentWayPoint.WorldPosition.y) > 0.1f))
                    {
                        moveToward = new Vector2(moveToward.x, moveToward.y + yVelocity);

                        // prevent it from falling
                        if (moveToward.y < Unit.CurrentWayPoint.WorldPosition.y)
                            moveToward.y = Unit.CurrentWayPoint.WorldPosition.y;

                        Unit.transform.position = Vector2.MoveTowards(Unit.transform.position,
                   moveToward, (float)Math.Sqrt((Unit.Speed * Unit.Speed) + (yVelocity * yVelocity)) * Time.deltaTime);
                        // adjust unit position to current way point a little bit
                        Unit.transform.position = Vector2.MoveTowards(Unit.transform.position,
                   Unit.CurrentWayPoint.WorldPosition, 0.1f * Time.deltaTime);
                        // trigger land when near landing point
                        if (Vector2.Distance(Unit.transform.position, Unit.CurrentWayPoint.WorldPosition) < 1)
                        {
                            UnitAnim.SetTrigger(Unit.LandHash);
                            Unit.Speed = Unit.LandingSpeed;
                        }
                        // negative acceleration (gravity)
                        yVelocity -= Time.deltaTime * 8;
                        if (yVelocity < 0)
                            UnitAnim.SetTrigger(Unit.StartedFallingHash);
                        yield return null;
                    }
                    Unit.GetCurrentNode().ToJumpTo = false; // clear up the path JumpTo property
                    do
                    {
                        // Don't let it bounce when jump cross and walk
                        yield return new WaitForSeconds(1);
                        _stateInfo = UnitAnim.GetCurrentAnimatorStateInfo(0);
                        Unit.transform.position = Unit.GetCurrentNode().WorldPosition;
                        Unit.Speed = 0f;
                        yield return null;
                    } while (_stateInfo.shortNameHash == Unit.LandingStateHash);
                    break;
                }
                // during prejump state, it slows down and only move to X-direction
                while (_stateInfo.shortNameHash == Unit.PreJumpStateHash)
                {
                    _stateInfo = UnitAnim.GetCurrentAnimatorStateInfo(0);
                    Unit.transform.position = Vector2.MoveTowards(Unit.transform.position,
                   new Vector2(Unit.CurrentWayPoint.WorldPosition.x, Unit.transform.position.y), Unit.Speed * Time.deltaTime);
                    yield return null;
                }
                FMODUnity.RuntimeManager.PlayOneShot(Unit.WalkEvent);
            }
            FinishWalkingOrCliming(UnitAnim.GetCurrentAnimatorStateInfo(0));
        }
        Unit.Path = new List<Node>();
        GameController.UnitMoving = Unit.UnitMoving = false;
        DecideCrouchOrStanding();
        Unit.GetCurrentNode().Occupied = true;
        _grid.DrawFOW();
    }

    /// <summary>
    /// check if its moving from ladder to ladder and to the different floor
    /// </summary>
    /// <param name="currentWayPoint"></param>
    /// <returns>bool</returns>
    public bool IsClimbing(Node currentWayPoint)
    {
        return currentWayPoint.OnLadder && Unit.GetCurrentNode().OnLadder && (currentWayPoint.GridY != Unit.GetCurrentNode().GridY);
    }

    private void DecideWalkingOrClimbOrJump(Node currentWayPoint)
    {
        UnitAnim.SetBool(Unit.UndercoverHash, false);
        if (IsClimbing(currentWayPoint))
        {
            UnitAnim.SetTrigger(Unit.GoUpLadderHash);
        }
        else if (IsJumping(currentWayPoint))
        {
            UnitAnim.SetTrigger(Unit.JumpHash);
            UnitAnim.SetBool(Unit.IsWalkingHash, false);
        }
        else
        {
            UnitAnim.SetBool(Unit.IsWalkingHash, true);
        }
        DecideSpeedAccordingToAnimationState(UnitAnim.GetCurrentAnimatorStateInfo(0));
    }

    private bool IsJumping(Node currentWayPoint)
    {
        return currentWayPoint.ToJumpTo;
    }

    private void FinishWalkingOrCliming(AnimatorStateInfo state)
    {
        if (state.shortNameHash == Unit.ClimbStateHash)
        {
            UnitAnim.SetTrigger(Unit.GoOutLadderHash);
        }
        else if (state.shortNameHash == Unit.WalkStateHash)
        {
            UnitAnim.SetBool(Unit.IsWalkingHash, false);
        }
        DecideSpeedAccordingToAnimationState(UnitAnim.GetCurrentAnimatorStateInfo(0));
    }

    private void DecideSpeedAccordingToAnimationState(AnimatorStateInfo state)
    {
        if (state.shortNameHash == Unit.TurnStateHash || state.shortNameHash == Unit.TurnBackStateHash ||
            state.shortNameHash == Unit.IdleStateHash || state.shortNameHash == Unit.DieStateHash ||
            state.shortNameHash == Unit.CrouchStateHash)
        {
            Unit.Speed = 0f;
        }
        else if (state.shortNameHash == Unit.PreJumpStateHash)
        {
            Unit.Speed = Unit.PrejumpSpeed;
            // UnapplyJumpPhysics();
        }
        else if (state.shortNameHash == Unit.JumpRisingStateHash 
            || state.shortNameHash == Unit.JumpFallingStateHash)
        {
            Unit.Speed = Unit.MidJumpLandingSpeed;
            UnitAnim.SetBool(Unit.IsWalkingHash, false);
            // ApplyJumpPhysics();
        }
        else if (state.shortNameHash == Unit.LandingStateHash)
        {
            Unit.Speed = Unit.MidJumpLandingSpeed;
            // UnapplyJumpPhysics();
        }
        else if (state.shortNameHash == Unit.ClimbStateHash)
        {
            Unit.Speed = Unit.ClimbingSpeed;
        }
        else if (state.shortNameHash == Unit.WalkStateHash)
        {
            Unit.Speed = Unit.WalkingSpeed;
        }
    }

    /// <summary>
    /// Decide if a unit should be Crouching or standing depending on if the tile is covered
    /// </summary>
    public void DecideCrouchOrStanding()
    {
        UnitAnim.SetBool(Unit.IsWalkingHash, false);
        if (Unit.GetCurrentNode().CoveredFromLeft)
        {
			Unit.inCover = true;
            Unit.transform.localScale = Unit.LeftScale;
            UnitAnim.SetBool(Unit.UndercoverHash, true);

        }
        else if (Unit.GetCurrentNode().CoveredFromRight)
        {
			Unit.inCover = true;
            Unit.transform.localScale = Unit.RightScale;
            UnitAnim.SetBool(Unit.UndercoverHash, true);

        }
        else
        {
			Unit.inCover = false;
            UnitAnim.SetBool(Unit.UndercoverHash, false);

        }
    }

    public void DecideFaceDirection(Node faceTo)
    {
        Unit.transform.localScale = faceTo.WorldPosition.x < Unit.transform.position.x ? Unit.LeftScale : Unit.RightScale;
        _grid.DrawFOW();
    }

    public void FlipFaceDirection()
    {
        Unit.transform.localScale = Unit.transform.localPosition == Unit.LeftScale ? Unit.RightScale : Unit.LeftScale;
    }

    public void UpdateGreenBar()
    {
        if (Unit.GreenBar != null)
            Unit.GreenBar.FillBar(Unit.ActionPoint/10);
    }
}
