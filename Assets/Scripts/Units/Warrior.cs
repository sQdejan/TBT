using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit {

	public override void ShowPossibleMoves ()
	{
		int curUnitHeightInd = curTile.GetComponent<Tile>().HeightIndex;
		int curUnitWidthInd = curTile.GetComponent<Tile>().WidthIndex;

		if(curUnitHeightInd == GridController.Instance.gridHeight - 1) {
			moveDirection = Direction.UP;
		} else if (curUnitHeightInd == 0) {
			moveDirection = Direction.DOWN;
		}

		if(moveDirection == Direction.DOWN) {
			for(int i = curUnitHeightInd; i < GridController.Instance.gridHeight; i++) {
				for(int j = 0; j < GridController.Instance.gridWidth; j++) {
					if(!GridController.Instance.tileArray[i,j].occupied || GridController.Instance.tileArray[i,j].occupier.Equals(this.gameObject)) {
						if(Mathf.Abs(i - curUnitHeightInd) <= possibleMovesStraight && Mathf.Abs(j - curUnitWidthInd) <= possibleMovesStrafe) {
							GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[i,j].spriteTilePossibleMove;
							GridController.Instance.tileArray[i,j].available = true;
						}
					}
				}
			}
		} else {
			for(int i = curUnitHeightInd; i >= 0; i--) {
				for(int j = 0; j < GridController.Instance.gridWidth; j++) {
					if(!GridController.Instance.tileArray[i,j].occupied || GridController.Instance.tileArray[i,j].occupier.Equals(this.gameObject)) {
						if(Mathf.Abs(i - curUnitHeightInd) <= possibleMovesStraight && Mathf.Abs(j - curUnitWidthInd) <= possibleMovesStrafe) {
							GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[i,j].spriteTilePossibleMove;
							GridController.Instance.tileArray[i,j].available = true;
						}
					}
				}
			}
		}
	}
	
	public override void Attack (GameObject moveToObj, GameObject attackObj) {


		//Move the obj in order to attack, if moveToObj is not null
		//it means that the attack is done by the AI
		if(moveToObj)
			Move(moveToObj);
		else
			Move(attackMoveTile);

		attackObj.GetComponent<Unit>().TakeDamage(damage);
	}

	public override bool IsAttackPossible (GameObject obj) {
		
		//I have to check if any of the nearest tiles are 
		//available then an attack is possible
		GameObject closestTile = ClosestTile(obj);
		
		//I want to mark which tile will be moved to if an attack is possible
		if(closestTile) {
			if(attackMoveTile) {
				attackMoveTile.GetComponent<SpriteRenderer>().sprite = attackMoveTile.GetComponent<Tile>().spriteTilePossibleMove;
			}
			attackMoveTile = closestTile;
			attackMoveTile.GetComponent<SpriteRenderer>().sprite = attackMoveTile.GetComponent<Tile>().spriteAttackMove;
			return true;
		}
		
		return false;
	}

	GameObject ClosestTile(GameObject obj) {

		List<GameObject> listGameobject = AvailableTiles(obj);

		if(listGameobject.Count == 0) 
			return null;

		float shortestDistance = float.MaxValue;
		GameObject shortestDistanceObj = null;

		//Just looking at the distance to the mouse
		foreach(GameObject o in listGameobject) {
				
			float tmpValue = Vector2.Distance(o.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));

			if(tmpValue < shortestDistance) {
				shortestDistance = tmpValue;
				shortestDistanceObj = o;
			}
		}

		return shortestDistanceObj;
	}

	List<GameObject> AvailableTiles(GameObject obj) {

		List<GameObject> returnList = new List<GameObject>();
		
		Tile enemyUnitTile = obj.GetComponent<Unit>().curTile.GetComponent<Tile>();

		int tmpHeight = enemyUnitTile.HeightIndex;
		int tmpWidth = enemyUnitTile.WidthIndex;

		int checkDirection = 1;

		if(attackDirection == Direction.DOWN)
			checkDirection = -1;

		int y = tmpHeight + checkDirection;

		if(y >= 0 && y < GridController.Instance.gridHeight) {
			for(int i = -attackRange; i <= attackRange; i++) {
				int x = tmpWidth + i;
				if(x >= 0 && x < GridController.Instance.gridWidth) {
					if(GridController.Instance.tileArray[y,x].available) {
						returnList.Add(GridController.Instance.gridArray[y,x]);
					}
				}
			}
		}

		return returnList;
	}	

	protected override void ChangeDirection (GameObject nextTile) {
		if(nextTile.GetComponent<Tile>().HeightIndex == GridController.Instance.gridHeight - 1) {
			GetComponentInChildren<SpriteRenderer>().sprite = upSprite;
			spriteChild.transform.localPosition = spritePosUp;
			moveDirection = Direction.UP;
			attackDirection = Direction.UP;
		} else if (nextTile.GetComponent<Tile>().HeightIndex == 0) {
			GetComponentInChildren<SpriteRenderer>().sprite = downSprite;
			spriteChild.transform.localPosition = spritePosDown;
			moveDirection = Direction.DOWN;
			attackDirection = Direction.DOWN;
		}
	}
}
