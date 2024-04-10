using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	[SerializeField, Min(1)] private float minCameraZoom;
	[SerializeField, Min(1)] private float maxCameraZoom;
	[SerializeField, Min(0)] private float cameraBorder;
}
