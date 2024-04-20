using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Entity {
	[Header("Laser Subclass - References")]
	[SerializeField] private SpriteRenderer beamSpriteRenderer;
	[SerializeField] private Sprite[ ] laserSprites;

	protected override void UpdateHazardPositions ( ) {
		// The hazard positions for the laser are going to be along either a horizontal or vertical line, depending on the way it is facing
		// The worst case scenario for the laser is if all of the tiles on the board are in a perfect line, where the laser is at the end of it
		// We need to account for this case as it means that all other cases will be fine
		List<Vector2Int> newHazardPositions = new List<Vector2Int>( );

		// If the facing direction is not equal to zero, then this laser is facing on the x axis
		// If it is not facing any direction on the x axis, it must be facing on the y axis
		if (Direction.x != 0) {
			// Loop and add all possible board positions on the line of the laser
			for (int i = -BoardManager.Instance.TotalTiles + 1; i < BoardManager.Instance.TotalTiles; i++) {
				// Do not add the current board position of this laser to the hazard tile list
				if (i == 0) {
					continue;
				}

				newHazardPositions.Add(BoardPosition + new Vector2Int(i, 0));
			}
		} else {
			// Loop and add all possible board positions on the line of the laser
			for (int i = -BoardManager.Instance.TotalTiles + 1; i < BoardManager.Instance.TotalTiles; i++) {
				// Do not add the current board position of this laser to the hazard tile list
				if (i == 0) {
					continue;
				}

				newHazardPositions.Add(BoardPosition + new Vector2Int(0, i));
			}
		}

		// Set the hazard board positions to the new line
		HazardPositions = newHazardPositions;

		// Update the shown hazard board positions in the main entity manager class
		EntityManager.Instance.UpdateShownHazardPositions( );
	}

	protected override void SetDirection (Vector2Int facingDirection) {
		base.SetDirection(facingDirection);

		beamSpriteRenderer.flipX = entitySpriteRenderer.flipX = isFacingLeft;
	}

	public override IEnumerator PerformTurn ( ) {
		// If this laser is killed, then return from the function
		if (isKilled) {
			yield break;
		}

		// Decrement the number of turns until this laser does its action
		TurnsUntilAction--;

		// Only do this laser's action once its turn count is equal to 0
		if (TurnsUntilAction > 0) {
			yield break;
		}

		// Enable the laser beam
		beamSpriteRenderer.enabled = true;

		// Loop through all tiles that are effected by the hazard positions of this laser
		foreach (Tile effectedTile in BoardManager.Instance.SearchForTilesAt(HazardPositions, onlyEntityTiles: true)) {
			// Since there is an entity on the current tile, kill it
			yield return effectedTile.Entity.OnKill( );
		}

		// Make sure the beam is shown for a little bit before disappearing again
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed * 2);
		beamSpriteRenderer.enabled = false;

		// Lasers stay on the board until they are destroyed, so just get a new turn count for it
		TurnsUntilAction = EntityManager.Instance.GetRandomTurnCount( );
	}

	public override IEnumerator OnKill ( ) {
		isKilled = true;
		EntityManager.Instance.EntityTurnQueue.Remove(this);
		EntityManager.Instance.Entities.Remove(this);

		// Update the laser destroyed stat
		GameManager.Instance.LasersDestroyed++;

		// Disable beam sprite renderer (it shouldn't be enabled at this point, but just in case)
		beamSpriteRenderer.enabled = false;

		StartCoroutine(ExplodeAnimation( ));

		yield return null;
	}

	public override IEnumerator OnCreate ( ) {
		entitySpriteRenderer.sprite = laserSprites[0];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[1];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[2];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[3];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[4];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[5];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[6];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = laserSprites[7];
	}
}
