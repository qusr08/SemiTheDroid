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
	}

	protected override void SetBoardPosition (Vector2Int boardPosition) {
		base.SetBoardPosition(boardPosition);

		// Set the sorting order of the arrow, making sure that it appears above the tile it is on
		// This makes the arrow show up above the entity that is on the tile it is placed on
		arrowSpriteRenderer.sortingOrder = entitySpriteRenderer.sortingOrder + (isFacingUp ? -4 : 6);
	}

	public override void PerformTurn ( ) {
		// If this robot is killed, then return from the function
		if (isKilled) {
			return;
		}

	}

	protected override void UpdateHazardPositions ( ) { }

	public override void Kill ( ) {
		isKilled = true;
		EntityManager.Instance.EntityTurnQueue.Remove(this);

		// TODO: Play animation of robot exploding

		Destroy(gameObject);

		GameManager.Instance.SetGameState(GameState.GAME_OVER);
	}
}
