using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{

	private const int RECALCULATE_PATH_COOLDOWN = 1;

	[Header("Attack")]
	[SerializeField] float minRange;
	[SerializeField] float maxRange;
	[SerializeField] float lineOfSight;
	[SerializeField] float damage;
	[SerializeField] float attackCooldown;




	[SerializeField]
	[Range(0.1f, 10)]
	private float speed;

	[SerializeField] Team team;

	private Formation _formation;

	private Vector2 position;
	private Vector3 size;

	private IDamageable target;
	private bool isAttacking;

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

	public float Damage
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




	public bool IsMoving { get; protected set; }


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
		this.target = u;
		this.isAttacking = true;
		this.MoveTo(u.Position);
		this.lastTimePathCalculated = Time.time;
	}

	public void StopAttack()
	{
		this.StopMovement();
		this.isAttacking = false;
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
		if(this.isAttacking)
			HandleAttack();
	}

	private void HandleAttack()
	{
		if(Utils.SqrDistance(this.Position, target.Position) > this.MaxRange * this.MaxRange)
		{
			if(this.lastTimePathCalculated + Unit.RECALCULATE_PATH_COOLDOWN < Time.time || !this.IsMoving)
			{
				this.MoveTo(this.target.Position);
				this.lastTimePathCalculated = Time.time;
			}
		}
		else if(Utils.SqrDistance(this.Position, target.Position) < this.MinRange * this.MinRange)
		{
			if (this.lastTimePathCalculated + Unit.RECALCULATE_PATH_COOLDOWN < Time.time || !this.IsMoving)
			{
				Vector2 dir = (this.Position - this.target.Position).normalized;
				this.MoveTo(this.target.Position + dir * this.MaxRange);
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
}


