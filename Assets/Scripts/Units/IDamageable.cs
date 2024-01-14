using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable 
{
	void Hit(float damagePoints);
	void Heal(float healingPoints);
	bool IsDead { get; }
	Team Team { get; }
	Vector2 Position { get; }
}
