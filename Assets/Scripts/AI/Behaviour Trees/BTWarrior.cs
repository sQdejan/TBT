using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BTWarrior : BehaviourTree {

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

		if(!CanIAttack()) 
			MoveAggresive();

		StartCoroutine(EndMove());
	}

	#region Attack, first part of tree

	bool CanIAttack () {

		if(AnyTargets()) {
			AttackLowestTarget();
			return true;
		}

		return false;
	}

	bool AnyTargets() {
		for(int i = 0; i < poss.Count; i++) {
			if(poss[i].action == Action.ATTACK)
				return true;
		}

		return false;
	}

	void AttackLowestTarget() {
		int lowestHP = int.MaxValue;

		//Find lowest HP target if any
		for(int i = 0; i < poss.Count; i++) {
			if(poss[i].action == Action.ATTACK) {
				int tmpHP = GridController.Instance.tileArray[poss[i].gsH, poss[i].gsW].occupier.GetComponent<Unit>().health;

				if(tmpHP < lowestHP) {
					lowestHP = tmpHP;
				}
			}
		}

		//Find equal fuckers for break-even scenarios
		List<int> indexes = new List<int>();
		for(int i = 0; i < poss.Count; i++) {
			if(poss[i].action == Action.ATTACK) {
				int tmpHP = GridController.Instance.tileArray[poss[i].gsH, poss[i].gsW].occupier.GetComponent<Unit>().health;
				
				if(tmpHP == lowestHP) {
					indexes.Add(i);
				}
			}
		}

		move = poss[indexes[Random.Range(0, indexes.Count)]];
	}


	#endregion

	#region Aggressive move, last part of tree

	void MoveAggresive() {

		if(!AnyTilesAvoidingRangedAttack())
			if(!AnyAggressiveTile(null))
				TileFarthestAway(null);

	}

	bool AnyTilesAvoidingRangedAttack() {
		List<int> indexes = new List<int>();

		for(int i = 0; i < poss.Count; i++) {
			if(IsTileHomeFree(poss[i].gsH, poss[i].gsW))
				indexes.Add(i);
		}

		if(indexes.Count == 0)
			return false;

		if(!AnyAggressiveTile(indexes)) 
			TileFarthestAway(indexes);

		return true;
	}

	bool IsTileHomeFree(int h, int w) {

		int gridH = GridController.Instance.gridHeight;
		int gridW = GridController.Instance.gridWidth;

		for(int i = -1; i <= 1; i += 2) {
			int hDir = h + i;

			//Up/down
			while(hDir >= 0 && hDir < gridH) {
				if(GridController.Instance.tileArray[hDir, w].occupied) {
					if(GridController.Instance.tileArray[hDir, w].occupier.tag == "PlayerUnit" && GridController.Instance.tileArray[hDir, w].occupier.GetComponent<Unit>().classType == ClassType.RANGED)
						return false;
					else
						break;
				} 

				hDir += i;
			}

			int wDir = w + i;

			//Left/right
			while(wDir >= 0 && wDir < gridW) {
				if(GridController.Instance.tileArray[h, wDir].occupied) {
					if(GridController.Instance.tileArray[h, wDir].occupier.tag == "PlayerUnit" && GridController.Instance.tileArray[h, wDir].occupier.GetComponent<Unit>().classType == ClassType.RANGED)
						return false;
					else
						break;
				} 
				
				wDir += i;
			}
		}

		return true;
	}

	bool AnyAggressiveTile(List<int> indexes) {

		float shortestDistance = float.MaxValue;

		List<Points2D> tmpList = CheckForEnemies();

		if(tmpList.Count == 0)
			return false;

		if(indexes != null) {
			for(int i = 0; i < indexes.Count; i++) {
				for(int j = 0; j < tmpList.Count; j++) {
					float tmpDistance = Distance(poss[indexes[i]].gsH, poss[indexes[i]].gsW, tmpList[j]);
					if(tmpDistance < shortestDistance) {
						shortestDistance = tmpDistance;
						move = poss[indexes[i]];
					}
				}
			}
		} else {
			for(int i = 0; i < poss.Count; i++) {
				for(int j = 0; j < tmpList.Count; j++) {
					float tmpDistance = Distance(poss[i].gsH, poss[i].gsW, tmpList[j]);
					if(tmpDistance < shortestDistance) {
						shortestDistance = tmpDistance;
						move = poss[i];
					}
				}
			}
		}
	
		return true;
	}

	List<Points2D> CheckForEnemies() {

		List<Points2D> rList = new List<Points2D>();

		int gridH = GridController.Instance.gridHeight;
		int gridW = GridController.Instance.gridWidth;

		int checkDirection = -1;
		if(unit.moveDirection == Direction.DOWN)
			checkDirection = 1;
		
		int checkIndex = unit.curTile.GetComponent<Tile>().HeightIndex + checkDirection;

		for(int i = checkIndex; i >= 0 && i < gridH; i += checkDirection) {
			for(int j = 0; j < gridW; j++) {
				if(GridController.Instance.tileArray[i,j].occupied && GridController.Instance.tileArray[i,j].occupier.tag == "PlayerUnit") {
					rList.Add(new Points2D(i,j));
				}
			}
		}

		return rList;
	}

	void TileFarthestAway(List<int> indexes) {

		float longestDistance = float.MinValue;
		Tile tmpTile = unit.curTile.GetComponent<Tile>();

		if(indexes != null) {
			for(int i = 0; i < indexes.Count; i++) {
				float tmpDistance = Distance(poss[indexes[i]].gsH, poss[indexes[i]].gsW, new Points2D(tmpTile.HeightIndex, tmpTile.WidthIndex));
				if(tmpDistance > longestDistance) {
					longestDistance = tmpDistance;
					move = poss[indexes[i]];
				}
			}
		} else {
			for(int i = 0; i < poss.Count; i++) {
				float tmpDistance = Distance(poss[i].gsH, poss[i].gsW, new Points2D(tmpTile.HeightIndex, tmpTile.WidthIndex));
				if(tmpDistance > longestDistance) {
					longestDistance = tmpDistance;
					move = poss[i];
				}
			}
		}
	}

	#endregion
}
