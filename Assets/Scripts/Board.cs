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

	private List<List<Tile>> _tiles = new List<List<Tile>>( );

	public List<List<Tile>> Tiles { get => _tiles; private set => _tiles = value; }

	protected override void Awake ( ) {
		base.Awake( );

		// Declare the list of tile groups
		Tiles = new List<List<Tile>>( );
	}

	private void Start ( ) {
		Generate( );
	}

	/// <summary>
	/// Generate all of the tiles and tile groups that will be on the board
	/// </summary>
	private void Generate ( ) {
		// A list that stores all of the available positions to expand
		List<Vector2Int> availablePositions = new List<Vector2Int>( ) { Vector2Int.zero };

		// Generate all the tiles
		for (int i = 0; i < tileCount; i++) {
			// Get a random available position from the list
			Vector2Int newBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];
			availablePositions.Remove(newBoardPosition);

			// Get the cardinal tile group IDs surrounding the board position
			// Need to subtract 1 from max so the method accounts for the fact that the current tile will be added to the tile group
			List<int> cardinalTileGroupIDs = GetCardinalTileGroupIDs(newBoardPosition, maxGroupSize: maxGroupTileCount - 1);

			// If there are no valid cardinal tile group IDs, create a new tile group
			// If there are valid cardinal tile group IDs, then select a random one to set the tile to
			if (cardinalTileGroupIDs.Count == 0) {
				// The new tile group ID will be the current count of tile groups on the board
				CreateTile(newBoardPosition, tileGroupID: Tiles.Count);
			} else {
				// Select the smallest cardinal tile group to add this tile to
				CreateTile(newBoardPosition, tileGroupID: cardinalTileGroupIDs[0]);
			}

			// Add all surrounding tile positions to the available tile positions
			foreach (Vector2Int cardinalPosition in GetCardinalVoids(newBoardPosition)) {
				// Do not add the position if it has already been added
				if (!availablePositions.Contains(cardinalPosition)) {
					availablePositions.Add(cardinalPosition);
				}
			}
		}

		// TEST: Set each tile group to a different color to better visualize them
		for (int i = 0; i < Tiles.Count; i++) {
			Color color = Color.HSVToRGB((float) i / Tiles.Count, 1f, 1f);
			for (int j = 0; j < Tiles[i].Count; j++) {
				Tiles[i][j].GetComponent<SpriteRenderer>( ).color = color;
			}
		}
	}

	/// <summary>
	/// Get all cardinal tiles around the specified board position (if they exist)
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal tiles around</param>
	/// <param name="tileGroupID">The ID that the tile must be in order for this function to return a tile object. Having this value be -1 ignores this feature</param>
	/// <returns>A list of all cardinal tiles around the specified board position. If an element is null, then that tile either doesn't exist or does not match the specified tile group ID</returns>
	private List<Tile> GetCardinalTiles (Vector2Int boardPosition, int tileGroupID = -1) {
		// Create a list for storing all of the cardinal tiles
		List<Tile> cardinalTiles = new List<Tile>( );

		// Get a list of all the cardinal positions around the specified board position
		List<Vector2Int> cardinalPositions = GetCardinalBoardPositions(boardPosition);

		// Loop through all the tiles on the board and check to see if there a tile at one of the cardinal positions
		// If there is a tile at the cardinal position, add it to the cardinal tile list
		for (int i = 0; i < Tiles.Count; i++) {
			for (int j = 0; j < Tiles[i].Count; j++) {
				// If one of the tiles on the board is at one of the cardinal positions, then add that tile to be a cardinal tile
				if (cardinalPositions.Contains(Tiles[i][j].BoardPosition)) {
					cardinalTiles.Add(Tiles[i][j]);
				}
			}
		}

		return cardinalTiles;
	}

	/// <summary>
	/// Get all cardinal tile group IDs surrounding the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check around</param>
	/// <param name="minGroupSize">The minimum size for the tile group to be valid</param>
	/// <param name="maxGroupSize">The maximum size for the tile group to be valid</param>
	/// <returns>A distinct list of all cardinal tile group IDs surrounding the specified board position</returns>
	private List<int> GetCardinalTileGroupIDs (Vector2Int boardPosition, int minGroupSize = 0, int maxGroupSize = 9999999) {
		// Create a list for storing all of the cardinal tile group IDs
		List<int> cardinalTileGroupIDs = new List<int>( );

		// Loop through all cardinal tiles and save all valid tile group IDs
		foreach (Tile cardinalTile in GetCardinalTiles(boardPosition)) {
			// If the cardinal tile does not have a tile group or the tile group ID has already been added, then continue to the next tile
			if (cardinalTile.TileGroupID == -1 || cardinalTileGroupIDs.Contains(cardinalTile.TileGroupID)) {
				continue;
			}

			// Get the size of the tile group that the cardinal tile belongs to
			int tileGroupSize = Tiles[cardinalTile.TileGroupID].Count;

			// If the tile group size is within the specified size range, then it is a valid tile group and should be added to the list
			if (tileGroupSize >= minGroupSize && tileGroupSize <= maxGroupSize) {
				cardinalTileGroupIDs.Add(cardinalTile.TileGroupID);
			}
		}

		return cardinalTileGroupIDs.OrderBy(ID => Tiles[ID].Count).ToList( );
	}

	/// <summary>
	/// Get all cardinal voids surrounding the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check around</param>
	/// <returns>A distinct list of all cardinal voids surrounding the specified board position</returns>
	private List<Vector2Int> GetCardinalVoids (Vector2Int boardPosition) {
		// Create a list for storing all of the cardinal voids
		// To start, have all of the possible cardinal directions be voids
		List<Vector2Int> cardinalVoids = GetCardinalBoardPositions(boardPosition);

		// Loop through all of the tiles on the board to check for voids
		for (int i = 0; i < Tiles.Count; i++) {
			for (int j = 0; j < Tiles[i].Count; j++) {
				// If a tile is currently on one of the possible void positions, then remove the cardinal position
				if (cardinalVoids.Contains(Tiles[i][j].BoardPosition)) {
					cardinalVoids.Remove(Tiles[i][j].BoardPosition);
				}
			}
		}

		return cardinalVoids;
	}

	/// <summary>
	/// Get all cardinal board positions around the specified board position
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

		// Set the variables of the tile
		tile.BoardPosition = boardPosition;
		tile.TileGroupID = tileGroupID;

		return tile;
	}

	/// <summary>
	/// Clear all the tiles that are currently on the board
	/// </summary>
	private void Clear ( ) {
		// Destroy all tiles that are already in the list
		while (Tiles.Count > 0) {
			while (Tiles[0].Count > 0) {
				Destroy(Tiles[0][0].gameObject);
				Tiles[0].RemoveAt(0);
			}

			Tiles.RemoveAt(0);
		}
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
