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
	public float reward;

	public MCTSNode(MCTSNode parent, Action action, int mbagsH, int mbagsW, int gsH, int gsW) {
		this.parent = parent;
		this.action = action;
		this.mbagsH = mbagsH;
		this.mbagsW = mbagsW;
		this.gsH = gsH;
		this.gsW = gsW;
	}

	public bool Equals(MCTSNode node) {
		if(node == null) {
			Debug.LogError("obj is null");
			return false;
		}

		return (action == node.action) && (mbagsH == node.mbagsH) && (mbagsW == node.mbagsW) && (gsH == node.gsH) && (gsW == node.gsW);
	}
}
