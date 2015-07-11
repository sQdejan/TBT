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

	float EC = (float)(1f/Math.Sqrt(2)); //Exploration constant

	System.Random rnd = new System.Random();

	int totalcalls; //Just for personal stats on the MCTS - can be deleted

	void Start() {
		instance = this;
	}

	public void StartProcess() {
		GetMove(new MCTSNode(null, Action.ATTACK, 0, 0, 0, 0));
	}

	void GetMove(MCTSNode node) {
		rootNode = node;

		//A while loop will run here
//		int index = 0;
//
//		while (index < 100) {
//			TreePolicy();
//			float reward = DefaultPolicy();
//			BackPropagate(reward);
//			index++;
//		}
//
//		UnityEngine.Debug.Log("Donski");
//		currentNode = rootNode;
//		BestChild(0);
//		AIGameFlow.move = currentNode;
//		AIGameFlow.finished = true;

		Stopwatch sw = new Stopwatch();
		sw.Start();
		int i = 0;
		totalcalls = 0;
		while(sw.ElapsedMilliseconds < 5000) {
			TreePolicy();
			float reward = DefaultPolicy();
			BackPropagate(reward);
			i++;
		}
		sw.Stop();

		//Find the best move
		currentNode = rootNode;
		BestChild(0);
		AIGameFlow.move = currentNode;

		UnityEngine.Debug.Log("Total calls for default " + totalcalls);
		UnityEngine.Debug.Log("Cycles = " + i);

		AIGameFlow.finished = true;
	}

	/// <summary>
	/// The tree policy is how I traverse the tree and also makes sure to expand nodes
	/// if needed.
	/// </summary>
	void TreePolicy() {

		gameState = AIGameFlow.Instance.GetCopyOfGameState();
		currentNode = rootNode;

		while(true) {
			if(!FullyExpanded()) {
				Expand();
				break;
			} else {
				BestChild(EC);
			}
		}
	}

	/// <summary>
	/// Find the best child in order to traverse the tree
	/// Once it has been found, a DefaultPolicy will be played out
	/// </summary>
	/// <param name="C">C.</param>
	void BestChild(float C) {

		int bestIndex = 0;
		float bestScore = float.MinValue;

		for(int i = 0; i < currentNode.children.Count; i++) {
			float tmpUCTValue = UCTValue(currentNode.children[i], C);

			if(tmpUCTValue > bestScore) {
				bestScore = tmpUCTValue;
				bestIndex = i;
			}
		}

		currentNode = currentNode.children[bestIndex];

		//I need to make the move as I do NOT store gamestate on each node
		//if C == 0 I am done with the process and no move is required
		//as it will give an error!!
		if(C != 0) {
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
	}
	
	/// <summary>
	/// If the currentnode is not fully expanded we just take the next
	/// child in the list - this is to make sure that all childs are visited
	/// at least once
	/// </summary>
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

	/// <summary>
	/// Default policy. In this case it just does a uniformly distributed random move to 
	/// do the play-out. Once a final state has been reached, a reward for that current state
	/// will be returned:
	/// 	1 if the AI wins
	/// 	0 if the Player wins
	/// </summary>
	/// <returns>The default policy rewards.</returns>
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

			totalcalls++;
			result = AIGameFlow.Instance.StartNextTurn();
		}

		return result;
	}

	/// <summary>
	/// This version of MCTS is called UCT, and the below calculation is used in order to find
	/// the next child in the list, given that all childs have been visited once. First part of
	/// the equation is the exploitation part, the second part is the exploration.
	/// </summary>
	float UCTValue(MCTSNode n, float C) {
		return (float)((n.reward / n.timeVisited) + C*(Math.Sqrt(2*Math.Log(n.parent.timeVisited) / n.timeVisited)));
	}

	/// <summary>
	/// Back propagate the reward for future UCT value calculations.
	/// </summary>
	/// <param name="reward">Reward.</param>
	void BackPropagate(float reward) {
		while(currentNode != null) {
			currentNode.timeVisited++;
			currentNode.reward += reward;
			currentNode = currentNode.parent;
		}
	}

}
