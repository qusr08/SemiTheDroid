using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGroup {
	private List<Tile> tiles;
	private TileState _tileGroupState;

	public Tile this[int index] { get => tiles[index]; set => tiles[index] = value; }

	/// <summary>
	/// The current number of tiles inside this tile group
	/// </summary>
	public int Count => tiles.Count;

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
	/// Get a list of all the tile groups that are adjacent (touching) this tile group on the board
	/// </summary>
	/// <returns>A TileGroup list with all of the adjacent tile groups in it</returns>
	public List<TileGroup> GetAdjacentTileGroups ( ) {
		// A list to store all of the adjacent tile groups
		List<TileGroup> adjacentTileGroups = new List<TileGroup>( );

		// Loop through each tile in this tile group to check for surrounding tile groups
		foreach (Tile tile in tiles) {
			// Loop through each of the tile groups around the current tile to try and add them to the adjacent tile groups array
			foreach (TileGroup tileGroup in BoardManager.Instance.GetCardinalTileGroups(tile.BoardPosition)) {
				// If the tile group has already been added or the tile group is equal to this tile group, continue to the next group
				if (adjacentTileGroups.Contains(tileGroup) || tileGroup == this) {
					continue;
				}

				adjacentTileGroups.Add(tileGroup);
			}
		}

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
