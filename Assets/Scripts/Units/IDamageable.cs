using UnityEngine;

public enum DamageType
{
	DIRECT,
	MELEE,
	PIERCE
}

public interface IDamageable 
{
	void Hit(DamageType damageType, int damagePoints);
	void Heal(int healingPoints);
	bool IsDead { get; }
	Team Team { get; }
	Vector2 Position { get; }
}
