using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBasicUnit : Unit
{
	[SerializeField]
	NavMeshAgent navAgent;

	private Vector2 targetPos;


	public override void MoveTo(Vector2 pos, bool isCheckpoint = false)
	{
		this.targetPos = pos;
		navAgent.SetDestination(new Vector3(pos.x, 0, pos.y));

	}


	protected override void Start()
    {
		base.Start();
    }

    void Update()
    {
		//this.transform.position = (this.targetPos - this.Position) * speed * Time.deltaTime;
    }

	protected override void FixedUpdate()
	{
		this.Position = new Vector2(this.transform.position.x, this.transform.position.z);
		base.FixedUpdate();
	}

	public override void StopMovement()
	{
		throw new System.NotImplementedException();
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
