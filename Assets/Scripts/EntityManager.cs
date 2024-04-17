using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.VersionControl;
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
	[SerializeField] private List<Entity> _entities;

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

			UpdateShownHazardPositions( );
		}
	}

	/// <summary>
	/// A list of all the entities on the board
	/// </summary>
	public List<Entity> Entities { get => _entities; private set => _entities = value; }

	/// <summary>
	/// A list of all the board positions that are currently being shown as a hazard
	/// </summary>
	public List<Vector2Int> ShownHazardPositions { get => _shownHazardPositions; private set => _shownHazardPositions = value; }

	protected override void Awake ( ) {
		base.Awake( );

		Entities = new List<Entity>( );
		ShownHazardPositions = new List<Vector2Int>( );
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

				// Set entity variables that are specific to robots
				newEntity.FacingDirection = BoardManager.Instance.GetCardinalPositions(Vector2Int.zero)[Random.Range(0, 4)];
				newEntity.TurnsUntilAction = 1;

				break;
			case EntityType.LASER:
				newEntity = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<Laser>( );

				// Set entity variables that are specific to lasers
				newEntity.FacingDirection = new List<Vector2Int>( ) { Vector2Int.up, Vector2Int.left }[Random.Range(0, 2)];
				newEntity.TurnsUntilAction = 3;

				break;
			case EntityType.BOMB:
				newEntity = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity).GetComponent<Bomb>( );

				// Set entity variables that are specific to bombs
				newEntity.TurnsUntilAction = 3;

				break;
		}

		// Set entity variables that are the same for every entity
		newEntity.Tile = tile;
	}

	/// <summary>
	/// Update all of the tiles on the board that need to show a hazard based on the currently hovered entity
	/// </summary>
	public void UpdateShownHazardPositions ( ) {
		// Lists to store all of the new hazard positions that will now be shown and hidden
		List<Vector2Int> newShownHazardPositions = new List<Vector2Int>( );

		// Loop through all the entities on the board
		foreach (Entity entity in Entities) {
			// If the hovered entity is null, then we want all of the entity hazard tiles to be visible
			// If the hovered entity is not null, then we only want to show the current entity's hazard tiles
			if ((HoveredEntity == null && entity.TurnsUntilAction == 1) || HoveredEntity == entity) {
				newShownHazardPositions.AddRange(entity.HazardPositions);
			}
		}

		// The hazard positions that are going to be removed is equal to whatever is currently shown minus what will be shown next
		// The hazard positions that are going to be added is equal to whatever we want to add minus what has already been added
		List<Vector2Int> hazardPositionsToRemove = ShownHazardPositions.Except(newShownHazardPositions).ToList( );
		List<Vector2Int> hazardPositionsToAdd = newShownHazardPositions.Except(ShownHazardPositions).ToList( );

		// Need to add and remove hazard positions separately because some of them might not line up with tiles
		// For example, the laser's line of hazard positions stretches way further than the board reaches, so those hazard positions need to be added and removed when the laser entity moves
		ShownHazardPositions.RemoveAll(hazardPosition => hazardPositionsToRemove.Contains(hazardPosition));
		ShownHazardPositions.AddRange(hazardPositionsToAdd);

		// Loop through all of the tiles at the specified board position and change their overlay state to not show a hazard tile anymore
		foreach (Tile oldHazardTile in BoardManager.Instance.SearchForTilesAt(hazardPositionsToRemove)) {
			oldHazardTile.TileOverlayState = TileOverlayState.NONE;
		}

		// Loop through all of the tiles at the specified board position and change their overlay state to show a hazard tile
		foreach (Tile newHazardTile in BoardManager.Instance.SearchForTilesAt(hazardPositionsToAdd)) {
			newHazardTile.TileOverlayState = TileOverlayState.HAZARD;
		}
	}
}
