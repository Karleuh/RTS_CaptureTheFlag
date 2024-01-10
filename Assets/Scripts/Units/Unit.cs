using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
	[SerializeField] float minRange;
	[SerializeField] float maxRange;
	[SerializeField] float damage;
	IDamageable target;

	[SerializeField] Team team;

	public float MinRange
	{
		get { return this.minRange; }

	}

	public float MaxRange
	{
		get { return this.maxRange; }

	}

	public float Damage
	{
		get { return this.damage; }
	}



	[SerializeField]
	[Range(0.1f, 10)]
	private float speed;

	private Formation _formation;

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

	private Vector2 position;
	private Vector3 size;

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
	public abstract void Attack(Unit u);

	protected virtual void FixedUpdate()
	{
		ChunkPos oldcp = this.ChunkPosition;
		this.ChunkPosition = new ChunkPos(this.Position, UnitManager.chunckSize);
		if (this.ChunkPosition != oldcp)
		{
			UnitManager.OnUnitMove(this, oldcp);
		}
	}
}

