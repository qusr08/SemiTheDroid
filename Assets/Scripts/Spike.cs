using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Spike : Entity {
	public override void PerformTurn ( ) { /* The spike is chill and just sits there */ }

	public override void Kill ( ) { /* The spike cannot be killed :) */ }

	protected override void UpdateHazardPositions ( ) {
		// For the spike, its new hazard position will just be its board position
		HazardPositions = new List<Vector2Int>( ) { BoardPosition };

		// Update the shown hazard board positions in the main entity manager class
		EntityManager.Instance.UpdateShownHazardPositions( );
	}
}
