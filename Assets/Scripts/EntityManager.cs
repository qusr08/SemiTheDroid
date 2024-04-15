using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : Singleton<EntityManager> {
	[Header("References")]
	[SerializeField] private GameObject spikePrefab;
	[SerializeField] private GameObject robotPrefab;
	[SerializeField] private GameObject laserPrefab;
	[SerializeField] private GameObject bombPrefab;

	/// <summary>
	/// Spawn an entity onto a tile
	/// </summary>
	/// <param name="entityType">The type of entity to spawn</param>
	/// <param name="tile">The tile to spawn the entity on</param>
	public void SpawnEntity (EntityType entityType, Tile tile) {
		switch (entityType) {
			case EntityType.SPIKE:
				tile.Entity = Instantiate(spikePrefab, Vector3.zero, Quaternion.identity).GetComponent<Spike>( );
				break;
			case EntityType.ROBOT:
				tile.Entity = Instantiate(robotPrefab, Vector3.zero, Quaternion.identity).GetComponent<Spike>( );
				break;
			case EntityType.LASER:
				tile.Entity = Instantiate(laserPrefab, Vector3.zero, Quaternion.identity).GetComponent<Spike>( );
				break;
			case EntityType.BOMB:
				tile.Entity = Instantiate(bombPrefab, Vector3.zero, Quaternion.identity).GetComponent<Spike>( );
				break;
		}
	}
}
