using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicUnit : Unit, IDamageable
{
	[SerializeField] private Stance stance;

	[Header("Health")]
	[SerializeField]
	float maxHealth = 100;
	[SerializeField] HealthBar healthBar;

	[Header("Audio")]
	[SerializeField] private AudioClip hitAudio;
	[SerializeField] private AudioClip deathAudio;
	[SerializeField] private AudioSource audioSource;

	[Header("Animation")]
	[SerializeField] protected Animation anim;
	[SerializeField] String dieAnimation;


	float health;
	float lastTimeUnitsChecked;
	float timeDie;

	private const int CHECK_FOR_UNITS_COOLDOWN = 1;
	private const float DIE_COOLDOWN = 1.5f;
	private const int MAXDISTANCE_DEFENSIVE_STAND = 10;


	Vector2 defensivePosition;


	//List<Vector2Int> checkpoints = new List<Vector2Int>();
	SimpleConcatLinkedList<Vector2Int> checkpoints = new SimpleConcatLinkedList<Vector2Int>();
	Vector2 target;
	float timeSinceFormationSpotInObstacle;
	public bool IsDead => this.health <= 0;

	public Stance Stance
	{
		get => this.stance;
		set
		{
			this.stance = value;
			this.StopAttack();
		}
	}


	public override void MoveTo(Vector2 pos, bool isCheckpoint = false)
	{
		Vector2 prevTarget = this.target;
		bool isTargetInObstacle = Vector2Int.FloorToInt(pos) != Terrain.instance.GetClosestAccessiblePos(Vector2Int.FloorToInt(pos));
		this.target = !isTargetInObstacle ? pos : Terrain.instance.GetClosestAccessiblePos(Vector2Int.FloorToInt(pos)) + new Vector2(0.5f, 0.5f);
		this.IsMoving = true;

		if (!isCheckpoint)
		{
			this.checkpoints.Clear();

			if ((this.target - this.Position).sqrMagnitude > 2)
				AStar.A_Star(this.Position, this.target, this.checkpoints);
		}
		else
		{
			if ((this.target - this.Position).sqrMagnitude > 2)
			{


				if (isTargetInObstacle && this.Formation != null)
				{
					if (Time.time > this.timeSinceFormationSpotInObstacle + 1)
					{
						this.checkpoints.Clear();
						AStar.A_Star(this.Position, this.target, this.checkpoints);
						this.timeSinceFormationSpotInObstacle = Time.time;
					}
					else
					{
						SimpleConcatLinkedList<Vector2Int> temp = new SimpleConcatLinkedList<Vector2Int>();
						AStar.A_Star(prevTarget, this.target, temp);
						this.checkpoints.ConcatBefore(temp);
					}
				}
				else
				{
					if (this.timeSinceFormationSpotInObstacle != 0)
					{
						this.checkpoints.Clear();
						AStar.A_Star(this.Position, this.target, this.checkpoints);
						this.timeSinceFormationSpotInObstacle = 0;
					}
					else
					{
						SimpleConcatLinkedList<Vector2Int> temp = new SimpleConcatLinkedList<Vector2Int>();
						AStar.A_Star(prevTarget, this.target, temp);
						this.checkpoints.ConcatBefore(temp);
					}
				}

				AStar.SmoothPath(this.checkpoints);
			}
			else
				this.checkpoints.Clear();

		}


	}


	protected override void Start()
    {
		base.Start();

		this.health = maxHealth;
	}

	protected override void FixedUpdate()
    {
		if (this.IsDead)
		{
			if(Time.time > this.timeDie + BasicUnit.DIE_COOLDOWN)
				Destroy(this.gameObject);
			return;
		}


		if(this.IsWaitingForAction && (this.Formation == null || (!this.Formation.IsMoving && !this.IsAttacking)))
			this.HandleStance();

		HandleMovement();

		base.FixedUpdate();
    }




	private void HandleStance()
	{
		switch (this.Stance)
		{
			case Stance.AGGRESSIVE:
				if (!this.IsAttacking)
				{
					if (Time.time > this.lastTimeUnitsChecked + BasicUnit.CHECK_FOR_UNITS_COOLDOWN)
					{
						IDamageable target = this.FindClosestUnitInLineOfSight();

						if (target != null)
							this.Attack(target);
					}
				}
				break;
			case Stance.DEFENSIVE:
				if (!this.IsAttacking && !this.IsMoving)
				{
					if (Time.time > this.lastTimeUnitsChecked + BasicUnit.CHECK_FOR_UNITS_COOLDOWN)
					{
						IDamageable target = this.FindClosestUnitInLineOfSight();

						if (target != null)
						{
							this.Attack(target);
							this.defensivePosition = this.Position;
						}
					}
				}
				else
				{
					if(this.DamageableTarget.IsDead || Utils.SqrDistance(this.Position, this.defensivePosition) > BasicUnit.MAXDISTANCE_DEFENSIVE_STAND * BasicUnit.MAXDISTANCE_DEFENSIVE_STAND)
					{
						List<IDamageable> damageables = UnitManager.OverlapCircleUnitDamageable(this.Position, this.lineOfSight, this.Team == Team.ATTACKER ? Team.DEFENDER : Team.ATTACKER);
						IDamageable minTarget = null;
						float minDistance = float.PositiveInfinity;
						foreach(IDamageable d in damageables)
						{
							float dist = Utils.SqrDistance(d.Position, this.Position);
							if(dist < minDistance && Utils.SqrDistance(this.defensivePosition, d.Position) < BasicUnit.MAXDISTANCE_DEFENSIVE_STAND * BasicUnit.MAXDISTANCE_DEFENSIVE_STAND)
							{
								minTarget = d;
								minDistance = dist;
							}
						}

						if (minTarget != null)
							this.Attack(minTarget);
						else
						{
							this.StopAttack();
							this.MoveTo(this.defensivePosition);
						}
					}
				}
				break;
			case Stance.STAND_GROUND:
				if (!this.IsAttacking)
				{
					if (Time.time > this.lastTimeUnitsChecked + BasicUnit.CHECK_FOR_UNITS_COOLDOWN)
					{
						IDamageable target = this.FindClosestUnitInLineOfSight();

						if (target != null && Utils.SqrDistance(this.Position, target.Position) < this.MaxRange * this.MaxRange)
							this.Attack(target);
					}
				}
				break;
			case Stance.NO_ATTACK:
				break;
		}
	}


	private void HandleMovement()
	{
		if (this.timeSinceFormationSpotInObstacle != 0 && Time.time > this.timeSinceFormationSpotInObstacle + 2)
		{
			this.checkpoints.Clear();
			AStar.A_Star(this.Position, this.target, this.checkpoints);
			this.timeSinceFormationSpotInObstacle = 0;
		}

		if (this.IsMoving)
		{
			Vector3 tempTarget;
			bool isLastCheckpoint = this.checkpoints.Count == 0;




			if (this.checkpoints.Count > 1)
			{
				//Vector2Int d = this.checkpoints[this.checkpoints.Count - 1];
				//Vector2Int dd = this.checkpoints[this.checkpoints.Count - 2];
				Vector2Int d = this.checkpoints.Last.Value;
				Vector2Int dd = this.checkpoints.Last.Prev.Value;
				tempTarget = new Vector3((d.x + 0.5f + dd.x) / 2, 0, (d.y + 0.5f + dd.y) / 2);
			}
			else if (!isLastCheckpoint)
			{
				//Vector2Int d = this.checkpoints[this.checkpoints.Count - 1];
				Vector2Int d = this.checkpoints.Last.Value;
				tempTarget = new Vector3((d.x + 0.5f + this.target.x) / 2, 0, (d.y + 0.5f + this.target.y) / 2);
			}
			else
				tempTarget = new Vector3(this.target.x, 0, this.target.y);

			bool isLateInFormationOrNotInFormation = this.Formation == null || (this.target - this.Position).sqrMagnitude > .25f;
			float currentSpeed = isLateInFormationOrNotInFormation ? this.Speed : this.Formation.Speed;

			Vector3 forward = (tempTarget - this.transform.position).normalized;

			this.transform.position += forward * currentSpeed * Time.fixedDeltaTime;

			if (forward.sqrMagnitude == 1)
				this.transform.forward = forward;

			if ((this.transform.position - tempTarget).sqrMagnitude < 0.25f)
			{
				if (isLastCheckpoint)
					this.IsMoving = false;
				else
					//this.checkpoints.RemoveAt(this.checkpoints.Count - 1);
					this.checkpoints.RemoveLast();
			}
		}
		this.Position = new Vector2(this.transform.position.x, this.transform.position.z);
	}




	public override void StopMovement()
	{
		this.checkpoints.Clear();
		this.IsMoving = false;
	}

	public void DebugPath()
	{
		foreach(var u in this.checkpoints)
		{
			GameObject go = new GameObject();
			go.transform.position = new Vector3(u.x, 0, u.y);
			go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			go.AddComponent<MeshFilter>().sharedMesh = Player.smesh;
			go.AddComponent<MeshRenderer>();
		}
	}

	public void Hit(float damagePoints)
	{
		this.health -= damagePoints;
		if (this.health < 0)
		{
			this.health = 0;
			this.audioSource.PlayOneShot(this.deathAudio);
			this.anim.Play(this.dieAnimation);
			this.timeDie = Time.time;
			this.healthBar.SetAmount(0);

			return;
		}
		this.healthBar.SetAmount(this.health / this.maxHealth);

		//Audio hit

		this.audioSource.PlayOneShot(this.hitAudio);

	}

	public void Heal(float healingPoints)
	{
		this.health += healingPoints;
		if (this.health > this.maxHealth)
			this.health = this.maxHealth;
		this.healthBar.SetAmount(this.health / this.maxHealth);
	}
}
