using UnityEngine;
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
	

	public GameStateUnit[,] gameState;

	MCTSNode rootNode;
	MCTSNode currentNode;

	float EC = (float)(1f/Math.Sqrt(2)); //Exploration constant

	System.Random rnd = new System.Random();
	
	List<MCTSNode> defaultList = new List<MCTSNode>(); //Used for optimising default

	void Start() {
		instance = this;

		for(int i = 0; i < 50; i++) {
			defaultList.Add(new MCTSNode(null, Action.ATTACK, 1, 1, 1, 1));
		}

	}

	public void StartProcess(object sender, DoWorkEventArgs e) {
		BackgroundWorker worker = sender as BackgroundWorker;
		GetMove(new MCTSNode(null, Action.ATTACK, 0, 0, 0, 0), worker, e);
	}

	void GetMove(MCTSNode node, BackgroundWorker worker, DoWorkEventArgs e) {
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

			if(worker.CancellationPending) {
				e.Cancel = true;
				break;
			}

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

		UnityEngine.Debug.Log("Highest child count is = " + highestIndex);
		UnityEngine.Debug.Log("Average child count is = " + (maxIndex / totalcalls));
		UnityEngine.Debug.Log("Average default depth = " + (totalcalls / defaultCalls));

		UnityEngine.Debug.Log("Deepest node = " + maxChildIndex);

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
		int k = 0;
		while(true) {
			if(!FullyExpanded()) {
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

	int maxChildIndex = 0;

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
		if(curTurn == AIGameFlow.GS_AI || C == 0) {
			if(C==0)
			bestScore = float.MinValue;

			for(int i = 0; i < currentNode.children.Count; i++) {
				float tmpUCTValue = UCTValueAI(currentNode.children[i], C);

				if(tmpUCTValue > bestScore) {
					bestScore = tmpUCTValue;
					bestIndex = i;
				}
			}
		} else if (curTurn == AIGameFlow.GS_PLAYER) {
			bestScore = float.MaxValue;
			
			for(int i = 0; i < currentNode.children.Count; i++) {
				float tmpUCTValue = UCTValuePlayer(currentNode.children[i], C);
				
				if(tmpUCTValue < bestScore) {
					bestScore = tmpUCTValue;
					bestIndex = i;
				}
			}
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
				if(currentNode.mbagsH == -1)
					AIGameFlow.activeUnit.Attack(null, gameState[currentNode.gsH, currentNode.gsW]);
				else
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
	/// <returns>The default policy reward.</returns>
	float DefaultPolicy() {
		
		int result = AIGameFlow.Instance.IsGameOver();

		defaultCalls++;

		while(result == -1) {
			int index = AIGameFlow.activeUnit.GetPossibleMovesDefaultPolicy(defaultList);

			if(index > highestIndex)
				highestIndex = index;

			maxIndex += index;

			MCTSNode action = defaultList[rnd.Next(0, index + 1)];
			if(action.action == Action.MOVE) {
				AIGameFlow.activeUnit.Move(gameState[action.gsH, action.gsW]);
			} else if(action.action == Action.ATTACK) {
				if(action.mbagsH == -1) {
					AIGameFlow.activeUnit.Attack(null, gameState[action.gsH, action.gsW]);
				}
				else
					AIGameFlow.activeUnit.Attack(gameState[action.mbagsH, action.mbagsW], gameState[action.gsH, action.gsW]);
			}

			totalcalls++;
			result = AIGameFlow.Instance.StartNextTurn();
		}

		return result;
	}

	//For testing/debugging
	int highestIndex = 0;
	long maxIndex = 0;
	int totalcalls = 0;
	int defaultCalls = 0;

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
