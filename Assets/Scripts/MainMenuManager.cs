using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuManager : Singleton<MainMenuManager> {
	[Header("References")]
	[SerializeField] private GameObject creditsMenu;
	[SerializeField] private GameObject howToPlayMenu;

	/// <summary>
	/// Function that is called to start the game
	/// </summary>
	public void PlayGame ( ) {
		StartCoroutine(GameManager.Instance.SetGameState(GameState.GENERATE));
	}
}
