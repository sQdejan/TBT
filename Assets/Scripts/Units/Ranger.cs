using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ranger : Unit {

	public override void ShowPossibleMoves () {
		int curUnitHeightInd = curTile.GetComponent<Tile>().HeightIndex;
		int curUnitWidthInd = curTile.GetComponent<Tile>().WidthIndex;
		
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				if(!GridController.Instance.tileArray[i,j].occupied || GridController.Instance.tileArray[i,j].occupier.Equals(this.gameObject)) {
					if(Mathf.Abs(i - curUnitHeightInd) <= possibleMovesStraight && Mathf.Abs(j - curUnitWidthInd) <= possibleMovesStrafe) {
						GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[i,j].spriteTilePossibleMove;
						GridController.Instance.tileArray[i,j].available = true;
					}
				}
			}
		}

//		int y = curUnitHeightInd - possibleMovesStraight;
//
//		while(y <= curUnitHeightInd + possibleMovesStraight) {
//			if(y >= 0 && y < GridController.Instance.gridHeight) {
//				if(!GridController.Instance.tileArray[y,curUnitWidthInd].occupied || GridController.Instance.tileArray[y,curUnitWidthInd].occupier.Equals(this.gameObject)) {
//					GridController.Instance.gridArray[y,curUnitWidthInd].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[y,curUnitWidthInd].spriteTilePossibleMove;
//					GridController.Instance.tileArray[y,curUnitWidthInd].available = true;
//				}
//			}
//
//			y++;
//		}
//
//		int x = curUnitWidthInd - possibleMovesStrafe;
//
//		while(x <= curUnitWidthInd + possibleMovesStraight) {
//			if(x >= 0 && x < GridController.Instance.gridWidth) {
//				if(!GridController.Instance.tileArray[curUnitHeightInd,x].occupied) {
//					GridController.Instance.gridArray[curUnitHeightInd,x].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[curUnitHeightInd,x].spriteTilePossibleMove;
//					GridController.Instance.tileArray[curUnitHeightInd,x].available = true;
//				}
//			}
//			
//			x++;
//		}
	}

	public override void Attack (GameObject moveToObj, GameObject attackObj) {
		attackObj.GetComponent<Unit>().TakeDamage(damage);
	}

	//For this attack I check horizontally/vertically if the enemy is in line of sight
	//and also if an "unit" is standing between this unit and the enemy. If so, it can't shoot
	//through that target.
	public override bool IsAttackPossible (GameObject obj) {
		int enemyHeight = obj.GetComponent<Unit>().curTile.GetComponent<Tile>().HeightIndex;
		int enemyWidth = obj.GetComponent<Unit>().curTile.GetComponent<Tile>().WidthIndex;

		int thisHeight = curTile.GetComponent<Tile>().HeightIndex;
		int thisWidth = curTile.GetComponent<Tile>().WidthIndex;

		//If thisWidth == enemyWidth we go up/down to check
		if(thisWidth == enemyWidth) {
			int y = thisHeight;

			//Go down
			if(thisHeight - enemyHeight > 0) 
				while(--y >= 0 && y < GridController.Instance.gridHeight) {
					if(GridController.Instance.tileArray[y,thisWidth].occupied && GridController.Instance.tileArray[y,thisWidth].occupier.Equals(obj))
						return true;
					else if (GridController.Instance.tileArray[y, thisWidth].occupied)
						return false;
				}
			else //Go up
				while(++y >= 0 && y < GridController.Instance.gridHeight) {
					if(GridController.Instance.tileArray[y,thisWidth].occupied && GridController.Instance.tileArray[y,thisWidth].occupier.Equals(obj))
						return true;
					else if (GridController.Instance.tileArray[y, thisWidth].occupied)
						return false;
				}
		}

		//If thisHeight == enemyHeight we go left/right to check
		if(thisHeight == enemyHeight) {
			int x = thisWidth;

			//Go left
			if(thisWidth - enemyWidth >= 0) 
				while(--x >= 0 && x < GridController.Instance.gridWidth) {
					if(GridController.Instance.tileArray[thisHeight,x].occupied && GridController.Instance.tileArray[thisHeight,x].occupier.Equals(obj))
						return true;
					else if (GridController.Instance.tileArray[thisHeight,x].occupied)
						return false;
				}
			else //Go right
				while(++x >= 0 && x < GridController.Instance.gridWidth) {
					if(GridController.Instance.tileArray[thisHeight,x].occupied && GridController.Instance.tileArray[thisHeight,x].occupier.Equals(obj))
						return true;
					else if (GridController.Instance.tileArray[thisHeight,x].occupied)
						return false;
				}
		}

		return false;
	}

	protected override void ChangeDirection (GameObject nextTile) {
		if(nextTile.GetComponent<Tile>().HeightIndex < curTile.GetComponent<Tile>().HeightIndex) {
			GetComponentInChildren<SpriteRenderer>().sprite = upSprite;
			spriteChild.transform.localPosition = spritePosUp;
		} else if (nextTile.GetComponent<Tile>().HeightIndex > curTile.GetComponent<Tile>().HeightIndex) {
			GetComponentInChildren<SpriteRenderer>().sprite = downSprite;
			spriteChild.transform.localPosition = spritePosDown;
		}
	}

	//------------------------------- For testing ------------------------------

	public override void AttacksForAutomation (List<MCTSNode> list, GameObject ene) {

//		List<MCTSNode> tmpList = new List<MCTSNode>();

		int enemyHeight = ene.GetComponent<Unit>().curTile.GetComponent<Tile>().HeightIndex;
		int enemyWidth = ene.GetComponent<Unit>().curTile.GetComponent<Tile>().WidthIndex;
		
		int thisHeight = curTile.GetComponent<Tile>().HeightIndex;
		int thisWidth = curTile.GetComponent<Tile>().WidthIndex;
		
		//If thisWidth == enemyWidth we go up/down to check
		if(thisWidth == enemyWidth) {
			int y = thisHeight;
			
			if(thisHeight - enemyHeight > 0) {
				while(--y >= 0 && y < GridController.Instance.gridHeight) {
					if(GridController.Instance.tileArray[y,thisWidth].occupied && GridController.Instance.tileArray[y,thisWidth].occupier.Equals(ene))
						list.Add(new MCTSNode(null, Action.ATTACK, 1, 1, y, thisWidth));
					else if (GridController.Instance.tileArray[y, thisWidth].occupied)
						break;
				}
			} else {
				while(++y >= 0 && y < GridController.Instance.gridHeight) {
					if(GridController.Instance.tileArray[y,thisWidth].occupied && GridController.Instance.tileArray[y,thisWidth].occupier.Equals(ene))
						list.Add(new MCTSNode(null, Action.ATTACK, 1, 1, y, thisWidth));
					else if (GridController.Instance.tileArray[y, thisWidth].occupied)
						break;
				}
			}

		}
		
		//If thisHeight == enemyHeight we go left/right to check
		if(thisHeight == enemyHeight) {
			int x = thisWidth;
			
			if(thisWidth - enemyWidth > 0) {
				while(--x >= 0 && x < GridController.Instance.gridWidth) {
					if(GridController.Instance.tileArray[thisHeight,x].occupied && GridController.Instance.tileArray[thisHeight,x].occupier.Equals(ene))
						list.Add(new MCTSNode(null, Action.ATTACK, 1, 1, thisHeight, x));
					else if (GridController.Instance.tileArray[thisHeight,x].occupied)
						break;
				}
			} else {
				while(++x >= 0 && x < GridController.Instance.gridWidth) {
					if(GridController.Instance.tileArray[thisHeight,x].occupied && GridController.Instance.tileArray[thisHeight,x].occupier.Equals(ene))
						list.Add(new MCTSNode(null, Action.ATTACK, 1, 1, thisHeight, x));
					else if (GridController.Instance.tileArray[thisHeight,x].occupied)
						break;
				}
			}
		}

//		if(tmpList.Count > 0) {
//			list.Clear();
//			foreach(MCTSNode node in tmpList) {
//				list.Add(node);
//			}
//		}
	}

	//---------------------------- Testing end ------------------
}
