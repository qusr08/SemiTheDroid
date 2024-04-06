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

	public void Generate ( ) {
		List<Vector2Int> availablePositions = new List<Vector2Int>( ) { Vector2Int.zero };
		List<Vector2Int> takenPositions = new List<Vector2Int>( );

		// Generate a random size for the tile group
		int tileGroupSize = Random.Range(minGroupSize, maxGroupSize + 1);

		for (int i = 0; i < tileGroupSize; i++) {
			// Get a random available position from the list
			Vector2Int randomBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];

			// Create a new tile at that position
			Board.Instance.CreateTile(this, randomBoardPosition);
			availablePositions.Remove(randomBoardPosition);
			takenPositions.Add(randomBoardPosition);

			Vector2Int upPosition = randomBoardPosition + Vector2Int.up;
			Vector2Int rightPosition = randomBoardPosition + Vector2Int.right;
			Vector2Int downPosition = randomBoardPosition + Vector2Int.down;
			Vector2Int leftPosition = randomBoardPosition + Vector2Int.left;

			// Add all surrounding tile positions to the available tile positions
			// Do not add the position if it has already been taken by a tile though OR if it has already been added
			if (!takenPositions.Contains(upPosition) && !availablePositions.Contains(upPosition)) {
				availablePositions.Add(upPosition);
			}
			if (!takenPositions.Contains(rightPosition) && !availablePositions.Contains(rightPosition)) {
				availablePositions.Add(rightPosition);
			}
			if (!takenPositions.Contains(downPosition) && !availablePositions.Contains(downPosition)) {
				availablePositions.Add(downPosition);
			}
			if (!takenPositions.Contains(leftPosition) && !availablePositions.Contains(leftPosition)) {
				availablePositions.Add(leftPosition);
			}
		}
	}
}
