using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackMove : UnitAction
{
	private Queue<Vector2> checkpoints;
	private Queue<IDamageable> damageables;

	private bool isTargetingDamageables;
	private bool isNotStarted = true;


	public AttackMove(Unit u) : base(u) {}

	public override bool IsFinished
	{
		get => !isNotStarted && ((this.isTargetingDamageables && this.damageables.Count == 0) || (!this.isTargetingDamageables && this.checkpoints.Count == 0));
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
			if(!this.Unit.IsMoving && this.checkpoints.Count > 0)
			{
				this.Unit.MoveTo(checkpoints.Dequeue());
			}
		}
	}
}

