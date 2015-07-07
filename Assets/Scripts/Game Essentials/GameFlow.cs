using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

	public GameObject playerUnits;
	public GameObject AIUnits;
	public Text turnDisplayer;

	public static bool playersCurrentTurn = true;

	private List<GameObject> unitTurnOrderList = new List<GameObject>(); 
	private int curTurnIndex = -1;

	#region properties

	public List<GameObject> UnitTurnOrderList {
		get {
			return unitTurnOrderList;
		}
	}

	public int CurTurnIndex {
		get {
			return curTurnIndex;
		}
	}

	#endregion

	void Start() {
		instance = this;

		for(int i = 0; i < playerUnits.transform.childCount; i++) {
			unitTurnOrderList.Add(playerUnits.transform.GetChild(i).gameObject);
			unitTurnOrderList.Add(AIUnits.transform.GetChild(i).gameObject);
		}

		EndTurn();
	}

	public void EndTurn() {
		if(!IsGameOver()) {
			playersCurrentTurn = StartNextTurn();
			UpdateTurnText();
		} else 
			playersCurrentTurn = false; //Just to reset shashizzle in playercontroller
	}

	//return true if player's turn
	bool StartNextTurn() {

		if(++curTurnIndex >= unitTurnOrderList.Count)
			curTurnIndex = 0;

		GameObject tmpObject = unitTurnOrderList[curTurnIndex];

		if(tmpObject.tag == "PlayerUnit") {
			PlayerController.Instance.currentUnit = tmpObject;
			//Show what unit is the current active
			PlayerController.Instance.currentUnit.GetComponent<SpriteRenderer>().sprite = PlayerController.Instance.currentUnit.GetComponent<Unit>().hoverSprite;
			//Show posssible moves
			PlayerController.Instance.ShowPossibleTiles(PlayerController.Instance.currentUnit.GetComponent<Unit>().possibleMoves);

			return true;
		} else {
			//AI shashizzle in here
			StartCoroutine(Testing());

			return false;
		}	
	}

	void UpdateTurnText() {
		string tmpString = "";
		for(int i = curTurnIndex; i < unitTurnOrderList.Count; i++) {
			tmpString += unitTurnOrderList[i].name + " ";
		}
		turnDisplayer.text = tmpString;
	}

	bool IsGameOver() {
		bool foundEnemyUnit = false;
		bool foundPlayerUnit = false;

		foreach(GameObject obj in unitTurnOrderList) {
			if(obj.tag == "EnemyUnit")
				foundEnemyUnit = true;
			else if (obj.tag == "PlayerUnit")
				foundPlayerUnit = true;
		}

		if(foundEnemyUnit && foundPlayerUnit)
			return false; 

		if(foundEnemyUnit)
			Debug.Log("AI won, noob!");
		else
			Debug.Log("You won, nice!");

		return true;
	}

	public void KillUnit(GameObject obj) {
		int tmpIndex = unitTurnOrderList.IndexOf(obj);
		if(tmpIndex < curTurnIndex) 
			curTurnIndex--;

		unitTurnOrderList.Remove(obj);
	}

	IEnumerator Testing() {
		yield return new WaitForSeconds(1);

		EndTurn();
	}

}
