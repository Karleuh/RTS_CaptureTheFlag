using System.Collections.Generic;
using UnityEngine;



public class Projectile : MonoBehaviour
{
	[SerializeField]
	float speed;

	[SerializeField]
	float gravity;

	[SerializeField]
	LayerMask unitLayer;

	[SerializeField]
	bool isExplosive;

	[SerializeField]
	float explosionRadius;

	[SerializeField]
	float explosionForce;

	int damage;
	Unit source;
	Rigidbody body;


	public float Speed { get => this.speed;}
	public float Gravity { get => this.gravity;}
	Vector3 previousPos;

	private void Start()
	{
		this.body = this.GetComponent<Rigidbody>();
	}

	public void Launch(int damage, Unit source, Vector3 position, Vector3 speed)
	{
		this.body = this.GetComponent<Rigidbody>();

		this.transform.position = position;
		this.transform.forward = speed.normalized;
		this.damage = damage;
		this.source = source;
		this.previousPos = position;

		this.body.velocity = speed;
	}


	public void FixedUpdate()
	{
		//collision check
		Vector3 direction = this.transform.position - this.previousPos;
		float length = direction.magnitude;
		direction.x = direction.x/length;
		direction.y = direction.y/length;
		direction.z = direction.z/length;
		if (Physics.Raycast(this.previousPos, direction, out RaycastHit hit, length, this.unitLayer))
		{
			if (this.isExplosive)
			{
				this.Explose(hit.point);
			}
			else
			{
				IDamageable damageable = hit.collider.gameObject.GetComponent<IDamageable>();

				if (damageable != null && !damageable.IsDead && damageable.Team != this.source.Team)
				{
					Destroy(this.gameObject);
					damageable.Hit(DamageType.PIERCE, this.damage);
					return;
				}
			}



		}

		float t = (0 - this.previousPos.y) / (direction.y); //check ground collision
		if (t > 0 && t <= length)
		{
			if (this.isExplosive)
			{
				Vector3 pos = previousPos + t*direction;
				this.Explose(pos);
			}
			Destroy(this.gameObject);
			return;
		}

		this.previousPos = this.transform.position;

		Vector3 velocity = this.body.velocity;
		//velocity
		velocity.y -= this.Gravity * Time.fixedDeltaTime;
		this.body.velocity = velocity;
		this.transform.forward = velocity.normalized;
	}



	public void Explose(Vector3 point)
	{
		List<IDamageable> units = UnitManager.OverlapCircleUnitDamageable(new Vector2(point.x, point.z), this.explosionRadius, this.source.Team == Team.ATTACKER ? Team.DEFENDER : Team.ATTACKER);

		foreach(IDamageable d in units)
		{
			d.Hit(DamageType.MELEE, this.damage);

			Vector3 direction = new Vector3(d.Position.x - point.x, 0, d.Position.y - point.z);
			direction.Normalize();

			d.ApplyForce(direction * this.explosionForce);
		}

	}

}

