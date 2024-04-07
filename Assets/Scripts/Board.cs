using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : Singleton<Board> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[Header("Properties")]
	[SerializeField, Min(1)] private int tileCount;
	[SerializeField, Min(1)] private int minGroupTileCount;
	[SerializeField, Min(1)] private int maxGroupTileCount;

	private List<List<Tile>> tiles = new List<List<Tile>>( );

	protected override void Awake ( ) {
		base.Awake( );

		// Declare the list of tile groups
		tiles = new List<List<Tile>>( );
	}

	private void Start ( ) {
		Generate( );
	}

	/// <summary>
	/// Clear all the tiles that are currently on the board
	/// </summary>
	private void Clear ( ) {
		// Destroy all tiles that are already in the list
		while (tiles.Count > 0) {
			while (tiles[0].Count > 0) {
				Destroy(tiles[0][0].gameObject);
				tiles[0].RemoveAt(0);
			}

			tiles.RemoveAt(0);
		}
	}

	/// <summary>
	/// Generate all of the tiles and tile groups that will be on the board
	/// </summary>
	private void Generate ( ) {
		// Lists that store what positions on the board have already been taken or are available to spawn a tile at
		List<Vector2Int> availablePositions = new List<Vector2Int>( ) { Vector2Int.zero };
		List<Vector2Int> takenPositions = new List<Vector2Int>( );
		List<Tile> createdTiles = new List<Tile>( );

		// Generate all the tiles
		for (int i = 0; i < tileCount; i++) {
			// Get a random available position from the list
			Vector2Int randomBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];
			availablePositions.Remove(randomBoardPosition);

			// Create a new tile at that position
			createdTiles.Add(CreateTile(randomBoardPosition));
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

		/*
		// Select random tiles on the board that will be starting points for the tile groups
		for (int i = 0; i < tileGroupCount; i++) {
			// Find a random tile on the board
			Tile randomTile = createdTiles[Random.Range(0, createdTiles.Count)];
			createdTiles.Remove(randomTile);

			// Add the tile to its tile group
			tiles.Add(new List<Tile>( ) { randomTile });
			randomTile.TileGroupID = i;

			// TEST: change the color of the tile to a random color to show the different tile groups
			randomTile.GetComponent<SpriteRenderer>( ).color = Color.HSVToRGB((float) i / tileGroupCount, 1f, 0.5f);
		}
		*/

		// The number of groups that have currently been created
		int groupCount = 0;

		while (createdTiles.Count > 0) {
			// Generate a random size for the current tile group
			int groupTileCount = Random.Range(minGroupTileCount, maxGroupTileCount + 1);

			// Find a random tile on the board to start the tile group
			Tile randomTile = createdTiles[Random.Range(0, createdTiles.Count)];
			createdTiles.Remove(randomTile);

			for (int i = 0; i < groupTileCount; i++) {
				// Add the tile to its tile group
				tiles.Add(new List<Tile>( ) { randomTile });
				randomTile.TileGroupID = groupCount;
			}

			groupCount++;
		}
	}

	/// <summary>
	/// Get the tile at the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check for a tile at</param>
	/// <returns>A reference to the tile if there is one at the inputted board position, null otherwise</returns>
	private Tile GetTile (Vector2Int boardPosition) {
		// The raycast origin will be right above the tile on the z axis
		Vector3 raycastOrigin = BoardPositionToWorldPosition(boardPosition) + Vector3.back;

		// Fire a raycast in the direction of the tiles. If it hits a tile, return it
		if (Physics.Raycast(raycastOrigin, Vector3.forward, out RaycastHit hit)) {
			return hit.transform.GetComponent<Tile>( );
		}

		return null;
	}

	/// <summary>
	/// Create a new tile on the board
	/// </summary>
	/// <param name="boardPosition">The board position to initially set the tile to</param>
	/// <param name="tileGroupID">The ID of the tile group for this block</param>
	/// <returns>A reference to the newly created tile</returns>
	public Tile CreateTile (Vector2Int boardPosition, int tileGroupID = -1) {
		// Create the tile object in the scene
		Tile tile = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<Tile>( );
		
		// Set the tile variables
		tile.BoardPosition = boardPosition;
		tile.TileGroupID = tileGroupID;

		return tile;
	}

	/// <summary>
	/// Convert a board position to a world position
	/// </summary>
	/// <param name="boardPosition">The 2D board position to convert</param>
	/// <returns>A Vector3 that is the world position equivelant to the inputted board position</returns>
	public static Vector3 BoardPositionToWorldPosition (Vector2Int boardPosition) {
		return new Vector3((boardPosition.x + boardPosition.y) * 0.5f, (boardPosition.y - boardPosition.x) * 0.25f);
	}
}
