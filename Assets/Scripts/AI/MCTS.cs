﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
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

	const long RUN_TIME = 5000;

	public GameStateUnit[,] gameState;

	MCTSNode rootNode;
	MCTSNode currentNode;

	float EC = (float)(1f/Math.Sqrt(2)); //Exploration constant

	System.Random rnd = new System.Random();
	
	List<MCTSNode> defaultList = new List<MCTSNode>(); //Used for optimising default

	//For testing/debugging
	int highestIndex = 0;
	long maxIndex = 0;
	int totalcalls = 0;
	int maxChildIndex = 0;

	long totalTime = 0;
	Stopwatch test = new Stopwatch();

	void Start() {
		instance = this;

		for(int i = 0; i < 50; i++) {
			defaultList.Add(new MCTSNode(null, Action.ATTACK, 1, 1, 1, 1));
		}
	}

	public void StartProcess(object sender, DoWorkEventArgs e) {
		BackgroundWorker worker = sender as BackgroundWorker;
		GetMove(worker, e);
	}

	/// <summary>
	/// I use this function in order to continue on the tree previously expanded
	/// in order not to lose the information/statistic already discovered
	/// </summary>
	bool UpdateRootNode() {

		if(currentNode == null) {
			return false;
		}

		//If the player did not have the turn we can just continue from
		//the current node
		if(!GameFlow.didPlayerHaveATurn) {
			rootNode = currentNode;
			rootNode.parent = null;
			return true;
		} else {
			GameFlow.didPlayerHaveATurn = false;
		}

		foreach(MCTSNode n in GameFlow.playerLastMoveList) {
			bool match = false;

			foreach(MCTSNode node in currentNode.children) {
				if(node.Equals(n)) {
					currentNode = node;
					rootNode = currentNode;
					rootNode.parent = null;
					return true;
				}
			}

			if(!match)
				break;
		}

		return false;
	}

	void GetMove(BackgroundWorker worker, DoWorkEventArgs e) {

		highestIndex = 0;
		maxIndex = 0;
		totalcalls = 0;	
		maxChildIndex = 0;
		totalTime = 0;

		if (!UpdateRootNode()) {
//			UnityEngine.Debug.Log("FALSE - I RESET ROOTNODE");
			rootNode = new MCTSNode(null, Action.ATTACK, 0, 0, 0, 0);
		}

		Stopwatch sw = new Stopwatch();
		sw.Start();
		int i = 0;

		while(sw.ElapsedMilliseconds < RUN_TIME) {

			if(worker.CancellationPending) {
				e.Cancel = true;
				break;
			}
//			test.Start();
			TreePolicy();
			float reward = DefaultPolicy();
			BackPropagate(reward);
			i++;
//			test.Stop();
//			totalTime += test.ElapsedMilliseconds;
//			test.Reset();
		}
		sw.Stop();

		//Find the best move
		currentNode = rootNode;
		BestChild(0);
		AIGameFlow.move = currentNode;

//		UnityEngine.Debug.Log("Highest child count is = " + highestIndex);
//		UnityEngine.Debug.Log("Average child count is = " + (maxIndex / totalcalls));
//		UnityEngine.Debug.Log("Average default depth = " + (totalcalls / i));
//
//		UnityEngine.Debug.Log("Deepest node = " + maxChildIndex);
//
//		UnityEngine.Debug.Log("Total calls for default " + totalcalls);
//		UnityEngine.Debug.Log("Cycles = " + i);
//
//		UnityEngine.Debug.Log("Total time for what you are testing " + (totalTime));

		AIGameFlow.finished = true;
	}

	/// <summary>
	/// The tree policy is how I traverse the tree and also makes sure to expand nodes
	/// if needed.
	/// </summary>
	void TreePolicy() {
		gameState = AIGameFlow.Instance.GetCopyOfGameState();
		currentNode = rootNode;
		int k = 0;
		while(true) {
			if(!FullyExpanded()) {
				currentNode.curChildIndex++;
				Expand();
				break;
			} else {
				k++;
				if(!BestChild(EC))
					break;
			}
		}

		if(k > maxChildIndex)
			maxChildIndex = k;

//		UnityEngine.Debug.Log("I visit best child " + k + " times");
	}

	/// <summary>
	/// Find the best child in order to traverse the tree
	/// Once it has been found, a DefaultPolicy will be played out
	/// </summary>
	/// <param name="C">C.</param>
	bool BestChild(float C) {

		int bestIndex = 0;
		float bestScore = 0;

		//I need to consider if it's the AI or the Player's move
		char curTurn = AIGameFlow.Instance.turnOrderList[AIGameFlow.Instance.curTurnOrderIndex].curgsUnit.state;

		List<UCTValueHolder> UCTValues = new List<UCTValueHolder>();

		if(curTurn == AIGameFlow.GS_AI || C == 0) {
			for(int i = 0; i < currentNode.children.Count; i++) {
				float tmpUCTValue = UCTValueAI(currentNode.children[i], C);

				if(tmpUCTValue > bestScore) {
					bestScore = tmpUCTValue;
					bestIndex = i;
				}
			}

			UCTValues.Add(new UCTValueHolder(bestIndex, bestScore));

			//Find those that are equal in order to take a random decision
			for(int i = 0; i < currentNode.children.Count; i++) {
				if(i != UCTValues[0].index) {
					float tmpUCTValue = UCTValueAI(currentNode.children[i], C);
					if(tmpUCTValue == UCTValues[0].UCTValue)
						UCTValues.Add(new UCTValueHolder(i, tmpUCTValue));
				}
			}

			bestIndex = UCTValues[rnd.Next(0, UCTValues.Count)].index;

		} else if (curTurn == AIGameFlow.GS_PLAYER) {
			bestScore = float.MaxValue;
			
			for(int i = 0; i < currentNode.children.Count; i++) {
				float tmpUCTValue = UCTValuePlayer(currentNode.children[i], C);

				if(tmpUCTValue < bestScore) {
					bestScore = tmpUCTValue;
					bestIndex = i;
				}
			}

			UCTValues.Add(new UCTValueHolder(bestIndex, bestScore));
			
			//Find those that are equal in order to take a random decision
			for(int i = 0; i < currentNode.children.Count; i++) {
				if(i != UCTValues[0].index) {
					float tmpUCTValue = UCTValueAI(currentNode.children[i], C);
					if(tmpUCTValue == UCTValues[0].UCTValue)
						UCTValues.Add(new UCTValueHolder(i, tmpUCTValue));
				}
			}
			
			bestIndex = UCTValues[rnd.Next(0, UCTValues.Count)].index;

		} else {
			UnityEngine.Debug.Log("I should never ever be here!!");
		}

		currentNode = currentNode.children[bestIndex];

		//I need to make the move as I do NOT store gamestate on each node.
		//If C == 0 I am done with the process and no move is required
		//as it will give an error!!
		if(C != 0) {
			if(currentNode.action == Action.MOVE) {
				AIGameFlow.activeUnit.Move(gameState[currentNode.gsH, currentNode.gsW]);
			} else if(currentNode.action == Action.ATTACK) {
				AIGameFlow.activeUnit.Attack(gameState[currentNode.mbagsH, currentNode.mbagsW], gameState[currentNode.gsH, currentNode.gsW]);
			}

			AIGameFlow.Instance.StartNextTurn();
		} else {
			return true;
		}

		//If I have reached a final state return false. 
		//This is to make sure I don't expand further.
		if(AIGameFlow.Instance.IsGameOver() == -1) {
			return true;
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// If the currentnode is not fully expanded we just take the next
	/// child in the list - this is to make sure that all childs are visited
	/// at least once.
	/// </summary>
	void Expand() {

		currentNode = currentNode.children[currentNode.curChildIndex];

		if(currentNode.action == Action.MOVE) {
			AIGameFlow.activeUnit.Move(gameState[currentNode.gsH, currentNode.gsW]);
		} else if(currentNode.action == Action.ATTACK) {
//			if(currentNode.mbagsH == -1)
//				AIGameFlow.activeUnit.Attack(null, gameState[currentNode.gsH, currentNode.gsW]);
//			else
				AIGameFlow.activeUnit.Attack(gameState[currentNode.mbagsH, currentNode.mbagsW], gameState[currentNode.gsH, currentNode.gsW]);
		}

		AIGameFlow.Instance.StartNextTurn();
	}

	bool FullyExpanded() {
		if(currentNode.children == null) {
			currentNode.children = AIGameFlow.activeUnit.GetPossibleMoves(currentNode);
		}

		return currentNode.curChildIndex >= currentNode.children.Count - 1;
	}

	/// <summary>
	/// Default policy. In this case it just does a uniformly distributed random move to 
	/// do the play-out. Once a final state has been reached, a reward for that current state
	/// will be returned:
	/// 	1 if the AI wins
	/// 	0 if the Player wins
	/// </summary>
	/// <returns>The default policy reward.</returns>
	float DefaultPolicy() {
		
		int result = AIGameFlow.Instance.IsGameOver();

		while(result == -1) {
			int index = AIGameFlow.activeUnit.GetPossibleMovesDefaultPolicy(defaultList);

			if(index > highestIndex)
				highestIndex = index;

			maxIndex += index;

			MCTSNode action = defaultList[rnd.Next(0, index + 1)];

			if(action.action == Action.MOVE) {
				AIGameFlow.activeUnit.Move(gameState[action.gsH, action.gsW]);
			} else if(action.action == Action.ATTACK) {
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
	float UCTValueAI(MCTSNode n, float C) {
		return (float)((n.reward / n.timeVisited) + C*(Math.Sqrt(2*Math.Log(n.parent.timeVisited) / n.timeVisited)));
	}

	float UCTValuePlayer(MCTSNode n, float C) {
		return (float)((n.reward / n.timeVisited) - C*(Math.Sqrt(2*Math.Log(n.parent.timeVisited) / n.timeVisited)));
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

class UCTValueHolder {
	public int index;
	public float UCTValue;

	public UCTValueHolder(int index, float UCTValue) {
		this.index = index;
		this.UCTValue = UCTValue;
	}
}
