using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.ComponentModel;

public class EnhAIGameFlow : MonoBehaviour {
	
	#region Singleton
	
	private static EnhAIGameFlow instance;
	
	public static EnhAIGameFlow Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<EnhAIGameFlow>();
			}
			
			return instance;
		}
	}
	
	#endregion
	
	public const char GS_PLAYER = 'P';
	public const char GS_EMPTY = 'E';
	public const char GS_MOVE = 'M';
	public const char GS_AI = 'A';
	
	public static EnhAIUnit activeUnit;
	public static bool finished = false;
	public static int cycles = 0;
	
	public static MCTSNode move;
	
	public List<EnhAIUnit> turnOrderList;
	[HideInInspector] public int curTurnOrderIndex;
	[HideInInspector] public int turnsTaken;
	
	int height, width;
	
	EnhGameStateUnit[,] gameState;
	EnhGameStateUnit[] turnOrderArray;	//A static version of the initial turn order, used to create the dynamic list above (turnOrderList)
	
	Thread MCTSThread;
	BackgroundWorker MCTSBackgroundWorker;
	
	void Start() {
		instance = this;
	}
	
	void Update() {
		if(!finished)
			return;
		
		GameFlow.Instance.MoveHasBeenCalculated(false);
	}
	
	#region Setup related
	
	//Initialise the current game state and prepare for EnhMCTS
	public void SetupGameState() {
		
		height = GridController.Instance.gridHeight;
		width = GridController.Instance.gridWidth;
		
		gameState = new EnhGameStateUnit[height, width];
		turnOrderArray = new EnhGameStateUnit[GameFlow.Instance.UnitTurnOrderList.Count];
		
		for(int i = 0; i < height; i++) {
			for(int j = 0; j < width; j++) {
				if(!GridController.Instance.tileArray[i,j].occupied) {
					gameState[i,j] = new EnhGameStateUnit(GS_EMPTY, i, j, null);
				} else {
					GameObject occupier = GridController.Instance.tileArray[i,j].occupier;
					EnhGameStateUnit gsUnit = null;
					EnhAIUnit tmpUnit = null;
					
					if(occupier.GetComponent<Unit>().classType == ClassType.WARRIOR) {
						tmpUnit = new EnhAIWarrior();
					} else if (occupier.GetComponent<Unit>().classType == ClassType.RANGED) {
						tmpUnit = new EnhAIRanger();
					} 
					
					if(occupier.tag == "PlayerUnit") {
						tmpUnit.possibleTarget = GS_AI;
						gsUnit = new EnhGameStateUnit(GS_PLAYER, i, j, tmpUnit);
					} else {
						tmpUnit.possibleTarget = GS_PLAYER;
						gsUnit = new EnhGameStateUnit(GS_AI, i, j, tmpUnit);
					}
					
					CopyStats(occupier.GetComponent<Unit>(), gsUnit.occupier);
					gameState[i,j] = gsUnit;
					
					//Add the gsUnit to turn order
					turnOrderArray[GameFlow.Instance.UnitTurnOrderList.IndexOf(occupier)] = gsUnit;
				}
			}
		}
		
		//Delegate the methods needed and start a thread
		MCTSBackgroundWorker = new BackgroundWorker();
		
		MCTSBackgroundWorker.WorkerSupportsCancellation = true;
		MCTSBackgroundWorker.DoWork += new DoWorkEventHandler(EnhMCTS.Instance.StartProcess);
		
		MCTSBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MCTSBackgroundWorker_RunWorkerCompleted);
		MCTSBackgroundWorker.RunWorkerAsync();
		
		//		EnhMCTS.Instance.StartProcess();
		
		//		MCTSThread = new Thread(EnhMCTS.Instance.StartProcess);
		//		MCTSThread.Start();
	}
	
	void MCTSBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
		
		if(e.Error != null) {
			Debug.LogError(e.Error.Message + e.Error.StackTrace);
		} else if(e.Cancelled) {
			Debug.Log("Event is cancelled and the game is restarted");
			GameFlow.restartGame = true;
		}
	}
	
	void CopyStats(Unit from, EnhAIUnit to) {
		to.possibleMovesStraight = from.possibleMovesStraight;
		to.possibleMovesStrafe = from.possibleMovesStrafe;
		to.attackRange = from.attackRange;
		to.health = from.health;
		to.damage = from.damage;
		to.attackDirection = from.attackDirection;
		to.moveDirection = from.moveDirection;
	}
	
	#endregion
	
	#region Gameflow related
	
	public float StartNextTurn() {
		
		float isGameOver = IsGameOver();
		
		if(isGameOver >= 0)
			return isGameOver;
		
		if(++curTurnOrderIndex >= turnOrderList.Count)
			curTurnOrderIndex = 0;
		
		activeUnit = turnOrderList[curTurnOrderIndex];
		
		turnsTaken++;
		
		return isGameOver;
	}
	
	public void KillUnit(EnhAIUnit unit) {
		
		int tmpIndex = turnOrderList.IndexOf(unit);
		
		unit.curgsUnit.state = GS_EMPTY;
		unit.curgsUnit.occupier = null;
		
		if(tmpIndex < curTurnOrderIndex)
			curTurnOrderIndex--;
		
		turnOrderList.RemoveAt(tmpIndex);
	}
	
	public float IsGameOver() {
		
		if(turnsTaken >= GameFlow.MAX_TURNS) {
			return 0.2f;
		}
		
		bool foundPlayer = false;
		bool foundAI = false;
		
		foreach(EnhAIUnit u in turnOrderList) {
			if(u.curgsUnit.state == EnhAIGameFlow.GS_PLAYER)
				foundPlayer = true;
			else if(u.curgsUnit.state == EnhAIGameFlow.GS_AI)
				foundAI = true;
		}
		
		//Continue
		if(foundPlayer && foundAI)
			return -1;
		
		//If found player the AI lost
		if(foundPlayer)
			return 0;
		
		//Else the AI won
		if(foundAI)
			return 1;
		
		//Else the compiler complains
		return -99;
	}
	
	#endregion
	
	#region Helpers/Misc 
	
	//For resetting the EnhMCTS - also resetting the turn order list here
	public EnhGameStateUnit[,] GetCopyOfGameState() {
		
		EnhGameStateUnit[,] rArray = new EnhGameStateUnit[height, width];
		
		for(int i = 0; i < height; i++) {
			for(int j = 0; j < width; j++) {
				rArray[i,j] = new EnhGameStateUnit(gameState[i,j]);
			}
		}
		
		//Resetting the turnOrderList
		curTurnOrderIndex = GameFlow.Instance.CurTurnIndex - 1; //I need to subtract one because I increment right away
		turnOrderList = new List<EnhAIUnit>();
		for(int i = 0; i < turnOrderArray.Length; i++) {
			turnOrderList.Add(rArray[turnOrderArray[i].h, turnOrderArray[i].w].occupier);
		}
		
		turnsTaken = GameFlow.Instance.turnsTaken - 2;
		
		//Start the process
		StartNextTurn();
		
		return rArray;
	}
	
	public static void ClearGameState(EnhGameStateUnit[,] gs) {
		for(int i = 0; i < gs.GetLength(0); i++) {
			for(int j = 0; j < gs.GetLength(1); j++) {
				if(gs[i,j].state == GS_MOVE)
					gs[i,j].state = GS_EMPTY;
			}
		}
	}
	
	public static void PrintGameState(EnhGameStateUnit[,] gs) {
		
		for(int i = 0; i < gs.GetLength(0); i++) {
			string printString = i + ". ";
			
			for(int j = 0; j < gs.GetLength(1); j++) {
				printString += gs[i,j].state + " ";
			}
			
			Debug.Log(printString);
		}
	}
	
	public static void PrintUnits(EnhGameStateUnit[,] gs) {
		for(int i = 0; i < gs.GetLength(0); i++) {
			for(int j = 0; j < gs.GetLength(1); j++) {
				if(gs[i,j].state == GS_AI || gs[i,j].state == GS_PLAYER) {
					Debug.Log("Health " + gs[i,j].occupier.health);
					Debug.Log("Possible Moves Straight " + gs[i,j].occupier.possibleMovesStraight);
					Debug.Log("Attack range " + gs[i,j].occupier.attackRange);
					Debug.Log("Damage " + gs[i,j].occupier.damage);
					Debug.Log("Possible target " + gs[i,j].occupier.possibleTarget);
					Debug.Log("----------------------");
				}
			}
		}
	}
	
	//If I reset the scene I abort the thread for good measures
	public void CancelBackgroundWorker() {
		MCTSBackgroundWorker.CancelAsync();
	}
	
	#endregion
}

public class EnhGameStateUnit {
	
	public EnhGameStateUnit(char state, int h, int w, EnhAIUnit occupier) {
		this.state = state;
		this.h = h;
		this.w = w;
		this.occupier = occupier;
	}
	
	public EnhGameStateUnit(EnhGameStateUnit other) {
		this.state = other.state;
		this.h = other.h;
		this.w = other.w;
		if(other.occupier != null)
			this.occupier = other.occupier.Copy(this);
	} 
	
	public char state;
	public readonly int h, w;
	public EnhAIUnit occupier = null;
}

