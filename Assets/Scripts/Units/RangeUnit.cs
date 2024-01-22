using System;
using System.Collections.Generic;
using UnityEngine;


public class RangeUnit : BasicUnit
{
	[Header("Animation")]
	[SerializeField]
	string attackAnimation;
	[SerializeField]
	float delayToAttack;
	[SerializeField]
	Projectile projectile;

	[SerializeField]
	float maxArrowAngle = 60;

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
			if(!String.IsNullOrEmpty(this.attackAnimation))
				this.anim.Play(this.attackAnimation);
			this.hit = false;
		}
		if (!hit && Time.time > this.timeAttackWasPerformed + delayToAttack)
		{
			this.hit = true;

			this.Shoot();
		}
	}


	public Vector3 CalculateDirection(Vector3 source, Vector3 target)
	{
		double d = (target.x - source.x) * (target.x - source.x) + (target.z - source.z) * (target.z - source.z);

		double sqrtDelta = Math.Sqrt(this.projectile.Speed * this.projectile.Speed * this.projectile.Speed * this.projectile.Speed - 4 * this.projectile.Gravity * this.projectile.Gravity * d);

		double T = (this.projectile.Speed * this.projectile.Speed + sqrtDelta) / (2 * this.projectile.Gravity * this.projectile.Gravity);

		double t = Math.Sqrt(T);


		Vector3 res =  new Vector3
			(
				(float)((target.x - source.x) / t),
				(float)(t * this.projectile.Gravity/2),
				(float)((target.z - source.z) / t)

			);

		if(Vector3.Angle(res, new Vector3(target.x - source.x, 0, target.z - source.z)) > this.maxArrowAngle)
		{
			T = (this.projectile.Speed * this.projectile.Speed - sqrtDelta) / (2 * this.projectile.Gravity * this.projectile.Gravity);
			t = Math.Sqrt(T);

			res = new Vector3
			(
				(float)((target.x - source.x) / t),
				(float)(t * this.projectile.Gravity / 2),
				(float)((target.z - source.z) / t)

			);
		}

		return res;

	}
	
	//public Vector3 CalculateDirection(Vector3 source, Vector3 target)
	//{
	//	double d = (target.x - source.x) * (target.x - source.x) + (target.z - source.z) * (target.z - source.z);

	//	double sqrtDelta = Math.Sqrt(this.projectile.Speed * this.projectile.Speed * this.projectile.Speed * this.projectile.Speed - 4 * this.projectile.Gravity * this.projectile.Gravity * d);
	//	double T = (this.projectile.Speed * this.projectile.Speed - sqrtDelta) / (2 * this.projectile.Gravity * this.projectile.Gravity);

	//	double t = Math.Sqrt(T);


	//	return new Vector3
	//		(
	//			(float)((target.x - source.x) / t),
	//			(float)(t * this.projectile.Gravity),
	//			(float)((target.z - source.z) / t)

	//		);

	//}



	public void Shoot()
	{
		Vector3 direction = this.CalculateDirection(this.transform.position + Vector3.up, new Vector3(this.DamageableTarget.Position.x, 1, this.DamageableTarget.Position.y));

		Projectile p = GameObject.Instantiate(this.projectile);
		p.enabled = true;
		p.Launch(this.Damage, this, this.transform.position + Vector3.up, direction);

	}
}
