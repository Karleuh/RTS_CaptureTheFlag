using UnityEngine;

public enum UnitActionType
{
	NONE,
	ATTACK_MOVE,
	PATROL,
	DEFEND
}

public abstract class UnitAction
{
	public static UnitAction FromType(UnitActionType type, Unit unit)
	{
		switch(type)
		{
			case UnitActionType.ATTACK_MOVE:
				return new AttackMoveAction(unit);
			case UnitActionType.PATROL:
				return new PatrolAction(unit);
			case UnitActionType.DEFEND:
				return new DefendAction(unit);
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

	public abstract UnitActionType UnitActionType { get; }
	public abstract bool IsFriendlyAction { get; }
}

