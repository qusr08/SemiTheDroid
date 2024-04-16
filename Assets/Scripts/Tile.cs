using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The order of the sprites inside the "sprites" list should match up to the order of these enum values
public enum TileSpriteType {
	NONE = -1,
	DEF_SOL_SOL, DEF_SOL_DOT, DEF_DOT_SOL, DEF_DOT_DOT,
	HOV_SOL_SOL, HOV_SOL_DOT, HOV_DOT_SOL, HOV_DOT_DOT,
	HAZ_F1, HAZ_F2, HAZ_F3, HAZ_F4,
	OUT_F1, OUT_F2, OUT_F3, OUT_F4,
	HAZ_HOV_F1, HAZ_HOV_F2, HAZ_HOV_F3, HAZ_HOV_F4
}

public enum TileState {
	DEFAULT, SELECTED, HOVERED
}

public enum TileOverlayState {
	NONE, HAZARD
}

public class Tile : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer tileSpriteRenderer;
	[SerializeField] private SpriteRenderer detailTileSpriteRenderer;
	[SerializeField] private SpriteRenderer overlayTileSpriteRenderer;
	[SerializeField] private Transform tileTransform;
	[SerializeField] private Sprite[ ] sprites;
	[Header("Information")]
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private Vector2Int _resetPosition;
	[SerializeField] private TileState _tileState;
	[SerializeField] private TileOverlayState _tileOverlayState;
	[SerializeField] private TileSpriteType _tileSpriteType;
	[SerializeField] private TileSpriteType _detailTileSpriteType;
	[SerializeField] private TileSpriteType _overlayTileSpriteType;
	[SerializeField] private Entity _entity;
	[SerializeField] private int drawOrder;

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
	/// The current state of the tile overlay
	/// </summary>
	public TileOverlayState TileOverlayState {
		get => _tileOverlayState;
		set {
			// If the tile overlay state is being set to the same value, return and do nothing
			if (_tileOverlayState == value) {
				return;
			}

			_tileOverlayState = value;

			UpdateTileSprite( );
		}
	}

	/// <summary>
	/// The type of sprite that is showing on this tile
	/// </summary>
	public TileSpriteType TileSpriteType {
		get => _tileSpriteType;
		set {
			// Do not update the sprite if it is being set to the same value
			if (_tileSpriteType == value) {
				return;
			}

			_tileSpriteType = value;

			// Set the sprite of this tile
			if (_tileSpriteType != TileSpriteType.NONE) {
				tileSpriteRenderer.sprite = sprites[(int) _tileSpriteType];
			} else {
				tileSpriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The type of sprite that is showing on the hovered tile
	/// </summary>
	public TileSpriteType DetailTileSpriteType {
		get => _detailTileSpriteType;
		set {
			// Do not update the sprite if it is being set to the same value
			if (_detailTileSpriteType == value) {
				return;
			}

			_detailTileSpriteType = value;

			// Set the sprite of the detail tile
			if (_detailTileSpriteType != TileSpriteType.NONE) {
				detailTileSpriteRenderer.sprite = sprites[(int) _detailTileSpriteType];
			} else {
				detailTileSpriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The type of sprite that is showing on the overlay tile
	/// </summary>
	public TileSpriteType OverlayTileSpriteType {
		get => _overlayTileSpriteType;
		set {
			// Do not update the sprite if it is being set to the same value
			if (_overlayTileSpriteType == value) {
				return;
			}

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
				_tileGroup.Tiles.Remove(this);
			}

			// Add this tile to the new group if it is not null
			if (value != null) {
				value.Tiles.Add(this);
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
			// If the board position is being set to the same position, then return and do nothing
			if (_boardPosition == value) {
				return;
			}

			_boardPosition = value;

			// Make sure tiles always align to the isometric grid
			transform.position = BoardManager.Instance.BoardToWorldPosition(_boardPosition);

			// Make sure tiles that have a lower y position appear in front of others
			drawOrder = (_boardPosition.x - _boardPosition.y) * 5;
			detailTileSpriteRenderer.sortingOrder = drawOrder;
			tileSpriteRenderer.sortingOrder = drawOrder + 1;
			overlayTileSpriteRenderer.sortingOrder = drawOrder + 2;

			// Set the entity sorting order as well if there is one on this tile
			if (Entity != null) {
				Entity.SetSpriteSortingOrder(drawOrder + 3);
			}
		}
	}

	/// <summary>
	/// Used to track the position that this tile will reset back to if a tile group selection is cancelled
	/// </summary>
	public Vector2Int ResetPosition { get => _resetPosition; set => _resetPosition = value; }

	/// <summary>
	/// The entity currently on this tile
	/// </summary>
	public Entity Entity {
		get => _entity;
		set {
			// If the entity value is being set to the same value, return
			if (_entity == value) {
				return;
			}

			// Set this entity's tile value to be this tile
			_entity = value;

			// Make sure the entity follows this tile (as in, when it gets selected and moves upwards, the entity also moves)
			if (_entity != null) {
				_entity.transform.SetParent(tileTransform, false);
				_entity.Tile = this;
				_entity.SetSpriteSortingOrder(drawOrder + 3);
			}
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

		TileGroup.TileGroupState = TileState.DEFAULT;
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
		List<Tile> tiles = BoardManager.Instance.SearchForTilesAt(
			new List<Vector2Int>( ) { BoardPosition + Vector2Int.left, BoardPosition + Vector2Int.up },
			exclusiveTileGroups: new List<TileGroup>( ) { TileGroup }
		);

		// Set the regular sprite of this tile based on the above tiles
		switch (tiles.Count) {
			case 0:
				// This means there are no tiles above this tile in the tile group
				currentTileSpriteType = TileSpriteType.DEF_SOL_SOL;

				break;
			case 1:
				// This means one of tiles above this tile are in the tile group, we just need to know which one specifically
				if (tiles[0].BoardPosition == BoardPosition + Vector2Int.left) {
					currentTileSpriteType = TileSpriteType.DEF_DOT_SOL;
				} else {
					currentTileSpriteType = TileSpriteType.DEF_SOL_DOT;
				}

				break;
			case 2:
				// This means both tiles above this tile are in the tile group
				currentTileSpriteType = TileSpriteType.DEF_DOT_DOT;

				break;
		}

		// Update the tile sprite
		UpdateTileSprite( );
	}

	/// <summary>
	/// Update the sprite of the tile based on the current tile state
	/// </summary>
	private void UpdateTileSprite ( ) {
		// Update the sprite renderers based on the new tile state
		switch (TileState) {
			case TileState.DEFAULT:
				TileSpriteType = currentTileSpriteType;
				DetailTileSpriteType = TileSpriteType.NONE;

				// Update the position of the tile
				tileTransform.localPosition = new Vector3(0f, 0f, 0f);

				break;
			case TileState.SELECTED:
				TileSpriteType = currentTileSpriteType;
				DetailTileSpriteType = TileSpriteType.OUT_F1 + GameManager.Instance.CurrentAnimationFrame;

				// If the tile is just now selected, save its reset position
				ResetPosition = BoardPosition;

				// Update the position of the tile
				tileTransform.localPosition = new Vector3(0f, 1.825f, 0f);

				break;
			case TileState.HOVERED:
				TileSpriteType = currentTileSpriteType + 4;
				DetailTileSpriteType = TileSpriteType.NONE;

				// Update the position of the tile
				tileTransform.localPosition = new Vector3(0f, 0f, 0f);

				break;
		}

		// Update the overlay sprite renderer based on the new state
		switch (TileOverlayState) {
			case TileOverlayState.NONE:
				OverlayTileSpriteType = TileSpriteType.NONE;

				break;
			case TileOverlayState.HAZARD:
				OverlayTileSpriteType = (TileState == TileState.HOVERED ? TileSpriteType.HAZ_HOV_F1 : TileSpriteType.HAZ_F1) + GameManager.Instance.CurrentAnimationFrame;

				break;
		}
	}

	/// <summary>
	/// Updates only the sprites necessary for animation
	/// </summary>
	private void UpdateAnimationFrame ( ) {
		// Only update the necessary sprites during necessary states
		// Updating all tiles multiple tiles a second each with three sprites causes a lot of lag
		switch (TileState) {
			case TileState.SELECTED:
				DetailTileSpriteType = TileSpriteType.OUT_F1 + GameManager.Instance.CurrentAnimationFrame;

				break;
		}

		switch (TileOverlayState) {
			case TileOverlayState.HAZARD:
				OverlayTileSpriteType = (TileState == TileState.HOVERED ? TileSpriteType.HAZ_HOV_F1 : TileSpriteType.HAZ_F1) + GameManager.Instance.CurrentAnimationFrame;

				break;
		}
	}
}
