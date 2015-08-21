using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//IMPORTANT remember that SetAILastMove has to be called in MoveHasBeenCalculated()
//IMPORTANT	right now it's using EnhMCTS, remember StartNextTurn() and MoveHasBeenCalculated() in order to change

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

	public const int MAX_TURNS = 100;

	public GameObject playerUnits;
	public GameObject AIUnits;
	public Text turnDisplayer;

	public GameObject hoverPlayerUnits;
	public GameObject hoverAIUnits;
	public Vector3 hoverActiveScale;

	[HideInInspector]
	public int turnsTaken = 0;

	public static bool playersCurrentTurn = false;
	public static bool restartGame = false;
	public static float gameOutcome = 0;

	//The two following variables are used in order to save what is on the curren tree
	//to use the statistics for further simulation in MCTS
	public static bool didPlayerHaveATurn = false;
	public static List<MCTSNode> playerLastMoveList = new List<MCTSNode>();

	public static bool didAIHaveATurn = false;
	public static List<MCTSNode> AILastMoveList = new List<MCTSNode>();

	List<GameObject> unitTurnOrderList = new List<GameObject>(); 
	int curTurnIndex = -1;

	List<GameObject> displayTurnOrderList = new List<GameObject>();

	RectTransform rtActive;
	Vector3 rtOriScale;

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

			displayTurnOrderList.Add(hoverAIUnits.transform.GetChild(i).gameObject);
			displayTurnOrderList.Add(hoverPlayerUnits.transform.GetChild(i).gameObject);
		}

		rtOriScale = displayTurnOrderList[0].GetComponent<RectTransform>().localScale;

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
			UpdateImageTurnDisplay();
