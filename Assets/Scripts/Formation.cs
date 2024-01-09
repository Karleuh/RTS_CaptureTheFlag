using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{
	protected List<Unit> units = new List<Unit>();

	protected Vector2 forward;
	protected Vector2 position;

	List<Vector2Int> checkpoints = new List<Vector2Int>();
	Vector2 target;

	bool waitForPosition;

	public float Speed
	{
		get;
		private set;
	}

	public void OnCreation(IEnumerable<Unit> units)
	{
		this.units.AddRange(units);
		Vector2 pos = Vector2.zero;
		bool isSet = false;

		this.Speed = float.MaxValue;

		foreach(Unit unit in units)
		{
			unit.Formation = this;

			if (unit.Speed < this.Speed)
				this.Speed = unit.Speed;
			if (!isSet)
			{
				pos = unit.Position;
				isSet = true;
			}
			else
				pos += unit.Position;
		}
		this.Speed = 0.75f * this.Speed;
		Debug.Log("Speed : " + this.Speed);

		this.position = pos / this.units.Count;
		this.transform.position = new Vector3(this.position.x, 0, this.position.y);
	}

	public void MoveTo(Vector2 pos)
	{
		this.target = Vector2Int.FloorToInt(pos) == Terrain.instance.GetClosestAccessiblePos(Vector2Int.FloorToInt(pos)) ? pos : Terrain.instance.GetClosestAccessiblePos(Vector2Int.FloorToInt(pos)) + new Vector2(0.5f, 0.5f);
		this.checkpoints.Clear();
		this.position = new Vector2(this.transform.position.x, this.transform.position.z);
		Unit.A_Star(this.position, this.target, this.checkpoints);

		if (this.checkpoints.Count >= 2)
			this.forward = ((Vector2)(this.checkpoints[this.checkpoints.Count - 2] - this.checkpoints[this.checkpoints.Count - 1])).normalized;
		else
			this.forward = (this.target - this.position).normalized;
		if (this.forward.sqrMagnitude == 0)
			this.forward = Vector2.up; 

		this.UpdateUnits(true);
		this.waitForPosition = true;
	}


	public void FixedUpdate()
	{
		if (this.waitForPosition)
		{
			foreach(Unit u in this.units)
				if (u.IsMoving)
					return;
			this.waitForPosition = false;
		}
		this.UpdatePosition();
		this.UpdateUnits();
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
				this.DestroyFormation();
			else
				this.checkpoints.RemoveAt(this.checkpoints.Count - 1);
		}




		this.forward = new Vector2(forw.x, forw.z);
		this.position = new Vector2(this.transform.position.x, this.transform.position.z);
	}



	public void DestroyFormation()
	{
		GameObject.Destroy(this.gameObject);

		foreach (Unit u in this.units)
			u.LeaveFormationWithoutNotification();
	}

	public void RemoveUnit(Unit u)
	{
		this.units.Remove(u);
	}







	private void UpdateUnits(bool isFirstMove = false)
	{
		for(int i=0; i<this.units.Count; i++)
		{
			Vector2 left = Vector2.Perpendicular(this.forward);
			// balance on multiple ticks
			Vector2 u = this.position + left * (i - this.units.Count / 2);
			Vector2Int flu = Vector2Int.FloorToInt(u);
			if (Terrain.instance.IsInTerrain(flu))
			{
				//if(!Terrain.instance.IsObstacle(flu))
				this.units[i].MoveTo(u, !isFirstMove);
				//else if (this.updateCount % 2 * ((int)(1.0f / Time.fixedDeltaTime)) == 0)
			}
		}
	}


	public void Attack(Unit u)
	{

	}
}
