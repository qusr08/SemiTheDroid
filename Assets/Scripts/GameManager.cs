using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum GameState {
	GENERATE, PLAYER_TURN, ENTITY_TURN, GAME_OVER
}

public class GameManager : Singleton<GameManager> {
	[Header("References")]
	[SerializeField] private TextMeshProUGUI turnCountText;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject gameOverMenu;
	[SerializeField] private TextMeshProUGUI statsText;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float animationSpeed;
	[SerializeField] private GameState _gameState;
	[SerializeField, Range(0f, 0.1f)] private float _difficultyValue;
	[Header("Information")]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;
	[SerializeField] private Vector2Int lastSelectedPosition;
	[SerializeField] private int _turnCount;
	[SerializeField] private int _lasersDestroyed;
	[SerializeField] private bool isPaused;
	[SerializeField] private float startTime;

	private TileGroup selectedTileGroup;
	private bool canPlaceSelectedTileGroup;

	public delegate void OnAnimationFrameEvent ( );
	public event OnAnimationFrameEvent OnAnimationFrame;

	/// <summary>
	/// A stat value to track how many lasers were destroyed
	/// </summary>
	public int LasersDestroyed { get => _lasersDestroyed; set => _lasersDestroyed = value; }

	/// <summary>
	/// The difficulty scaling value
	/// </summary>
	public float DifficultyValue { get => _difficultyValue; private set => _difficultyValue = value; }

	/// <summary>
	/// The current animation frame for all board elements
	/// </summary>
	public int CurrentAnimationFrame { get => _currentAnimationFrame; private set => _currentAnimationFrame = value; }

	/// <summary>
	/// Whether or not a tile group is currently selected
	/// </summary>
	public bool IsTileGroupSelected => selectedTileGroup != null;

	/// <summary>
	/// The number of turns that the player has currently survived
	/// </summary>
	public int TurnCount {
		get => _turnCount;
		private set {
			_turnCount = value;

			turnCountText.text = $"Turn {_turnCount}";
		}
	}

	/// <summary>
	/// The current gamestate of the game
	/// </summary>
	public GameState GameState { get => _gameState; private set => _gameState = value; }

	protected override void Awake ( ) {
		base.Awake( );

		animationTimer = 0;
		CurrentAnimationFrame = 0;
		TurnCount = 0;
		isPaused = false;
		startTime = Time.time;
	}

	private void Start ( ) {
		SetGameState(GameState.GENERATE);
	}

