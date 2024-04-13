using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGroup {
	private List<Tile> tiles;
	private TileState _tileGroupState;
	private Tile _originTile;

	public Tile this[int index] { get => tiles[index]; set => tiles[index] = value; }

	/// <summary>
	/// The current number of tiles inside this tile group
	/// </summary>
	public int Count => tiles.Count;

	/// <summary>
	/// The origin tile for this tile group. This is used to track its movement
	/// </summary>
	public Tile OriginTile {
		get => _originTile;
		set {
			// If the tile is not in this tile group, ignore it
			if (!tiles.Contains(value)) {
				return;
			}

			_originTile = value;
		}
	}

	/// <summary>
	/// The state of this tile group
	/// </summary>
	public TileState TileGroupState {
		get => _tileGroupState;
		set {
			_tileGroupState = value;

			// Update all tiles in this tile group
			foreach (Tile tile in tiles) {
				tile.TileState = _tileGroupState;
			}
		}
	}

	/// <summary>
	/// Default constructor for tile groups
	/// </summary>
	public TileGroup ( ) {
		tiles = new List<Tile>( );
	}

	/// <summary>
	/// Add a tile to this tile group
	/// </summary>
	/// <param name="tile">The tile to add</param>
	public void AddTile (Tile tile) {
		// If the tile is already in this group, return
		if (tile.TileGroup == this) {
			return;
		}

		// Add the tile to this group
		tiles.Add(tile);
	}

	/// <summary>
	/// Remove a tile from this tile group
	/// </summary>
	/// <param name="tile">The tile to remove</param>
	public void RemoveTile (Tile tile) {
		// If the tile is not in this group, return
		if (tile.TileGroup != this) {
			return;
		}

		// Add the tile to this group
		tiles.Remove(tile);
	}

	/// <summary>
	/// Try to move this tile group to a specific board position
	/// </summary>
	/// <param name="boardPosition">The board position to check if this tile group can be placed there</param>
	/// <returns>true if the tile group successfully moved to the specified board position, false otherwise</returns>
	public bool TryMove (Vector2Int boardPosition) {
		List<Tile> originTileOrder = new List<Tile>( ) { OriginTile };

		// Get the order of origin tiles to switch to
		// This makes the tile groups not move around as much when they are being placed
		for (int i = 0; i < tiles.Count; i++) {
			// Get all of the cardinal tiles to the current origin tile
			List<Tile> cardinalTiles = BoardManager.Instance.GetCardinalTiles(originTileOrder[i].BoardPosition, exclusiveTileGroup: this);

			// Add all cardinal tiles in this tile group that have not already been added
			for (int j = 0; j < cardinalTiles.Count; j++) {
				// Do not add tiles that have already been added
				if (originTileOrder.Contains(cardinalTiles[j])) {
					continue;
				}

				originTileOrder.Add(cardinalTiles[j]);

				// If all of the tiles have already been added to the list, then return at start trying to find 
				if (originTileOrder.Count == Count) {
					// Just in case this tile group fails to be placed, the original origin tile will be reset back to being the origin tile
					originTileOrder.Add(OriginTile);

					// Break out of all the current loops
					i = tiles.Count;
					break;
				}
			}
		}

		// Loop through all possible origin tiles to see if the tile group can fit at the location
		for (int i = 0; i < Count; i++) {
			// A list of all the new positions that all the blocks will move to
			List<Vector2Int> newPositions = new List<Vector2Int>( );

			// Calculate the offset between the origin tile and the board position to move this tile group to
			Vector2Int tileOffset = boardPosition - OriginTile.BoardPosition;

			// Whether or not the new position of this tile group will be adjacent to another tile group
			bool hasAdjacentTileGroup = false;

			// Add all of the new board positions and adjacent tile groups to their arrays
			for (int j = 0; j < Count; j++) {
				Vector2Int newPosition = tiles[j].BoardPosition + tileOffset;
				newPositions.Add(newPosition);

				// Check to see if the current tile has an adjacent tile group that is not the current tile group
				// This is to make sure the tile group is not floating away from the rest of the board positions
				if (!hasAdjacentTileGroup) {
					hasAdjacentTileGroup = BoardManager.Instance.GetCardinalTileGroups(newPosition, excludedTileGroup: this).Count > 0;
				}
			}

			// Make sure there are no tiles at the new positions and that there is at least one adjacent tile group
			if (hasAdjacentTileGroup && !BoardManager.Instance.HasTilesAt(newPositions, excludedTileGroup: this)) {
				// Move the selected tiles
				for (int j = 0; j < Count; j++) {
					tiles[j].BoardPosition += tileOffset;
				}

				// Since all of the tiles were moved, recalculate all of the tile sprites
				// RecalculateTileSprites( );

				return true;
			}

			// Since the tile group cannot move to the specified position with the current origin tile, try switching it to a different one
			OriginTile = originTileOrder[i + 1];
		}

		return false;
	}

	/// <summary>
	/// Get a list of all the tile groups that are adjacent (touching) this tile group on the board
	/// </summary>
	/// <returns>A TileGroup list with all of the adjacent tile groups in it</returns>
	public List<TileGroup> GetAdjacentTileGroups ( ) {
		// A list to store all of the adjacent tile groups
		List<TileGroup> adjacentTileGroups = new List<TileGroup>( );

		// Loop through each tile in this tile group to check for surrounding tile groups
		foreach (Tile tile in tiles) {
			// Get all of the cardinal tile groups around the current tile, excluding all of the adjacent tile groups already found
			adjacentTileGroups.AddRange(BoardManager.Instance.GetCardinalTileGroups(tile.BoardPosition, excludedTileGroups: adjacentTileGroups));
		}

		// Remove this tile group from the adjacent tile groups to finalize the list results
		adjacentTileGroups.Remove(this);
		return adjacentTileGroups;
	}

	/// <summary>
	/// Recalculate all of the tile sprites inside of this tile group
	/// </summary>
	public void RecalculateTileSprites ( ) {
		for (int i = 0; i < tiles.Count; i++) {
			tiles[i].RecalculateTileSprite( );
		}
	}
}
