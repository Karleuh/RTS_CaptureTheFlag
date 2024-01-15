using System;
using System.Collections.Generic;
using UnityEngine;

public class DefendAction : UnitAction
{
	Queue<IDamageable> defendTargets = new Queue<IDamageable>();
	IDamageable attackTarget;
	private bool isAttackingTarget;

	private const float DISTANCE_TO_DEFEND_TARGET = 2;
	private const float CHECK_COOLDOWN = 1;

	private float nextTimeCheck;

	public DefendAction(Unit unit) : base(unit)
	{

	}

	public override bool IsFinished => this.defendTargets.Count == 0;

	public override UnitActionType UnitActionType => UnitActionType.DEFEND;

	public override bool IsFriendlyAction => true;

	public override bool EnqueueAttack(IDamageable target)
	{
		if (target.Team != this.Unit.Team)
			return false;

		this.defendTargets.Enqueue(target);
		return true;
	}

	public override bool EnqueueMove(Vector2 target)
	{
		return false;
	}

	public override void FixedUpdate()
	{
		if(this.defendTargets.Count > 0)
		{
			IDamageable defendTarget = this.defendTargets.Peek();

			if (defendTarget.IsDead)
			{
				this.defendTargets.Dequeue();
				if (this.defendTargets.Count == 0)
					return;
				defendTarget = this.defendTargets.Peek();

				Vector2 dir = (defendTarget.Position - this.Unit.Position).normalized;
				this.Unit.MoveTo(defendTarget.Position - DefendAction.DISTANCE_TO_DEFEND_TARGET * dir);
			}

			if (!this.Unit.IsMoving && !this.isAttackingTarget && Utils.SqrDistance(this.Unit.Position, defendTarget.Position) > DefendAction.DISTANCE_TO_DEFEND_TARGET * DefendAction.DISTANCE_TO_DEFEND_TARGET)
			{
				Vector2 dir = (defendTarget.Position - this.Unit.Position).normalized;
				this.Unit.MoveTo(defendTarget.Position - DefendAction.DISTANCE_TO_DEFEND_TARGET * dir);
			}

			if (Time.time > this.nextTimeCheck)
			{
				if(this.isAttackingTarget && (!this.Unit.IsAttacking || Utils.SqrDistance(defendTarget.Position, this.Unit.Position) > this.Unit.LineOfSight * this.Unit.LineOfSight))
				{
					Vector2 dir = (defendTarget.Position - this.Unit.Position).normalized;

					this.Unit.MoveTo(defendTarget.Position - DefendAction.DISTANCE_TO_DEFEND_TARGET * dir);
					this.isAttackingTarget = false;
				}
				else
				{
					IDamageable closestUnit = null;

					if (Utils.SqrDistance(defendTarget.Position, this.Unit.Position) <= this.Unit.LineOfSight * this.Unit.LineOfSight  && (closestUnit = this.FindClosestUnitInLineOfSight()) != null)
					{
						this.isAttackingTarget = true;
						this.attackTarget = closestUnit;
						this.Unit.Attack(this.attackTarget);
					}
					else if(Utils.SqrDistance(this.Unit.Position, defendTarget.Position) > DefendAction.DISTANCE_TO_DEFEND_TARGET * DefendAction.DISTANCE_TO_DEFEND_TARGET)
					{
						Vector2 dir = (defendTarget.Position - this.Unit.Position).normalized;

						this.nextTimeCheck = Time.time + DefendAction.CHECK_COOLDOWN;
						this.Unit.MoveTo(defendTarget.Position - DefendAction.DISTANCE_TO_DEFEND_TARGET * dir);
					}
				}
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