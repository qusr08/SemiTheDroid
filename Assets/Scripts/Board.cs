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

		// A list containing all of the tiles that have been created and are unassigned to a tile group
		List<Tile> unassignedTiles = new List<Tile>( );

		// Generate all the tiles
		for (int i = 0; i < tileCount; i++) {
			// Get a random available position from the list
			Vector2Int randomBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];
			availablePositions.Remove(randomBoardPosition);

			// Create a new tile at that position
			unassignedTiles.Add(CreateTile(randomBoardPosition));
			takenPositions.Add(randomBoardPosition);

			// Add all surrounding tile positions to the available tile positions
			foreach (Vector2Int cardinalPosition in GetCardinalBoardPositions(randomBoardPosition)) {
				// Do not add the position if it has already been taken by a tile though OR if it has already been added
				if (!takenPositions.Contains(cardinalPosition) && !availablePositions.Contains(cardinalPosition)) {
					availablePositions.Add(cardinalPosition);
				}
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

		while (unassignedTiles.Count > 0) {
			// Generate a random size for the current tile group
			int groupTileCount = Random.Range(minGroupTileCount, maxGroupTileCount + 1);

			// Find a random tile on the board to start the tile group
			Tile randomTile = unassignedTiles[Random.Range(0, unassignedTiles.Count)];
			unassignedTiles.Remove(randomTile);

			for (int i = 1; i < groupTileCount; i++) {
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
	/// <param name="tileGroupID">The ID that the tile must be in order for this function to return a tile object. Having this value be -1 ignores this feature</param>
	/// <returns>A reference to the tile if there is one at the inputted board position, null otherwise</returns>
	private Tile GetTile (Vector2Int boardPosition, int tileGroupID = -1) {
		// The raycast origin will be right above the tile on the z axis
		Vector3 raycastOrigin = BoardPositionToWorldPosition(boardPosition) + Vector3.back;

		// Fire a raycast in the direction of the tiles to see if it hits one
		if (Physics.Raycast(raycastOrigin, Vector3.forward, out RaycastHit hit)) {
			// Get a reference to the tile that was hit by the raycast
			Tile hitTile = hit.transform.GetComponent<Tile>( );

			// Only return the tile if it has a matching tile group ID
			if ((tileGroupID != -1 && hitTile != null && hitTile.TileGroupID == tileGroupID) || tileGroupID == -1) {
				return hitTile;
			}
		}

		return null;
	}

	/// <summary>
	/// Get all four cardinal tiles around the specified board position (if they exist)
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal tiles around</param>
	/// <param name="tileGroupID">The ID that the tile must be in order for this function to return a tile object. Having this value be -1 ignores this feature</param>
	/// <returns>A list of all cardinal tiles around the specified board position. If an element is null, then that tile either doesn't exist or does not match the specified tile group ID</returns>
	private List<Tile> GetCardinalTiles (Vector2Int boardPosition, int tileGroupID = -1) {
		return new List<Tile>( ) {
			GetTile(boardPosition + Vector2Int.up, tileGroupID: tileGroupID),
			GetTile(boardPosition + Vector2Int.right, tileGroupID: tileGroupID),
			GetTile(boardPosition + Vector2Int.down, tileGroupID: tileGroupID),
			GetTile(boardPosition + Vector2Int.left, tileGroupID: tileGroupID)
		};
	}

	/// <summary>
	/// Get all four cardinal board positions around the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal board positions around</param>
	/// <returns>A list of all the cardinal board positions around the specified board position</returns>
	private List<Vector2Int> GetCardinalBoardPositions (Vector2Int boardPosition) {
		return new List<Vector2Int>( ) {
			boardPosition + Vector2Int.up,
			boardPosition + Vector2Int.right,
			boardPosition + Vector2Int.down,
			boardPosition + Vector2Int.left
		};
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
