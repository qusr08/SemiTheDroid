using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType {
	SPIKE, ROBOT, LASER, BOMB
}

public abstract class Entity : MonoBehaviour {
	[Header("References")]
	[SerializeField] protected SpriteRenderer entitySpriteRenderer;
	[Header("Properties")]
	[SerializeField] private int _turnsUntilAction;
	[SerializeField] private int _turnOrder;
	[SerializeField] private EntityType _entityType;
	[Header("Information")]
	[SerializeField] private Tile _tile;
	[SerializeField] private Vector2Int _facingDirection;

	protected bool isFacingUp;
	protected bool isFacingLeft;

	/// <summary>
	/// The tile that this entity is currently standing on
	/// </summary>
	public Tile Tile { get => _tile; set => SetTile(value); }

	/// <summary>
	/// The direction that this entity is currently facing
	/// </summary>
	public Vector2Int FacingDirection { get => _facingDirection; set => SetFacingDirection(value); }

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
	public int TurnsUntilAction {
		get => _turnsUntilAction;
		set {
			_turnsUntilAction = value;
		}
	}

	/// <summary>
	/// The order within the actions taking place on this entities turn that this entity will act
	/// </summary>
	public int TurnOrder {
		get => _turnOrder;
		set {
			_turnOrder = value;
		}
	}

	protected void OnMouseEnter ( ) {
		/// TODO: Show only this entities hazard tiles
	}

	protected void OnMouseExit ( ) {
		/// TODO: Show all entities hazard tiles
	}

	/// <summary>
	/// Set the tile that this entity is currently on
	/// </summary>
	/// <param name="tile">The tile to set the entity on</param>
	protected virtual void SetTile (Tile tile) {
		// If the tile is being set to the same value, return
		if (_tile == tile) {
			return;
		}

		// Set the tile value and its entity value to this entity
		_tile = tile;
		_tile.Entity = this;
	}

	/// <summary>
	/// Set the direction that this entity is facing
	/// </summary>
	/// <param name="facingDirection"></param>
	protected virtual void SetFacingDirection (Vector2Int facingDirection) {
		// If the facing direction is equal to 0, then return
		if (facingDirection == Vector2Int.zero) {
			return;
		}

		_facingDirection = facingDirection;

		// Recalculate if the entity is facing up or left
		isFacingUp = (FacingDirection.x < 0 || FacingDirection.y > 0);
		isFacingLeft = (FacingDirection.x < 0 || FacingDirection.y < 0);
	}

	/// <summary>
	/// Set this entity's sprite renderer's sorting order
	/// </summary>
	/// <param name="sortingOrder">The sorting order value to set</param>
	public virtual void SetSpriteSortingOrder (int sortingOrder) {
		entitySpriteRenderer.sortingOrder = sortingOrder;
	}

	/// <summary>
	/// A custom action that this entity performs when it is its turn
	/// </summary>
	public abstract void PerformAction ( );
}
