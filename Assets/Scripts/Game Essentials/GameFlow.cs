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

	public static bool playersCurrentTurn = false;

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
			unitTurnOrderList.Add(AIUnits.transform.GetChild(i).gameObject);
			unitTurnOrderList.Add(playerUnits.transform.GetChild(i).gameObject);
		}

		StartCoroutine(DelayStart());
	}

	//I need to delay it if AI has to start because
	//of physics calc. needs to finish on Unit.cs
	IEnumerator DelayStart() {
		yield return new WaitForSeconds(0.1f);
		EndTurn();
	}

	public void EndTurn() {
		if(!IsGameOver()) {
			playersCurrentTurn = StartNextTurn();
			UpdateTurnText();
		} else {
			playersCurrentTurn = false; //Just to reset shashizzle in playercontroller
		}
	}

	//return true if player's turn
	bool StartNextTurn() {

		if(++curTurnIndex >= unitTurnOrderList.Count)
			curTurnIndex = 0;

		GameObject tmpObject = unitTurnOrderList[curTurnIndex];

		if(tmpObject.tag == "PlayerUnit") {
			PlayerController.Instance.currentUnit = tmpObject;
			//Show what unit is the current active
			PlayerController.Instance.currentUnit.GetComponentInChildren<SpriteRenderer>().color = PlayerController.Instance.currentUnit.GetComponent<Unit>().changeSpriteColor;
			//Show posssible moves
			PlayerController.Instance.currentUnit.GetComponent<Unit>().ShowPossibleMoves();

			return true;
		} else {
			//AI shashizzle in here
			Debug.Log("Starting the process");

			tmpObject.GetComponentInChildren<SpriteRenderer>().color = tmpObject.GetComponent<Unit>().changeSpriteColor;
			tmpObject.GetComponent<Unit>().ShowPossibleMoves();

			AIGameFlow.Instance.SetupGameState();
			return false;
		}	
	}

	void UpdateTurnText() {

		string tmpString = "";
		for(int i = curTurnIndex; i < unitTurnOrderList.Count; i++) {
			if(curTurnIndex == 0 && i == unitTurnOrderList.Count - 1)
				tmpString += unitTurnOrderList[i].name;
			else 
				tmpString += unitTurnOrderList[i].name + "    ";
		}

		if(curTurnIndex > 0) {
			for(int i = 0; i < curTurnIndex; i++) {
				if(i != curTurnIndex - 1) 
					tmpString += unitTurnOrderList[i].name + "    ";
				else 
					tmpString += unitTurnOrderList[i].name;
			}
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

	public void MoveHasBeenCalculated() {

		Debug.Log("Applying move");

		Unit curUnit = unitTurnOrderList[curTurnIndex].GetComponent<Unit>();

		if(AIGameFlow.move.action == Action.MOVE) {
			Debug.Log("I move");
			curUnit.Move(GridController.Instance.gridArray[AIGameFlow.move.gsH, AIGameFlow.move.gsW]);
		} else if (AIGameFlow.move.action == Action.ATTACK) {
			Debug.Log("I attack");
			if(AIGameFlow.move.mbagsH == -1)
				curUnit.Attack(null, GridController.Instance.tileArray[AIGameFlow.move.gsH, AIGameFlow.move.gsW].occupier);
			else 
				curUnit.Attack(GridController.Instance.gridArray[AIGameFlow.move.mbagsH, AIGameFlow.move.mbagsW], GridController.Instance.tileArray[AIGameFlow.move.gsH, AIGameFlow.move.gsW].occupier);
		}

		unitTurnOrderList[curTurnIndex].GetComponentInChildren<SpriteRenderer>().color = unitTurnOrderList[curTurnIndex].GetComponentInChildren<Unit>().oriSpriteColor;
		GridController.Instance.ClearGrid();

		AIGameFlow.finished = false;
		EndTurn();
	}

	public void RestartGame() {
		AIGameFlow.Instance.CancelBackgroundWorker();
//		Application.LoadLevel(Application.loadedLevel);
	}

}
