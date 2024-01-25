using System;
using System.Collections.Generic;
using UnityEngine;

public class OnagerUnit : RangeUnit
{
	public void AttackPosition(Vector2 position)
	{
		if(Utils.SqrDistance(position, this.Position) < this.MaxRange * this.MaxRange && Utils.SqrDistance(position, this.Position) > this.MinRange * this.MinRange)
		{

			Vector2 forward = (position - this.Position).normalized;
			this.transform.forward = new Vector3(forward.x, 0, forward.y);
			if (!String.IsNullOrEmpty(this.attackAnimation))
				this.anim.Play(this.attackAnimation);

			Vector3 direction = this.CalculateDirection(this.transform.position + Vector3.up, new Vector3(position.x, 1, position.y));

			Projectile p = GameObject.Instantiate(this.projectile);
			p.enabled = true;
			p.Launch(this.Damage, this, this.transform.position + Vector3.up, direction);
		}
	}
}
