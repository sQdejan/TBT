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

	void Start() {
		instance = this;
	}

	public GameStateUnit[,] gameState;

	MCTSNode rootNode;
	MCTSNode currentNode;

	public void GetMove(MCTSNode node) {
		rootNode = node;

		//A while loop will run here
		TreePolicy();
	}

	//VERY IMPORTANT: remember to update the TURNORDERLIST item each time a move is happening
	void TreePolicy() {
		gameState = AIGameFlow.Instance.GetCopyOfGameState();
		currentNode = rootNode;
		rootNode.children = AIGameFlow.activegsUnit.occupier.GetPossibleMoves(rootNode);
		AIGameFlow.activegsUnit.occupier.Move(rootNode.children[0].gsUnit);


//		AIGameFlow.PrintGameState(gameState);
//		gameState[0,5].occupier.Move(gameState[2,5]);
//		UnityEngine.Debug.Log("-----");
//		AIGameFlow.PrintGameState(gameState);
	}

	void BestChild() {
	}

	void Expand() {
	}

	bool FullyExpanded(MCTSNode node) {
		return false;
	}

	void NextUntriedAction(MCTSNode node) {
	}

	float DefaultPolicy() {
		return 0;
	}

	float UCTValue() {
		return 0;
	}

	void BackPropagate(float reward) {
	}

}
