using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EntityType {
	SPIKE, ROBOT, LASER, BOMB
}

public enum EntitySpriteType {
	NONE = -1, 
	EXPL_1, EXPL_2, EXPL_3, EXPL_4
}

public abstract class Entity : MonoBehaviour {
	[Header("References")]
	[SerializeField] protected SpriteRenderer entitySpriteRenderer;
	[SerializeField] private Sprite[ ] entitySprites;
	[Header("Properties")]
	[SerializeField] private int _turnsUntilAction;
	[SerializeField] private int _turnOrder;
	[SerializeField] private EntityType _entityType;
	[SerializeField] private string _entityName;
	[SerializeField] private string _entityDescription;
	[Header("Information")]
	[SerializeField] private Tile _tile;
	[SerializeField] private Vector2Int _direction;
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private List<Vector2Int> _hazardPositions;
	[SerializeField] protected bool isKilled;
	[SerializeField] protected bool isFacingUp;
	[SerializeField] protected bool isFacingLeft;
	[SerializeField] private EntitySpriteType entitySpriteType;

	/// <summary>
	/// The name of this entity
	/// </summary>
	public string EntityName { get => _entityName; private set => _entityName = value; }

	/// <summary>
	/// The description of this entity
	/// </summary>
	public string EntityDescription { get => _entityDescription; private set => _entityDescription = value; }

	/// <summary>
	/// The tile that this entity is currently standing on
	/// </summary>
	public Tile Tile {
		get => _tile;
		set {
			// If the tile is being set to the same value, return and do nothing
			if (_tile == value) {
				return;
			}

			_tile = value;

			// If the new tile is not null, then set this entity's board position and set the new tile's entity
			if (_tile != null) {
				BoardPosition = _tile.BoardPosition;

				_tile.Entity = this;
			}
		}
	}

	/// <summary>
	/// A list of all the board positions that will be hazards based on this entity's attack
	/// </summary>
	public List<Vector2Int> HazardPositions { get => _hazardPositions; protected set => _hazardPositions = value; }

	/// <summary>
	/// The direction that this entity is currently facing
	/// </summary>
	public Vector2Int Direction { get => _direction; set => SetDirection(value); }

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
	public int TurnsUntilAction { get => _turnsUntilAction; set => _turnsUntilAction = value; }

	/// <summary>
	/// The order within the actions taking place on this entities turn that this entity will act
	/// </summary>
	public int TurnOrder { get => _turnOrder; set => _turnOrder = value; }

	private void Awake ( ) {
		HazardPositions = new List<Vector2Int>( );
		isKilled = false;
		isFacingUp = false;
		isFacingLeft = false;
	}

	protected void OnMouseEnter ( ) {
		// If there is currently a selected tile group, then entities should not be able to be hovered
		if (GameManager.Instance.GameState != GameState.PLAYER_TURN) {
			return;
		}

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
		// If the entity manager still exists, then remove this entity from the main entity lists
		if (EntityManager.Instance != null) {
			EntityManager.Instance.Entities.Remove(this);
		}
	}

	/// <summary>
	/// Set the board position of this entity
	/// </summary>
	/// <param name="boardPosition"></param>
	protected virtual void SetBoardPosition (Vector2Int boardPosition) {
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
	protected virtual void SetDirection (Vector2Int facingDirection) {
		// If the facing direction is equal to 0 or it is being set to the same facing direction, then return
		if (facingDirection == Vector2Int.zero || _direction == facingDirection) {
			return;
		}

		_direction = facingDirection;

		// Recalculate if the entity is facing up or left
		isFacingUp = (Direction.x < 0 || Direction.y > 0);
		isFacingLeft = (Direction.x < 0 || Direction.y < 0);

		// Since the direction of this entity has changed, update the hazard board positions
		UpdateHazardPositions( );
	}

	/// <summary>
	/// Update all of the hazard positions of this entity. Make sure to call EntityManager.Instance.UpdateHazardPositions() at the end of the method.
	/// </summary>
	protected abstract void UpdateHazardPositions ( );

	/// <summary>
	/// Set the sprite type of this entity
	/// </summary>
	/// <param name="entitySpriteType">The entity sprite to set</param>
	private void SetEntitySpriteType (EntitySpriteType entitySpriteType) {
		// Do not update the sprite if it is being set to the same value
		if (this.entitySpriteType == entitySpriteType) {
			return;
		}

		this.entitySpriteType = entitySpriteType;

		// Set the sprite of the entity
		if (this.entitySpriteType != EntitySpriteType.NONE) {
			entitySpriteRenderer.sprite = entitySprites[(int) this.entitySpriteType];
		} else {
			entitySpriteRenderer.sprite = null;
		}
	}

	protected IEnumerator ExplodeAnimation ( ) {
		// Make sure the explosion goes over all surrounding tiles and entities
		entitySpriteRenderer.sortingOrder = 999;

		// Go through all the sprites in the animation
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		SetEntitySpriteType(EntitySpriteType.EXPL_1);
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		SetEntitySpriteType(EntitySpriteType.EXPL_2);
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		SetEntitySpriteType(EntitySpriteType.EXPL_3);
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		SetEntitySpriteType(EntitySpriteType.EXPL_4);
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		SetEntitySpriteType(EntitySpriteType.NONE);
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);

		// If the type of this entity is a robot, then set the game state to game over
		if (EntityType == EntityType.ROBOT) {
			yield return GameManager.Instance.SetGameState(GameState.GAME_OVER);
		}

		// Destroy the game object
		Destroy(gameObject);
	}

	/// <summary>
	/// A custom action that this entity performs when it is its turn
	/// </summary>
	public abstract IEnumerator PerformTurn ( );

	/// <summary>
	/// A function called whenever this entity is killed on the board
	/// </summary>
	public abstract IEnumerator OnKill ( );

	/// <summary>
	/// Called whenever this entity is created
	/// </summary>
	public abstract IEnumerator OnCreate ( );
}
