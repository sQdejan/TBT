using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BTRanger : BehaviourTree {

	public override void TakeMove () {
		poss = new List<MCTSNode>();
		
		//possible moves
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				if(GridController.Instance.tileArray[i,j].available) { 
					poss.Add(new MCTSNode(null, Action.MOVE, 1, 1, i, j));
				}
			}
		}
		
		//possible attacks
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				if(GridController.Instance.tileArray[i,j].occupied && GridController.Instance.tileArray[i,j].occupier.tag == "PlayerUnit") { 
					unit.AttacksForAutomation(poss, GridController.Instance.tileArray[i,j].occupier);
				}
			}
		}

		move = poss[Random.Range(0, poss.Count)]; 

		StartCoroutine(EndMove());
	}
}
