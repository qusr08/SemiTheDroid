using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGroup {
	private List<Tile> tiles;
	private bool _isSelected;
	private bool _isHovered;

	public Tile this[int index] { get => tiles[index]; set => tiles[index] = value; }

	/// <summary>
	/// The current number of tiles inside this tile group
	/// </summary>
	public int Count => tiles.Count;

	/// <summary>
	/// Whether or not this tile is selected or not
	/// </summary>
	public bool IsSelected {
		get => _isSelected;
		set {
			// Do nothing if you are setting the selection to the same value
			if (_isSelected == value) {
				return;
			}

			_isSelected = value;

			// Update all tiles in this tile group
			foreach (Tile tile in tiles) {
				tile.UpdateTileType( );
			}
		}
	}

	/// <summary>
	/// Whether or not this tile group is hovered
	/// </summary>
	public bool IsHovered {
		get => _isHovered;
		set {
			// Do nothing if you are setting the selection to the same value
			if (_isHovered == value) {
				return;
			}

			_isHovered = value;

			// Update all tiles in this tile group
			foreach (Tile tile in tiles) {
				tile.UpdateTileType( );
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
}
