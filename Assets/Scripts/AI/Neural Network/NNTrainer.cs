using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public class NNTrainer : UnitController {

	#region Singleton
	
	private static NNTrainer instance;
	
	public static NNTrainer Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<NNTrainer>();
			}
			
			return instance;
		}
	}
	
	#endregion

	IBlackBox box;

	int[,] gameState;

	bool boxActive = false;

	void Start() {
		gameState = new int[GridController.Instance.gridHeight, GridController.Instance.gridWidth];
	}

	public float ValueOfState(/*MCTSNode action*/) {

//		if(!boxActive) {
//			Debug.LogError("Box is not active - something is wrong");
//			return 0;
//		}

//		SetupGameState();
//
//		Unit curUnit = PlayerController.Instance.currentUnit.GetComponent<Unit>();
//		Tile curUnitTile = curUnit.curTile.GetComponent<Tile>();
//
//		if(action.action == Action.MOVE) {
//			if(curUnitTile.HeightIndex != action.gsH || curUnitTile.WidthIndex != action.gsW) {
//				gameState[action.gsH, action.gsW] = gameState[curUnitTile.HeightIndex, curUnitTile.WidthIndex];
//				gameState[curUnitTile.HeightIndex, curUnitTile.WidthIndex] = 0;
//			}
//
//		} else {
//			if(curUnit.classType == ClassType.RANGED) {
//				gameState[action.gsH, action.gsW] += 1;
//			} else {
//				if(curUnitTile.HeightIndex != action.mbagsH || curUnitTile.WidthIndex != action.mbagsW) {
//					gameState[action.mbagsH, action.mbagsW] = gameState[curUnitTile.HeightIndex, curUnitTile.WidthIndex];
//					gameState[curUnitTile.HeightIndex, curUnitTile.WidthIndex] = 0;
//				}
//
//				gameState[action.gsH, action.gsW] += 1;
//			}
//		}

		ISignalArray inputArr = box.InputSignalArray;
	
		int index = 0;

		for(int i = 0; i < NNMCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < NNMCTS.Instance.gameState.GetLength(1); j++) {
				if(NNMCTS.Instance.gameState[i,j].state == NNAIGameFlow.GS_EMPTY)
					inputArr[index++] = 0;
				else if (NNMCTS.Instance.gameState[i,j].state == NNAIGameFlow.GS_PLAYER)
					inputArr[index++] = NNMCTS.Instance.gameState[i,j].occupier.health;
				else if (NNMCTS.Instance.gameState[i,j].state == NNAIGameFlow.GS_AI)
					inputArr[index++] = -NNMCTS.Instance.gameState[i,j].occupier.health;
				else
					Debug.LogError("ERROR!!!!!!");
			}
		}

		box.Activate();

		ISignalArray outputArr = box.OutputSignalArray;

		return (float)outputArr[0];
	}

//	IEnumerator DelayedSetup() {
//		yield return new WaitForSeconds(0.3f);
//		ValueOfState(new MCTSNode(null, Action.MOVE, -1, -1, 0, 2));
//	}

	void SetupGameState() {
		for(int i = 0; i < gameState.GetLength(0); i++) {
			for(int j = 0; j < gameState.GetLength(1); j++) {
				if(GridController.Instance.tileArray[i,j].occupier == null) {
					gameState[i,j] = 0;
				} else {
					if(GridController.Instance.tileArray[i,j].occupier.tag == "PlayerUnit") {
						gameState[i,j] = GridController.Instance.tileArray[i,j].occupier.GetComponent<Unit>().health;
					} else {
						gameState[i,j] = -GridController.Instance.tileArray[i,j].occupier.GetComponent<Unit>().health;
					}
				}
			}
		}

//		PrintGameState(gameState);
	}

	#region Overriden methods

	public override void Activate (IBlackBox box) {
		this.box = box;
		this.boxActive = true;
	}

	public override float GetFitness () {

		this.boxActive = false;

		//return: 0 for loss
		//		  20 for win
		if(GameFlow.Instance.IsGameOver()) {
			return GameFlow.gameOutcome;
		} 

		float playerUnits = 0;
		float playerHealth = 0;

		float AIUnits = 0;
		float AIHealth = 0;

		for(int i = 0; i < GridController.Instance.tileArray.GetLength(0); i++) {
			for(int j = 0; j < GridController.Instance.tileArray.GetLength(1); j++) {
				if(GridController.Instance.tileArray[i,j].occupied) {
					if(GridController.Instance.tileArray[i,j].occupier.tag == "PlayerUnit") {
						playerUnits += 1;
						playerHealth += (float)GridController.Instance.tileArray[i,j].occupier.GetComponent<Unit>().health;
					} else {
						AIUnits += 1;
						AIHealth += (float)GridController.Instance.tileArray[i,j].occupier.GetComponent<Unit>().health;
					}
				}
			}
		}

		//The more health the PLAYER has over the AI the better
		//and also true for amount of units. Max value = 6 + 12 = 18
		float fitness = playerHealth / AIHealth + playerUnits / AIUnits;

		return fitness;
	}

	public override void Stop () {
		throw new System.NotImplementedException ();
	}

	#endregion

	#region Misc/helpers

	public static void PrintGameState(int[,] gs) {

		Debug.Log("GameState from NNController");

		for(int i = 0; i < gs.GetLength(0); i++) {
			string s = "";
			for(int j = 0; j < gs.GetLength(1); j++) {
				s += gs[i,j] + " ";
			}
			Debug.Log(s);
		}

		Debug.Log("--------------------------");

	}

	#endregion
}
