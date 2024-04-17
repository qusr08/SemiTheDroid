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
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private List<Vector2Int> _hazardPositions;

	protected bool isFacingUp;
	protected bool isFacingLeft;

	/// <summary>
	/// The tile that this entity is currently standing on
	/// </summary>
	public Tile Tile {
		get => _tile;
		set {
			if (_tile == value) {
				return;
			}

			_tile = value;
			_tile.Entity = this;
			BoardPosition = _tile.BoardPosition;
		}
	}

	/// <summary>
	/// A list of all the board positions that will be hazards based on this entity's attack
	/// </summary>
	public List<Vector2Int> HazardPositions { get => _hazardPositions; protected set => _hazardPositions = value; }

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
	public Vector2Int BoardPosition { get => _boardPosition; set => SetBoardPosition(value); }

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

	private void Awake ( ) {
		HazardPositions = new List<Vector2Int>( );
	}

	private void Start ( ) {
		EntityManager.Instance.Entities.Add(this);
	}

	protected void OnMouseEnter ( ) {
		// Set the entity that is being hovered to this entity
		EntityManager.Instance.HoveredEntity = this;
	}

	protected void OnMouseExit ( ) {
		// If the entity that is currently being hovered is not equal to this, then return
		if (EntityManager.Instance.HoveredEntity != this) {
			return;
		}

		// Set there to be no hovered entity anymore
		EntityManager.Instance.HoveredEntity = null;
	}

	private void OnDestroy ( ) {
		// If the entity manager still exists, then remove this entity from the main entity list
		if (EntityManager.Instance != null) {
			EntityManager.Instance.Entities.Remove(this);
		}
	}

	/// <summary>
	/// Set the board position of this entity
	/// </summary>
	/// <param name="boardPosition"></param>
	protected virtual void SetBoardPosition (Vector2Int boardPosition) {
		// Do nothing if the board position is being set to the same value
		if (_boardPosition == boardPosition) {
			return;
		}

		_boardPosition = boardPosition;

		// Set the sprite sorting order based on the new position
		entitySpriteRenderer.sortingOrder = ((boardPosition.x - boardPosition.y) * 5) + 3;

		// Since the position of this entity has changed, update its hazard board positions
		UpdateHazardPositions( );
	}

	/// <summary>
	/// Set the direction that this entity is facing
	/// </summary>
	/// <param name="facingDirection"></param>
	protected virtual void SetFacingDirection (Vector2Int facingDirection) {
		// If the facing direction is equal to 0 or it is being set to the same facing direction, then return
		if (facingDirection == Vector2Int.zero || _facingDirection == facingDirection) {
			return;
		}

		_facingDirection = facingDirection;

		// Recalculate if the entity is facing up or left
		isFacingUp = (FacingDirection.x < 0 || FacingDirection.y > 0);
		isFacingLeft = (FacingDirection.x < 0 || FacingDirection.y < 0);

		// Since the direction of this entity has changed, update the hazard board positions
		UpdateHazardPositions( );
	}

	/// <summary>
	/// Update all of the hazard positions of this entity. Make sure to call EntityManager.Instance.UpdateHazardPositions() at the end of the method.
	/// </summary>
	protected abstract void UpdateHazardPositions ( );

	/// <summary>
	/// A custom action that this entity performs when it is its turn
	/// </summary>
	public abstract void PerformAction ( );
}
