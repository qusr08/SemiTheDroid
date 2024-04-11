using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[Header("References")]
	[SerializeField] private Camera gameCamera;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float minCameraZoom;
	[SerializeField, Min(0.01f)] private float maxCameraZoom;
	[SerializeField, Min(0.01f)] private float cameraZoomStep;
	[SerializeField, Min(0)] private float cameraBorder;
	[Space]
	[SerializeField] private Vector3 panOrigin;

	private void Update ( ) {
		// When the middle mouse button is pressed, reset the last mouse position
		if (Input.GetMouseButtonDown(2)) {
			panOrigin = gameCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		// If the player is pressing the middle mouse button, pan the camera around based on the mouse movement
		if (Input.GetMouseButton(2)) {
			// Calculate the movement of the mouse since the last frame
			Vector3 panMotion = panOrigin - gameCamera.ScreenToWorldPoint(Input.mousePosition);
			Vector3 newCameraPosition = transform.position + panMotion;

			// Calculate the bounds of the screen
			Vector2 screenBounds = new Vector2(
				(gameCamera.orthographicSize * gameCamera.aspect) - cameraBorder,
				gameCamera.orthographicSize - cameraBorder
			);

			// Clamp how far the camera position can get from the center of the board
			Vector3 boardCenterDifference = (Vector3) BoardManager.Instance.CenterPosition - newCameraPosition;
			boardCenterDifference.x = Mathf.Clamp(boardCenterDifference.x, -screenBounds.x, screenBounds.x);
			boardCenterDifference.y = Mathf.Clamp(boardCenterDifference.y, -screenBounds.y, screenBounds.y);
			newCameraPosition = (Vector3) BoardManager.Instance.CenterPosition - boardCenterDifference;

			// Set the position of the camera
			SetTransformPositionWithoutZ(transform, newCameraPosition);
		}

		// Zoom the camera in and out based on the scroll wheel value
		gameCamera.orthographicSize -= Input.mouseScrollDelta.y * cameraZoomStep;
		gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize, minCameraZoom, maxCameraZoom);
	}

	/// <summary>
	/// Set the position of a transform without changing the z value
	/// </summary>
	/// <param name="transform">The transform to set</param>
	/// <param name="position">The position to set the transform to</param>
	public static void SetTransformPositionWithoutZ (Transform transform, Vector3 position) {
		transform.position = new Vector3(position.x, position.y, transform.position.z);
	}
}
