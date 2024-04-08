using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Header("Properties")]
	[SerializeField] private Vector2Int _boardPosition;
	[SerializeField] private int _tileGroupID;

	public int TileGroupID {
		get => _tileGroupID;
		set {
			// Remove this tile from the previous group if it was in one
			if (_tileGroupID >= 0) {
				Board.Instance.Tiles[_tileGroupID].Remove(this);
			}

			// Make sure that the tile group ID never exceeds the size of the main tiles array
			_tileGroupID = Mathf.Min(value, Board.Instance.Tiles.Count);

			// If the specified tile group is greater than or equal to the current amount of tile groups, then create a new group
			// If there is already a tile group for the specified ID, then add this tile to that group
			if (_tileGroupID >= Board.Instance.Tiles.Count) {
				Board.Instance.Tiles.Add(new List<Tile>( ) { this });
			} else if (_tileGroupID >= 0) {
				Board.Instance.Tiles[_tileGroupID].Add(this);
			}
		}
	}

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
