using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeUnit : BasicUnit
{
	float minRange;
	float maxRange;
	float damage;
	float projectileSpeed;

	IDamageable target;

    protected override void Start()
    {
		base.Start();   
    }

	protected override void FixedUpdate()
    {
		base.FixedUpdate();

    }



	enum State
	{
		WALKING,
		ATTACKING,
		PATROLLING
	}
}
