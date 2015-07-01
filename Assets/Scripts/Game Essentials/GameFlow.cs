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
	public GameObject endTurnButton;
	public int amountOfResourcesPerTurn = 10;
	public bool playerWillStart = true;

	[HideInInspector]
	public int resourcesLeft = 10;

	public static bool playersCurrentTurn = true;

	private Text resourceDisplayText;
	private string originalText;

	void Awake() {
		resourcesLeft = amountOfResourcesPerTurn;
		resourceDisplayText = resourceDisplay.GetComponent<Text>();
		originalText = resourceDisplayText.text;
		resourceDisplayText.text += " " + resourcesLeft;
	}

	public void EndTurn() {
		Debug.Log("Turn has ended");

		playersCurrentTurn = !playersCurrentTurn;

		if(playersCurrentTurn) {
			endTurnButton.GetComponent<Button>().interactable = true;
			resourcesLeft = amountOfResourcesPerTurn;
			UpdateResourceText();
			Debug.Log("Player's turn");
		} else {
			endTurnButton.GetComponent<Button>().interactable = false;
			StartCoroutine(Testing());
			Debug.Log("AI's turn");
		}
	}

	//Remember to delete
	IEnumerator Testing() {
		yield return new WaitForSeconds(5f);

		EndTurn();
	}

	public void UpdateResourceText() {
		resourceDisplayText.text = originalText + " " + resourcesLeft;
	}
}
