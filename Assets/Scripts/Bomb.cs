using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Entity {
	[Header("Bomb Subclass - References")]
	[SerializeField] private Sprite[ ] bombSprites;

	protected override void UpdateHazardPositions ( ) {
		// For bombs, its hazard positions will be around its board position
		List<Vector2Int> newHazardPositions = new List<Vector2Int>( );

		// Loop through all the positions around the bomb's board position
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				newHazardPositions.Add(BoardPosition + new Vector2Int(x, y));
			}
		}

		// Set the hazard board positions to the new area
		HazardPositions = newHazardPositions;

		// Update the shown hazard board positions in the main entity manager class
		EntityManager.Instance.UpdateShownHazardPositions( );
	}

	public override IEnumerator PerformTurn ( ) {
		// If this bomb is killed, then return from the function
		if (isKilled) {
			yield break;
		}

		// Decrement the number of turns until this laser does its action
		TurnsUntilAction--;

		// Only do this laser's action once its turn count is equal to 0
		if (TurnsUntilAction > 0) {
			yield break;
		}

		// Loop through all tiles that are effected by the hazard positions of this laser
		foreach (Tile effectedTile in BoardManager.Instance.SearchForTilesAt(HazardPositions, onlyEntityTiles: true)) {
			// Since there is an entity on the current tile, kill it
			yield return effectedTile.Entity.OnKill( );
		}
	}

	public override IEnumerator OnKill ( ) {
		// Immediately perform this bomb's turn if it is killed
		// This will cause the bomb to explode
		if (TurnsUntilAction > 0) {
			TurnsUntilAction = 1;

			yield return PerformTurn( );
		} else {
			isKilled = true;
			EntityManager.Instance.EntityTurnQueue.Remove(this);
			EntityManager.Instance.Entities.Remove(this);

			StartCoroutine(ExplodeAnimation( ));
		}
	}

	public override IEnumerator OnCreate ( ) {
		entitySpriteRenderer.sprite = bombSprites[0];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[1];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[2];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[3];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[4];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[5];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[6];
		yield return new WaitForSeconds(GameManager.Instance.AnimationSpeed);
		entitySpriteRenderer.sprite = bombSprites[7];
	}
}
