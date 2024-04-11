using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;

public class BoardManager : Singleton<BoardManager> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[SerializeField] private Camera gameCamera;
	[Header("Properties")]
	[SerializeField, Min(1)] private int totalTiles;
	[SerializeField, Min(1)] private int minTileGroupSize;
	[SerializeField, Min(1)] private int maxTileGroupSize;
	[Space]
	[SerializeField] private Vector2 _centerPosition;

	private List<TileGroup> tileGroups;

	/// <summary>
	/// The center position of all the tiles on the board
	/// </summary>
	public Vector2 CenterPosition { get => _centerPosition; private set => _centerPosition = value; }

	protected override void Awake ( ) {
		base.Awake( );

		// Declare the list of tile groups
		tileGroups = new List<TileGroup>( );
	}

	private void Start ( ) {
		Generate( );
	}

	private void Update ( ) {
		// TEST: When you press the spacebar, the board is regenerated
		if (Input.GetKeyDown(KeyCode.Space)) {
			for (int i = tileGroups.Count - 1; i >= 0; i--) {
				for (int j = 0; j < tileGroups[i].Count; j++) {
					Destroy(tileGroups[i][j].gameObject);
				}

				tileGroups.RemoveAt(i);
			}

			Generate( );
		}
	}

	/// <summary>
	/// Generate all of the tiles and tile groups that will be on the board
	/// </summary>
	private void Generate ( ) {
		// All available tiles across the entire board, regardless of what tile group it is next to
		List<Vector2Int> globalAvailableTiles = new List<Vector2Int>( ) { Vector2Int.zero };

		// All available tiles that are adjacent to the the current tile group
		List<Vector2Int> tileGroupAvailableTiles = new List<Vector2Int>( );

		// A list to store all of the future tiles that will be created from the current tile group
		List<Vector2Int> tileGroupTilePositions = new List<Vector2Int>( );

		// The current tiles that are left to be generated on the board
		int remaingingTiles = totalTiles;

		// The total number of tile groups on the board
		// Adding 1 because the max tile group size is inclusive in the range of tile group sizes
		int totalTileGroups = maxTileGroupSize - minTileGroupSize + 1;

		// Keep generating tiles until it has reached the total number of board tiles
		while (remaingingTiles > 0) {
			// A list of available tile group sizes for the current number of tiles generated
			List<int> validTileGroupSizes = new List<int>( );

			// Loop through all of the possible group sizes to see if they are able to be generated
			for (int i = 0; i < totalTileGroups; i++) {
				// Do some math to check and see if this tile group size is valid
				int quotient = Mathf.Max(0, remaingingTiles - i) / minTileGroupSize;
				int remainder = Mathf.Max(0, remaingingTiles - i) % minTileGroupSize;

				if (remainder < ((quotient - 1) * 2) + 1) {
					validTileGroupSizes.Add(minTileGroupSize + i);
				}
			}

			// Find a random group size from the valid group size list
			int randomTileGroupSize = validTileGroupSizes[Random.Range(0, validTileGroupSizes.Count)];
			remaingingTiles -= randomTileGroupSize;

			// Clear all previous tile group available tiles
			tileGroupAvailableTiles.Clear( );
			tileGroupTilePositions.Clear( );

			// Create a new tile group object
			TileGroup tileGroup = new TileGroup( );
			tileGroups.Add(tileGroup);

			// Generate all of the tiles in the current tile group
			for (int i = 0; i < randomTileGroupSize; i++) {
				// Get an available tile position to generate a tile at
				Vector2Int tilePosition;
				if (i == 0) {
					// If this is the first tile of this tile group, get any available tile on the board
					tilePosition = globalAvailableTiles[Random.Range(0, globalAvailableTiles.Count)];
				} else {
					// If there are no available tiles for this tile group to generate with, then restart the generation of the tile group
					if (tileGroupAvailableTiles.Count == 0) {
						i = -1;
						tileGroupTilePositions.Clear( );

						continue;
					}

					// If this tile is not the first tile of this tile group, make sure it is connected to the other tiles in this group
					tilePosition = tileGroupAvailableTiles[Random.Range(0, tileGroupAvailableTiles.Count)];
					tileGroupAvailableTiles.Remove(tilePosition);
				}

				// All positions inside the tile group available tiles list are inside the global tile positions list
				// No matter what, the position needs to be remove from the global list
				globalAvailableTiles.Remove(tilePosition);

				// Save the new tile position in the tile position array for this tile group
				tileGroupTilePositions.Add(tilePosition);

				// Add all surrounding tile positions to the available tile position list
				foreach (Vector2Int cardinalPosition in GetCardinalVoids(tilePosition)) {
					// If the current cardinal position has a tile that will be generated there in the future, skip it
					if (tileGroupTilePositions.Contains(cardinalPosition)) {
						continue;
					}

					// Do not add the position if it has already been added
					if (!globalAvailableTiles.Contains(cardinalPosition)) {
						globalAvailableTiles.Add(cardinalPosition);
					}

					// Do not add the position if it has already been added
					if (!tileGroupAvailableTiles.Contains(cardinalPosition)) {
						tileGroupAvailableTiles.Add(cardinalPosition);
					}
				}
			}

			// Create all of the tile objects at once
			foreach (Vector2Int tilePosition in tileGroupTilePositions) {
				CreateTile(tilePosition, tileGroup);
			}
		}

		// After all the tiles have been generated, recalculate the center position of the board
		RecalculateCenterPosition(setCameraPosition: true);
	}

	/// <summary>
	/// Get if a tile is at a specific position
	/// </summary>
	/// <param name="boardPosition">The board position to check</param>
	/// <param name="tileGroup">The group that the tile must be in order to return a tile object. Having this value be null ignores this feature</param>
	/// <returns>A reference to the tile object at the specified board position if it exists, null otherwise</returns>
	public Tile GetTile (Vector2Int boardPosition, TileGroup tileGroup = null) {
		// Fire a raycast in the direction of the tiles to see if it hits one
		RaycastHit2D hit = Physics2D.Raycast(BoardToWorldPosition(boardPosition), Vector2.zero, 0.25f);

		// Check to see if the raycast hit something
		if (hit.collider != null) {
			// Get a reference to the tile that was hit by the raycast
			Tile hitTile = hit.transform.GetComponent<Tile>( );

			// Only return the tile if it has a matching tile group
			if (tileGroup == null || (tileGroup != null && hitTile != null && hitTile.TileGroup == tileGroup)) {
				return hitTile;
			}
		}

		return null;
	}

	/// <summary>
	/// Get all cardinal tiles around the specified board position (if they exist)
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal tiles around</param>
	/// <param name="tileGroup">The group that the tile must be in order to return a tile object. Having this value be null ignores this feature</param>
	/// <returns>A list of all cardinal tiles around the specified board position. If an element is null, then that tile either doesn't exist or is not part of the specified group</returns>
	public List<Tile> GetCardinalTiles (Vector2Int boardPosition, TileGroup tileGroup = null) {
		// Create a list for storing all of the cardinal tiles
		List<Tile> cardinalTiles = new List<Tile>( );

		// Loop through all cardinal positions and check to see if a tile exists at the position
		foreach (Vector2Int cardinalPosition in GetCardinalBoardPositions(boardPosition)) {
			// Get the tile at the cardinal position
			Tile cardinalTile = GetTile(cardinalPosition, tileGroup: tileGroup);

			// If the tile exists, then add it to the cardinal tiles
			if (cardinalTile != null) {
				cardinalTiles.Add(cardinalTile);
			}
		}

		return cardinalTiles;
	}

	/// <summary>
	/// Get all cardinal voids surrounding the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check around</param>
	/// <returns>A distinct list of all cardinal voids surrounding the specified board position</returns>
	public List<Vector2Int> GetCardinalVoids (Vector2Int boardPosition) {
		// Create a list for storing all of the cardinal voids
		List<Vector2Int> cardinalVoids = new List<Vector2Int>( );

		// Loop through all cardinal positions and add them to the void list if a tile does not exist at the board position
		foreach (Vector2Int cardinalPosition in GetCardinalBoardPositions(boardPosition)) {
			// Add the position if a tile does not exist there
			if (GetTile(cardinalPosition) == null) {
				cardinalVoids.Add(cardinalPosition);
			}
		}

		return cardinalVoids;
	}

	/// <summary>
	/// Get all cardinal board positions around the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal board positions around</param>
	/// <returns>A list of all the cardinal board positions around the specified board position</returns>
	public List<Vector2Int> GetCardinalBoardPositions (Vector2Int boardPosition) {
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
	/// <param name="tileGroup">The tile group for this tile</param>
	/// <returns>A reference to the newly created tile</returns>
	private Tile CreateTile (Vector2Int boardPosition, TileGroup tileGroup) {
		// Create the tile object in the scene
		Tile tile = Instantiate(tilePrefab, BoardToWorldPosition(boardPosition), Quaternion.identity, transform).GetComponent<Tile>( );

		// Set the variables of the tile
		tile.TileGroup = tileGroup;
		tile.BoardPosition = boardPosition;

		return tile;
	}

	/// <summary>
	/// Recalculate the center position of the tiles on the board
	/// </summary>
	/// <param name="setCameraPosition">Whether or not to set the camera position when the center position is recalculated</param>
	public void RecalculateCenterPosition (bool setCameraPosition = false) {
		// Variables to track totals
		Vector2 sumPosition = Vector2.zero;
		int tileCount = 0;

		// Add up the total number of tiles on the board and their positions
		for (int i = 0; i < tileGroups.Count; i++) {
			for (int j = 0; j < tileGroups[i].Count; j++) {
				sumPosition += (Vector2) tileGroups[i][j].transform.position;
				tileCount++;
			}
		}

		// The average of all the positions will be the center position of the board
		CenterPosition = sumPosition / tileCount;

		// Set the camera position if the flag is set to true
		if (setCameraPosition) {
			CameraController.SetTransformPositionWithoutZ(gameCamera.transform, CenterPosition);
		}
	}

	/// <summary>
	/// Convert a board position to a world position
	/// </summary>
	/// <param name="boardPosition">The 2D board position to convert</param>
	/// <returns>A Vector3 that is the world position equivelant to the inputted board position</returns>
	public Vector3 BoardToWorldPosition (Vector2Int boardPosition) {
		return new Vector3(
			(boardPosition.x + boardPosition.y) * 0.5f,
			(boardPosition.y - boardPosition.x) * 0.25f,
			(boardPosition.y - boardPosition.x) * 0.05f
		);
	}

	/// <summary>
	/// Covert a world position to the nearest board position
	/// </summary>
	/// <param name="worldPosition">The world position to convert</param>
	/// <returns>A Vector2Int that is the nearest board position to the inputted world position</returns>
	public Vector2Int WorldToBoardPosition (Vector3 worldPosition) {
		// The exact middle of the tiles is slightly lower than the center of the top face of the tile
		// The y value of the inputted world position needs to be adjusted in order for it to be more accurate
		worldPosition.y -= 0.085f;

		return new Vector2Int(
			Mathf.RoundToInt(worldPosition.x - (worldPosition.y * 2)),
			Mathf.RoundToInt((worldPosition.y * 2) + worldPosition.x)
		);
	}
}
