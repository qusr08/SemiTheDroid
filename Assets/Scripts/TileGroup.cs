using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGroup : MonoBehaviour {
	[Header("Properties")]
	[SerializeField, Min(1)] private int minGroupSize;
	[SerializeField, Min(1)] private int maxGroupSize;

	public Vector2 CenterPosition {
		get {
			// If this tile group has no tiles, then the center should just be (0, 0)
			if (transform.childCount == 0) {
				return Vector2.zero;
			}

			// The sum of all the positions of each tile that is a part of this tile group
			Vector2 sumPosition = Vector2.zero;

			// Add up all of the tile positions
			foreach (Tile tile in transform.GetComponentsInChildren<Tile>( )) {
				sumPosition += tile.BoardPosition;
			}

			// Divide the sum of the positions to find the average position of all the tiles
			Vector2 averagePosition = sumPosition / transform.childCount;

			// Find the nearest isometric world position to be the center of the rotation
			float centerX = Mathf.Round(averagePosition.x * 2f) / 2f;
			float centerY = Mathf.Round(averagePosition.y * 4f) / 4f;
			return new Vector2(centerX, centerY);
		}
	}
}
