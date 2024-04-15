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
}
