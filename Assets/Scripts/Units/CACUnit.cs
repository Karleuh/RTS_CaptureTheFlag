using System;
using System.Collections.Generic;
using UnityEngine;


public class CACUnit : BasicUnit
{
	[SerializeField]
	String attackAnimation;
	[SerializeField]
	float delayToAttack;

	float timeAttackWasPerformed;
	bool hit = false;

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if (this.IsTargetInRange())
			PerformAttack();
	}


	private void PerformAttack()
	{
		if (Time.time > this.timeAttackWasPerformed + this.AttackCooldown)
		{
			Vector2 forward = (this.DamageableTarget.Position - this.Position).normalized;
			this.transform.forward = new Vector3(forward.x, 0, forward.y);

			this.timeAttackWasPerformed = Time.time;
			this.anim.Play(this.attackAnimation);
			this.hit = false;
		}
		else if(!hit && Time.time > this.timeAttackWasPerformed + delayToAttack)
		{
			this.hit = true;
			this.DamageableTarget.Hit(this.Damage);
		}
	}

}
