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
				Unit.A_Star(this.Position, this.target, this.checkpoints);
		}
		else
		{
			if (Time.time + 2 > this.timeSinceFormationSpotInObstacle)
			{
				this.checkpoints.Clear();
				Unit.A_Star(prevTarget, this.target, this.checkpoints);
			}
			else if ((this.target - this.Position).sqrMagnitude > 2)
			{
				//List<Vector2Int> temp = new List<Vector2Int>();
				//Unit.A_Star(prevTarget, pos, temp);
				//this.checkpoints.InsertRange(0, temp);
				SimpleConcatLinkedList<Vector2Int> temp = new SimpleConcatLinkedList<Vector2Int>();
				Unit.A_Star(prevTarget, this.target, temp);
				this.checkpoints.ConcatBefore(temp);


				if (isTargetInObstacle && this.Formation != null && Time.time + 2 > this.timeSinceFormationSpotInObstacle)
				{
					this.timeSinceFormationSpotInObstacle = Time.time;
				}
			}


		}


	}


	protected override void Start()
    {
		base.Start();
    }

    protected override void FixedUpdate()
    {
		//if ((this.checkpoints.Count > 0 && Terrain.instance.IsObstacle(this.checkpoints.Last.Value)) || Terrain.instance.IsObstacle((int)this.target.x, (int)this.target.y))
		//{
		//	this.checkpoints.Clear();
		//	this.IsMoving = false;
		//}

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
				tempTarget = new Vector3((d.x + 0.5f + dd.x)/2, 0, (d.y + 0.5f + dd.y)/2);
			}
			else if (!isLastCheckpoint)
			{
				//Vector2Int d = this.checkpoints[this.checkpoints.Count - 1];
				Vector2Int d = this.checkpoints.Last.Value;
				tempTarget = new Vector3((d.x + 0.5f + this.target.x)/2, 0, (d.y + 0.5f + this.target.y)/2);
			}
			else
				tempTarget = new Vector3(this.target.x, 0, this.target.y);

			bool isLateInFormationOrNotInFormation = this.Formation == null || (this.target - this.Position).sqrMagnitude > .75f;
			float currentSpeed =  isLateInFormationOrNotInFormation ? this.Speed : this.Formation.Speed;

			Vector3 forward = (tempTarget - this.transform.position).normalized;

			this.transform.position += forward * currentSpeed * Time.fixedDeltaTime;

			if(forward.sqrMagnitude == 1)
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

		base.FixedUpdate();
    }


	public void DebugPath()
	{
		foreach(var u in this.checkpoints)
		{
			GameObject go = new GameObject();
			go.transform.position = new Vector3(u.x, 0, u.y);
			go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
			go.AddComponent<MeshFilter>().sharedMesh = Orderer.smesh;
			go.AddComponent<MeshRenderer>();
		}
	}
}
