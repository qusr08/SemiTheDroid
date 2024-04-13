using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour {
	[Header("Properties")]
	[SerializeField] private int _turnsUntilAction;
	[Header("Information")]
	[SerializeField] private Tile _tile;

	/// <summary>
	/// The tile that this entity is currently standing on
	/// </summary>
	public Tile Tile { get => _tile; set => _tile = value; }

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
