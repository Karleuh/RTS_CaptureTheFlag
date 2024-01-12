using UnityEngine;

public enum UnitActionType
{
	ATTACK_MOVE,
	PATROL
}

public abstract class UnitAction
{
	public static UnitAction FromType(UnitActionType type, Unit unit)
	{
		switch(type)
		{
			case UnitActionType.ATTACK_MOVE:
				return null;
			case UnitActionType.PATROL:
				return new PatrolAction(unit);
		}

		return null;
	}


	protected Unit Unit { get; private set; }

	public abstract bool IsFinished { get; }

	public UnitAction(Unit unit)
	{
		this.Unit = unit;
	}

	public abstract void FixedUpdate();

	/// <summary>
	/// Called when a move is enqueued, for example when the player right click on a tile
	/// </summary>
	/// <param name="target"></param>
	/// <returns> True if the move is used, False if this does not correspond to this action </returns>
	public abstract bool EnqueueMove(Vector2 target);

	/// <summary>
	/// Called when a attack order is enqueued, for example when the player right click on an unit
	/// </summary>
	/// <param name="target"></param>
	/// <returns> True if the attack order is used, False if this does not correspond to this action </returns>
	public abstract bool EnqueueAttack(IDamageable target);
}