	private void Update ( ) {
		// Have the escape be to toggle the paused state
		if (Input.GetKeyDown(KeyCode.Escape)) {
			SetPauseState(!isPaused);
		}

		UpdateAnimationFrame( );

		// Update the selected tile group's position if there is one selected
		if (IsTileGroupSelected) {
			// Get the closest board tile to the mouse position
			Vector2Int closestBoardPosition = BoardManager.Instance.WorldToBoardPosition(CameraManager.Instance.GameCamera.ScreenToWorldPoint(Input.mousePosition));

			// If the closest board position is not equal to the last tile position, then update the position of the selected tile group
			if (closestBoardPosition != lastSelectedPosition) {
				// Try to move the selected tile group to the new board position
				if (selectedTileGroup.TryMoveAndRotate(closestBoardPosition, 0)) {
					// Update the last selected position that the selected tile group moved to
					lastSelectedPosition = closestBoardPosition;
				}
			}

			// If the player presses the spacebar, try to rotate the tile group by 90 degrees
			if (Input.GetKeyDown(KeyCode.Space)) {
				selectedTileGroup.TryMoveAndRotate(lastSelectedPosition, 1);
			}

			// Since selecting and placing the tile groups are done with the same mouse button, we need to wait for the mouse to be lifted in order for the tile group to be placed
			if (Input.GetMouseButtonUp(0)) {
				canPlaceSelectedTileGroup = true;
			}

			// If the left mouse button is pressed, then deselect the tile group and place it where it currently is positioned
			// Make sure the tile group is not at its starting position either
			if (canPlaceSelectedTileGroup && Input.GetMouseButtonDown(0) && !selectedTileGroup.IsAtSavedTileState) {
				// Update the center of the board because tiles were moved
				BoardManager.Instance.RecalculateCenter( );

				SelectTileGroup(null);

				// Now that the player has successfully moved, have the entities do their turn
				SetGameState(GameState.ENTITY_TURN);
			}

			// If the right mouse button is pressed, deselect the tile group and reset its position
			if (Input.GetMouseButtonDown(1)) {
				selectedTileGroup.ResetTileStates( );

				SelectTileGroup(null);
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

		// If the current gamestate is not the player's turn, then do not let the player select a tile group
		if (GameState != GameState.PLAYER_TURN) {
			return;
		}

		// Do not do anything while the game is paused
		if (isPaused) {
			return;
		}

		// Check to see if the tile group can be moved
		if (tileGroup != null) {
			// This is one of the adjacent tile groups to the newly selected tile group
			// This will act as a starting point for searching and seeing if this tile group can be selected
			TileGroup startingTileGroup = BoardManager.Instance.GetAdjacentTileGroups(tileGroup)[0];

			// The tile groups that will be searched next
			List<TileGroup> nextTileGroups = new List<TileGroup>( ) { startingTileGroup };

			// The already searched tile groups on the board
			List<TileGroup> searchedTileGroups = new List<TileGroup>( );

			// A list that contains all of the tile groups that have been seen, both ones that were searched and ones that are next to be searched
			List<TileGroup> seenTileGroups = new List<TileGroup>( ) { startingTileGroup, tileGroup };

			// Keep going until there are no more tile groups to search
			while (nextTileGroups.Count > 0) {
				// Loop through all of the searchable tile groups
				for (int i = nextTileGroups.Count - 1; i >= 0; i--) {
					// Get all of the adjacent tile groups to the current tile group be searched
					List<TileGroup> adjacentTileGroups = BoardManager.Instance.GetAdjacentTileGroups(nextTileGroups[i], excludedTileGroups: seenTileGroups);

					// Add all of the new adjacent tile groups to be queued to search next
					nextTileGroups.AddRange(adjacentTileGroups);
					seenTileGroups.AddRange(adjacentTileGroups);

					// Now that all of the adjacent tile groups have been added, this tile group has been fully searched
					// Remove it from the next tile groups list and add it to the already searched list
					searchedTileGroups.Add(nextTileGroups[i]);
					nextTileGroups.RemoveAt(i);
				}
			}

			// If the searched tile groups count is less than 1 less than all of the tile groups, then this tile group cannot be removed
			// This means that when the current tile group is removed, all of the remaining tile groups will not connect together
			if (searchedTileGroups.Count < BoardManager.Instance.TileGroups.Count - 1) {
				return;
			}
		}

		// Set the previous selected tile group to not be selected anymore
		if (selectedTileGroup != null) {
			selectedTileGroup.TileGroupState = TileState.DEFAULT;
		}

		selectedTileGroup = tileGroup;

		// Set the new tile group to be selected
		if (selectedTileGroup != null) {
			selectedTileGroup.TileGroupState = TileState.SELECTED;

			// Select the origin tile
			selectedTileGroup.OriginTile = originTile;
			lastSelectedPosition = originTile.BoardPosition;

			// Now that this tile group is selected, save the states of its tiles
			selectedTileGroup.SaveTileStates( );
		}

		canPlaceSelectedTileGroup = false;

		// Since entity hazards are shown when they are on a selected tile group, update the shown positions
		EntityManager.Instance.UpdateShownHazardPositions( );
	}

	/// <summary>
	/// Set the current game state of the game
	/// </summary>
	/// <param name="gameState">The new game state to set the game to</param>
	public void SetGameState (GameState gameState) {
		// If the game state is being set to the same value, return and do nothing
		if (GameState == gameState) {
			return;
		}

		// Save the old gamestate as there are certain things that we might want to do based on it
		GameState oldGameState = GameState;
		GameState = gameState;

		// Do specific things based on the new game state
		switch (GameState) {
			case GameState.GENERATE:
				BoardManager.Instance.Generate( );

				break;
			case GameState.PLAYER_TURN:
				/// TODO: Display player turn text

				// Since the player has survived the entity turn or the board has just been generated, increment the number of survived rounds
				if (oldGameState == GameState.ENTITY_TURN || oldGameState == GameState.GENERATE) {
					TurnCount++;
				}

				break;
			case GameState.ENTITY_TURN:
				/// TODO: Display entity turn text

				// Update all the turns of every entity on the board
				EntityManager.Instance.UpdateEntityTurns( );

				// Spawn new random entities in
				// Make sure entities are never spawned on the same tile group has the robot
				EntityManager.Instance.SpawnRandomEntities(excludedTileGroups: new List<TileGroup>( ) { EntityManager.Instance.Robot.Tile.TileGroup });

				break;
			case GameState.GAME_OVER:
				statsText.text = $"Turns survived: {TurnCount}\nLasers destroyed: {LasersDestroyed}\nTotal run time: {(int) (Time.time - startTime)} seconds";
				gameOverMenu.SetActive(true);

				break;
		}
	}

	public void SetPauseState (bool isPaused) {
		this.isPaused = isPaused;

		// Set the pause menu visibility based on the pause state
		pauseMenu.SetActive(isPaused);
	}

	/// <summary>
	/// Update the current animation frame
	/// </summary>
	private void UpdateAnimationFrame ( ) {
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
			// If there are no subscribed events, this throws an error
			OnAnimationFrame?.Invoke( );
		}
	}
}