//			UpdateTurnText();
		} else {
			playersCurrentTurn = false; //Just to reset shashizzle in playercontroller
		}

		turnsTaken++;
	}

	//return true if player's turn
	bool StartNextTurn() {

		if(++curTurnIndex >= unitTurnOrderList.Count)
			curTurnIndex = 0;

		GameObject tmpObject = unitTurnOrderList[curTurnIndex];

		if(tmpObject.tag == "PlayerUnit") {
			//--------------------------Below for player controller

//			PlayerController.Instance.currentUnit = tmpObject;
//			PlayerController.Instance.currentUnit.GetComponentInChildren<SpriteRenderer>().color = PlayerController.Instance.currentUnit.GetComponent<Unit>().activeSpriteColor;
//			PlayerController.Instance.currentUnit.GetComponent<Unit>().ShowPossibleMoves();
//
//			//Used in MCTS
//			didPlayerHaveATurn = true;

			//--------------------------Below for NNMCTS

			tmpObject.GetComponentInChildren<SpriteRenderer>().color = tmpObject.GetComponent<Unit>().activeSpriteColor;
			tmpObject.GetComponent<Unit>().ShowPossibleMoves();

			NNAIGameFlow.Instance.SetupGameState();
			didPlayerHaveATurn = true;

			return true;
		} else {
			//--------------------------For BT
			tmpObject.GetComponent<Unit>().ShowPossibleMoves();
			tmpObject.GetComponent<BehaviourTree>().TakeMove();

			didAIHaveATurn = true;

			//--------------------------Below for MCTS
//			tmpObject.GetComponentInChildren<SpriteRenderer>().color = tmpObject.GetComponent<Unit>().activeSpriteColor;
//			tmpObject.GetComponent<Unit>().ShowPossibleMoves();
//
////			AIGameFlow.Instance.SetupGameState();
//			EnhAIGameFlow.Instance.SetupGameState();
//			didAIHaveATurn = true;

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

	void UpdateImageTurnDisplay() {

		if(rtActive) {
			rtActive.localScale = rtOriScale;
			rtActive = null;
		}


		bool firstTime = true;

		int offset = Screen.width / 14;
		int yOffset = Screen.height / 14;
		int startpoint = Screen.width / 15;

		for(int i = curTurnIndex; i < displayTurnOrderList.Count; i++) {
			Vector3 toPos = new Vector3(startpoint, yOffset, 0);
			StartCoroutine(SlideImageTurnDisplay(displayTurnOrderList[i], toPos));

			//Just add a little extra for the first one after the active unit
			if(firstTime) {
				rtActive = displayTurnOrderList[i].GetComponent<RectTransform>();
				rtActive.localScale = hoverActiveScale;
//				startpoint += offset;
				firstTime = false;
			}
			startpoint += offset;
		}
		
		if(curTurnIndex > 0) {
			for(int i = 0; i < curTurnIndex; i++) {
				Vector3 toPos = new Vector3(startpoint, yOffset, 0);
				StartCoroutine(SlideImageTurnDisplay(displayTurnOrderList[i], toPos));

				//Just add a little extra for the first one after the active unit
				if(firstTime) {
					rtActive = displayTurnOrderList[i].GetComponent<RectTransform>();
					rtActive.localScale = hoverActiveScale;
//					startpoint += offset;
					firstTime = false;
				}
				startpoint += offset;
			}
		}
	}

	IEnumerator SlideImageTurnDisplay(GameObject slideObj, Vector3 toPos) {
		float travelTime = 1f;
		float time = 0;

		RectTransform tmpRT = slideObj.GetComponent<RectTransform>();

		while(time < travelTime) {
			tmpRT.position = Vector3.Lerp(tmpRT.position, toPos, time/travelTime);
			time += Time.fixedDeltaTime;
			
			yield return new WaitForFixedUpdate();
		}
	}

	public static int teamWhite = 0;
	public static int teamRed = 0;
	public static int draw = 0;

	public bool IsGameOver() {

		if(turnsTaken >= MAX_TURNS) {
			draw++;
			gameOutcome = 10;
			StartCoroutine(DelayedRestart());
			return true;
		}

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

		if(foundEnemyUnit) {
//			Debug.Log("AI won, noob!");
			teamRed++;
			gameOutcome = 0;
		} else {
//			Debug.Log("You won, nice!");
			teamWhite++;
			gameOutcome = 20;
		}

		StartCoroutine(DelayedRestart());

		return true;
	}

	IEnumerator DelayedRestart() {
		yield return new WaitForSeconds(1.1f);
		SoftRestartGame();
	}

	public void KillUnit(GameObject obj) {
		int tmpIndex = unitTurnOrderList.IndexOf(obj);
		if(tmpIndex < curTurnIndex) 
			curTurnIndex--;

		unitTurnOrderList.Remove(obj);

		//Turn of the turn displayer shower object
		displayTurnOrderList[tmpIndex].SetActive(false);
		displayTurnOrderList.RemoveAt(tmpIndex);
	}

	public void MoveHasBeenCalculated(bool playerMove) {
		if(!playerMove) {
			Unit curUnit = unitTurnOrderList[curTurnIndex].GetComponent<Unit>();
			
			if(EnhAIGameFlow.move.action == Action.MOVE) {
				curUnit.Move(GridController.Instance.gridArray[EnhAIGameFlow.move.gsH, EnhAIGameFlow.move.gsW]);
			} else if (EnhAIGameFlow.move.action == Action.ATTACK) {
				curUnit.Attack(GridController.Instance.gridArray[EnhAIGameFlow.move.mbagsH, EnhAIGameFlow.move.mbagsW], GridController.Instance.tileArray[EnhAIGameFlow.move.gsH, EnhAIGameFlow.move.gsW].occupier);
			}
			
			unitTurnOrderList[curTurnIndex].GetComponentInChildren<SpriteRenderer>().color = unitTurnOrderList[curTurnIndex].GetComponentInChildren<Unit>().oriSpriteColor;
			GridController.Instance.ResetGrid();
			
			EnhAIGameFlow.finished = false;
			playerLastMoveList.Clear();

			SetAILastMove(EnhAIGameFlow.move.action, EnhAIGameFlow.move.mbagsH, EnhAIGameFlow.move.mbagsW, EnhAIGameFlow.move.gsH, EnhAIGameFlow.move.gsW);

		} else {
			Unit curUnit = unitTurnOrderList[curTurnIndex].GetComponent<Unit>();

			if(NNAIGameFlow.move.action == Action.MOVE) {
				//			Debug.Log("I move");
				curUnit.Move(GridController.Instance.gridArray[NNAIGameFlow.move.gsH, NNAIGameFlow.move.gsW]);
			} else if (NNAIGameFlow.move.action == Action.ATTACK) {
				//			Debug.Log("I attack");
				curUnit.Attack(GridController.Instance.gridArray[NNAIGameFlow.move.mbagsH, NNAIGameFlow.move.mbagsW], GridController.Instance.tileArray[NNAIGameFlow.move.gsH, NNAIGameFlow.move.gsW].occupier);
			}
			
			unitTurnOrderList[curTurnIndex].GetComponentInChildren<SpriteRenderer>().color = unitTurnOrderList[curTurnIndex].GetComponentInChildren<Unit>().oriSpriteColor;
			GridController.Instance.ResetGrid();
			
			NNAIGameFlow.finished = false;
			AILastMoveList.Clear();

			SetPlayerLastMove(NNAIGameFlow.move.action, NNAIGameFlow.move.mbagsH, NNAIGameFlow.move.mbagsW, NNAIGameFlow.move.gsH, NNAIGameFlow.move.gsW);
		}
//		if(!playerMove) {
//			Unit curUnit = unitTurnOrderList[curTurnIndex].GetComponent<Unit>();
//
//			if(AIGameFlow.move.action == Action.MOVE) {
//				curUnit.Move(GridController.Instance.gridArray[AIGameFlow.move.gsH, AIGameFlow.move.gsW]);
//			} else if (AIGameFlow.move.action == Action.ATTACK) {
//				curUnit.Attack(GridController.Instance.gridArray[AIGameFlow.move.mbagsH, AIGameFlow.move.mbagsW], GridController.Instance.tileArray[AIGameFlow.move.gsH, AIGameFlow.move.gsW].occupier);
//			}
//
//			unitTurnOrderList[curTurnIndex].GetComponentInChildren<SpriteRenderer>().color = unitTurnOrderList[curTurnIndex].GetComponentInChildren<Unit>().oriSpriteColor;
//			GridController.Instance.ResetGrid();
//
//			AIGameFlow.finished = false;
//			playerLastMoveList.Clear();
//
////			SetAILastMove(AIGameFlow.move.action, AIGameFlow.move.mbagsH, AIGameFlow.move.mbagsW, AIGameFlow.move.gsH, AIGameFlow.move.gsW);
//		} else {
//			Unit curUnit = unitTurnOrderList[curTurnIndex].GetComponent<Unit>();
//
//			if(NNAIGameFlow.move.action == Action.MOVE) {
//				//			Debug.Log("I move");
//				curUnit.Move(GridController.Instance.gridArray[NNAIGameFlow.move.gsH, NNAIGameFlow.move.gsW]);
//			} else if (NNAIGameFlow.move.action == Action.ATTACK) {
//				//			Debug.Log("I attack");
//				curUnit.Attack(GridController.Instance.gridArray[NNAIGameFlow.move.mbagsH, NNAIGameFlow.move.mbagsW], GridController.Instance.tileArray[NNAIGameFlow.move.gsH, NNAIGameFlow.move.gsW].occupier);
//			}
//			
//			unitTurnOrderList[curTurnIndex].GetComponentInChildren<SpriteRenderer>().color = unitTurnOrderList[curTurnIndex].GetComponentInChildren<Unit>().oriSpriteColor;
//			GridController.Instance.ResetGrid();
//			
//			NNAIGameFlow.finished = false;
//			AILastMoveList.Clear();
//
//			SetPlayerLastMove(NNAIGameFlow.move.action, NNAIGameFlow.move.mbagsH, NNAIGameFlow.move.mbagsW, NNAIGameFlow.move.gsH, NNAIGameFlow.move.gsW);
//		}

		EndTurn();
	}

	public void SetPlayerLastMove(Action a, int mh, int mw, int h, int w) {
		playerLastMoveList.Add(new MCTSNode(null, a, mh, mw, h, w));
	}

	public void SetAILastMove(Action a, int mh, int mw, int h, int w) {
		AILastMoveList.Add(new MCTSNode(null, a, mh, mw, h, w));
	}

	static int gamesplayed = 0;

	public void HardRestartGame() {
		Debug.Log("GAMES PLAYED = " + (++gamesplayed));
		Application.LoadLevel(Application.loadedLevel);
	}

	void Update() {
		if(restartGame) {
			StartCoroutine(DelayedRestart());
			restartGame = false;
		}
	}

	//Restarting the game without reloading the level
	//necessary for NEAT.
	public void SoftRestartGame() {

		Debug.Log("After game " + ++gamesplayed + " the score is white(enh++, 7000): " + teamWhite + " - red(BT): " + teamRed + " - draw: " + draw + " - turns taken: " + turnsTaken);

		turnsTaken = 0;

		GridController.Instance.HardResetGrid();
		MCTS.Instance.ResetCurrentNode();
		NNMCTS.Instance.ResetCurrentNode();
		EnhMCTS.Instance.ResetCurrentNode();

		playersCurrentTurn = false;
		didPlayerHaveATurn = false;
		didAIHaveATurn = false;
		AILastMoveList = new List<MCTSNode>();
		playerLastMoveList = new List<MCTSNode>();
		unitTurnOrderList = new List<GameObject>(); 
		displayTurnOrderList = new List<GameObject>();
		curTurnIndex = -1;

		for(int i = 0; i < playerUnits.transform.childCount; i++) {
			AIUnits.transform.GetChild(i).GetComponent<Unit>().ResetUnit();
			playerUnits.transform.GetChild(i).GetComponent<Unit>().ResetUnit();

			unitTurnOrderList.Add(AIUnits.transform.GetChild(i).gameObject);
			unitTurnOrderList.Add(playerUnits.transform.GetChild(i).gameObject);

			hoverPlayerUnits.transform.GetChild(i).gameObject.SetActive(true);
			hoverAIUnits.transform.GetChild(i).gameObject.SetActive(true);

			displayTurnOrderList.Add(hoverAIUnits.transform.GetChild(i).gameObject);
			displayTurnOrderList.Add(hoverPlayerUnits.transform.GetChild(i).gameObject);
		}

		StartCoroutine(DelayStart());
	}

}
