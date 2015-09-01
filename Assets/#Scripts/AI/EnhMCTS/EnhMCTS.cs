using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System;

public class EnhMCTS : MonoBehaviour {
	
	#region Singleton
	
	private static EnhMCTS instance;
	
	public static EnhMCTS Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<EnhMCTS>();
			}
			
			return instance;
		}
	}
	
	#endregion
	
	const long RUN_TIME = 6000;
	
	public EnhGameStateUnit[,] gameState;
	
	MCTSNode rootNode;
	MCTSNode currentNode;
	
	float EC = (float)(1f/Math.Sqrt(2)); //Exploration constant
	float ECAtt = (float)(1.3f/Math.Sqrt(2));

	System.Random rnd = new System.Random();
	
	List<MCTSNode> defaultList = new List<MCTSNode>(); //Used for optimising default
	
	//For testing/debugging
	int minDefaultDepth = int.MaxValue;
	int maxDefaultDepth = int.MinValue;
	int totalcalls = 0;
	int maxChildIndex = 0;
	int maxFinalNodes = 0;
	int tmpFinalNodes = 0;
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
	
	void GetMove(BackgroundWorker worker, DoWorkEventArgs e) {

		totalcalls = 0;	
		maxChildIndex = 0;
		totalTime = 0;
		minDefaultDepth = int.MaxValue;
		maxDefaultDepth = int.MinValue;
		tmpFinalNodes = 0;
		
		if (!UpdateRootNode()) {
			//			UnityEngine.Debug.Log("FALSE - I RESET ROOTNODE");
			rootNode = new MCTSNode(null, Action.ATTACK, 0, 0, 0, 0);
		}
		
		Stopwatch sw = new Stopwatch();
		sw.Start();
		int i = 0;

//		sw.ElapsedMilliseconds < RUN_TIME
//		i < 15000

		while(i < 15000) {
			
			if(worker.CancellationPending) {
				e.Cancel = true;
				return;
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
		BestMove();
		EnhAIGameFlow.move = currentNode;

//		UnityEngine.Debug.Log("Total calls " + i + " elapsed time " + (sw.ElapsedMilliseconds / 100));
//
//		UnityEngine.Debug.Log("I found amount of nodes with value 1 " + tmpFinalNodes);
//
//		if(tmpFinalNodes > maxFinalNodes) {
//			maxFinalNodes = tmpFinalNodes;
//			UnityEngine.Debug.Log("Amount of final nodes = " + maxFinalNodes);
//		}
//		UnityEngine.Debug.Log("Average default depth = " + (totalcalls / i));
//
//		UnityEngine.Debug.Log("Deepest node = " + maxChildIndex);
//
//		UnityEngine.Debug.Log("Total calls for default " + totalcalls);
//		UnityEngine.Debug.Log("Min default depth is " + minDefaultDepth);
//		UnityEngine.Debug.Log("Max default depth is " + maxDefaultDepth);
//
//		UnityEngine.Debug.Log("Total time for what you are testing " + (totalTime));
		
		EnhAIGameFlow.finished = true;
	}

	/// <summary>
	/// I use this function in order to continue on the tree previously expanded
	/// in order not to lose the information/statistic already discovered
	/// </summary>
	bool UpdateRootNode() {
		
		if(currentNode == null || currentNode.children == null) {
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
		
		//		UnityEngine.Debug.Log("------");
		bool match = false;
		
		foreach(MCTSNode n in GameFlow.playerLastMoveList) {
			match = false;
			
			//Just check if the node has even been expanded
			if(currentNode.children == null)
				return false;
			
			foreach(MCTSNode node in currentNode.children) {
				if(node.Equals(n)) {
					//					UnityEngine.Debug.Log("MATCH MATCH");
					currentNode = node;
					rootNode = currentNode;
					rootNode.parent = null;
					match = true;
				}
			}
			
			if(!match)
				break;
		}
		
		if(match)
			return true;
		else 
			return false;
	}

	/// <summary>
	/// Return best robust child
	/// </summary>
	void BestMove() {

		int bestIndex = 0;
		float bestScore = 0;
		List<UCTValueHolder> UCTValues = new List<UCTValueHolder>();

		for(int i = 0; i < currentNode.children.Count; i++) {
			float tmpUCTValue = 0;
			
			//Bias towards attack
			if(currentNode.children[i].action == Action.ATTACK)
				tmpUCTValue = UCTValueAIRobustChild(currentNode.children[i], EC);
			else
				tmpUCTValue = UCTValueAIRobustChild(currentNode.children[i], 0);
			
			if(tmpUCTValue > bestScore) {
				bestScore = tmpUCTValue;
				bestIndex = i;
			}
		}
		
		UCTValues.Add(new UCTValueHolder(bestIndex, bestScore));
		
		//Find those that are equal in order to take a random decision
		for(int i = 0; i < currentNode.children.Count; i++) {
			if(i != UCTValues[0].index) {
				float tmpUCTValue = 0;

				if(currentNode.children[i].action == Action.ATTACK)
					tmpUCTValue = UCTValueAIRobustChild(currentNode.children[i], EC);
				else
					tmpUCTValue = UCTValueAIRobustChild(currentNode.children[i], 0);

				if(tmpUCTValue == UCTValues[0].UCTValue)
					UCTValues.Add(new UCTValueHolder(i, tmpUCTValue));
			}
		}

		WeedOutMoveFromList(ref UCTValues);
		bestIndex = UCTValues[rnd.Next(0, UCTValues.Count)].index;
		
		currentNode = currentNode.children[bestIndex];
	}
	
	/// <summary>
	/// The tree policy is how I traverse the tree and also makes sure to expand nodes
	/// if needed.
	/// </summary>
	void TreePolicy() {
		gameState = EnhAIGameFlow.Instance.GetCopyOfGameState();
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
		
		char curTurn = EnhAIGameFlow.Instance.turnOrderList[EnhAIGameFlow.Instance.curTurnOrderIndex].curgsUnit.state;
		
		List<UCTValueHolder> UCTValues = new List<UCTValueHolder>();
		
		if(curTurn == EnhAIGameFlow.GS_AI) {
			for(int i = 0; i < currentNode.children.Count; i++) {
				float tmpUCTValue = 0;

				//Bias towards attack
				if(currentNode.children[i].action == Action.ATTACK)
					tmpUCTValue = UCTValueAI(currentNode.children[i], ECAtt);
				else
					tmpUCTValue = UCTValueAI(currentNode.children[i], C);
				
				if(tmpUCTValue > bestScore) {
					bestScore = tmpUCTValue;
					bestIndex = i;
				}
			}
			
			UCTValues.Add(new UCTValueHolder(bestIndex, bestScore));
			
			//Find those that are equal in order to take a random decision
			for(int i = 0; i < currentNode.children.Count; i++) {
				if(i != UCTValues[0].index) {
					float tmpUCTValue = 0;

					//Bias towards attack
					if(currentNode.children[i].action == Action.ATTACK)
						tmpUCTValue = UCTValueAI(currentNode.children[i], ECAtt);
					else
						tmpUCTValue = UCTValueAI(currentNode.children[i], C);

					if(tmpUCTValue == UCTValues[0].UCTValue)
						UCTValues.Add(new UCTValueHolder(i, tmpUCTValue));
				}
			}
			
		} else if (curTurn == EnhAIGameFlow.GS_PLAYER) {
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
					float tmpUCTValue = UCTValuePlayer(currentNode.children[i], C);
					if(tmpUCTValue == UCTValues[0].UCTValue)
						UCTValues.Add(new UCTValueHolder(i, tmpUCTValue));
				}
			}
		}
		
		WeedOutMoveFromList(ref UCTValues);
		bestIndex = UCTValues[rnd.Next(0, UCTValues.Count)].index;
		
		currentNode = currentNode.children[bestIndex];
		
		//I need to make the move as I do NOT store gamestate on each node.
		//If C == 0 I am done with the process and no move is required
		//as it will give an error!!
		if(currentNode.action == Action.MOVE) {
			EnhAIGameFlow.activeUnit.Move(gameState[currentNode.gsH, currentNode.gsW]);
		} else if(currentNode.action == Action.ATTACK) {
			EnhAIGameFlow.activeUnit.Attack(gameState[currentNode.mbagsH, currentNode.mbagsW], gameState[currentNode.gsH, currentNode.gsW]);
		}
		
		EnhAIGameFlow.Instance.StartNextTurn();
		
		//If I have reached a final state return false. 
		//This is to make sure I don't expand further.
		if(EnhAIGameFlow.Instance.IsGameOver() == -1) {
			return true;
		} else {
			return false;
		}
	}
	
	/// <summary>
	/// Used to priotise ATTACK over MOVE if break-even is happening.
	/// If this is not implemented the game can continue for a long time
	/// even though the computer has already won because all moves will lead
	/// to a victory.
	/// </summary>
	/// <param name="list">List.</param>
	void WeedOutMoveFromList(ref List<UCTValueHolder> list) {
		
		List<UCTValueHolder> tmpList = new List<UCTValueHolder>();
		
		for(int i = 0; i < list.Count; i++) {
			if(currentNode.children[list[i].index].action == Action.ATTACK)
				tmpList.Add(list[i]);
		}
		
		if(tmpList.Count > 0)
			list = tmpList;
	}
	
	/// <summary>
	/// If the currentnode is not fully expanded we just take the next
	/// child in the list - this is to make sure that all childs are visited
	/// at least once.
	/// </summary>
	void Expand() {
		
		currentNode = currentNode.children[currentNode.curChildIndex];
		
		if(currentNode.action == Action.MOVE) {
			EnhAIGameFlow.activeUnit.Move(gameState[currentNode.gsH, currentNode.gsW]);
		} else if(currentNode.action == Action.ATTACK) {
			//			if(currentNode.mbagsH == -1)
			//				EnhAIGameFlow.activeUnit.Attack(null, gameState[currentNode.gsH, currentNode.gsW]);
			//			else
			EnhAIGameFlow.activeUnit.Attack(gameState[currentNode.mbagsH, currentNode.mbagsW], gameState[currentNode.gsH, currentNode.gsW]);
		}
		
		EnhAIGameFlow.Instance.StartNextTurn();
	}
	
	bool FullyExpanded() {
		if(currentNode.children == null) {
			currentNode.children = EnhAIGameFlow.activeUnit.GetPossibleMoves(currentNode);
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
		
		float result = EnhAIGameFlow.Instance.IsGameOver();
		
//		int depth = 0;
		
		while(result == -1) {
			int index = EnhAIGameFlow.activeUnit.GetPossibleMovesDefaultPolicy(defaultList);

			int attint = DefaultPolicyAttackFilter(index+1);

			if(attint >= 0)
				index = attint;

			MCTSNode action = defaultList[rnd.Next(0, index + 1)];
			
			if(action.action == Action.MOVE) {
				EnhAIGameFlow.activeUnit.Move(gameState[action.gsH, action.gsW]);
			} else if(action.action == Action.ATTACK) {
				EnhAIGameFlow.activeUnit.Attack(gameState[action.mbagsH, action.mbagsW], gameState[action.gsH, action.gsW]);
			}
			
//			depth++;
//			totalcalls++;
			result = EnhAIGameFlow.Instance.StartNextTurn();
		}
		
//		if(depth < minDefaultDepth)
//			minDefaultDepth = depth;
//		
//		if(depth > maxDefaultDepth)
//			maxDefaultDepth = depth;
//		
//		if(result == 1)
//			tmpFinalNodes++;
		
		return result;
	}

	int DefaultPolicyAttackFilter(int index) {

		int rValue = -1;

		for(int i = 0; i < index; i++) {
			if(defaultList[i].action == Action.ATTACK) {
				defaultList[++rValue] = defaultList[i];
			}
		}

		return rValue;
	}
	
	/// <summary>
	/// This version of EnhMCTS is called UCT, and the below calculation is used in order to find
	/// the next child in the list, given that all childs have been visited once. First part of
	/// the equation is the exploitation part, the second part is the exploration.
	/// </summary>
	float UCTValueAI(MCTSNode n, float C) {
		return (float)((n.reward / n.timeVisited) + 2*C*(Math.Sqrt(Math.Log10(n.parent.timeVisited) / n.timeVisited)));
	}
	
	float UCTValuePlayer(MCTSNode n, float C) {
		return (float)((n.reward / n.timeVisited) - 2*C*(Math.Sqrt(Math.Log10(n.parent.timeVisited) / n.timeVisited)));
	}
	
	float UCTValueAIRobustChild(MCTSNode n, float C) {
		return (float)((n.reward / n.timeVisited) + C*(Math.Sqrt(Math.Log10(n.timeVisited))));
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

	//Used for resetting the game in GameFlow.cs
	public void ResetCurrentNode() {
		currentNode = null;
	}
}
