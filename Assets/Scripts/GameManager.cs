using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum GameState {
	GENERATE, PLAYER_TURN, ENTITY_TURN, GAME_OVER
}

public enum MenuState {
	MAIN, PAUSE, GAME_OVER, CREDITS, HOW_TO_PLAY, PLAY
}

public enum SoundEffectType {
	EXPLOSION, LASER, PLACE_TILES, PLAYER_TURN, ROTATE, SELECT_TILES, SPAWN, STEP, CANCEL
}

public class GameManager : Singleton<GameManager> {
	[Header("References")]
	[SerializeField] private TextMeshProUGUI turnCountText;
	[SerializeField] private TextMeshProUGUI turnIndicatorText;
	[SerializeField] private TextMeshProUGUI statsText;
	[SerializeField] private GameObject pauseMenu;
	[SerializeField] private GameObject gameOverMenu;
	[SerializeField] private GameObject creditsMenu;
	[SerializeField] private GameObject howToPlayMenu;
	[SerializeField] private GameObject mainMenu;
	[Space]
	[SerializeField] private AudioSource audioSource;
	[SerializeField] private AudioClip[ ] soundEffects;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float _animationSpeed;
	[SerializeField] private GameState _gameState;
	[SerializeField] private MenuState menuState;
	[SerializeField, Range(0f, 0.1f)] private float _difficultyValue;
	[Header("Information")]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;
	[SerializeField] private Vector2Int lastSelectedPosition;
	[SerializeField] private int _turnCount;
	[SerializeField] private int _lasersDestroyed;
	[SerializeField] private float startTime;

	private TileGroup selectedTileGroup;
	private bool canPlaceSelectedTileGroup;

	public delegate void OnAnimationFrameEvent ( );
	public event OnAnimationFrameEvent OnAnimationFrame;

