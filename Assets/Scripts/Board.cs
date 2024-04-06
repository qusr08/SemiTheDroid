using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Board : Singleton<Board> {
	[Header("References")]
	[SerializeField] private GameObject tilePrefab;
	[SerializeField] private GameObject tileGroupPrefab;
	[Header("Properties")]
	[SerializeField, Min(1)] private int boardTileCount;

	private void Start ( ) {
		CreateTileGroup( ).Generate( );
	}

	private Tile GetTile (Vector2Int boardPosition) {
		// The raycast origin will be right above the tile on the z axis
		Vector3 raycastOrigin = BoardPositionToWorldPosition(boardPosition) + Vector3.back;

		// Fire a raycast in the direction of the tiles. If it hits a tile, return it
		if (Physics.Raycast(raycastOrigin, Vector3.forward, out RaycastHit hit)) {
			return hit.transform.GetComponent<Tile>( );
		}

		return null;
	}

	public Tile CreateTile (TileGroup tileGroup, Vector2Int boardPosition) {
		Tile tile = Instantiate(tilePrefab, (Vector2) boardPosition, Quaternion.identity, tileGroup.transform).GetComponent<Tile>( );
		tile.BoardPosition = boardPosition;
		tile.TileGroup = tileGroup;

		return tile;
	}

	private TileGroup CreateTileGroup ( ) {
		return Instantiate(tileGroupPrefab, Vector3.zero, Quaternion.identity, transform).GetComponent<TileGroup>( );
	}

	public static Vector3 BoardPositionToWorldPosition (Vector2Int boardPosition) {
		return new Vector3((boardPosition.x + boardPosition.y) * 0.5f, (boardPosition.y - boardPosition.x) * 0.25f);
	}
}
