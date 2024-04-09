using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The order of the sprites inside the "sprites" list should match up to the order of these enum values
public enum TileType {
	NONE = -1,
	REG_SOL_SOL, REG_SOL_DOT, REG_DOT_SOL, REG_DOT_DOT,
	SEL_SOL_SOL, SEL_SOL_DOT, SEL_DOT_SOL, SEL_DOT_DOT,
	HAZ_F1, HAZ_F2, HAZ_F3, HAZ_F4,
	OUT_F1, OUT_F2, OUT_F3, OUT_F4
}

public class Tile : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private SpriteRenderer hoverTileSpriteRenderer;
	[SerializeField] private SpriteRenderer overlayTileSpriteRenderer;
	[SerializeField] private Sprite[ ] sprites;
	[Header("Properties")]
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private TileType _tileType;
	[SerializeField] private TileType _hoveredTileType;
	[SerializeField] private TileType _overlayTileType;
	[SerializeField] private bool _isHazard;

	private TileGroup _tileGroup;
	private bool topLeftTileValue;
	private bool topRightTileValue;

	public bool IsHazard {
		get => _isHazard;
		set {
			// Do nothing if you are setting the selection to the same value
			if (_isHazard == value) {
				return;
			}

			_isHazard = value;

			// Update the tile sprite
			UpdateTileType( );
		}
	}

	/// <summary>
	/// The type of sprite that is showing on this tile
	/// </summary>
	public TileType TileType {
		get => _tileType;
		set {
			_tileType = value;

			// Set the sprite of this tile
			if (_tileType != TileType.NONE) {
				spriteRenderer.sprite = sprites[(int) _tileType];
			} else {
				spriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The type of sprite that is showing on the hovered tile
	/// </summary>
	public TileType HoveredTileType {
		get => _hoveredTileType;
		set {
			_hoveredTileType = value;

			// Set the sprite of the hovered tile
			if (_hoveredTileType != TileType.NONE) {
				hoverTileSpriteRenderer.sprite = sprites[(int) _hoveredTileType];
			} else {
				hoverTileSpriteRenderer.sprite = null;
			}
		}
	}

	/// <summary>
	/// The type of sprite that is showing on the hovered tile
	/// </summary>
	public TileType OverlayTileType {
		get => _overlayTileType;
		set {
			_overlayTileType = value;

			// Set the sprite of the overlay tile
			if (_overlayTileType != TileType.NONE) {
				overlayTileSpriteRenderer.sprite = sprites[(int) _overlayTileType];
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
			UpdateTileType(updateTileValues: true);

			// Update connecting tiles as well
			Tile bottomLeftTile = Board.Instance.GetTile(_boardPosition + Vector2Int.down);
			if (bottomLeftTile != null) {
				bottomLeftTile.UpdateTileType(updateTileValues: true);
			}

			Tile bottomRightTile = Board.Instance.GetTile(_boardPosition + Vector2Int.right);
			if (bottomRightTile != null) {
				bottomRightTile.UpdateTileType(updateTileValues: true);
			}
		}
	}

	private void OnMouseEnter ( ) {
		// TileGroup.IsSelected = true;
		TileGroup.IsHovered = true;
		// IsHazard = true;
	}

	private void OnMouseExit ( ) {
		// TileGroup.IsSelected = false;
		TileGroup.IsHovered = false;
		// IsHazard = false;
	}

	private void Start ( ) {
		UpdateTileType( );

		Board.OnAnimationFrame += ( ) => {
			if (!TileGroup.IsHovered && !IsHazard) {
				return;
			}

			UpdateTileType( );
		};
	}

	/// <summary>
	/// Update the type of this tile based on the surrounding tiles
	/// </summary>
	/// <param name="updateTileValues">Whether or not to update the type of tile based on the surrounding tiles. Automatically set to false</param>
	public void UpdateTileType (bool updateTileValues = false) {
		// Get whether or not the surrounding tiles are part of the same tile group
		if (updateTileValues) {
			topLeftTileValue = Board.Instance.GetTile(_boardPosition + Vector2Int.left, tileGroup: TileGroup) != null;
			topRightTileValue = Board.Instance.GetTile(_boardPosition + Vector2Int.up, tileGroup: TileGroup) != null;
		}

		// The type of the tile normally
		TileType normalType = (TileType) (
			(TileGroup.IsSelected ? 4 : 0) +
			((topLeftTileValue ? 1 : 0) * 2) +
			((topRightTileValue ? 1 : 0) * 1)
		);

		// Set the type of the tile
		if (TileGroup.IsHovered) {
			TileType = TileType.OUT_F1 + Board.Instance.CurrentAnimationFrame;
			HoveredTileType = normalType;
		} else {
			TileType = normalType;
			HoveredTileType = TileType.NONE;
		}

		// If the board is currently showing a hazard, then update the overlay tile
		if (IsHazard) {
			OverlayTileType = TileType.HAZ_F1 + Board.Instance.CurrentAnimationFrame;
		} else {
			OverlayTileType = TileType.NONE;
		}
	}
}
