using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {
	[Header("References")]
	[SerializeField] private GameObject spikePrefab;
	[SerializeField] private GameObject robotPrefab;
	[SerializeField] private GameObject laserPrefab;
	[SerializeField] private GameObject bombPrefab;
	[Space]
	[SerializeField] private GameObject entityInfoBox;
	[SerializeField] private TextMeshProUGUI entityNameText;
	[SerializeField] private TextMeshProUGUI entityDescriptionText;
	[SerializeField] private TextMeshProUGUI entityTurnText;
	[SerializeField] private TextMeshProUGUI entityTurnOrderText;
	[Header("Properties")]
	[SerializeField, Min(1)] private int minStartingTurnCount;
	[SerializeField, Min(1)] private int maxStartingTurnCount;
	[Header("Information")]
	[SerializeField] private Entity _hoveredEntity;
	[SerializeField] private List<Vector2Int> _shownHazardPositions;
	[SerializeField] private List<Entity> _entities;
	[SerializeField] private List<Entity> _entityTurnQueue;

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

			// If the hovered entity is not equal to null, then set the text fields for entity info
			entityInfoBox.SetActive(_hoveredEntity != null);
			if (_hoveredEntity != null) {
				entityNameText.text = $"~ {_hoveredEntity.EntityName} ~";
				entityDescriptionText.text = _hoveredEntity.EntityDescription;

				if (_hoveredEntity.EntityType != EntityType.SPIKE) {
					string s = _hoveredEntity.TurnsUntilAction == 1 ? "" : "s";
					entityTurnText.text = $"Will act in {_hoveredEntity.TurnsUntilAction} turn{s}";
					entityTurnOrderText.text = $"Order in turn: {_hoveredEntity.TurnOrder}";
				} else {
					entityTurnText.text = "";
					entityTurnOrderText.text = "";
				}
			}

			UpdateShownHazardPositions( );
		}
	}

	/// <summary>
	/// A list of all the entities on the board
	/// </summary>
	public List<Entity> Entities { get => _entities; private set => _entities = value; }

	/// <summary>
	/// A list used as queue that holds the order in which entities will execute their turns
	/// </summary>
	public List<Entity> EntityTurnQueue { get => _entityTurnQueue; private set => _entityTurnQueue = value; }

	/// <summary>
	/// A list of all the board positions that are currently being shown as a hazard
	/// </summary>
	public List<Vector2Int> ShownHazardPositions { get => _shownHazardPositions; private set => _shownHazardPositions = value; }

	protected override void Awake ( ) {
		base.Awake( );

		Entities = new List<Entity>( );
		ShownHazardPositions = new List<Vector2Int>( );
		EntityTurnQueue = new List<Entity>( );
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
				newEntity.TurnsUntilAction = -1;

				break;
			case EntityType.ROBOT:
				newEntity = Instantiate(robotPrefab, Vector3.zero, Quaternion.identity).GetComponent<Robot>( );
				newEntity.Direction = BoardManager.Instance.GetCardinalPositions(Vector2Int.zero)[Random.Range(0, 4)];
				newEntity.TurnsUntilAction = 1;

				break;
			case EntityType.LASER:
				newEntity = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<Laser>( );
				newEntity.Direction = new List<Vector2Int>( ) { Vector2Int.up, Vector2Int.left }[Random.Range(0, 2)];
				newEntity.TurnsUntilAction = GetRandomTurnCount( );

				break;
			case EntityType.BOMB:
				newEntity = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity).GetComponent<Bomb>( );
				newEntity.TurnsUntilAction = GetRandomTurnCount( );

				break;
		}

		// Set entity variables that are the same for every entity
		newEntity.Tile = tile;
		newEntity.TurnOrder = 1;

		// If there are no entities spawned yet, just add the new entity to the list, there is no need to sort it
		if (Entities.Count == 0) {
			Entities.Add(newEntity);
			return;
		}

		// Loop through all of the entities on the board to sort the new entity by its turn count and turn order
		// Lower turn counts are closer towards the start of the array and within that turn the lowest turn order is closer to the start as well
		// So, the entity closest to the start of the array is the next entity to perform their action
		for (int i = 0; i <= Entities.Count; i++) {
			// If the current entity's turn count is higher than the new entities turn count, then insert the new entity at the current index
			if (i == Entities.Count) {
				// If the end of the entity array has been reached, just add the new entity to the end
				Entities.Add(newEntity);
				break;
			} else if (Entities[i].TurnsUntilAction > newEntity.TurnsUntilAction) {
				// If the entity needs to be added to the middle of the array, insert it
				Entities.Insert(i, newEntity);
				break;
			} else if (Entities[i].TurnsUntilAction == newEntity.TurnsUntilAction) {
				// If the current entity has the same turn count as the new entity, then increment the turn order of the new entity
				// This will make the new entity go after all previously spawned entities with the same remaining turn count
				newEntity.TurnOrder++;
			}
		}
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

	/// <summary>
	/// Perform all entity turns based on their turn order and turn count
	/// </summary>
	public void UpdateEntityTurns ( ) {
		// Because some variables are updating as a result of the entity turn, just disable the info box
		entityInfoBox.SetActive(false);

		// Copy the order of the entities over to the turn list
		// The entities should already be order from next to move to last to move
		EntityTurnQueue = new List<Entity>(Entities);

		// While there are entities that need to take their turns, continue looping
		while (EntityTurnQueue.Count > 0) {
			// Get the next entity in the queue and remove it from the queue
			Entity nextEntity = EntityTurnQueue[0];
			EntityTurnQueue.RemoveAt(0);

			// Perform that entities action
			nextEntity.PerformTurn( );
		}

		// Since the entities were updated, update the shown hazard tiles
		UpdateShownHazardPositions( );

		// Once the entity turn is finished, switch the turn back to the player
		// If the robot was destroyed, then the game state should not be the entities turn anymore and should be in the game over state
		// Only switch the state if that did not happen
		if (GameManager.Instance.GameState == GameState.ENTITY_TURN) {
			GameManager.Instance.SetGameState(GameState.PLAYER_TURN);
		}
	}

	/// <summary>
	/// Get a random turn count for an entity that is scaled to the board difficulty
	/// </summary>
	/// <returns>A random integer that is the turn count</returns>
	public int GetRandomTurnCount ( ) {
		// Get a range value to scale the difficulty of the board based on the survived turn count
		float rangeValue = maxStartingTurnCount * Mathf.Exp(-GameManager.Instance.SurvivedTurnCount * 0.04f);

		// Get a random number between the range of +-1 of the range value
		// Round that number up to the nearest int
		// Clamp the value between the min and max turn count
		return Mathf.Clamp(Mathf.CeilToInt(Random.Range(rangeValue - 1f, rangeValue + 1f)), minStartingTurnCount, maxStartingTurnCount);
	}
}
