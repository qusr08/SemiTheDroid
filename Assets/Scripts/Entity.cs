using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType {
	SPIKE, ROBOT, LASER, BOMB
}

public abstract class Entity : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Header("Properties")]
	[SerializeField] private int _turnsUntilAction;
	[SerializeField] private EntityType _entityType;
	[Header("Information")]
	[SerializeField] private Tile _tile;

	/// <summary>
	/// The tile that this entity is currently standing on
	/// </summary>
	public Tile Tile {
		get => _tile;
		set {
			// If the tile is being set to the same value, return
			if (_tile == value) {
				return;
			}

			// Set the tile value and its entity value to this entity
			_tile = value;
			_tile.Entity = this;
		}
	}

	/// <summary>
	/// The type of this entity
	/// </summary>
	public EntityType EntityType { get => _entityType; protected set => _entityType = value; }

	/// <summary>
	/// The board position of this entity
	/// </summary>
	public Vector2Int BoardPosition => Tile.BoardPosition;

	/// <summary>
	/// The number of turns until this entity does its action
	/// </summary>
	public int TurnsUntilAction { get => _turnsUntilAction; set => _turnsUntilAction = value; }

	/// <summary>
	/// A custom action that this entity performs when it is its turn
	/// </summary>
	public abstract void PerformAction ( );
}
