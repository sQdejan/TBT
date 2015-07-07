using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Action{ATTACK, MOVE};

public class MCTSNode {

	public MCTSNode parent = null;

	//The action it takes to get to THIS node
	public Action action;

	//Where in the array will action happen?
	public GameStateUnit gsUnit;

	//To traverse the tree and also see if a node is fully expanded
	public List<MCTSNode> children;
	public int curChildIndex = 0;

	//For the statistics
	public int timeVisited;
	public int reward;

	public MCTSNode(MCTSNode parent, Action action, GameStateUnit gsUnit) {
		this.parent = parent;
		this.action = action;
		this.gsUnit = gsUnit;
	}

}
