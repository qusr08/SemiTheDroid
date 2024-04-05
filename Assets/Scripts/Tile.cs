using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {
	[Header("References")]
	[SerializeField] private SpriteRenderer spriteRenderer;
	[Header("Properties")]
	[SerializeField] private Vector2Int position;

	private void OnValidate ( ) {
		UpdateIsometricPosition( );
	}

	private void Awake ( ) {
		OnValidate( );
	}

	private void Update ( ) {
		UpdateIsometricPosition( );
	}

	private void UpdateIsometricPosition ( ) {
		// Make sure tiles always align to the isometric grid
		transform.position = new Vector2((position.x + position.y) * 0.5f, (position.y - position.x) * 0.25f);

		// Make sure tiles that have a lower y position appear in front of others
		spriteRenderer.sortingOrder = position.x - position.y;
	}
}
