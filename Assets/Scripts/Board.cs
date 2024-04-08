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
		// A list that stores all of the available positions to expand
		List<Vector2Int> availablePositions = new List<Vector2Int>( ) { Vector2Int.zero };

		// A list of the already taken positions on the board
		List<Vector2Int> takenPositions = new List<Vector2Int>( );

		// A list containing all of the tiles that have been created and are unassigned to a tile group
		// List<Tile> unassignedTiles = new List<Tile>( );

		// Tracks the lowest tile generated on the board
		// This will be used later for the start of generating tile groups
		// Tile lowestTile = null;

		// Generate all the tiles
		for (int i = 0; i < tileCount; i++) {
			// Get a random available position from the list
			Vector2Int newBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];
			availablePositions.Remove(newBoardPosition);

			Debug.Log("Chosen position: " + newBoardPosition);

			// Get the cardinal tile group IDs surrounding the board position
			// Need to subtract 1 from max so the method accounts for the fact that the current tile will be added to the tile group
			List<int> cardinalTileGroupIDs = GetCardinalTileGroupIDs(newBoardPosition, maxGroupSize: maxGroupTileCount - 1);

			Debug.Log("Cardinal tile group count: " + cardinalTileGroupIDs.Count);

			// If there are no valid cardinal tile group IDs, create a new tile group
			// If there are valid cardinal tile group IDs, then select a random one to set the tile to
			if (cardinalTileGroupIDs.Count == 0) {
				// The new tile group ID will be the current count of tile groups on the board
				CreateTile(newBoardPosition, tileGroupID: tiles.Count);
			} else {
				// Select a random cardinal tile group ID
				CreateTile(newBoardPosition, tileGroupID: cardinalTileGroupIDs[Random.Range(0, cardinalTileGroupIDs.Count)]);
			}

			// Add all surrounding tile positions to the available tile positions
			foreach (Vector2Int cardinalPosition in GetCardinalVoids(newBoardPosition)) {
				// Do not add the position if it has already been added
				if (!availablePositions.Contains(cardinalPosition)) {
					availablePositions.Add(cardinalPosition);
					Debug.Log("Added position: " + cardinalPosition);
				}
			}
		}

		// TEST: Set each tile group to a different color to better visualize them
		for (int i = 0; i < tiles.Count; i++) {
			Color color = Color.HSVToRGB((float) i / tiles.Count, 1f, 1f);
			for (int j = 0; j < tiles[i].Count; j++) {
				tiles[i][j].GetComponent<SpriteRenderer>( ).color = color;
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

		/*
		// The number of groups that have currently been created
		int groupCount = 0;

		// Clear the previous board position lists to use for generating the tile groups
		availablePositions.Clear( );
		takenPositions.Clear( );

		// Make sure all tiles are assigned
		while (unassignedTiles.Count > 0) {
			// Generate a random size for the current tile group
			int groupTileCount = Random.Range(minGroupTileCount, maxGroupTileCount + 1);

			// Get the tile group starting tile
			Tile groupStartingTile;
			if (groupCount == 0) {
				groupStartingTile = lowestTile;
			} else {
				groupStartingTile = unassignedTiles[Random.Range(0, unassignedTiles.Count)];
			}

			unassignedTiles.Remove(groupStartingTile);

			Tile randomTile = unassignedTiles[Random.Range(0, unassignedTiles.Count)];
			unassignedTiles.Remove(randomTile);

			for (int i = 1; i < groupTileCount; i++) {
				// Add the tile to its tile group
				tiles.Add(new List<Tile>( ) { randomTile });
				randomTile.TileGroupID = groupCount;
			}

			groupCount++;
		}
		*/
	}

	/*
	/// <summary>
	/// Get the tile at the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check for a tile at</param>
	/// <param name="tileGroupID">The ID that the tile must be in order for this function to return a tile object. Having this value be -1 ignores this feature</param>
	/// <returns>A reference to the tile if there is one at the inputted board position, null otherwise</returns>
	private Tile GetTile (Vector2Int boardPosition, int tileGroupID = -1) {
		// Fire a raycast in the direction of the tiles to see if it hits one
		// RaycastHit2D hit = Physics2D.Raycast((Vector2) BoardPositionToWorldPosition(boardPosition) + new Vector2(0f, 0.25f), Vector2.down, 0.25f);

		// Check to see if the raycast hit something
		// if (hit.collider != null) {
			// Get a reference to the tile that was hit by the raycast
			// Tile hitTile = hit.transform.GetComponent<Tile>( );

			// Only return the tile if it has a matching tile group ID
			// if (tileGroupID == -1 || (tileGroupID != -1 && hitTile != null && hitTile.TileGroupID == tileGroupID)) {
				// return hitTile;
			// }
		// }

		// Fire a raycast in the direction of the tiles to see if it hits one
		Vector3 raycastOrigin = BoardPositionToWorldPosition(boardPosition) + Vector3.back;

		// Check to see if the raycast hit something
		if (Physics.Raycast(raycastOrigin, Vector3.forward, out RaycastHit hit)) {
			// Get a reference to the tile that was hit by the raycast
			Tile hitTile = hit.transform.GetComponent<Tile>( );

			// Only return the tile if it has a matching tile group ID
			if (tileGroupID == -1 || (tileGroupID != -1 && hitTile != null && hitTile.TileGroupID == tileGroupID)) {
				return hitTile;
			}
		}

		return null;
	}
	*/

	/// <summary>
	/// Get all four cardinal tiles around the specified board position (if they exist)
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
		for (int i = 0; i < tiles.Count; i++) {
			for (int j = 0; j < tiles[i].Count; j++) {
				if (cardinalPositions.Contains(tiles[i][j].BoardPosition)) {
					cardinalTiles.Add(tiles[i][j]);
					Debug.Log("GetCardinalTiles -> cardinal position: " + tiles[i][j].BoardPosition);
				}
			}
		}

		/*
		// Loop through all cardinal positions and check to see if a tile exists at the position
		foreach (Vector2Int cardinalPosition in GetCardinalBoardPositions(boardPosition)) {
			// If the tile exists, then add it to the cardinal tiles
			Tile cardinalTile = GetTile(cardinalPosition, tileGroupID: tileGroupID);
			if (cardinalTile != null) {
				cardinalTiles.Add(cardinalTile);

				Debug.Log("GetCardinalTiles -> cardinal position: " + cardinalPosition);
			}
		}
		*/

		Debug.Log("Cardinal tile count: " + cardinalTiles.Count);

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
			int tileGroupSize = tiles[cardinalTile.TileGroupID].Count;

			Debug.Log("Tile group size: " + tileGroupSize);

			// If the tile group size is within the specified size range, then it is a valid tile group and should be added to the list
			if (tileGroupSize >= minGroupSize && tileGroupSize <= maxGroupSize) {
				cardinalTileGroupIDs.Add(cardinalTile.TileGroupID);
			}
		}

		return cardinalTileGroupIDs;
	}

	private List<Vector2Int> GetCardinalVoids (Vector2Int boardPosition) {
		// Create a list for storing all of the cardinal voids
		// To start, have all of the possible cardinal directions be voids
		List<Vector2Int> cardinalVoids = GetCardinalBoardPositions(boardPosition);

		// Loop through all of the tiles on the board to check for voids
		for (int i = 0; i < tiles.Count; i++) {
			for (int j = 0; j < tiles[i].Count; j++) {
				// If a tile is currently on one of the possible void positions, then remove the cardinal position
				if (cardinalVoids.Contains(tiles[i][j].BoardPosition)) {
					cardinalVoids.Remove(tiles[i][j].BoardPosition);
				}
			}
		}

		/*
		// Loop through all cardinal positions and add them to the void list if a tile does not exist at the board position
		foreach (Vector2Int cardinalPosition in GetCardinalBoardPositions(boardPosition)) {
			// Add the position if a tile does not exist there
			if (GetTile(cardinalPosition) == null) {
				cardinalVoids.Add(cardinalPosition);
			}
		}
		*/

		return cardinalVoids;
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

		// Set the board position of the tile
		tile.BoardPosition = boardPosition;

		// Set the tile group ID of this tile
		// If the specified tile group is greater than or equal to the current amount of tile groups, then create a new group
		// If there is already a tile group for the specified ID, then add this tile to that group
		if (tileGroupID == tiles.Count) {
			tiles.Add(new List<Tile>( ) { tile });
		} else if (tileGroupID != -1) {
			tiles[tileGroupID].Add(tile);
		}
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
