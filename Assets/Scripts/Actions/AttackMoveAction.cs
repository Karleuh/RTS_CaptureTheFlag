using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackMove : UnitAction
{
	private Queue<Vector2> checkpoints;
	private Queue<IDamageable> damageables;

	private bool isTargetingDamageables;
	private bool isNotStarted = true;
	//private bool isTargetingWhileMoving;
	//private IDamageable targetWhileMoving;
	//private Vector2 pointLeavingPathToAttack;

	//private float timeToCheckForUnits;

	private const float CHECK_COOLDOWN = 1;


	public AttackMove(Unit u) : base(u) {}

	public override bool IsFinished
	{
		get => !isNotStarted && ((this.isTargetingDamageables && this.damageables.Count == 0) || (!this.isTargetingDamageables && this.checkpoints.Count == 0) && !this.Unit.IsMoving && !this.Unit.IsAttacking);
	}


	public override bool EnqueueAttack(IDamageable target)
	{
		if(this.isNotStarted)
		{
			this.isNotStarted = false;
			this.isTargetingDamageables = true;
			this.damageables = new Queue<IDamageable>();
			this.damageables.Enqueue(target);

			return true;
		}
		else if (this.isTargetingDamageables)
		{
			this.damageables.Enqueue(target);
			return true;
		}
		else
		{
			return false;
		}
	}

	public override bool EnqueueMove(Vector2 target)
	{
		if (this.isNotStarted)
		{
			this.isNotStarted = false;
			this.isTargetingDamageables = false;
			this.checkpoints = new Queue<Vector2>();
			this.checkpoints.Enqueue(target);

			return true;
		}
		else if (!this.isTargetingDamageables)
		{
			this.checkpoints.Enqueue(target);
			return true;
		}
		else
		{
			return false;
		}
	}

	public override void FixedUpdate()
	{
		if (this.isNotStarted)
			return;

		if (this.isTargetingDamageables)
		{
			if(!this.Unit.IsAttacking && this.damageables.Count > 0)
			{
				IDamageable target = null;
				do
				{
					target = this.damageables.Dequeue();
				} while (target.IsDead && this.damageables.Count > 0);

				if(!target.IsDead)
				this.Unit.Attack(target);
			}
		}
		else
		{
			//if(Time.time > this.timeToCheckForUnits)
			//{
			//	IDamageable target = this.FindClosestUnitInLineOfSight();
			//	if(target != null)
			//	{
			//		this.isTargetingWhileMoving = true;
			//		this.targetWhileMoving = target;
			//	}
			//}

			if(!this.Unit.IsMoving && this.checkpoints.Count > 0)
			{
				this.Unit.MoveTo(checkpoints.Dequeue());
			}
		}
	}


	private IDamageable FindClosestUnitInLineOfSight()
	{
		IDamageable u = null;
		List<IDamageable> units = UnitManager.OverlapCircleUnitDamageable(this.Unit.Position, this.Unit.LineOfSight, this.Unit.Team == Team.ATTACKER ? Team.DEFENDER : Team.ATTACKER);
		if (units.Count > 0)
		{

			//find closest unit
			float minDistance = float.MaxValue;
			for (int i = 0; i < units.Count; i++)
			{
				float dist = Utils.SqrDistance(units[i].Position, this.Unit.Position);
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

