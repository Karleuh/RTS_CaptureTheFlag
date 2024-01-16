using UnityEngine;



public class Projectile : MonoBehaviour
{
	[SerializeField]
	float speed;

	[SerializeField]
	float gravity;

	[SerializeField]
	LayerMask unitLayer;


	float damage;
	Unit source;
	Rigidbody body;


	public float Speed { get => this.speed;}
	public float Gravity { get => this.gravity;}
	float previousDeltaTime;

	private void Start()
	{
		this.body = this.GetComponent<Rigidbody>();
	}

	public void Launch(float damage, Unit source, Vector3 position, Vector3 speed)
	{
		this.body = this.GetComponent<Rigidbody>();

		this.transform.position = position;
		this.transform.forward = speed.normalized;
		this.damage = damage;
		this.source = source;
		this.previousDeltaTime = Time.fixedDeltaTime;

		this.body.velocity = speed;
	}


	public void FixedUpdate()
	{
		//collision check
		Vector3 velocity = this.body.velocity;
		if (Physics.Raycast(this.transform.position - previousDeltaTime * velocity, velocity, out RaycastHit hit, velocity.magnitude * previousDeltaTime * 2, this.unitLayer))
		{
			IDamageable damageable = hit.collider.gameObject.GetComponent<IDamageable>();

			if (damageable != null && !damageable.IsDead && damageable.Team != this.source.Team)
			{
				Destroy(this.gameObject);
				damageable.Hit(this.damage);
				Debug.Log("HIT");
				return;
			}
		}

		//delete
		if (this.transform.position.y <= 0)
		{
			Destroy(this.gameObject);
			return;
		}




		//velocity
		velocity.y -= this.Gravity * Time.fixedDeltaTime;
		this.body.velocity = velocity;
		this.transform.forward = velocity.normalized;

		this.previousDeltaTime = Time.fixedDeltaTime;

	}

}

