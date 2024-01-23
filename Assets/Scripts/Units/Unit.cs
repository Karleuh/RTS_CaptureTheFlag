using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{

	private const int RECALCULATE_PATH_COOLDOWN = 1;

	[Header("Attack")]
	[SerializeField] protected float minRange;
	[SerializeField] protected float maxRange;
	[SerializeField] protected float lineOfSight;
	[SerializeField] protected int damage;
	[SerializeField] protected float attackCooldown;






	[SerializeField]
	[Range(0.1f, 10)]
	private float speed;

	[SerializeField] protected Team team;

	private Formation _formation;

	private Vector2 position;
	private Vector3 size;
	protected bool canMoveWhileAttacking = true;

	public IDamageable DamageableTarget { get; private set; }

	float lastTimePathCalculated;



	Queue<UnitAction> unitActions = new Queue<UnitAction>();

	public float MinRange
	{
		get => this.minRange;

	}

	public float MaxRange
	{
		get => this.maxRange;

	}

	public int Damage
	{
		get => this.damage;
	}

	public float AttackCooldown
	{
		get => this.attackCooldown;
	}

	public float LineOfSight
	{
		get => this.lineOfSight;
	}

	public UnitActionType UnitActionType
	{
		get => this.unitActions.Count == 0 ? UnitActionType.NONE : this.unitActions.Peek().UnitActionType;
	}

	public Formation Formation
	{
		protected get => this._formation;
		set
		{
			if (this._formation != null)
				this._formation.RemoveUnit(this);
			this._formation = value;
		}
	}

	public void LeaveFormationWithoutNotification()
	{
		this._formation = null;
	}

	public float Speed
	{
		get => this.speed;
		protected set => this.speed = value;
	}

	public bool IsWaitingForAction { get => this.unitActions.Count == 0; }

	private bool _isMoving;
	public bool IsMoving { get; protected set; }
	public bool IsAttacking { get; private set; }


	public virtual Vector2 Position
	{
		get => this.position;
		set => this.position = value;
	}

	public ChunkPos ChunkPosition
	{
		get;
		private set;
	}

	public Team Team { get => this.team; private set => this.team = value; }

	public virtual bool IsSelectable { get; }
	public abstract int Weight { get; }

	protected virtual void Start()
	{
		this.Position = new Vector2(this.transform.position.x, this.transform.position.z);
		this.ChunkPosition = new ChunkPos(this.Position, UnitManager.chunckSize);
		UnitManager.RegisterUnit(this);
	}

	public abstract void MoveTo(Vector2 pos, bool isCheckpoint = false);
	public abstract void StopMovement();



	public void Attack(IDamageable u)
	{
		this.DamageableTarget = u;
		this.IsAttacking = true;
		this.MoveTo(u.Position);
		this.lastTimePathCalculated = Time.time;
	}

	public bool IsTargetInRange()
	{
		if (!this.IsAttacking)
			return false;
		float sqrdist = Utils.SqrDistance(this.DamageableTarget.Position, this.Position);
		return sqrdist > this.MinRange * this.MinRange && sqrdist < this.MaxRange * this.MaxRange;
	}

	public virtual void StopAttack()
	{
		this.StopMovement();
		this.IsAttacking = false;
	}


	public void StopAll()
	{
		this.StopMovement();
		this.IsAttacking = false;
	}

	protected virtual void FixedUpdate()
	{
		//position

		ChunkPos oldcp = this.ChunkPosition;
		this.ChunkPosition = new ChunkPos(this.Position, UnitManager.chunckSize);
		if (this.ChunkPosition != oldcp)
		{
			UnitManager.OnUnitMove(this, oldcp);
		}

		//actions
		if (this.unitActions.Count > 0 && this.unitActions.Peek().IsFinished)
			this.unitActions.Dequeue();

		if (this.unitActions.Count > 0)
			this.unitActions.Peek().FixedUpdate();


		//attack
		if(this.IsAttacking)
			HandleAttack();
	}

	private void HandleAttack()
	{
		if (this.DamageableTarget == null || this.DamageableTarget.IsDead)
			this.StopAttack();
		else if((!this.IsWaitingForAction || this.canMoveWhileAttacking) && Utils.SqrDistance(this.Position, DamageableTarget.Position) > this.MaxRange * this.MaxRange)
		{
			if(this.lastTimePathCalculated + Unit.RECALCULATE_PATH_COOLDOWN < Time.time || !this.IsMoving)
			{
				this.MoveTo(this.DamageableTarget.Position);
				this.lastTimePathCalculated = Time.time;
			}
		}
		else if((!this.IsWaitingForAction || this.canMoveWhileAttacking) && Utils.SqrDistance(this.Position, DamageableTarget.Position) < this.MinRange * this.MinRange)
		{
			if (this.lastTimePathCalculated + Unit.RECALCULATE_PATH_COOLDOWN < Time.time || !this.IsMoving)
			{
				Vector2 dir = (this.Position - this.DamageableTarget.Position).normalized;
				this.MoveTo(this.DamageableTarget.Position + dir * this.MaxRange);
				this.lastTimePathCalculated = Time.time;
			}
		}
		else if(this.IsMoving)
		{
			this.StopMovement();
		}
	}



	public bool EnqueueMove(Vector2 target)
	{
		if (this.unitActions.Count == 0)
			return false;
		return this.unitActions.Peek().EnqueueMove(target);
	}


	public bool EnqueueAttack(IDamageable target)
	{
		if (this.unitActions.Count == 0)
			return false;
		return this.unitActions.Peek().EnqueueAttack(target);
	}

	public void EnqueueAction(UnitAction action, bool clear)
	{
		if (clear)
		{
			this.unitActions.Clear();
			this.StopAttack();
			this.StopMovement();
		}

		this.unitActions.Enqueue(action);
	}


	public IDamageable FindClosestUnitInLineOfSight(bool random = false)
	{
		IDamageable u = null;
		List<IDamageable> units = UnitManager.OverlapCircleUnitDamageable(this.Position, this.LineOfSight, this.Team == Team.ATTACKER ? Team.DEFENDER : Team.ATTACKER);

		if (units.Count > 0)
		{
			if (random)
				return units[UnityEngine.Random.Range(0, units.Count)];

			//find closest unit
			float minDistance = float.MaxValue;
			for (int i = 0; i < units.Count; i++)
			{
				float dist = Utils.SqrDistance(units[i].Position, this.Position);
				if (dist < minDistance)
				{
					minDistance = dist;
					u = units[i];
				}
			}


		}

		return u;
	}
}


public enum Stance
{
	AGGRESSIVE,
	DEFENSIVE,
	STAND_GROUND,
	NO_ATTACK
}


