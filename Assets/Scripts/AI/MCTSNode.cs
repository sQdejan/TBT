using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MCTSNode {

	public MCTSNode parent = null;

	public GameObject unitUsed;
	public GameObject moveAction;
	public GameObject enemyAttacked;

	public int currentDepth;

	public List<MCTSNode> children = new List<MCTSNode>();

	public MCTSNode(MCTSNode parent, GameObject unitUsed, GameObject moveAction, GameObject enemyAttacked, int currentDepth) {
		this.parent = parent;
		this.unitUsed = unitUsed;
		this.moveAction = moveAction;
		this.enemyAttacked = enemyAttacked;
		this.currentDepth = currentDepth;
	}

}
