using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RobotSpriteType {
	UP_1, UP_2, UP_3, UP_4,
	DOWN_1, DOWN_2, DOWN_3, DOWN_4
}

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
		entitySpriteRenderer.sprite = robotSprites[(int) (isFacingUp ? RobotSpriteType.UP_4 : RobotSpriteType.DOWN_4)];

		UpdateArrowSpriteSortOrder( );
	}

	protected override void SetBoardPosition (Vector2Int boardPosition) {
		base.SetBoardPosition(boardPosition);

		UpdateArrowSpriteSortOrder( );
	}

	protected override void UpdateHazardPositions ( ) { }

	private void UpdateArrowSpriteSortOrder ( ) {
		// Set the sorting order of the arrow, making sure that it appears above the tile it is on
		// This makes the arrow show up above the entity that is on the tile it is placed on
		arrowSpriteRenderer.sortingOrder = entitySpriteRenderer.sortingOrder + (isFacingUp ? -4 : 6);
	}

	public override IEnumerator PerformTurn ( ) {
		// If this robot is killed, then return from the function
		if (isKilled) {
			yield break;
		}

		// The robots turn counter never changes as it moves every turn
		// It should also always be the first entity to move each turn

		// Get the tile that the robot is going to move to (if it exists)
		List<Tile> toTileList = BoardManager.Instance.SearchForTilesAt(new List<Vector2Int>( ) { BoardPosition + Direction });

		// If there is no tile found, then the robot is killed
		if (toTileList.Count == 0) {
			yield return OnKill( );
			yield break;
		}

		Tile toTile = toTileList[0];

		// If the tile that this entity is walking to has an entity on it, do certain things 
		if (toTile.Entity != null) {
			// If the entity is a laser, then do nothing. The robot just gets stuck if it tries to walk into a laser
			if (toTile.Entity.EntityType == EntityType.LASER) {
				yield break;
			}

			// If the entity is a spike or a bomb, then the robot dies
			if (toTile.Entity.EntityType == EntityType.SPIKE || toTile.Entity.EntityType == EntityType.BOMB) {
				yield return OnKill( );
				yield break;
			}
		}

		// If the tile is found and doesnt have an entity on it, then set that tile to have this robot as its entity
		Tile.Entity = null;
		Tile = toTile;
	}

	public override IEnumerator OnKill ( ) {
		isKilled = true;
		EntityManager.Instance.EntityTurnQueue.Remove(this);
		EntityManager.Instance.Entities.Remove(this);

		/// TODO: Play animation of robot exploding

		Destroy(gameObject);

		yield return GameManager.Instance.SetGameState(GameState.GAME_OVER);
	}

	public override IEnumerator OnCreate ( ) {
		yield return null;
	}
}
