using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;

public class Board : Singleton<Board> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[Header("Properties")]
	[SerializeField, Min(1)] private int tileCount;
	[SerializeField, Min(1)] private int minTileGroupSize;
	[SerializeField, Min(1)] private int maxTileGroupSize;
	[SerializeField, Min(0.01f)] private float animationSpeed;
	[SerializeField] private Tile _selectedTile;
	[Space]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;
	private List<TileGroup> tileGroups;
	private TileGroup _selectedTileGroup;

	public delegate void OnAnimationFrameEvent ( );
	public static event OnAnimationFrameEvent OnAnimationFrame;

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

		// 36 (     ) (       ) (     ) (     )  0 [0 0] [0 0]
		// 35 (     ) (       ) (     ) (     )  1 [0 1] [0 1]
		// 34 (     ) (       ) (2    ) (     )  2 [0 2] [0 2]
		// 33 (     ) (       ) (  3  ) (     )  3 [0 3] [0 3]
		// 32 (4    ) (4      ) (2   4) (     )  4 [0 4] [0 4]
		// 31 (  5  ) (  5    ) (2 3  ) (     )  5 [0 5] [0 5]
		// 30 (    6) (    6  ) (2 3 4) (     )  6 [0 6] [0 6]
		// 29 (     ) (      7) (2 3 4) (7    )  7 [1 0] [0 7]
		// 28 (4    ) (4      ) (2 3 4) (  8  )  8 [1 1] [1 0]
		// 27 (4 5  ) (4 5    ) (2 3 4) (    9)  9 [1 2] [1 1]
		// 26 (4 5 6) (4 5 6  ) (2 3 4) (     ) 10 [1 3] [1 2]
		// 25 (  5 6) (4 5 6 7) (2 3 4) (     ) 11 [1 4] [1 3]
		// 24 (4   6) (4 5 6 7) (2 3 4) (     ) 12 [1 5] [1 4]
		// 23 (4 5  ) (4 5 6 7) (2 3 4) (     ) 13 [1 6] [1 5]
		// 22 (4 5 6) (4 5 6 7) (2 3 4) (7    ) 14 [2 0] [1 6]
		// 21 (4 5 6) (4 5 6 7) (2 3 4) (7 8  ) 15 [2 1] [1 7]
		// 20 (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 16 [2 2] [2 0]
		// 19 (4 5 6) (4 5 6 7) (2 3 4) (  8 9) 17 [2 3] [2 1]
		// 18 (4 5 6) (4 5 6 7) (2 3 4) (    9) 18 [2 4] [2 2]
		// 17 (4 5 6) (4 5 6 7) (2 3 4) (     ) 19 [2 5] [2 3]
		// 16 (4 5 6) (4 5 6 7) (2 3 4) (     ) 20 [2 6] [2 4]
		// 15 (4 5 6) (4 5 6 7) (2 3 4) (7    ) 21 [3 0] [2 5]
		// 14 (4 5 6) (4 5 6 7) (2 3 4) (7 8  ) 22 [3 1] [2 6]
		// 13 (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 23 [3 2] [2 7]
		// 12 (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 24 [3 3] [3 0]
		// 11 (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 25 [3 4] [3 1]
		// 10 (4 5 6) (4 5 6 7) (2 3 4) (  8 9) 26 [3 5] [3 2]
		// 9  (4 5 6) (4 5 6 7) (2 3 4) (    9) 27 [3 6] [3 3]
		// 8  (4 5 6) (4 5 6 7) (2 3 4) (7    ) 28 [4 0] [3 4]
		// 7  (4 5 6) (4 5 6 7) (2 3 4) (7 8  ) 29 [4 1] [3 5]
		// 6  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 30 [4 2] [3 6]
		// 5  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 31 [4 3] [3 7]
		// 4  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 32 [4 4] [4 0]
		// 3  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 33 [4 5] [4 1]
		// 2  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 34 [4 6] [4 2]
		// 1  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 35 [5 0] [4 3]
		// 0  (4 5 6) (4 5 6 7) (2 3 4) (7 8 9) 36 [5 1] [4 4]

		int minGroupSize = 4;
		int maxGroupSize = 6;
		int totalTileCount = 36;

		int groupCount = maxGroupSize - minGroupSize + 1;

		for (int currentTiles = 0; currentTiles <= totalTileCount; currentTiles++) {
			int remainingTiles = totalTileCount - currentTiles;
			string validSizes = remainingTiles + " | ";

			for (int i = 0; i < groupCount; i++) {
				int quotient = Mathf.Max(0, currentTiles - i) / minGroupSize;
				int remainder = Mathf.Max(0, currentTiles - i) % minGroupSize;

				if (remainder < ((quotient - 1) * 2) + 1) {
					validSizes += (minGroupSize + i) + " ";
				} else {
					validSizes += ". ";
				}
			}

			Debug.Log(validSizes);
		}
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

			// Call the event that will update all objects that have subscribed to it
			OnAnimationFrame( );
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
		// A list that stores all of the available positions to expand
		List<Vector2Int> availablePositions = new List<Vector2Int>( ) { Vector2Int.zero };

		// Generate all the tiles
		for (int i = 0; i < tileCount; i++) {
			// Get a random available position from the list
			Vector2Int newBoardPosition = availablePositions[Random.Range(0, availablePositions.Count)];
			availablePositions.Remove(newBoardPosition);

			// Get the cardinal tile groups surrounding the board position
			// Need to subtract 1 from max so the method accounts for the fact that the current tile will be added to the tile group
			List<TileGroup> cardinalTileGroups = GetCardinalTileGroups(newBoardPosition, maxTileGroupSize: maxTileGroupSize - 1);

			// If there are no valid cardinal tile groups, create a new tile group
			// If there are valid cardinal tile groups, then select a random one to set the tile to
			if (cardinalTileGroups.Count == 0) {
				// Create a new tile group
				TileGroup tileGroup = new TileGroup( );
				tileGroups.Add(tileGroup);

				// The new tile group will be the current count of tile groups on the board
				CreateTile(newBoardPosition, tileGroup);
			} else {
				// Select the smallest cardinal tile group to add this tile to
				CreateTile(newBoardPosition, cardinalTileGroups[0]);
			}

			// Add all surrounding tile positions to the available tile positions
			foreach (Vector2Int cardinalPosition in GetCardinalVoids(newBoardPosition)) {
				// Do not add the position if it has already been added
				if (!availablePositions.Contains(cardinalPosition)) {
					availablePositions.Add(cardinalPosition);
				}
			}
		}

		return;

		// Remove all tile groups that have not met the minimum size
		for (int i = tileGroups.Count - 1; i >= 0; i--) {
			// If the tile group is too small, remove all of its tiles and clear it
			// Do not remove the array index though as that will mess up 
			if (tileGroups[i].Count < minTileGroupSize) {
				for (int j = 0; j < tileGroups[i].Count; j++) {
					Destroy(tileGroups[i][j].gameObject);
				}

				tileGroups.RemoveAt(i);
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
