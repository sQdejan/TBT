using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour {

#region Singleton
	
	private static GameFlow instance;
	
	public static GameFlow Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<GameFlow>();
			}
			
			return instance;
		}
	}
	
#endregion

	public GameObject resourceDisplay;
	public int amountOfResourcesPerTurn = 10;
	public bool playerWillStart = true;

	[HideInInspector]
	public int resourcesLeft = 10;

	private string originalText;
	private Text resourceDisplayText;
	private bool playersCurrentTurn = true;

	void Awake() {
		resourceDisplayText = resourceDisplay.GetComponent<Text>();
		originalText = resourceDisplayText.text;
		resourceDisplayText.text += " " + resourcesLeft;

	}

	public void EndTurn() {
		Debug.Log("Turn has ended");

		playersCurrentTurn = !playersCurrentTurn;

		if(playersCurrentTurn) {
			resourcesLeft = amountOfResourcesPerTurn;
			UpdateResourceText();
			Debug.Log("Player's turn");
		} else {
			Debug.Log("AI's turn");
		}
	}

	public void UpdateResourceText() {
		resourceDisplayText.text = originalText + " " + resourcesLeft;
	}
}
