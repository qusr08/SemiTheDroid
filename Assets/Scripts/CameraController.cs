using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - Make it so that when the mouse is scrolled, it also updates the position of the camera

public class CameraController : MonoBehaviour {
	[Header("References")]
	[SerializeField] private Camera gameCamera;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float minCameraZoom;
	[SerializeField, Min(0.01f)] private float maxCameraZoom;
	[SerializeField, Min(0.01f)] private float cameraZoomStep;
	[SerializeField, Min(0)] private float extensionPerZoomLevel;
	[Space]
	[SerializeField] private Vector3 panOrigin;

	private void Update ( ) {
		// Variables to store the current camera values
		Vector3 cameraPosition = transform.position;
		float cameraOrthographicSize = gameCamera.orthographicSize;

		// When the middle mouse button is pressed, reset the last mouse position
		if (Input.GetMouseButtonDown(2)) {
			panOrigin = gameCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		// Zoom the camera in and out based on the scroll wheel value
		gameCamera.orthographicSize -= Input.mouseScrollDelta.y * cameraZoomStep;
		gameCamera.orthographicSize = Mathf.Clamp(gameCamera.orthographicSize, minCameraZoom, maxCameraZoom);

		// If the player is pressing the middle mouse button, pan the camera around based on the mouse movement
		if (Input.GetMouseButton(2)) {
			// Calculate the movement of the mouse since the last frame
			cameraPosition += panOrigin - gameCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		// If something about the camera has changed, then update the position
		if (cameraPosition != transform.position || cameraOrthographicSize != gameCamera.orthographicSize) {
			// Get the current camera padding value that scales based on the current zoom level of the camera
			float cameraExtension = (maxCameraZoom - gameCamera.orthographicSize) * extensionPerZoomLevel;

			// Calculate the bounds of the screen
			Vector2 screenBounds = new Vector2(
				(gameCamera.orthographicSize * gameCamera.aspect) + cameraExtension,
				gameCamera.orthographicSize + cameraExtension
			);

			// Clamp how far the camera position can get from the center of the board
			Vector3 boardCenterDifference = (Vector3) BoardManager.Instance.CenterPosition - cameraPosition;
			boardCenterDifference.x = Mathf.Clamp(boardCenterDifference.x, -screenBounds.x, screenBounds.x);
			boardCenterDifference.y = Mathf.Clamp(boardCenterDifference.y, -screenBounds.y, screenBounds.y);
			cameraPosition = (Vector3) BoardManager.Instance.CenterPosition - boardCenterDifference;

			// Set the position of the camera
			SetTransformPositionWithoutZ(transform, cameraPosition);
		}
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
