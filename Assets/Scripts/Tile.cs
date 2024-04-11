using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The order of the sprites inside the "sprites" list should match up to the order of these enum values
public enum TileSpriteType {
	NONE = -1,
	REG_SOL_SOL, REG_SOL_DOT, REG_DOT_SOL, REG_DOT_DOT,
	SEL_SOL_SOL, SEL_SOL_DOT, SEL_DOT_SOL, SEL_DOT_DOT,
	HAZ_F1, HAZ_F2, HAZ_F3, HAZ_F4,
	OUT_F1, OUT_F2, OUT_F3, OUT_F4
}

public enum TileState {
	REGULAR, HAZARD, SELECTED, HOVERED
}

public class Tile : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer hoverTileSpriteRenderer;
	[SerializeField] private SpriteRenderer overlayTileSpriteRenderer;
	[SerializeField] private Sprite[ ] sprites;
	[Header("Properties")]
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private TileSpriteType _tileSpriteType;
	[SerializeField] private TileSpriteType _hoveredTileSpriteType;
	[SerializeField] private TileSpriteType _overlayTileSpriteType;
	[SerializeField] private TileState _tileState;

	private TileGroup _tileGroup;
	private TileSpriteType currentTileSpriteType;

	/// <summary>
	/// The current state of this tile
	/// </summary>
	public TileState TileState {
		get => _tileState;
		set {
			// If the tile state is being set to the same value, return and do nothing
			if (_tileState == value) {
				return;
			}

			_tileState = value;

			UpdateTileSprite( );
		}
	}

	/// <summary>
	/// The type of sprite that is showing on this tile
	/// </summary>
	public TileSpriteType TileSpriteType {
		get => _tileSpriteType;
		set {
			_tileSpriteType = value;

			// Set the sprite of this tile
			if (_tileSpriteType != TileSpriteType.NONE) {
				spriteRenderer.sprite = sprites[(int) _tileSpriteType];
			} else {
				spriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The type of sprite that is showing on the hovered tile
	/// </summary>
	public TileSpriteType HoveredTileSpriteType {
		get => _hoveredTileSpriteType;
		set {
			_hoveredTileSpriteType = value;

			// Set the sprite of the hovered tile
			if (_hoveredTileSpriteType != TileSpriteType.NONE) {
				hoverTileSpriteRenderer.sprite = sprites[(int) _hoveredTileSpriteType];
			} else {
				hoverTileSpriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The type of sprite that is showing on the overlay tile
	/// </summary>
	public TileSpriteType OverlayTileSpriteType {
		get => _overlayTileSpriteType;
		set {
			_overlayTileSpriteType = value;

			// Set the sprite of the overlay tile
			if (_overlayTileSpriteType != TileSpriteType.NONE) {
				overlayTileSpriteRenderer.sprite = sprites[(int) _overlayTileSpriteType];
			} else {
				overlayTileSpriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The tile group that this tile is part of
	/// </summary>
	public TileGroup TileGroup {
		get => _tileGroup;
		set {
			// Remove this tile from the previous group if it was in one
			if (_tileGroup != null) {
				_tileGroup.RemoveTile(this);
			}

			// Add this tile to the new group if it is not null
			if (value != null) {
				value.AddTile(this);
			}

			_tileGroup = value;
		}
	}

	/// <summary>
	/// The board position of this tile
	/// </summary>
	public Vector2Int BoardPosition {
		get => _boardPosition;
		set {
			_boardPosition = value;

			// Make sure tiles always align to the isometric grid
			transform.position = BoardManager.Instance.BoardToWorldPosition(_boardPosition);

			// Make sure tiles that have a lower y position appear in front of others
			spriteRenderer.sortingOrder = _boardPosition.x - _boardPosition.y;
			hoverTileSpriteRenderer.sortingOrder = _boardPosition.x - _boardPosition.y;
			overlayTileSpriteRenderer.sortingOrder = (_boardPosition.x - _boardPosition.y) + 1;
		}
	}

	private void OnMouseEnter ( ) {
		// Do not update the hover state while there is a selected tile group
		if (GameManager.Instance.IsTileGroupSelected) {
			return;
		}

		TileGroup.TileGroupState = TileState.HOVERED;
	}

	private void OnMouseExit ( ) {
		// Do not update the hover state while there is a selected tile group
		if (GameManager.Instance.IsTileGroupSelected) {
			return;
		}

		TileGroup.TileGroupState = TileState.REGULAR;
	}

	private void OnMouseDown ( ) {
		// If there is already a selected tile group, then do not select another tile group
		if (GameManager.Instance.IsTileGroupSelected) {
			return;
		}

		// When this tile is pressed, select its tile group
		GameManager.Instance.SelectTileGroup(TileGroup, originTile: this);
	}

	private void Start ( ) {
		GameManager.Instance.OnAnimationFrame += UpdateAnimationFrame;
	}

	private void OnDestroy ( ) {
		// Check to make sure the game manager instance is not null before trying to reference it
		// This was throwing a lot of errors when closing out the editor play mode, this seems to fix it
		if (GameManager.Instance != null) {
			GameManager.Instance.OnAnimationFrame -= UpdateAnimationFrame;
		}
	}

	/// <summary>
	/// Update the main sprite of the tile based on surrounding tiles
	/// </summary>
	public void RecalculateTileSprite ( ) {
		// Get the values of the tiles around this tile
		// Only the top left and right tiles matter because the top row of pixels of each tile cover the one above it
		List<Tile> likeTiles = BoardManager.Instance.SearchForTilesAt(
			new List<Vector2Int>( ) { BoardPosition + Vector2Int.left, BoardPosition + Vector2Int.up },
			exclusiveTileGroups: new List<TileGroup>( ) { TileGroup }
		);

		// The type of the tile normally
		currentTileSpriteType = (TileSpriteType) (((likeTiles[0] != null ? 1 : 0) * 2) + ((likeTiles[1] != null ? 1 : 0) * 1));

		// Update the tile sprite
		UpdateTileSprite( );
	}

	/// <summary>
	/// Update the sprite of the tile based on the current tile state
	/// </summary>
	private void UpdateTileSprite ( ) {
		// Update the sprite renderers based on the new tile state
		switch (_tileState) {
			case TileState.REGULAR:
				TileSpriteType = currentTileSpriteType;
				HoveredTileSpriteType = TileSpriteType.NONE;
				OverlayTileSpriteType = TileSpriteType.NONE;

				break;
			case TileState.HAZARD:
				TileSpriteType = currentTileSpriteType;
				HoveredTileSpriteType = TileSpriteType.NONE;
				OverlayTileSpriteType = TileSpriteType.HAZ_F1 + GameManager.Instance.CurrentAnimationFrame;

				break;
			case TileState.SELECTED:
				TileSpriteType = TileSpriteType.OUT_F1 + GameManager.Instance.CurrentAnimationFrame;
				HoveredTileSpriteType = currentTileSpriteType;
				OverlayTileSpriteType = TileSpriteType.NONE;

				break;
			case TileState.HOVERED:
				TileSpriteType = currentTileSpriteType + 4;
				HoveredTileSpriteType = TileSpriteType.NONE;
				OverlayTileSpriteType = TileSpriteType.NONE;

				break;
		}
	}

	/// <summary>
	/// Updates only the sprites necessary for animation
	/// </summary>
	private void UpdateAnimationFrame ( ) {
		// Only update the necessary sprites during necessary states
		// Updating all tiles multiple tiles a second each with three sprites causes a lot of lag
		switch (_tileState) {
			case TileState.HAZARD:
				OverlayTileSpriteType = TileSpriteType.HAZ_F1 + GameManager.Instance.CurrentAnimationFrame;

				break;
			case TileState.SELECTED:
				TileSpriteType = TileSpriteType.OUT_F1 + GameManager.Instance.CurrentAnimationFrame;

				break;
		}
	}
}
