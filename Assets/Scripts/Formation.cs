using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : Unit
{
	protected List<Unit> units = new List<Unit>();

	protected Vector2 forward;

	List<Vector2Int> checkpoints = new List<Vector2Int>();
	Vector2 target;

	bool waitForPosition;
	bool areUnitsAttacking = false;
	bool isDestroyed;

	public FormationType FormationType { get; set; } = FormationType.LINE;
	public override bool IsSelectable => false;
	public override int Weight => 0;


	public void OnCreation(IEnumerable<Unit> units)
	{
		this.units.AddRange(units);
		Vector2 pos = Vector2.zero;
		bool isSet = false;

		this.Speed = float.MaxValue;
		this.maxRange = 0;

		foreach(Unit unit in units)
		{
			if (unit == null || unit.gameObject == null)
				continue;

			unit.Formation = this;
			unit.StopAll();

			if (unit.Speed < this.Speed)
				this.Speed = unit.Speed;

			if (unit.MinRange > this.MinRange)
				this.minRange = unit.MinRange;
			if (unit.MaxRange > this.maxRange)
				this.maxRange = unit.MaxRange;
			if (unit.LineOfSight > this.LineOfSight)
				this.lineOfSight = unit.LineOfSight;

			if (!isSet)
			{
				this.team = unit.Team;

				pos = unit.Position;
				isSet = true;
			}
			else
				pos += unit.Position;
		}
		this.Speed = 0.75f * this.Speed;

		this.Position = pos / this.units.Count;
		this.transform.position = new Vector3(this.Position.x, 0, this.Position.y);


		this.waitForPosition = true;
	}

	public override void MoveTo(Vector2 pos, bool isCheckpoint = false)
	{
		this.IsMoving = true;

		this.target = Vector2Int.FloorToInt(pos) == Terrain.instance.GetClosestAccessiblePos(Vector2Int.FloorToInt(pos)) ? pos : Terrain.instance.GetClosestAccessiblePos(Vector2Int.FloorToInt(pos)) + new Vector2(0.5f, 0.5f);
		this.checkpoints.Clear();
		this.Position = new Vector2(this.transform.position.x, this.transform.position.z);

		AStar.A_Star(this.Position, this.target, this.checkpoints);

		if (this.checkpoints.Count >= 2)
			this.forward = ((Vector2)(this.checkpoints[this.checkpoints.Count - 2] - this.checkpoints[this.checkpoints.Count - 1])).normalized;
		else
			this.forward = (this.target - this.Position).normalized;
		if (this.forward.sqrMagnitude == 0)
			this.forward = Vector2.up; 

		this.UpdateUnits(true);
		//this.waitForPosition = true;
	}


	protected override void FixedUpdate()
	{
		if (this.isDestroyed)
			return;

		if (this.IsMoving && this.waitForPosition)
		{
			foreach(Unit u in this.units)
				if (u.IsMoving)
					return;
			this.waitForPosition = false;
		}

		if (!this.IsMoving && !this.IsAttacking && this.IsWaitingForAction)
			this.waitForPosition = true;

		if (this.IsMoving)
		{
			this.UpdatePosition();
			this.UpdateUnits();
		}


		if (this.IsAttacking && !this.areUnitsAttacking && this.IsTargetInRange())
		{
			this.areUnitsAttacking = true;
			foreach (Unit u in this.units)
			{
				u.Attack(this.DamageableTarget);
			}
		}
		
		if(this.IsAttacking && this.DamageableTarget.IsDead && this.IsWaitingForAction)
		{
			this.DestroyFormation();
			return;
		}

		if (!this.isDestroyed)
			base.FixedUpdate();
	}


	private void UpdatePosition()
	{
		Vector3 tempTarget;
		bool isLastCheckpoint = this.checkpoints.Count == 0;
		if (this.checkpoints.Count > 1)
		{
			Vector2Int d = this.checkpoints[this.checkpoints.Count - 1];
			Vector2Int dd = this.checkpoints[this.checkpoints.Count - 2];
			tempTarget = new Vector3((d.x + 0.5f + dd.x) / 2, 0, (d.y + 0.5f + dd.y) / 2);
		}
		else if (!isLastCheckpoint)
		{
			Vector2Int d = this.checkpoints[this.checkpoints.Count - 1];
			tempTarget = new Vector3((d.x + 0.5f + this.target.x) / 2, 0, (d.y + 0.5f + this.target.y) / 2);
		}
		else
			tempTarget = new Vector3(this.target.x, 0, this.target.y);

		Vector3 forw = (tempTarget - this.transform.position).normalized;
		this.transform.position += forw * this.Speed * Time.fixedDeltaTime;

		if ((this.transform.position - tempTarget).sqrMagnitude < 0.25f)
		{
			if (isLastCheckpoint)
				this.IsMoving = false;
			else
				this.checkpoints.RemoveAt(this.checkpoints.Count - 1);
		}




		this.forward = new Vector2(forw.x, forw.z);
		this.Position = new Vector2(this.transform.position.x, this.transform.position.z);
	}


	public override void StopAttack()
	{
		base.StopAttack();
		this.areUnitsAttacking = false;
	}


	public void DestroyFormation()
	{
		this.isDestroyed = true;
		GameObject.Destroy(this.gameObject);

		foreach (Unit u in this.units)
			u.LeaveFormationWithoutNotification();
	}

	public void RemoveUnit(Unit u)
	{
		this.units.Remove(u);

		if (this.units.Count == 0)
			this.DestroyFormation();
	}




	[Range(0, 10)]
	[SerializeField]
	float ratio = 2;

	private void UpdateUnits(bool isFirstMove = false)
	{
		switch (this.FormationType)
		{
			case FormationType.LINE:
				int width = ((int)Mathf.Sqrt(ratio * this.units.Count));
				if (width == 0)
					width = 1;

				Vector2 left = Vector2.Perpendicular(this.forward);

				for (int i = 0; i < this.units.Count; i++)
				{
					if (isFirstMove)
						this.units[i].StopAll();

					Vector2 u = this.Position + left * 1.2f * (i % width - width / 2) - this.forward * 1.2f*(i/width);
					Vector2Int flu = Vector2Int.FloorToInt(u);
					if (Terrain.instance.IsInTerrain(flu))
					{
						this.units[i].MoveTo(u, !isFirstMove);
					}
				}
				break;
			case FormationType.SQUARE:

				break;
			case FormationType.SPACED:
				int doubleWidth = ((int)Mathf.Sqrt(ratio*2 * this.units.Count));
				width = (doubleWidth + 1) / 2;
				if (width == 0)
					width = 1;
				left = Vector2.Perpendicular(this.forward);

				for (int i = 0; i < this.units.Count; i++)
				{
					if (isFirstMove)
						this.units[i].StopAll();

					Vector2 u = this.Position;
					if (i % doubleWidth < width)
						u += left * 3.0f * (i % doubleWidth - width / 2.0f) - this.forward * 3.0f * (i / doubleWidth) * 2;
					else
						u += left * 3.0f * (i % doubleWidth - width - (width -1)/ 2.0f) - this.forward * 3.0f * ((i / doubleWidth) * 2 + 1);

					Vector2Int flu = Vector2Int.FloorToInt(u);
					if (Terrain.instance.IsInTerrain(flu))
					{
						this.units[i].MoveTo(u, !isFirstMove);
					}
				}
				break;
			case FormationType.SEPERATED:
				width = ((int)Mathf.Sqrt(ratio * this.units.Count)) / 2;
				if (width == 0)
					width = 1;

				left = Vector2.Perpendicular(this.forward);

				for (int i = 0; i < this.units.Count; i++)
				{
					if (isFirstMove)
						this.units[i].StopAll();

					if (i < this.units.Count / 2)
					{
						Vector2 u = this.Position - left * width * 1.5f + left * 1.2f * (i % width - width / 2) - this.forward * 1.2f * (i / width);
						Vector2Int flu = Vector2Int.FloorToInt(u);
						if (Terrain.instance.IsInTerrain(flu))
						{
							this.units[i].MoveTo(u, !isFirstMove);
						}
					}
					else
					{
						Vector2 u = this.Position + left * width * 1.5f + left * 1.2f * ((i- this.units.Count / 2) % width - width / 2) - this.forward * 1.2f * ((i- this.units.Count / 2) / width);
						Vector2Int flu = Vector2Int.FloorToInt(u);
						if (Terrain.instance.IsInTerrain(flu))
						{
							this.units[i].MoveTo(u, !isFirstMove);
						}
					}
				}
				break;
		}
	}


	public override void StopMovement()
	{
		this.IsMoving = false;
		this.checkpoints.Clear();
	}


	public void DebugPath()
	{
		foreach (var u in this.checkpoints)
		{
			GameObject go = new GameObject();
			go.transform.position = new Vector3(u.x, 0, u.y);
			go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			go.AddComponent<MeshFilter>().sharedMesh = Player.smesh;
			go.AddComponent<MeshRenderer>();
		}
	}
}


public enum FormationType
{
	LINE,
	SQUARE,
	SPACED,
	SEPERATED
}
