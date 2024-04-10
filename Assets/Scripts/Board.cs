using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;

public class Board : Singleton<Board> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[Header("Properties")]
	[SerializeField, Min(1)] private int totalTiles;
	[SerializeField, Min(1)] private int minTileGroupSize;
	[SerializeField, Min(1)] private int maxTileGroupSize;
	[SerializeField, Min(0.01f)] private float animationSpeed;
	[Space]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;

	private List<TileGroup> tileGroups;
	private Tile _selectedTile;
	private TileGroup _selectedTileGroup;

	/// <summary>
	/// The current animation frame for all board elements
	/// </summary>
	public int CurrentAnimationFrame { get => _currentAnimationFrame; private set => _currentAnimationFrame = value; }
	
	/// <summary>
	/// The currently selected tile group that is being moved around by the player
	/// </summary>
	public TileGroup SelectedTileGroup {
		get => _selectedTileGroup;
		set {
			// If the selected tile group is trying to be set to the same value, then return and do nothing
			if (_selectedTileGroup == value) {
				return;
			}

			// Set the current selected tile group to not be selected anymore
			if (_selectedTileGroup != null) {
				_selectedTileGroup.TileGroupState = TileGroupState.REGULAR;
			}
	
			_selectedTileGroup = value;

			// Set the new tile group to be selected
			if (_selectedTileGroup != null) {
				_selectedTileGroup.TileGroupState = TileGroupState.SELECTED;
			}
		}
	}

	/// <summary>
	/// The currently selected tile that will be used for selected tile group placement
	/// </summary>
	public Tile SelectedTile {
		get => _selectedTile;
		set {
			_selectedTile = value;
		}
	}

	protected override void Awake ( ) {
		base.Awake( );

		// Declare the list of tile groups
		tileGroups = new List<TileGroup>( );

		animationTimer = 0;
		CurrentAnimationFrame = 0;
	}

	private void Start ( ) {
		Generate( );
	}

	private void Update ( ) {
		// Increment the animation timer by the time that has passed since the last update call
		// Once the timer has reached the animation speed, update the sprites
		animationTimer += Time.deltaTime;
		if (animationTimer >= animationSpeed) {
			// Subtract the animation time from the animation timer
			// This makes it slightly more exact in when the animation changes sprites
			animationTimer -= animationSpeed;

			// Increment the current animation frame
			// All animations should use this so they are all synced
			// All animations will be 4 looped frames
			CurrentAnimationFrame = (CurrentAnimationFrame + 1) % 4;

			// Update all of the tiles if they need to be animated
			for (int i = 0; i < tileGroups.Count; i++) {
				// Get whether or not the current tile group is the selected one
				bool isSelectedTileGroup = tileGroups[i] == SelectedTileGroup;

				// Update all the tiles in the tile group
				for (int j = 0; j < tileGroups[i].Count; j++) {
					// If this tile group is selected OR the tile is currently showing a hazard overlay, then update the tile
					if (isSelectedTileGroup || tileGroups[i][j].OverlayTileState == OverlayTileState.HAZARD) {
						tileGroups[i][j].UpdateTile( );
					}
				}
			}
		}

		// If there is a selected tile group and the right mouse button is pressed, deselect the tile group
		if (SelectedTileGroup != null && Input.GetMouseButtonDown(1)) {
			SelectedTileGroup = null;
		}
	}

	/// <summary>
	/// Generate all of the tiles and tile groups that will be on the board
	/// </summary>
	private void Generate ( ) {
		// All available tiles across the entire board, regardless of what tile group it is next to
		List<Vector2Int> globalAvailableTiles = new List<Vector2Int>( ) { Vector2Int.zero };

		// All available tiles that are adjacent to the the current tile group
		List<Vector2Int> tileGroupAvailableTiles = new List<Vector2Int>( ) { };

		// The current tiles generated on the board
		int currentTiles = 0;

		// The total number of tile groups on the board
		int totalTileGroups = maxTileGroupSize - minTileGroupSize + 1;

		// Keep generating tiles until it has reached the total number of board tiles
		while (currentTiles < totalTiles) {
			// A list of available tile group sizes for the current number of tiles generated
			List<int> validTileGroupSizes = new List<int>( );

			// Loop through all of the possible group sizes to see if they are able to be generated
			for (int i = 0; i < totalTileGroups; i++) {
				// Do some math to check and see if this tile group size is valid
				int quotient = Mathf.Max(0, totalTiles - i) / minTileGroupSize;
				int remainder = Mathf.Max(0, totalTiles - i) % minTileGroupSize;

				if (remainder < ((quotient - 1) * 2) + 1) {
					validTileGroupSizes.Add(minTileGroupSize + i);
				}
			}

			// Find a random group size from the valid group size list
			int randomTileGroupSize = validTileGroupSizes[Random.Range(0, validTileGroupSizes.Count)];

			// Clear all previous tile group available tiles
			tileGroupAvailableTiles.Clear( );

			// Create a new tile group
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
					// If this tile is not the first tile of this tile group, make sure it is connected to the other tiles in this group
					tilePosition = tileGroupAvailableTiles[Random.Range(0, tileGroupAvailableTiles.Count)];
					tileGroupAvailableTiles.Remove(tilePosition);
				}

				// All positions inside the tile group available tiles list are inside the global tile positions list
				// No matter what, the position needs to be remove from the global list
				globalAvailableTiles.Remove(tilePosition);

				// Create a new tile at the new tile position
				CreateTile(tilePosition, tileGroup);
				currentTiles++;

				// Add all surrounding tile positions to the available tile position list
				foreach (Vector2Int cardinalPosition in GetCardinalVoids(tilePosition)) {
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
		}
	}

	/// <summary>
	/// Get if a tile is at a specific position
	/// </summary>
	/// <param name="boardPosition">The board position to check</param>
	/// <param name="tileGroup">The group that the tile must be in order to return a tile object. Having this value be null ignores this feature</param>
	/// <returns>A reference to the tile object at the specified board position if it exists, null otherwise</returns>
	public Tile GetTile (Vector2Int boardPosition, TileGroup tileGroup = null) {
		// Fire a raycast in the direction of the tiles to see if it hits one
		RaycastHit2D hit = Physics2D.Raycast(BoardPositionToWorldPosition(boardPosition), Vector2.zero, 0.25f);

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
	/// Get all cardinal tile groups surrounding the specified board position
	/// </summary>
	/// <param name="boardPosition">The board position to check around</param>
	/// <param name="minTileGroupSize">The minimum size for the tile group to be valid</param>
	/// <param name="maxTileGroupSize">The maximum size for the tile group to be valid</param>
	/// <returns>A distinct list of all cardinal tile groups surrounding the specified board position</returns>
	public List<TileGroup> GetCardinalTileGroups (Vector2Int boardPosition, int minTileGroupSize = 0, int maxTileGroupSize = 9999999) {
		// Create a list for storing all of the cardinal tile groups
		List<TileGroup> cardinalTileGroups = new List<TileGroup>( );

		// Loop through all cardinal tiles and save all valid tile groups
		foreach (Tile cardinalTile in GetCardinalTiles(boardPosition)) {
			// If the cardinal tile does not have a tile group or the tile group has already been added, then continue to the next tile
			if (cardinalTile.TileGroup == null || cardinalTileGroups.Contains(cardinalTile.TileGroup)) {
				continue;
			}

			// Get the size of the tile group that the cardinal tile belongs to
			int tileGroupSize = cardinalTile.TileGroup.Count;

			// If the tile group size is within the specified size range, then it is a valid tile group and should be added to the list
			if (tileGroupSize >= minTileGroupSize && tileGroupSize <= maxTileGroupSize) {
				cardinalTileGroups.Add(cardinalTile.TileGroup);
			}
		}

		return cardinalTileGroups.OrderBy(tileGroup => tileGroup.Count).ToList( );
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
		Tile tile = Instantiate(tilePrefab, BoardPositionToWorldPosition(boardPosition), Quaternion.identity, transform).GetComponent<Tile>( );

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
	public Vector3 BoardPositionToWorldPosition (Vector2Int boardPosition) {
		return new Vector3(
			(boardPosition.x + boardPosition.y) * 0.5f,
			(boardPosition.y - boardPosition.x) * 0.25f,
			(boardPosition.y - boardPosition.x) * 0.05f
		);
	}
}
