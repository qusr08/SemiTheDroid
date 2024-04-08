using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType {
	SOLID_SOLID, SOLID_DOTTED, DOTTED_SOLID, DOTTED_DOTTED
}

public class Tile : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[SerializeField] private Sprite[ ] sprites;
	[Header("Properties")]
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private TileType _tileType;

	private TileGroup _tileGroup;

	/// <summary>
	/// The type of this tile
	/// </summary>
	public TileType TileType {
		get => _tileType;
		set {
			_tileType = value;

			// Set the sprite of this tile
			spriteRenderer.sprite = sprites[(int) _tileType];
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
			transform.position = Board.BoardPositionToWorldPosition(_boardPosition);

			// Make sure tiles that have a lower y position appear in front of others
			spriteRenderer.sortingOrder = _boardPosition.x - _boardPosition.y;
		}
	}
}
