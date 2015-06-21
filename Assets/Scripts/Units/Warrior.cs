using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit {

	public override bool IsAttackPossible (GameObject obj) {

		int attackRange = possibleMoves + 2;
		Tile thisUnitTile = CurTile.GetComponent<Tile>();
		Tile enemyUnitTile = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>();

		int thisWidthIndex = thisUnitTile.WidthIndex;
		int thisHeightIndex = thisUnitTile.HeightIndex;

		int enemyWidthIndex = enemyUnitTile.WidthIndex;
		int enemyHeightIndex = enemyUnitTile.HeightIndex;

		//I have to do 2.5 checks:
		//1. Standin next to it so unit can attack
		//2.1 If within walking distance so I can walk and attack
		//2.2 and also if any tile available around it
		if(Mathf.Abs(thisWidthIndex - enemyWidthIndex) <= 1 && Mathf.Abs(thisHeightIndex - enemyHeightIndex) <= 1) {
			return true;
		}

		GameObject closestTile = ClosestTile(obj, thisWidthIndex, thisHeightIndex);

		//I want to mark which tile will be moved to if an attack is possible
		if((Mathf.Abs(enemyWidthIndex - (thisWidthIndex + thisHeightIndex - enemyHeightIndex)) <= attackRange && Mathf.Abs(enemyHeightIndex - (thisHeightIndex - thisWidthIndex + enemyWidthIndex)) <= attackRange) && closestTile) {
			attackMoveTile = closestTile;
			attackMoveTile.GetComponent<SpriteRenderer>().sprite = attackMoveTile.GetComponent<Tile>().spriteMoveTo;
			return true;
		}

		return false;
	}

	protected override void Attack (GameObject obj) {

		Tile thisUnitTile = CurTile.GetComponent<Tile>();
		Tile enemyUnitTile = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>();
		
		int thisWidthIndex = thisUnitTile.WidthIndex;
		int thisHeightIndex = thisUnitTile.HeightIndex;
		
		int enemyWidthIndex = enemyUnitTile.WidthIndex;
		int enemyHeightIndex = enemyUnitTile.HeightIndex;

		//First I have to check if I am within range to attack, for warrior the range is one
		if(Mathf.Abs(thisWidthIndex - enemyWidthIndex) <= 1 && Mathf.Abs(thisHeightIndex - enemyHeightIndex) <= 1) {
			obj.GetComponent<Unit>().TakeDamage(damage);
			return;
		} 

		//Else I have to move the unit in order to attack.
		//To do that, find the closest possible tile to move to
		Move (ClosestTile(obj, thisWidthIndex, thisHeightIndex));
	}

	public override void TakeDamage (int damage) {
		health -= damage;
	}

	GameObject ClosestTile(GameObject obj, int widthIndex, int heightIndex) {

		List<GameObject> listGameobject = AvailableTiles(obj);

		if(listGameobject.Count == 0) 
			return null;

		int shortestDistance = int.MaxValue;
		GameObject shortestDistanceObj = null;
		
		foreach(GameObject o in listGameobject) {
			int oWidth = o.GetComponent<Tile>().WidthIndex;
			int oHeight = o.GetComponent<Tile>().HeightIndex;
			
			int tmpValue = Mathf.Abs(widthIndex - oWidth) + Mathf.Abs(heightIndex - oHeight);
			
			if(tmpValue < shortestDistance) {
				shortestDistance = tmpValue;
				shortestDistanceObj = o;
			}
		}

		return shortestDistanceObj;
	}

	List<GameObject> AvailableTiles(GameObject obj) {
		
		List<GameObject> returnList = new List<GameObject>();
		
		Tile enemyUnitTile = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>();
		
		int enemyWidthIndex = enemyUnitTile.WidthIndex;
		int enemyHeightIndex = enemyUnitTile.HeightIndex;

		for(int i = -1; i <= 1; i++) {
			for(int j = -1; j <= 1; j++) {
				int x = enemyWidthIndex + i;
				if(x < 0 || x >= GridController.Instance.gridWidth)
					continue;
				
				int y = enemyHeightIndex + j;
				if(y < 0 || y >= GridController.Instance.gridHeight)
					continue;
				
				if(GridController.Instance.gridArray[x,y].GetComponent<Tile>().available) 
					returnList.Add(GridController.Instance.gridArray[x,y]);
			}
		}
		
		return returnList;
	}
}
