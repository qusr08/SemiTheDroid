using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Entity {
	public override void PerformTurn ( ) {
		// If this bomb is killed, then return from the function
		if (isKilled) {
			return;
		}

		// Decrement the number of turns until this laser does its action
		TurnsUntilAction--;

		// Only do this laser's action once its turn count is equal to 0
		if (TurnsUntilAction > 0) {
			return;
		}

		// TODO: Play animation of bomb exploding

		// Loop through all tiles that are effected by the hazard positions of this laser
		foreach (Tile effectedTile in BoardManager.Instance.SearchForTilesAt(HazardPositions, onlyEntityTiles: true)) {
			// Since there is an entity on the current tile, kill it
			effectedTile.Entity.Kill( );
		}

		// When a bomb performs its turn it explodes, killing itself
		Kill( );
	}

	public override void Kill ( ) {
		isKilled = true;
		EntityManager.Instance.EntityTurnQueue.Remove(this);
		EntityManager.Instance.Entities.Remove(this);

		// Immediately perform this bomb's turn if it is killed
		// This will cause the bomb to explode
		if (TurnsUntilAction > 0) {
			PerformTurn( );
		}

		Destroy(gameObject);
	}

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
}
