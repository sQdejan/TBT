using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MCTSNode {

	public MCTSNode parent = null;
	
	public List<Vector3> gameStateEnemies = new List<Vector3>();
	public List<Vector3> gameStateUnits = new List<Vector3>();

	public List<MCTSNode> children = new List<MCTSNode>();

//	public MCTSNode (List<GameObject>) {
//
//	}

}
