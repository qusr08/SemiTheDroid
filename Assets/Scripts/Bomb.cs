using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : Entity {
	public override void PerformAction ( ) {
		throw new System.NotImplementedException( );
	}

	protected override void UpdateHazardPositions ( ) {
		// For bombs, its hazard positions will be around its board position
		HazardPositions = BoardManager.Instance.SearchForPositionsAround(BoardPosition, 1);

		// Update the shown hazard board positions in the main entity manager class
		EntityManager.Instance.UpdateShownHazardPositions( );
	}
}
