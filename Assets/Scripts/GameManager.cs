using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
	[Header("References")]
	[SerializeField] private Camera gameCamera;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float animationSpeed;
	[Space]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;
	[SerializeField] private Vector2Int lastSelectedPosition;
	[SerializeField] private Vector2Int selectedOrigin;
	[SerializeField] private bool canPlaceSelectedTileGroup;

	private Tile selectedOriginTile;
	private TileGroup selectedTileGroup;

	public delegate void OnAnimationFrameEvent ( );
	public event OnAnimationFrameEvent OnAnimationFrame;

	/// <summary>
	/// The current animation frame for all board elements
	/// </summary>
	public int CurrentAnimationFrame { get => _currentAnimationFrame; private set => _currentAnimationFrame = value; }

	/// <summary>
	/// Whether or not a tile group is currently selected
	/// </summary>
	public bool IsTileGroupSelected => selectedTileGroup != null;

	protected override void Awake ( ) {
		base.Awake( );

		animationTimer = 0;
		CurrentAnimationFrame = 0;
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
			OnAnimationFrame( );
		}

		// If the right mouse button is pressed, deselect the tile group and reset its position
		if (Input.GetMouseButtonDown(1)) {
			// Reset all of the tiles back to where they originally were
			Vector2Int lastOriginPosition = selectedOriginTile.BoardPosition;
			for (int i = 0; i < selectedTileGroup.Count; i++) {
				selectedTileGroup[i].BoardPosition = selectedTileGroup[i].BoardPosition - lastOriginPosition + selectedOrigin;
			}

			SelectTileGroup(null);
		}

		// Update the selected tile group's position if there is one selected
		if (IsTileGroupSelected) {
			// Since selecting and placing the tile groups are done with the same mouse button, we need to wait for the mouse to be lifted in order for the tile group to be placed
			if (Input.GetMouseButtonUp(0)) {
				canPlaceSelectedTileGroup = true;
			}

			// If the left mouse button is pressed, then deselect the tile group and place it where it currently is positioned
			if (canPlaceSelectedTileGroup && Input.GetMouseButtonDown(0)) {
				SelectTileGroup(null);
			}

			// Get the closest board tile to the mouse position
			Vector2Int closestBoardPosition = BoardManager.Instance.WorldToBoardPosition(gameCamera.ScreenToWorldPoint(Input.mousePosition));

			// If the closest board position is not equal to the last tile position, then update the position of the selected tile group
			if (closestBoardPosition != lastSelectedPosition) {
				lastSelectedPosition = closestBoardPosition;

				// Calculate the offset of the closest board position and the origin position
				Vector2Int originTileOffset = closestBoardPosition - selectedOriginTile.BoardPosition;

				// Whether or not the currently selected board group can be placed on the new board position
				// Set to true by default, and if at least one tile cannot be placed, then it is set to false
				bool canMoveTileGroup = true;

				// All adjacent tile groups to the blocks of the currently selected tile group
				List<TileGroup> adjacentTileGroups = new List<TileGroup>( );

				for (int i = 0; i < selectedTileGroup.Count; i++) {
					// Get the board position of this tile around the origin tile position
					Vector2Int localTilePosition = selectedTileGroup[i].BoardPosition + originTileOffset;

					// Get the tile at the local board position
					Tile localTile = BoardManager.Instance.GetTile(localTilePosition);

					// If there is a tile at the location and it is not part of the currently selected tile group, then the selected tile group cannot move here
					if (localTile != null && localTile.TileGroup != selectedTileGroup) {
						canMoveTileGroup = false;

						break;
					}

					// Keep track of all the adjacent tile groups around each tile in the selected tile group
					foreach(TileGroup tileGroup in BoardManager.Instance.GetCardinalTileGroups(localTilePosition)) {
						// If the tile group is equal to the selected one, ignore it
						if (tileGroup == selectedTileGroup) {
							continue;
						}

						// If the tile group has already been added as an adjacent tile group, ignore it
						if (adjacentTileGroups.Contains(tileGroup)) {
							continue;
						}

						adjacentTileGroups.Add(tileGroup);
					}
				}

				// If there are no adjacent tile groups around the new locations for the blocks, then the selected tile group cannot move
				if (adjacentTileGroups.Count == 0) {
					canMoveTileGroup = false;
				}

				// If all of the tiles in the selected tile group have been checked, then move all of the tiles to the new position
				if (canMoveTileGroup) {
					for (int i = 0; i < selectedTileGroup.Count; i++) {
						selectedTileGroup[i].BoardPosition += originTileOffset;
					}
				}
			}
		}
	}

	/// <summary>
	/// Select a tile group on the board so it can be repositioned
	/// </summary>
	/// <param name="tileGroup">The tile group to select</param>
	/// <param name="originTile">The origin tile of the selection. This will be used only while a tile group is selected to track its original position</param>
	public void SelectTileGroup (TileGroup tileGroup, Tile originTile = null) {
		// If the selected tile group is trying to be set to the same value, then return and do nothing
		if (selectedTileGroup == tileGroup) {
			return;
		}

		// Check to see if the tile group can be moved
		if (tileGroup != null) {
			// The tile groups that will be searched next
			List<TileGroup> nextTileGroups = new List<TileGroup>( ) { tileGroup.GetAdjacentTileGroups( )[0] };

			// The already searched tile groups on the board
			List<TileGroup> searchedTileGroups = new List<TileGroup>( );

			// Keep going until there are no more tile groups to search
			while (nextTileGroups.Count > 0) {
				// Loop through all of the searchable tile groups
				for (int i = nextTileGroups.Count - 1; i >= 0; i--) {
					// Get all of the adjacent tile groups to the current tile group
					List<TileGroup> adjacentTileGroups = nextTileGroups[i].GetAdjacentTileGroups( );
					for (int j = 0; j < adjacentTileGroups.Count; j++) {
						// If the adjacent tile group is equal to the selected tile group, ignore it
						if (adjacentTileGroups[j] == tileGroup) {
							continue;
						}

						// If the new adjacent tile group has already been searched, continue to the next group
						if (searchedTileGroups.Contains(adjacentTileGroups[j])) {
							continue;
						}

						// If the new adjacent tile group has already been staged to be searched next, continue to the next group
						if (nextTileGroups.Contains(adjacentTileGroups[j])) {
							continue;
						}

						// Since this is a new tile group, add it to be searched next
						nextTileGroups.Add(adjacentTileGroups[j]);
					}

					// Now that all of the adjacent tile groups have been added, this tile group has been fully searched
					// Remove it from the next tile groups list and add it to the already searched list
					searchedTileGroups.Add(nextTileGroups[i]);
					nextTileGroups.RemoveAt(i);
				}
			}

			// If the searched tile groups count is less than 1 less than all of the tile groups, then this tile group cannot be removed
			// This means that when the current tile group is removed, all of the remaining tile groups will not connect together
			if (searchedTileGroups.Count < BoardManager.Instance.TileGroupCount - 1) {
				return;
			}
		}

		// Set the origin of the selected tile group to be the tile that was clicked on to select it
		// This will be used for position the tile group and returning it back to its original location if needed
		if (originTile != null) {
			selectedOriginTile = originTile;
			selectedOrigin = lastSelectedPosition = originTile.BoardPosition;
		}

		// Set the previous selected tile group to not be selected anymore
		if (selectedTileGroup != null) {
			selectedTileGroup.TileGroupState = TileState.REGULAR;
		}

		selectedTileGroup = tileGroup;

		// Set the new tile group to be selected
		if (selectedTileGroup != null) {
			selectedTileGroup.TileGroupState = TileState.SELECTED;
		}

		canPlaceSelectedTileGroup = false;
	}
}
