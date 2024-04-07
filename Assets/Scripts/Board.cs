using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : Singleton<Board> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[Header("Properties")]
	[SerializeField, Min(1)] private int tileCount;

	private List<List<Tile>> tiles = new List<List<Tile>>( );

	protected override void Awake ( ) {
		base.Awake( );

		// Create the list of tiles on the board
		tiles = new List<List<Tile>>( );
	}

	private void Start ( ) {
		Generate( );
	}

	private void Generate ( ) {
		// Destroy all tiles that are already in the list
		while (tiles.Count > 0) {
			while (tiles[0].Count > 0) {
				Destroy(tiles[0][0].gameObject);
				tiles[0].RemoveAt(0);
			}

			tiles.RemoveAt(0);
		}

		// Lists that store what positions on the board have already been taken or are available to spawn a tile at
		List<Vector2Int> availablePositions = new List<Vector2Int>( ) { Vector2Int.zero };
		List<Vector2Int> takenPositions = new List<Vector2Int>( );

		// Generate all the tiles
		for (int i = 0; i < tileCount; i++) {
			// Get a random available position from the list
			Vector2Int randomBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];

			// Create a new tile at that position
			CreateTile(randomBoardPosition);
			availablePositions.Remove(randomBoardPosition);
			takenPositions.Add(randomBoardPosition);

			// Calculate the four cardinal direction vectors from the newly chosen board position
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

	private Tile GetTile (Vector2Int boardPosition) {
		// The raycast origin will be right above the tile on the z axis
		Vector3 raycastOrigin = BoardPositionToWorldPosition(boardPosition) + Vector3.back;

		// Fire a raycast in the direction of the tiles. If it hits a tile, return it
		if (Physics.Raycast(raycastOrigin, Vector3.forward, out RaycastHit hit)) {
			return hit.transform.GetComponent<Tile>( );
		}

		return null;
	}

	public Tile CreateTile (Vector2Int boardPosition, int tileGroupID = -1) {
		Tile tile = Instantiate(tilePrefab, (Vector2) boardPosition, Quaternion.identity, transform).GetComponent<Tile>( );
		tile.BoardPosition = boardPosition;
		tile.TileGroupID = tileGroupID;

		return tile;
	}

	public static Vector3 BoardPositionToWorldPosition (Vector2Int boardPosition) {
		return new Vector3((boardPosition.x + boardPosition.y) * 0.5f, (boardPosition.y - boardPosition.x) * 0.25f);
	}
}