	/// <summary>
	/// The seconds in between animation frames
	/// </summary>
	public float AnimationSpeed { get => _animationSpeed; private set => _animationSpeed = value; }

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
	}

	private void Start ( ) {
		SetMenuState(MenuState.MAIN);
	}

	private void Update ( ) {
		// Have the escape be to toggle the paused state
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (menuState == MenuState.PLAY) {
				SetMenuState(MenuState.PAUSE);
			} else if (menuState == MenuState.PAUSE) {
				SetMenuState(MenuState.PLAY);
			}
		}

		if (menuState != MenuState.PLAY) {
			return;
		}

		UpdateAnimationFrame( );

		if (GameState != GameState.PLAYER_TURN) {
			return;
		}

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
				selectedTileGroup.TryMoveAndRotate(lastSelectedPosition, -1);
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

				PlaySoundEffect(SoundEffectType.PLACE_TILES);

				SelectTileGroup(null);

				// Now that the player has successfully moved, have the entities do their turn
				StartCoroutine(SetGameState(GameState.ENTITY_TURN));
			}

			// If the right mouse button is pressed, deselect the tile group and reset its position
			if (Input.GetMouseButtonDown(1)) {
				selectedTileGroup.ResetTileStates( );

				PlaySoundEffect(SoundEffectType.CANCEL);

				SelectTileGroup(null);
			}
		}
	}

	/// <summary>
	/// Update the current animation frame
	/// </summary>
	private void UpdateAnimationFrame ( ) {
		// Increment the animation timer by the time that has passed since the last update call
		// Once the timer has reached the animation speed, update the sprites
		animationTimer += Time.deltaTime;
		if (animationTimer >= AnimationSpeed) {
			// Subtract the animation time from the animation timer
			// This makes it slightly more exact in when the animation changes sprites
			animationTimer -= AnimationSpeed;

			// Increment the current animation frame
			// All animations should use this so they are all synced
			// All animations will be 4 looped frames
			CurrentAnimationFrame = (CurrentAnimationFrame + 1) % 4;

			// Update all of the tiles if they need to be animated
			// If there are no subscribed events, this throws an error
			OnAnimationFrame?.Invoke( );
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
		if (menuState == MenuState.PAUSE) {
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

			PlaySoundEffect(SoundEffectType.SELECT_TILES);

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
	public IEnumerator SetGameState (GameState gameState) {
		// If the game state is being set to the same value, return and do nothing
		if (GameState == gameState) {
			yield break;
		}

		// Save the old gamestate as there are certain things that we might want to do based on it
		GameState oldGameState = GameState;
		GameState = gameState;

		// No matter the game state, set the selected tile group to null
		SelectTileGroup(null);

		// Do specific things based on the new game state
		switch (GameState) {
			case GameState.GENERATE:
				// Set default values
				TurnCount = 0;
				startTime = Time.time;

				SetMenuState(MenuState.PLAY);

				yield return BoardManager.Instance.Generate( );

				break;
			case GameState.PLAYER_TURN:
				turnIndicatorText.text = "Player's Turn";

				// Since the player has survived the entity turn or the board has just been generated, increment the number of survived rounds
				if (oldGameState == GameState.ENTITY_TURN || oldGameState == GameState.GENERATE) {
					// Spawn new random entities in
					// Make sure entities are never spawned on the same tile group has the robot
					yield return EntityManager.Instance.SpawnRandomEntities(excludedTileGroups: new List<TileGroup>( ) { EntityManager.Instance.Robot.Tile.TileGroup });

					TurnCount++;
				}

				PlaySoundEffect(SoundEffectType.PLAYER_TURN);

				break;
			case GameState.ENTITY_TURN:
				turnIndicatorText.text = "Entities' Turn";

				// Update all the turns of every entity on the board
				yield return EntityManager.Instance.UpdateEntityTurns( );

				break;
			case GameState.GAME_OVER:
				// Wait for the robot to be destroyed
				yield return new WaitForSeconds(AnimationSpeed * 6f);

				// Update the stats text
				statsText.text = $"Turns survived: {TurnCount}\nLasers destroyed: {LasersDestroyed}\nTotal run time: {(int) (Time.time - startTime)} seconds";

				SetMenuState(MenuState.GAME_OVER);

				break;
		}
	}

	/// <summary>
	/// Set the menu state of the game
	/// </summary>
	/// <param name="menuState">The menu state to set the game to</param>
	public void SetMenuState (MenuState menuState) {
		// No matter the menu state, set the selected tile group to null
		SelectTileGroup(null);

		this.menuState = menuState;

		pauseMenu.SetActive(menuState == MenuState.PAUSE);
		gameOverMenu.SetActive(menuState == MenuState.GAME_OVER);
		mainMenu.SetActive(menuState == MenuState.MAIN);
		creditsMenu.SetActive(menuState == MenuState.CREDITS);
		howToPlayMenu.SetActive(menuState == MenuState.HOW_TO_PLAY);
	}

	/// <summary>
	/// Function that is called to start the game
	/// </summary>
	public void PlayGame ( ) {
		StartCoroutine(SetGameState(GameState.GENERATE));
	}

	/// <summary>
	/// Function that is called to quit the game :(
	/// </summary>
	public void Quit ( ) {
		Application.Quit( );
	}

	/// <summary>
	/// Function that is called to pause the game
	/// </summary>
	public void PauseGame ( ) {
		SetMenuState(MenuState.PAUSE);
	}

	/// <summary>
	/// Function called to unpause the game
	/// </summary>
	public void UnPauseGame ( ) {
		SetMenuState(MenuState.PLAY);
	}

	/// <summary>
	/// Function that navigates the player back to the main menu
	/// </summary>
	public void GoToMainMenu ( ) {
		SetMenuState(MenuState.MAIN);
	}

	/// <summary>
	/// Function that navigates to the credits menu
	/// </summary>
	public void GoToCreditsMenu ( ) {
		SetMenuState(MenuState.CREDITS);
	}

	/// <summary>
	/// Function that navigates to the how to play menu
	/// </summary>
	public void GoToHowToPlayMenu ( ) {
		SetMenuState(MenuState.HOW_TO_PLAY);
	}

	/// <summary>
	/// Plays the specified sound effect
	/// </summary>
	/// <param name="soundEffectType">The sound effect type to play</param>
	public void PlaySoundEffect (SoundEffectType soundEffectType) {
		audioSource.PlayOneShot(soundEffects[(int) soundEffectType]);
	}
}
