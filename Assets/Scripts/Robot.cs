using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
		arrowSpriteRenderer.sortingOrder = entitySpriteRenderer.sortingOrder + (isFacingUp ? -4 : 6);

		// Set the sprite of the robot
		entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 3 : 7];
	}

	protected override void SetBoardPosition (Vector2Int boardPosition) {
		base.SetBoardPosition(boardPosition);

		// Set the sorting order of the arrow, making sure that it appears above the tile it is on
		// This makes the arrow show up above the entity that is on the tile it is placed on
		arrowSpriteRenderer.sortingOrder = entitySpriteRenderer.sortingOrder + (isFacingUp ? -4 : 6);
	}

	protected override void UpdateHazardPositions ( ) { }

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
			yield return WalkToTileAnimation(null);
			yield break;
		}

		Tile toTile = toTileList[0];

		// If the tile that this entity is walking to has an entity on it, do certain things 
		if (toTile.Entity != null) {
			// If the entity is a laser, then do nothing. The robot just gets stuck if it tries to walk into a laser
			if (toTile.Entity.EntityType == EntityType.LASER) {
				yield break;
			}

			// If the entity is a spike, then the robot dies
			if (toTile.Entity.EntityType == EntityType.SPIKE) {
				// Do an animation for walking forward
				yield return WalkToTileAnimation(toTile);
				yield return OnKill( );
				yield break;
			}
		}

		// Do an animation for walking forward
		yield return WalkToTileAnimation(toTile);
	}

	public override IEnumerator OnKill ( ) {
		isKilled = true;
		EntityManager.Instance.EntityTurnQueue.Remove(this);
		EntityManager.Instance.Entities.Remove(this);

		// Disable arrow sprite renderer
		arrowSpriteRenderer.enabled = false;

		StartCoroutine(ExplodeAnimation( ));

		yield return GameManager.Instance.SetGameState(GameState.GAME_OVER);
	}

	public override IEnumerator OnCreate ( ) {
		yield return null;
	}

	/// <summary>
	/// Play an animation of this robot walking towards the specified tile
	/// </summary>
	/// <param name="toTile">The tile to walk towards</param>
	private IEnumerator WalkToTileAnimation (Tile toTile) {
		// Get the direction on the board that the robot will literally move in
		// Since the board is isometric, up is diagonal right instead
		Vector3 boardDirection = BoardManager.Instance.BoardToWorldPosition(Direction);

		// Get a vector for the movement each frame that the robot steps
		Vector3 movement = new Vector3(boardDirection.x, boardDirection.y, 0) * 0.25f;

		// Each frame that the robot is falling, it will be moved by this amount
		Vector3 fallMovement = new Vector3(0, -1f, 0);

		// If the robot is facing down, set the tile before walking
		if (!isFacingUp) {
			// Set the new tile
			Tile.Entity = null;
			Tile = toTile;

			// If the tile is equal to null, set the board position manually
			// This is mainly used to update the sprite renderer sorting order
			if (toTile == null) {
				BoardPosition += Direction;
			} else {
				// Set the transform to be back where the robot was just standing before the tile was set
				transform.localPosition = new Vector3(-boardDirection.x, -boardDirection.y, transform.localPosition.z);
			}
		}

		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 0 : 4];
		transform.localPosition += movement;
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 1 : 5];
		transform.localPosition += movement;
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 2 : 6];
		transform.localPosition += movement;
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 3 : 7];
		transform.localPosition += movement;

		// If the robot is facing up, set the tile after walking
		if (isFacingUp) {
			// Set the new tile
			Tile.Entity = null;
			Tile = toTile;

			// If the tile is equal to null, set the board position manually
			// This is mainly used to update the sprite renderer sorting order
			if (toTile == null) {
				BoardPosition += Direction;
			}
		}

		// If the tile was equal to null, then have the robot fall down and be killed
		if (toTile == null) {
			// Disable arrow sprite renderer
			arrowSpriteRenderer.enabled = false;

			yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
			entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 0 : 4];
			transform.localPosition += fallMovement;
			yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
			entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 1 : 5];
			transform.localPosition += fallMovement;
			yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
			entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 2 : 6];
			transform.localPosition += fallMovement;
			yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
			entitySpriteRenderer.sprite = robotSprites[isFacingUp ? 3 : 7];
			transform.localPosition += fallMovement;

			yield return OnKill( );
		}
	}
}

