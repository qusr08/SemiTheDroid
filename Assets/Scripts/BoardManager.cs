using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;

public class BoardManager : Singleton<BoardManager> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[Header("Properties")]
	[SerializeField, Min(1)] private int totalTiles;
	[SerializeField, Min(1)] private int minTileGroupSize;
	[SerializeField, Min(1)] private int maxTileGroupSize;
	[Header("Information")]
	[SerializeField] private Vector2 _centerPosition;

	private List<TileGroup> tileGroups;

	/// <summary>
	/// The center position of all the tiles on the board
	/// </summary>
	public Vector2 CenterPosition { get => _centerPosition; private set => _centerPosition = value; }

	/// <summary>
	/// The number of tile groups on the board
	/// </summary>
	public int TileGroupCount => tileGroups.Count;

	protected override void Awake ( ) {
		base.Awake( );

		// Declare the list of tile groups
		tileGroups = new List<TileGroup>( );
	}

	private void Update ( ) {
		// TEST: When you press the spacebar, the board is regenerated
		if (Input.GetKeyDown(KeyCode.Space)) {
			for (int i = tileGroups.Count - 1; i >= 0; i--) {
				for (int j = 0; j < tileGroups[i].Count; j++) {
					Destroy(tileGroups[i].Tiles[j].gameObject);
				}

				tileGroups.RemoveAt(i);
			}

			Generate( );
		}
	}

	/// <summary>
	/// Generate all of the tiles and tile groups that will be on the board
	/// </summary>
	public void Generate ( ) {
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
		int tileGroupSizeRange = maxTileGroupSize - minTileGroupSize;

		// Keep generating tiles until it has reached the total number of board tiles
		while (remaingingTiles > 0) {
			// A list of available tile group sizes for the current number of tiles generated
			List<int> validTileGroupSizes = new List<int>( );

			// Loop through all of the possible group sizes to see if they are able to be generated
			for (int i = 0; i <= tileGroupSizeRange; i++) {
				// Do some math to check and see if this tile group size is valid
				int quotient = Mathf.Max(0, remaingingTiles - i) / minTileGroupSize;
				int remainder = Mathf.Max(0, remaingingTiles - i) % minTileGroupSize;

				if (remainder < ((quotient - 1) * tileGroupSizeRange) + 1) {
					validTileGroupSizes.Add(minTileGroupSize + i);
				}
			}

			// If there are no valid tile group sizes, then stop generating the board to avoid any errors
			if (validTileGroupSizes.Count == 0) {
				return;
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
				// If this tile group is able to full generate, then this position will be turned into a tile object
				tileGroupTilePositions.Add(tilePosition);

				// Add all surrounding tile positions to the available tile position list
				foreach (Vector2Int cardinalPosition in GetCardinalVoids(tilePosition, excludedBoardPositions: tileGroupTilePositions)) {
					// Do not add the position if it has already been added
					if (!globalAvailableTiles.Contains(cardinalPosition)) {
						globalAvailableTiles.Add(cardinalPosition);
						tileGroupAvailableTiles.Add(cardinalPosition);
					}
				}
			}

			// Create all of the tile objects at once
			foreach (Vector2Int tilePosition in tileGroupTilePositions) {
				CreateTile(tilePosition, tileGroup);
			}

			tileGroup.RecalculateTileSprites( );
		}

		// Get the number of spikes that will be generated on the board
		int totalSpikes = totalTiles / 5;

		// Generate spikes onto the tiles
		for (int i = 0; i < totalSpikes; i++) {
			EntityManager.Instance.SpawnEntity(EntityType.SPIKE, GetRandomTile(ignoreEntityTiles: true));
		}

		// After all the tiles have been generated, recalculate the center position of the board
		RecalculateCenter( );
		CameraManager.SetTransformPositionWithoutZ(CameraManager.Instance.GameCamera.transform, CenterPosition);
	}

	/// <summary>
	/// Get all cardinal board positions around the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal board positions around</param>
	/// <param name="excludedBoardPositions">Do not add these positions to the returned list of cardinal board positions</param>
	/// <returns>A list of all the cardinal board positions around the specified board position</returns>
	public List<Vector2Int> GetCardinalPositions (Vector2Int boardPosition, List<Vector2Int> excludedBoardPositions = null) {
		// Create a list of all the cardinal board positions to the inputted position
		List<Vector2Int> cardinalPositions = new List<Vector2Int>( ) {
			boardPosition + Vector2Int.up,
			boardPosition + Vector2Int.right,
			boardPosition + Vector2Int.down,
			boardPosition + Vector2Int.left
		};

		// If there are some board positions that should be excluded, remove them from the list
		if (excludedBoardPositions != null) {
			cardinalPositions.RemoveAll(position => excludedBoardPositions.Contains(position));
		}

		return cardinalPositions;
	}

	/// <summary>
	/// Get all cardinal voids surrounding the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check around</param>
	/// <param name="excludedBoardPositions">Do not include these positions when searching for the inputted board positions</param>
	/// <returns>A distinct list of all cardinal voids surrounding the specified board position</returns>
	public List<Vector2Int> GetCardinalVoids (Vector2Int boardPosition, List<Vector2Int> excludedBoardPositions = null) {
		// Get all of the cardinal positions around the board position
		List<Vector2Int> cardinalPositions = GetCardinalPositions(boardPosition, excludedBoardPositions: excludedBoardPositions);

		// Get all of the cardinal tiles around the board position
		List<Tile> cardinalTiles = SearchForTilesAt(cardinalPositions);

		// Remove all cardinal positions that have a tile on them
		foreach (Tile tile in cardinalTiles) {
			cardinalPositions.Remove(tile.BoardPosition);
		}

		return cardinalPositions;
	}

	/// <summary>
	/// Get all cardinal tiles around the specified board position (if they exist)
	/// </summary>
	/// <param name="boardPosition">The board position to get the cardinal tiles around</param>
	/// <param name="exclusiveTileGroups">The tile groups in this list are the only ones that should be searched in</param>
	/// <param name="excludedTileGroups">The tile groups in this list are never searched in</param>
	/// <returns>A list of all cardinal tiles around the specified board position. If an element is null, then that tile either doesn't exist or is not part of the specified group</returns>
	public List<Tile> GetCardinalTiles (Vector2Int boardPosition, List<TileGroup> exclusiveTileGroups = null, List<TileGroup> excludedTileGroups = null) {
		// Find all tiles at the cardinal board positions
		List<Tile> cardinalTiles = SearchForTilesAt(
			GetCardinalPositions(boardPosition),
			exclusiveTileGroups: exclusiveTileGroups,
			excludedTileGroups: excludedTileGroups
		);

		return cardinalTiles;
	}

	/// <summary>
	/// Get all cardinal tile groups surrounding the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check around</param>
	/// <param name="exclusiveTileGroups">The tile groups in this list are the only ones that should be added</param>
	/// <param name="excludedTileGroups">The tile groups in this list are never added</param>
	/// <returns>A distinct list of all cardinal tile groups surrounding the specified board position</returns>
	public List<TileGroup> GetCardinalTileGroups (Vector2Int boardPosition, List<TileGroup> exclusiveTileGroups = null, List<TileGroup> excludedTileGroups = null) {
		// Create a list for storing all of the cardinal tile groups
		List<TileGroup> cardinalTileGroups = new List<TileGroup>( );

		// Loop through all cardinal tiles and save all valid tile groups
		foreach (Tile cardinalTile in GetCardinalTiles(boardPosition, exclusiveTileGroups: exclusiveTileGroups, excludedTileGroups: excludedTileGroups)) {
			cardinalTileGroups.Add(cardinalTile.TileGroup);
		}

		// return cardinalTileGroups.OrderBy(tileGroup => tileGroup.Count).ToList( );
		return cardinalTileGroups.Distinct( ).ToList( );
	}

	/// <summary>
	/// Search for tiles at specific board positions
	/// </summary>
	/// <param name="boardPositions">The list of positions to search for</param>
	/// <param name="exclusiveTileGroups">The tile groups in this list are the only ones that should be searched in</param>
	/// <param name="excludedTileGroups">The tile groups in this list are never searched in</param>
	/// <returns>A list of tiles where each index corresponds to the inputted board positions. If there is a tile object at a specific index, then a tile was found at the board position at the same index in the inputted array</returns>
	public List<Tile> SearchForTilesAt (List<Vector2Int> boardPositions, List<TileGroup> exclusiveTileGroups = null, List<TileGroup> excludedTileGroups = null) {
		// Create a list that has all of the found tiles in it
		List<Tile> foundTiles = new List<Tile>( );

		// Loop through all the tiles on the board
		foreach (TileGroup tileGroup in tileGroups) {
			// The exclusive tile groups are the only ones that should be searched
			if (exclusiveTileGroups != null && !exclusiveTileGroups.Contains(tileGroup)) {
				continue;
			}

			// The excluded tile groups should never be searched
			if (excludedTileGroups != null && excludedTileGroups.Contains(tileGroup)) {
				continue;
			}

			foreach (Tile tile in tileGroup.Tiles) {
				// If the board positions array contains the current board position, then add it to the found tiles list
				if (boardPositions.Contains(tile.BoardPosition)) {
					// Add the tile at the same index as the board position to the found tiles list
					foundTiles.Add(tile);

					// If all of the tiles that were needed have already been found, then quit out of the loops
					if (foundTiles.Count == boardPositions.Count) {
						return foundTiles;
					}
				}
			}
		}

		return foundTiles;
	}

	/*public List<Tile> SearchForTilesAround (Vector2Int boardPosition, List<TileGroup> exclusiveTileGroups = null, List<TileGroup> excludedTileGroups = null) {

	}*/

	/// <summary>
	/// Checks to see if there are any tiles at the specified board positions
	/// </summary>
	/// <param name="boardPositions">The list of positions to search at</param>
	/// <param name="exclusiveTileGroups">The tile groups in this list are the only ones that should be searched in</param>
	/// <param name="excludedTileGroups">The tile groups in this list are never searched in</param>
	/// <returns>true if there is at least one tile at one of the board positions, false otherwise</returns>
	public bool HasTilesAt (List<Vector2Int> boardPositions, List<TileGroup> exclusiveTileGroups = null, List<TileGroup> excludedTileGroups = null) {
		// Loop through all the tiles on the board
		foreach (TileGroup tileGroup in tileGroups) {
			// The exclusive tile groups are the only ones that should be searched
			if (exclusiveTileGroups != null && !exclusiveTileGroups.Contains(tileGroup)) {
				continue;
			}

			// The excluded tile groups should never be searched
			if (excludedTileGroups != null && excludedTileGroups.Contains(tileGroup)) {
				continue;
			}

			foreach (Tile tile in tileGroup.Tiles) {
				// If the board positions array contains the current board position, then there is at least one tile on an inputted board position
				if (boardPositions.Contains(tile.BoardPosition)) {
					return true;
				}
			}
		}

		return false;
	}

	/// <summary>
	/// Get a random tile on the board
	/// </summary>
	/// <param name="exclusiveTileGroups">The tile groups in this list are the only ones that should be searched in</param>
	/// <param name="excludedTileGroups">The tile groups in this list are never searched in</param>
	/// <param name="ignoreEntityTiles">Whether or not to ignore a tile if it has an entity on it</param>
	/// <returns>A reference to a random tile object</returns>
	public Tile GetRandomTile (List<TileGroup> exclusiveTileGroups = null, List<TileGroup> excludedTileGroups = null, bool ignoreEntityTiles = false) {
		// Create a list that has all of the found tiles in it
		List<Tile> validTiles = new List<Tile>( );

		// Loop through all the tiles on the board
		foreach (TileGroup tileGroup in tileGroups) {
			// The exclusive tile groups are the only ones that should be searched
			if (exclusiveTileGroups != null && !exclusiveTileGroups.Contains(tileGroup)) {
				continue;
			}

			// The excluded tile groups should never be searched
			if (excludedTileGroups != null && excludedTileGroups.Contains(tileGroup)) {
				continue;
			}

			foreach (Tile tile in tileGroup.Tiles) {
				// Do checks to make sure the tile is valid
				if (!ignoreEntityTiles || (ignoreEntityTiles && tile.Entity == null)) {
					// Add the tile at the same index as the board position to the found tiles list
					validTiles.Add(tile);
				}
			}
		}

		// Return a random valid tile
		return validTiles[Random.Range(0, validTiles.Count)];
	}

	/// <summary>
	/// Recalculate the center position of the tiles on the board
	/// </summary>
	public void RecalculateCenter ( ) {
		// Variables to track totals
		Vector2 sumPosition = Vector2.zero;
		int tileCount = 0;

		// Add up the total number of tiles on the board and their positions
		foreach (TileGroup tileGroup in tileGroups) {
			foreach (Tile tile in tileGroup.Tiles) {
				sumPosition += (Vector2) tile.transform.position;
				tileCount++;
			}
		}

		// The average of all the positions will be the center position of the board
		CenterPosition = sumPosition / tileCount;
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
