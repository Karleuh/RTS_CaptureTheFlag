using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
	void Hit(float damagePoints);
	void Heal(float healingPoints);
	Vector2 Position { get; }
}
