using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : Entity {
	protected override void SetFacingDirection (Vector2Int facingDirection) {
		base.SetFacingDirection(facingDirection);
	}

	protected override void SetBoardPosition (Vector2Int boardPosition) {
		base.SetBoardPosition(boardPosition);
	}

	public override void PerformAction ( ) {
		throw new System.NotImplementedException( );
	}
}
