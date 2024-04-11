using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> {
	[Header("Properties")]
	[SerializeField, Min(0.01f)] private float animationSpeed;
	[Space]
	[SerializeField] private float animationTimer;
	[SerializeField] private int _currentAnimationFrame;


	public delegate void OnAnimationFrameEvent ( );
	public event OnAnimationFrameEvent OnAnimationFrame;

	/// <summary>
	/// The current animation frame for all board elements
	/// </summary>
	public int CurrentAnimationFrame { get => _currentAnimationFrame; private set => _currentAnimationFrame = value; }
	
	protected override void Awake ( ) {
		base.Awake( );

		animationTimer = 0;
		CurrentAnimationFrame = 0;
	}

	private void Update ( ) {
		// Increment the animation timer by the time that has passed since the last update call
		// Once the timer has reached the animation speed, update the sprites
		animationTimer += Time.deltaTime;
		if (animationTimer >= animationSpeed) {
			// Subtract the animation time from the animation timer
			// This makes it slightly more exact in when the animation changes sprites
			animationTimer -= animationSpeed;

			// Increment the current animation frame
			// All animations should use this so they are all synced
			// All animations will be 4 looped frames
			CurrentAnimationFrame = (CurrentAnimationFrame + 1) % 4;

			// Update all of the tiles if they need to be animated
			OnAnimationFrame( );
		}
	}
}
