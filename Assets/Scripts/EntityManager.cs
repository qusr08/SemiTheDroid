using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {
	[Header("References")]
	[SerializeField] private GameObject spikePrefab;
	[SerializeField] private GameObject robotPrefab;
	[SerializeField] private GameObject laserPrefab;
	[SerializeField] private GameObject bombPrefab;

	private List<Entity> entities;

	protected override void Awake ( ) {
		base.Awake( );

		entities = new List<Entity>( );
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
		tile.Entity = newEntity;
		entities.Add(newEntity);

		// Face the entity in a random direction when starting
		newEntity.FacingDirection = BoardManager.Instance.GetCardinalPositions(Vector2Int.zero)[Random.Range(0, 4)];
	}
}
