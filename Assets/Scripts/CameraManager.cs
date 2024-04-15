using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {
	[Header("References")]
	[SerializeField] private Camera _gameCamera;
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float minCameraZoom;
	[SerializeField, Min(0.01f)] private float maxCameraZoom;
	[SerializeField, Min(0.01f)] private float cameraZoomStep;
	[SerializeField, Min(0)] private float extensionPerZoomLevel;
	[Header("Information")]
	[SerializeField] private Vector3 panOrigin;

	/// <summary>
	/// The main camera in the game scene
	/// </summary>
	public Camera GameCamera { get => _gameCamera; private set => _gameCamera = value; }

	private void Update ( ) {
		// Variables to store the current camera values
		Vector3 cameraPosition = transform.position;

		// When the middle mouse button is pressed, reset the last mouse position
		if (Input.GetMouseButtonDown(2)) {
			panOrigin = GameCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		// If the player is pressing the middle mouse button, pan the camera around based on the mouse movement
		if (Input.GetMouseButton(2)) {
			// Calculate the movement of the mouse since the last frame
			cameraPosition += panOrigin - GameCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		// Zoom the camera in and out based on the scroll wheel value
		GameCamera.orthographicSize = Mathf.Clamp(GameCamera.orthographicSize - (Input.mouseScrollDelta.y * cameraZoomStep), minCameraZoom, maxCameraZoom);

		// Get the current camera padding value that scales based on the current zoom level of the camera
		float cameraExtension = (maxCameraZoom - GameCamera.orthographicSize) * extensionPerZoomLevel;

		// Calculate the bounds of the screen
		Vector2 screenBounds = new Vector2(
			(GameCamera.orthographicSize * GameCamera.aspect) + cameraExtension,
			GameCamera.orthographicSize + cameraExtension
		);

		// Clamp how far the camera position can get from the center of the board
		Vector3 boardCenterDifference = (Vector3) BoardManager.Instance.CenterPosition - cameraPosition;
		boardCenterDifference.x = Mathf.Clamp(boardCenterDifference.x, -screenBounds.x, screenBounds.x);
		boardCenterDifference.y = Mathf.Clamp(boardCenterDifference.y, -screenBounds.y, screenBounds.y);
		cameraPosition = (Vector3) BoardManager.Instance.CenterPosition - boardCenterDifference;

		// Set the position of the camera
		Utilities.SetPositionWithoutZ(transform, cameraPosition);
	}
}
