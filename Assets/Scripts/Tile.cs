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

public enum OverlayTileState {
	NONE, HAZARD
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
	[SerializeField] private OverlayTileState _overlayTileState;

	private TileGroup _tileGroup;
	private bool topLeftTileValue;
	private bool topRightTileValue;

	/// <summary>
	/// Whether or not this tile is currently showing an overlay sprite
	/// </summary>
	public OverlayTileState OverlayTileState {
		get => _overlayTileState;
		set {
			// Do nothing if you are setting the selection to the same value
			if (_overlayTileState == value) {
				return;
			}

			_overlayTileState = value;

			// Update the tile sprite
			UpdateTile( );
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
	/// The type of sprite that is showing on the hovered tile
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
			transform.position = Board.Instance.BoardPositionToWorldPosition(_boardPosition);

			// Make sure tiles that have a lower y position appear in front of others
			spriteRenderer.sortingOrder = _boardPosition.x - _boardPosition.y;
			hoverTileSpriteRenderer.sortingOrder = _boardPosition.x - _boardPosition.y;
			overlayTileSpriteRenderer.sortingOrder = (_boardPosition.x - _boardPosition.y) + 1;

			// Update this tile's type based on the surrounding tiles
			UpdateTile(updateTileValues: true);

			// Update connecting tiles as well
			Tile bottomLeftTile = Board.Instance.GetTile(_boardPosition + Vector2Int.down);
			if (bottomLeftTile != null) {
				bottomLeftTile.UpdateTile(updateTileValues: true);
			}

			Tile bottomRightTile = Board.Instance.GetTile(_boardPosition + Vector2Int.right);
			if (bottomRightTile != null) {
				bottomRightTile.UpdateTile(updateTileValues: true);
			}
		}
	}

	private void OnMouseEnter ( ) {
		// If a tile group is selected, do not update tile states
		if (Board.Instance.SelectedTileGroup != null) {
			return;
		}

		TileGroup.TileGroupState = TileGroupState.HOVERED;
	}

	private void OnMouseExit ( ) {
		// If this tile's tile group is selected, then do not change its state
		if (Board.Instance.SelectedTileGroup != null) {
			return;
		}

		TileGroup.TileGroupState = TileGroupState.REGULAR;
	}

	private void OnMouseDown ( ) {
		// If this tile's tile group is selected, then do not change its state
		if (Board.Instance.SelectedTileGroup != null) {
			return;
		}

		// When this tile is pressed, select its tile group
		Board.Instance.SelectedTileGroup = TileGroup;
	}

	/// <summary>
	/// Update this tile based on the surrounding tiles
	/// </summary>
	/// <param name="updateTileValues">Whether or not to update the tile sprite based on the surrounding tiles. Automatically set to false</param>
	public void UpdateTile (bool updateTileValues = false) {
		// Get whether or not the surrounding tiles are part of the same tile group
		if (updateTileValues) {
			topLeftTileValue = Board.Instance.GetTile(_boardPosition + Vector2Int.left, tileGroup: TileGroup) != null;
			topRightTileValue = Board.Instance.GetTile(_boardPosition + Vector2Int.up, tileGroup: TileGroup) != null;
		}

		// The type of the tile normally
		TileSpriteType normalType = (TileSpriteType) (
			(TileGroup.TileGroupState == TileGroupState.HOVERED ? 4 : 0) +
			((topLeftTileValue ? 1 : 0) * 2) +
			((topRightTileValue ? 1 : 0) * 1)
		);

		// Set the type of the tile
		if (TileGroup.TileGroupState == TileGroupState.SELECTED) {
			TileSpriteType = TileSpriteType.OUT_F1 + Board.Instance.CurrentAnimationFrame;
			HoveredTileSpriteType = normalType;
		} else {
			TileSpriteType = normalType;
			HoveredTileSpriteType = TileSpriteType.NONE;
		}

		// If the board is currently showing a hazard, then update the overlay tile
		if (OverlayTileState == OverlayTileState.HAZARD) {
			OverlayTileSpriteType = TileSpriteType.HAZ_F1 + Board.Instance.CurrentAnimationFrame;
		}
	}
}
