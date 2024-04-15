using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Header("Properties")]
	[SerializeField] private int _turnsUntilAction;
	[Header("Information")]
	[SerializeField] private Tile _tile;

	/// <summary>
	/// The tile that this entity is currently standing on
	/// </summary>
	public Tile Tile {
		get => _tile;
		set {
			// If the tile is being set to the same value, return
			if (_tile == value) {
				return;
			}

			// Set the tile value and its entity value to this entity
			_tile = value;
			_tile.Entity = this;

			// Set the sorting order of this sprite to be above the tile
			spriteRenderer.sortingOrder = BoardPosition.x - BoardPosition.y + 5;
		}
	}

	/// <summary>
	/// The board position of this entity
	/// </summary>
	public Vector2Int BoardPosition => Tile.BoardPosition;

	/// <summary>
	/// The number of turns until this entity does its action
	/// </summary>
	public int TurnsUntilAction { get => _turnsUntilAction; set => _turnsUntilAction = value; }

	public abstract void PerformAction ( );
}
