using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
	[Header("References")]
	[SerializeField] private Camera gameCamera;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float animationSpeed;
	[Space]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;
	[SerializeField] private Vector2Int lastSelectedTilePosition;
	[SerializeField] private Vector2Int originSelectedTilePosition;

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
		
		// If the right mouse button is pressed, deselect the tile group
		if (Input.GetMouseButtonDown(1)) {
			SelectTileGroup(null);
		}

		// Update the selected tile group's position if there is one selected
		if (IsTileGroupSelected) {
			// Get the closest board tile to the mouse position
			Vector2Int closestBoardPosition = BoardManager.Instance.WorldToBoardPosition(gameCamera.ScreenToWorldPoint(Input.mousePosition));

			// If the closest board position is not equal to the last tile position, then update the position of the selected tile group
			if (closestBoardPosition != lastSelectedTilePosition) {
				lastSelectedTilePosition = closestBoardPosition;

				Debug.Log("UPDATE POSITION");
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

		// Set the previous selected tile group to not be selected anymore
		if (selectedTileGroup != null) {
			selectedTileGroup.TileGroupState = TileState.REGULAR;
		}

		selectedTileGroup = tileGroup;

		// Set the new tile group to be selected
		if (selectedTileGroup != null) {
			selectedTileGroup.TileGroupState = TileState.SELECTED;
		}

		// Set the origin of the selected tile group to be the tile that was clicked on to select it
		// This will be used for position the tile group and returning it back to its original location if needed
		if (originTile != null) {
			originSelectedTilePosition = lastSelectedTilePosition = originTile.BoardPosition;
		}
	}
}
