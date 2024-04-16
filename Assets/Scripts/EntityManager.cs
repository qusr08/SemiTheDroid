using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {
	[Header("References")]
	[SerializeField] private GameObject spikePrefab;
	[SerializeField] private GameObject robotPrefab;
	[SerializeField] private GameObject laserPrefab;
	[SerializeField] private GameObject bombPrefab;
	[Header("Information")]
	[SerializeField] private Entity _hoveredEntity;
	[SerializeField] private List<Vector2Int> _shownHazardPositions;

	private List<Entity> entities;

	/// <summary>
	/// The current entity that is being hovered
	/// </summary>
	public Entity HoveredEntity {
		get => _hoveredEntity;
		set {
			// If the same entity is trying to be hovered again, return and do nothing
			if (_hoveredEntity == value) {
				return;
			}

			_hoveredEntity = value;

			UpdateShownHazardTiles( );
		}
	}

	/// <summary>
	/// A list of all the board positions that are currently being shown as a hazard
	/// </summary>
	public List<Vector2Int> ShownHazardPositions { get => _shownHazardPositions; private set => _shownHazardPositions = value; }

	protected override void Awake ( ) {
		base.Awake( );

		entities = new List<Entity>( );
		_shownHazardPositions = new List<Vector2Int>( );
	}

	/// <summary>
	/// Spawn an entity onto a tile
	/// </summary>
	/// <param name="entityType">The type of entity to spawn</param>
	/// <param name="tile">The tile to spawn the entity on</param>
	public void SpawnEntity (EntityType entityType, Tile tile) {
		Entity newEntity = null;

		switch (entityType) {
			case EntityType.SPIKE:
				newEntity = Instantiate(spikePrefab, Vector3.zero, Quaternion.identity).GetComponent<Spike>( );
				break;
			case EntityType.ROBOT:
				newEntity = Instantiate(robotPrefab, Vector3.zero, Quaternion.identity).GetComponent<Robot>( );
				break;
			case EntityType.LASER:
				newEntity = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<Laser>( );
				break;
			case EntityType.BOMB:
				newEntity = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity).GetComponent<Bomb>( );
				break;
		}

		// Set the tile entity and add the entity to the list of entities
		entities.Add(newEntity);
		tile.Entity = newEntity;

		// Face the entity in a random direction when starting
		newEntity.FacingDirection = BoardManager.Instance.GetCardinalPositions(Vector2Int.zero)[Random.Range(0, 4)];
	}

	/// <summary>
	/// Update all of the tiles on the board that need to show a hazard based on the currently hovered entity
	/// </summary>
	public void UpdateShownHazardTiles ( ) {
		// Create a list to hold all of the new hazard board positions
		List<Vector2Int> newShownHazardPositions = new List<Vector2Int>( );

		// Loop through all the entities on the board
		foreach (Entity entity in entities) {
			// If the hovered entity is null, then we want all of the entity hazard tiles to be visible
			// If the hovered entity is not null, then we only want to show the current entity's hazard tiles
			if (HoveredEntity == null || HoveredEntity == entity) {
				newShownHazardPositions.AddRange(entity.HazardBoardPositions);
			}
		}

		// Only keep all of the board positions that changed in being shown as hazard
		List<Vector2Int> modifiedBoardPositions = ShownHazardPositions.Except(newShownHazardPositions).Union(newShownHazardPositions.Except(ShownHazardPositions)).ToList( );

		// Loop through all of the tiles at the modified board positions and change their tile overlay states
		foreach (Tile tile in BoardManager.Instance.SearchForTilesAt(modifiedBoardPositions)) {
			switch (tile.TileOverlayState) {
				case TileOverlayState.NONE:
					tile.TileOverlayState = TileOverlayState.HAZARD;

					break;
				case TileOverlayState.HAZARD:
					tile.TileOverlayState = TileOverlayState.NONE;

					break;
			}
		}

		// Set the current shown positions to be the new shown positions
		ShownHazardPositions = newShownHazardPositions;
	}
}
