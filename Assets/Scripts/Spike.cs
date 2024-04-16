using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spike : Entity {
	protected override void SetBoardPosition (Vector2Int boardPosition) {
		base.SetBoardPosition(boardPosition);

		// Test for spikes
		HazardBoardPositions.Clear( );
		HazardBoardPositions.Add(boardPosition);

		Debug.Log("SPIKE SET HAZARD");
	}

	public override void PerformAction ( ) { }
}
