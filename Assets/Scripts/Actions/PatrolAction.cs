using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class PatrolAction : UnitAction
{
	private bool isFinished;

	List<Vector2> checkpoints = new List<Vector2>();
	int currentIndex;

	bool isTargetingDamageable;
	IDamageable targetDamageable;
	Vector2 positionLeavingPatrol;

	float nextTimeToCheckForUnits;
	const float CHECK_FOR_UNITS_INTERVAL = 1;

	public override bool IsFinished { get => this.isFinished; }




	public PatrolAction(Unit unit) : base(unit)
	{
		this.isFinished = false;
	}

	public override void FixedUpdate()
	{
		if (!this.isTargetingDamageable)
		{
			//check for units


			if (Time.time > this.nextTimeToCheckForUnits)
			{
				this.nextTimeToCheckForUnits = Time.time + PatrolAction.CHECK_FOR_UNITS_INTERVAL;

				this.targetDamageable = this.FindClosestUnitInLineOfSight();
				if (this.targetDamageable != null)
				{
					this.isTargetingDamageable = true;

					this.positionLeavingPatrol = this.Unit.Position;
					this.Unit.Attack(this.targetDamageable);
				}
			}


			// follow checkpoints

			if (!this.isTargetingDamageable && !this.Unit.IsMoving && this.checkpoints.Count > 0)
			{
				this.currentIndex++;
				if (this.currentIndex >= this.checkpoints.Count)
					this.currentIndex = 0;

				this.Unit.MoveTo(this.checkpoints[this.currentIndex]);
			}
		}
		else
		{
			if(this.targetDamageable.IsDead)
			{
				this.Unit.StopAttack();

				IDamageable unit = this.FindClosestUnitInLineOfSight();
				if (unit != null && Utils.SqrDistance(this.positionLeavingPatrol, this.targetDamageable.Position) <= 4 * this.Unit.LineOfSight * this.Unit.LineOfSight)
				{
					this.isTargetingDamageable = true;
					this.targetDamageable = unit;
				}
				else
				{
					this.isTargetingDamageable = false;
					this.Unit.MoveTo(this.checkpoints[this.currentIndex]);
				}
			}
			else if (Time.time > this.nextTimeToCheckForUnits)
			{
				this.nextTimeToCheckForUnits = Time.time + PatrolAction.CHECK_FOR_UNITS_INTERVAL;

				//if we are too far away or the target is too far away
				if (Utils.SqrDistance(this.positionLeavingPatrol, this.targetDamageable.Position) > 4 * this.Unit.LineOfSight * this.Unit.LineOfSight)
				{
					this.Unit.StopAttack();

					IDamageable unit = this.FindClosestUnitInLineOfSight();
					if (unit != null && Utils.SqrDistance(this.positionLeavingPatrol, this.targetDamageable.Position) <= 4 * this.Unit.LineOfSight * this.Unit.LineOfSight)
					{
						this.isTargetingDamageable = true;
						this.targetDamageable = unit;
					}
					else
					{
						this.isTargetingDamageable = false;
						this.Unit.MoveTo(this.checkpoints[this.currentIndex]);
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

	public override bool EnqueueMove(Vector2 target)
	{
		this.checkpoints.Add(target);

		return true;
	}

	public override bool EnqueueAttack(IDamageable target)
	{
		return false;
	}
}

