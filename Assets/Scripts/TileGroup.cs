using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TreeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TileGroup {
	private List<Tile> _tiles;
	private TileState _tileGroupState;
	private Tile _originTile;
	private List<Tile> originTileOrder;

	/// <summary>
	/// A list of all the tiles that are a part of this tile group
	/// </summary>
	public List<Tile> Tiles { get => _tiles; private set => _tiles = value; }

	/// <summary>
	/// The current number of tiles inside this tile group
	/// </summary>
	public int Count => Tiles.Count;

	/// <summary>
	/// The origin tile for this tile group. This is used to track its movement
	/// </summary>
	public Tile OriginTile {
		get => _originTile;
		set {
			// If the tile is not in this tile group or it is being set to the same value, ignore it
			if (!Tiles.Contains(value) || _originTile == value) {
				return;
			}

			_originTile = value;

			// Reset the 
			originTileOrder = new List<Tile>( ) { _originTile };

			// Get the order of origin tiles to switch to
			// This makes the tile groups not move around as much when they are being placed
			for (int i = 0; i < Tiles.Count; i++) {
				// Get all of the cardinal tiles to the current origin tile
				List<Tile> cardinalTiles = BoardManager.Instance.GetCardinalTiles(originTileOrder[i].BoardPosition, exclusiveTileGroups: new List<TileGroup>( ) { this });

				// Add all cardinal tiles in this tile group that have not already been added
				foreach (Tile cardinalTile in cardinalTiles) {
					// Do not add tiles that have already been added
					if (originTileOrder.Contains(cardinalTile)) {
						continue;
					}

					originTileOrder.Add(cardinalTile);

					// If all of the tiles have already been added to the list, then return at start trying to find 
					if (originTileOrder.Count == Count) {
						// Just in case this tile group fails to be placed, the original origin tile will be reset back to being the origin tile
						originTileOrder.Add(_originTile);

						// Break out of all the current loops
						i = Tiles.Count;
						break;
					}
				}
			}
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
			foreach (Tile tile in Tiles) {
				tile.TileState = _tileGroupState;
			}
		}
	}

	/// <summary>
	/// Default constructor for tile groups
	/// </summary>
	public TileGroup ( ) {
		Tiles = new List<Tile>( );
	}

	/// <summary>
	/// Try to move this tile group to a specific board position
	/// </summary>
	/// <param name="movementDirection">The direction to move all of the tiles in this tile group by</param>
	/// <param name="rotationDirection">The direction to rotate the tile group around the origin tile by 90 degrees</param>
	/// <returns>true if the tile group successfully moved to the specified board position, false otherwise</returns>
	public bool TryMoveAndRotate (Vector2Int boardPosition, int rotationDirection) {
		// A list of all the new positions that all the blocks will move to
		List<Vector2Int> newTilePositions = new List<Vector2Int>( );

		// Loop through all possible origin tiles to see if the tile group can fit at the location
		foreach (Tile originTile in originTileOrder) {
			// Calculate the offset between the origin tile and the board position to move this tile group to
			Vector2Int tileOffset = boardPosition - originTile.BoardPosition;

			// Whether or not the new position of this tile group will be adjacent to another tile group
			bool hasAdjacentTileGroup = false;

			// Add all of the new board positions and adjacent tile groups to their arrays
			foreach (Tile tile in Tiles) {
				// Calculate the new position that the current tile will move to based on the movement and rotation direction
				Vector2Int newPosition = Utilities.Vector2IntRotateAround(tile.BoardPosition, originTile.BoardPosition, rotationDirection) + tileOffset;
				newTilePositions.Add(newPosition);

				// Check to see if the current tile has an adjacent tile group that is not the current tile group
				// This is to make sure the tile group is not floating away from the rest of the board positions
				if (!hasAdjacentTileGroup) {
					hasAdjacentTileGroup = BoardManager.Instance.GetCardinalTileGroups(newPosition, excludedTileGroups: new List<TileGroup>( ) { this }).Count > 0;
				}
			}

			// Make sure there are no tiles at the new positions and that there is at least one adjacent tile group
			if (hasAdjacentTileGroup && !BoardManager.Instance.HasTilesAt(newTilePositions, excludedTileGroups: new List<TileGroup>( ) { this })) {
				// Move the selected tiles by the calculated amount
				for (int i = 0; i < Count; i++) {
					// The positions calculated earlier are the new positions for each of the tiles
					Tiles[i].BoardPosition = newTilePositions[i];

					// If the tile being moved has an entity on it, rotate the entity along with the tile group
					Entity tileEntity = Tiles[i].Entity;
					if (tileEntity != null) {
						tileEntity.FacingDirection = Utilities.Vector2IntRotateAround(tileEntity.FacingDirection, Vector2Int.zero, rotationDirection);
					}
				}

				// The new origin tile will be equal to whatever origin had a successful movement
				// We do not want to set the propery here because the set part of it has special code that we dont not want to run
				_originTile = originTile;

				// Update the tile sprites if the tiles were rotated
				// If the tiles were just translated, then there is no need to update
				if (rotationDirection != 0) {
					RecalculateTileSprites( );
				}

				return true;
			}

			// Clear the positions array each time a new origin tile is chosen
			newTilePositions.Clear( );
		}

		return false;
	}

	public bool TryRotate (int rotateDirection) {
		return false;
	}

	/// <summary>
	/// Recalculate all of the tile sprites inside of this tile group
	/// </summary>
	public void RecalculateTileSprites ( ) {
		foreach (Tile tile in Tiles) {
			tile.RecalculateTileSprite( );
		}
	}
}
