using System.Collections.Generic;
using UnityEngine;

public class BasicUnit : Unit
{

	//List<Vector2Int> checkpoints = new List<Vector2Int>();
	SimpleConcatLinkedList<Vector2Int> checkpoints = new SimpleConcatLinkedList<Vector2Int>();
	Vector2 target;
	float timeSinceFormationSpotInObstacle;

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
			}
			else
				this.checkpoints.Clear();

		}


	}


	protected override void Start()
    {
		base.Start();
    }

    protected override void FixedUpdate()
    {

		HandleMovement();

		base.FixedUpdate();
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

			bool isLateInFormationOrNotInFormation = this.Formation == null || (this.target - this.Position).sqrMagnitude > .75f;
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

	public override void Hit(float damagePoints)
	{
		throw new System.NotImplementedException();
	}

	public override void Heal(float healingPoints)
	{
		throw new System.NotImplementedException();
	}
}
