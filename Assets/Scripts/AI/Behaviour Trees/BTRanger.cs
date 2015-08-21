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

		if(!CanIAttack())
			Move();

		StartCoroutine(EndMove());
	}

	#region Attack part of tree

	bool CanIAttack() {
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

	#region Move part of tree

	void Move() {
		if(!AnyTilesFreeOfAttack()) 
			if(!CanIAttackFromAnyTile(null))
				MoveDefensive(null);
	}


	/// <summary>
	/// Sequencer/Selector for taking choices
	/// </summary>
	bool AnyTilesFreeOfAttack() {
		List<int> indexes = new List<int>();
		
		for(int i = 0; i < poss.Count; i++) {
			if(IsTileHomeFree(poss[i].gsH, poss[i].gsW)) 
				indexes.Add(i);
		}
			
		if(indexes.Count == 0)
			return false;

		if(!CanIAttackFromAnyTile(indexes))
			MoveDefensive(indexes);

		return true;
	}

	/// <summary>
	/// Check if tile is free for any attacks
	/// </summary>
	bool IsTileHomeFree(int h, int w) {
		
		int gridH = GridController.Instance.gridHeight;
		int gridW = GridController.Instance.gridWidth;

		//Check for ranged attackers
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

		//Check for melee attackers
		for(int i = -3; i <= 3; i++) {
			for(int j = -2; j <= 2; j++) {
				int x = h + i;
				int y = w + j;

				if(x >= 0 && x < gridH && y >= 0 && y < gridW) {
					if(GridController.Instance.tileArray[x,y].occupied && GridController.Instance.tileArray[x,y].occupier.tag == "PlayerUnit") {
						if(GridController.Instance.tileArray[x,y].occupier.GetComponent<Unit>().classType == ClassType.WARRIOR)
							return false;
					}
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Sequencer/Selector for taking choices
	/// </summary>
	bool CanIAttackFromAnyTile(List<int> indexes) {

		List<int> tmpList = CheckForEnemiesForAttack(indexes);
		
		if(tmpList.Count == 0)
			return false;

		//Do it then
		move = poss[tmpList[Random.Range(0, tmpList.Count)]];

		return true;
	}

	/// <summary>
	/// In this function I check if it is possible to attack an enemy from any of the positions.
	/// Considering both the case where I found free tiles and where I didn't.
	/// </summary>
	List<int> CheckForEnemiesForAttack(List<int> indexes) {
		
		List<int> rList = new List<int>();
		
		int gridH = GridController.Instance.gridHeight;
		int gridW = GridController.Instance.gridWidth;

		if(indexes != null) {
			for(int k = 0; k < indexes.Count; k++) {

				int h = poss[indexes[k]].gsH;
				int w = poss[indexes[k]].gsW;
				bool didAdd = false;

				for(int i = -1; i <= 1; i += 2) {
					int hDir = h + i;
					
					//Up/down
					while(hDir >= 0 && hDir < gridH) {
						if(GridController.Instance.tileArray[hDir, w].occupied) {
							if(GridController.Instance.tileArray[hDir, w].occupier.tag == "PlayerUnit") {
								rList.Add(indexes[k]);
								didAdd = true;
								break;
							} else {
								break;
							}
						} 
						
						hDir += i;
					}

					if(didAdd)
						break;

					int wDir = w + i;
					
					//Left/right
					while(wDir >= 0 && wDir < gridW) {
						if(GridController.Instance.tileArray[h, wDir].occupied) {
							if(GridController.Instance.tileArray[h, wDir].occupier.tag == "PlayerUnit") {
								rList.Add(indexes[k]);
								break;
							} else {
								break;
							}
						} 
						
						wDir += i;
					}
				}
			}
		} else {
			for(int k = 0; k < poss.Count; k++) {
				
				int h = poss[k].gsH;
				int w = poss[k].gsW;
				bool didAdd = false;
				
				for(int i = -1; i <= 1; i += 2) {
					int hDir = h + i;
					
					//Up/down
					while(hDir >= 0 && hDir < gridH) {
						if(GridController.Instance.tileArray[hDir, w].occupied && GridController.Instance.tileArray[hDir, w].occupier.tag == "PlayerUnit") {
							rList.Add(k);
							didAdd = true;
							break;
						}
						
						hDir += i;
					}
					
					if(didAdd)
						break;
					
					int wDir = w + i;
					
					//Left/right
					while(wDir >= 0 && wDir < gridW) {
						if(GridController.Instance.tileArray[h, wDir].occupied && GridController.Instance.tileArray[h, wDir].occupier.tag == "PlayerUnit") {
							rList.Add(k);
							break;
						}
						
						wDir += i;
					}
				}
			}
		}
		
		return rList;
	}

	/// <summary>
	/// Find an position farthest away from the closest enemy
	/// </summary>
	void MoveDefensive(List<int> indexes) {

		int h = unit.curTile.GetComponent<Tile>().HeightIndex;
		int w = unit.curTile.GetComponent<Tile>().WidthIndex;

		int gridH = GridController.Instance.gridHeight;
		int gridW = GridController.Instance.gridWidth;

		Points2D enePoint = null;

		int checkRadius = 1;
		bool foundEnemy = false;
		float longestDistance = float.MinValue;

		if(indexes != null) {

			//find closest enemy
			while(!foundEnemy) {
				for(int i = -checkRadius; i <= checkRadius; i++) {
					for(int j = -checkRadius; j <= checkRadius; j++) {
						int x = h + i;
						int y = w + j;

						if(x >= 0 && x < gridH && y >= 0 && y < gridW) {
							if(GridController.Instance.tileArray[x,y].occupied && GridController.Instance.tileArray[x,y].occupier.tag == "PlayerUnit") {
								enePoint = new Points2D(x,y);
								foundEnemy = true;
								break;
							}
						}
					}

					if(foundEnemy)
						break;
				}

				checkRadius++;
			}

			//Find pos farthest away from that enemy
			for(int i = 0; i < indexes.Count; i++) {
				float tmpDistance = Distance(poss[indexes[i]].gsH, poss[indexes[i]].gsW, enePoint);

				if(tmpDistance > longestDistance) {
					longestDistance = tmpDistance;
					move = poss[indexes[i]];
				}
			}
		} else {
			while(!foundEnemy) {
				for(int i = -checkRadius; i <= checkRadius; i++) {
					for(int j = -checkRadius; j <= checkRadius; j++) {
						int x = h + i;
						int y = w + j;
						
						if(x >= 0 && x < gridH && y >= 0 && y < gridW) {
							if(GridController.Instance.tileArray[x,y].occupied && GridController.Instance.tileArray[x,y].occupier.tag == "PlayerUnit") {
								enePoint = new Points2D(x,y);
								foundEnemy = true;
								break;
							}
						}
					}
					
					if(foundEnemy)
						break;
				}

				checkRadius++;
			}
			
			for(int i = 0; i < poss.Count; i++) {
				float tmpDistance = Distance(poss[i].gsH, poss[i].gsW, enePoint);
				
				if(tmpDistance > longestDistance) {
					longestDistance = tmpDistance;
					move = poss[i];
				}
			}
		}
	}

	#endregion
}
