using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class MCTS : MonoBehaviour {

#region Singleton
	
	private static MCTS instance;
	
	public static MCTS Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<MCTS>();
			}
			
			return instance;
		}
	}
	
#endregion
	

	public GameStateUnit[,] gameState;

	MCTSNode rootNode;
	MCTSNode currentNode;

	System.Random rnd = new System.Random();

	void Start() {
		instance = this;
	}

	public void StartProcess() {
		GetMove(new MCTSNode(null, Action.ATTACK, 0, 0, 0, 0));
	}

	//Remember to end the function with storing the move somewhere, most likely in AIGameFlow
	void GetMove(MCTSNode node) {
		rootNode = node;

		//A while loop will run here
		int index = 0;

		while (index < 1) {
			TreePolicy();
			DefaultPolicy();
			index++;
		}



//		Stopwatch sw = new Stopwatch();
//		sw.Start();
//		int i = 0;
//		while(sw.ElapsedMilliseconds < 5000) {
//			TreePolicy();
//		}
//		sw.Stop();
//		AIGameFlow.finished = true;
	}

	//VERY IMPORTANT: remember to update the TURNORDERLIST item each time a move is happening (meaning change GameStateUnit in the list)
	void TreePolicy() {
		gameState = AIGameFlow.Instance.GetCopyOfGameState();
		currentNode = rootNode;

		while(true) {
			if(!FullyExpanded()) {
				Expand();
				break;
			} else {
				break;
			}
		}

//		AIGameFlow.PrintGameState(gameState);
//		gameState[0,5].occupier.Move(gameState[2,5]);
//		UnityEngine.Debug.Log("-----");
//		AIGameFlow.PrintGameState(gameState);
	}

	void BestChild() {
	}

	void Expand() {

		currentNode = currentNode.children[currentNode.curChildIndex];

		if(currentNode.action == Action.MOVE) {
			AIGameFlow.activeUnit.Move(gameState[currentNode.gsH, currentNode.gsW]);
		} else if(currentNode.action == Action.ATTACK) {
			if(currentNode.mbagsH == -1)
				AIGameFlow.activeUnit.Attack(null, gameState[currentNode.gsH, currentNode.gsW]);
			else
				AIGameFlow.activeUnit.Attack(gameState[currentNode.mbagsH, currentNode.mbagsW], gameState[currentNode.gsH, currentNode.gsW]);
		}

		AIGameFlow.Instance.StartNextTurn();
	}

	bool FullyExpanded() {
		if(currentNode.children == null) {
			currentNode.children = AIGameFlow.activeUnit.GetPossibleMoves(currentNode);
		}

		return ++currentNode.curChildIndex >= currentNode.children.Count;
	}

	float DefaultPolicy() {

		int result = -1;

		while(result == -1) {

			List<MCTSNode> possibleActions = AIGameFlow.activeUnit.GetPossibleMoves(null);
			MCTSNode action = possibleActions[rnd.Next(0, possibleActions.Count)];

			if(action.action == Action.MOVE) {
				AIGameFlow.activeUnit.Move(gameState[action.gsH, action.gsW]);
			} else if(action.action == Action.ATTACK) {
				if(action.mbagsH == -1)
					AIGameFlow.activeUnit.Attack(null, gameState[action.gsH, action.gsW]);
				else
					AIGameFlow.activeUnit.Attack(gameState[action.mbagsH, action.mbagsW], gameState[action.gsH, action.gsW]);
			}

			result = AIGameFlow.Instance.StartNextTurn();
		}

		UnityEngine.Debug.Log("2 good 2 be true with fucking result = " + result);

		return 0;
	}

	float UCTValue() {
		return 0;
	}

	void BackPropagate(float reward) {
	}

}
