using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Robot : Entity {
	[Header("Robot Subclass - References")]
	[SerializeField] private SpriteRenderer arrowSpriteRenderer;
	[SerializeField] private Transform arrowTransform;
	[SerializeField] private Sprite[ ] arrowSprites;
	[SerializeField] private Sprite[ ] robotSprites;

	protected override void SetDirection (Vector2Int facingDirection) {
		base.SetDirection(facingDirection);

		// Set the position of the arrow to be at the new facing direction
		arrowTransform.localPosition = BoardManager.Instance.BoardToWorldPosition(Direction);

		// Make sure the arrow and robot are facing the right direction and have the right sprite
		arrowSpriteRenderer.flipX = entitySpriteRenderer.flipX = isFacingLeft;
		arrowSpriteRenderer.sprite = arrowSprites[isFacingUp ? 0 : 1];
		entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 0 : 1];

		// Set the sorting order of the arrow, making sure that it appears above the tile it is on
		// This makes the arrow show up above the entity that is on the tile it is placed on
		arrowSpriteRenderer.sortingOrder = entitySpriteRenderer.sortingOrder + (isFacingUp ? -4 : 6);
	}

	protected override void SetBoardPosition (Vector2Int boardPosition) {
		base.SetBoardPosition(boardPosition);

		arrowSpriteRenderer.sortingOrder = entitySpriteRenderer.sortingOrder + (isFacingUp ? -4 : 6);
	}

	public override void PerformTurn ( ) {
		// If this robot is killed, then return from the function
		if (isKilled) {
			return;
		}

		// The robots turn counter never changes as it moves every turn
		// It should also always be the first entity to move each turn

		// Get the tile that the robot is going to move to (if it exists)
		List<Tile> toTileList = BoardManager.Instance.SearchForTilesAt(new List<Vector2Int>( ) { BoardPosition + Direction });

		// If there is no tile found, then the robot is killed
		if (toTileList.Count == 0) {
			/// TODO: Play some animation of the robot walking forward and then falling off
			
			Kill( );
			return;
		}

		Tile toTile = toTileList[0];

		// If the tile that this entity is walking to has an entity on it, do certain things 
		if (toTile.Entity != null) {
			// If the entity is a laser, then do nothing. The robot just gets stuck if it tries to walk into a laser
			if (toTile.Entity.EntityType == EntityType.LASER) {
				return;
			}

			// If the entity is a spike or a bomb, then the robot dies
			if (toTile.Entity.EntityType == EntityType.SPIKE || toTile.Entity.EntityType == EntityType.BOMB) {
				Kill( );
				return;
			}
		}

		/// TODO: Play some animation of robot walking towards the tile location

		// If the tile is found, then set that tile to have this robot as its entity
		Tile.Entity = null;
		Tile = toTile;
	}

	protected override void UpdateHazardPositions ( ) { }

	public override void Kill ( ) {
		isKilled = true;
		EntityManager.Instance.EntityTurnQueue.Remove(this);
		EntityManager.Instance.Entities.Remove(this);

		// TODO: Play animation of robot exploding

		Destroy(gameObject);

		GameManager.Instance.SetGameState(GameState.GAME_OVER);
	}
}
