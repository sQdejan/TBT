using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit {

	public override bool IsAttackPossible (GameObject obj) {

		//I have to do two checks:
		//1. If already within range then it is possible
		//2. If any of the nearest tiles are available then an attack is possible
		if(Vector3.Distance(obj.transform.position, transform.position) < 1.71f) {
			return true;
		}

		GameObject closestTile = ClosestTile(obj);

		//I want to mark which tile will be moved to if an attack is possible
		if(closestTile) {
			if(attackMoveTile) {
				attackMoveTile.GetComponent<SpriteRenderer>().sprite = attackMoveTile.GetComponent<Tile>().spriteTileMove;
			}
			attackMoveTile = closestTile;
			attackMoveTile.GetComponent<SpriteRenderer>().sprite = attackMoveTile.GetComponent<Tile>().spriteMoveTo;
			return true;
		}

		return false;
	}

	protected override void Attack (GameObject obj) {

		//First I have to check if I am within range to attack, for warrior the range is one
		if(Vector3.Distance(obj.transform.position, transform.position) < 1.71f) {
			obj.GetComponent<Unit>().TakeDamage(damage);
			return;
		} 

		//Else I have to move the unit in order to attack.
		//To do that, find the closest possible tile to move to
		Move (attackMoveTile);
	}

	public override void TakeDamage (int damage) {
		health -= damage;
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
		
		Tile enemyUnitTile = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>();

		for(int i = 0; i < enemyUnitTile.NeighboursArray.Length; i++) {
			if(enemyUnitTile.NeighboursArray[i]) {

				Tile tmpTile = enemyUnitTile.NeighboursArray[i].GetComponent<Tile>();

				if(tmpTile.available) {
					returnList.Add(enemyUnitTile.NeighboursArray[i]);
				}

				if(i % 2 == 0) {
					if(tmpTile.neighbourUpRight) 
						if(tmpTile.neighbourUpRight.GetComponent<Tile>().available) 
							returnList.Add(tmpTile.neighbourUpRight);

					if(tmpTile.neighbourDownLeft) 
						if(tmpTile.neighbourDownLeft.GetComponent<Tile>().available) 
							returnList.Add(tmpTile.neighbourDownLeft);
				} else {
					if(tmpTile.neighbourUpLeft) 
						if(tmpTile.neighbourUpLeft.GetComponent<Tile>().available)
							if(!returnList.Contains(tmpTile.neighbourUpLeft))
								returnList.Add(tmpTile.neighbourUpLeft);
					
					if(tmpTile.neighbourDownRight) 
						if(tmpTile.neighbourDownRight.GetComponent<Tile>().available)
							if(!returnList.Contains(tmpTile.neighbourDownRight))
								returnList.Add(tmpTile.neighbourDownRight);
				}
			}
		}

		return returnList;
	}	
}
