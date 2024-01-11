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

	bool isTargetingUnit;
	Unit targetUnit;
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
		if (!this.isTargetingUnit)
		{
			//check for units


			if (Time.time > this.nextTimeToCheckForUnits)
			{
				this.nextTimeToCheckForUnits = Time.time + PatrolAction.CHECK_FOR_UNITS_INTERVAL;

				this.targetUnit = this.FindClosestUnitInLineOfSight();
				if (this.targetUnit != null)
				{
					this.isTargetingUnit = true;

					this.positionLeavingPatrol = this.Unit.Position;
					this.Unit.Attack(this.targetUnit);
				}
			}


			// follow checkpoints

			if (!this.isTargetingUnit && !this.Unit.IsMoving && this.checkpoints.Count > 0)
			{
				if (currentIndex >= this.checkpoints.Count)
					currentIndex = 0;

				this.Unit.MoveTo(this.checkpoints[this.currentIndex++]);
			}
		}
		else
		{
			if (Time.time > this.nextTimeToCheckForUnits)
			{
				this.nextTimeToCheckForUnits = Time.time + PatrolAction.CHECK_FOR_UNITS_INTERVAL;

				//if we are too far away or the target is too far away
				if (Utils.SqrDistance(this.positionLeavingPatrol, this.Unit.Position) > 4 * this.Unit.LineOfSight * this.Unit.LineOfSight || Utils.SqrDistance(this.targetUnit.Position, this.Unit.Position) > this.Unit.LineOfSight * this.Unit.LineOfSight)
				{
					this.Unit.StopAttack();

					Unit unit = this.FindClosestUnitInLineOfSight();
					if (unit != null && Utils.SqrDistance(unit.Position, this.positionLeavingPatrol) > this.Unit.LineOfSight * this.Unit.LineOfSight)
					{
						this.isTargetingUnit = true;
						this.targetUnit = unit;
					}
					else
					{
						this.Unit.MoveTo(this.checkpoints[this.currentIndex]);
					}

					//TODO check if unit is dead or change is targeting
				}
			}
		}
	}


	private Unit FindClosestUnitInLineOfSight()
	{
		Unit u = null;
		List<Unit> units = UnitManager.OverlapCircle(this.Unit.Position, this.Unit.LineOfSight, this.Unit.Team == Team.ATTACKER ? Team.DEFENDER : Team.ATTACKER);
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

	public override bool EnqueueAttack(Unit target)
	{
		return false;
	}
}

