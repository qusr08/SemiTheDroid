using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities {
	/// <summary>
	/// Set the position of a transform without changing the z value
	/// </summary>
	/// <param name="transform">The transform to set</param>
	/// <param name="position">The position to set the transform to</param>
	public static void SetPositionWithoutZ (Transform transform, Vector3 position) {
		transform.position = new Vector3(position.x, position.y, transform.position.z);
	}

	/// <summary>
	/// Take the absolute value of each component of a Vector2Int
	/// </summary>
	/// <param name="vector">The vector to take the absolute value of</param>
	/// <returns>The inputted vector with all of the elements in the vector being positive</returns>
	public static Vector2Int Vector2IntAbs (Vector2Int vector) {
		return new Vector2Int(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}

	/// <summary>
	/// Round each component of a Vector2 to an integer
	/// </summary>
	/// <param name="vector">The vector to round</param>
	/// <returns>A Vector2Int where each component is the corresponding inputted component rounded to the nearest int</returns>
	public static Vector2Int Vector2RoundToInt (Vector2 vector) {
		return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
	}

	/// <summary>
	/// Rotate a Vector2Int around another Vector2Int by some increment of 90 degrees
	/// </summary>
	/// <param name="point">The point to rotate around the pivot point</param>
	/// <param name="pivot">The center of the rotation</param>
	/// <param name="rotationDirection">The number of times to rotate 90 degrees around the pivot point</param>
	/// <returns>The rotated Vector2Int point around the pivot vector by the specified number of 90 degrees</returns>
	public static Vector2Int Vector2IntRotateAround (Vector2Int point, Vector2Int pivot, int rotationDirection) {
		Quaternion rotation = Quaternion.Euler(0f, 0f, rotationDirection * 90f);
		Vector3 pivotVect3 = (Vector2) pivot;
		Vector3 pointVect3 = (Vector2) point;

		// https://forum.unity.com/threads/rotate-a-point-around-a-second-point-using-a-quaternion.504996/
		Vector3 result = (rotation * (pointVect3 - pivotVect3)) + pivotVect3;

		return Vector2RoundToInt(result);
	}
}
