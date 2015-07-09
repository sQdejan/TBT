using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Action{ATTACK, MOVE};

public class MCTSNode {

	public MCTSNode parent = null;

	//The action it takes to get to THIS node
	public Action action;

	//Move before attack gs unit, move to this one if not null before attacking
	public int mbagsH;
	public int mbagsW;

	//Where in the array will action happen?
	public int gsH;
	public int gsW;

	//To traverse the tree and also see if a node is fully expanded
	public List<MCTSNode> children;
	public int curChildIndex = -1;

	//For the statistics
	public int timeVisited;
	public int reward;

	public MCTSNode(MCTSNode parent, Action action, int mbagsH, int mbagsW, int gsH, int gsW) {
		this.parent = parent;
		this.action = action;
		this.mbagsH = mbagsH;
		this.mbagsW = mbagsW;
		this.gsH = gsH;
		this.gsW = gsW;
	}

}
