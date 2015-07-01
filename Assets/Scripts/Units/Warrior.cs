using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : PlayableCharacter {

	public override bool IsAttackPossible (GameObject obj) {

		//I have to do two checks:
		//1. If already within range then it is possible
		//2. If any of the nearest tiles are available then an attack is possible

		//1.
		int enemyHeight = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>().HeightIndex;
		int enemyWidth = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>().WidthIndex;

		int thisHeight = curTile.GetComponent<Tile>().HeightIndex;
		int thisWidth = curTile.GetComponent<Tile>().WidthIndex;

		if(Mathf.Abs(enemyHeight - thisHeight) <= attackRange && Mathf.Abs(enemyWidth - thisWidth) <= attackRange) {
			return true;
		}

		//2.
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

	protected override void Attack (GameObject obj) {

		//Am I within attack range?
		int enemyHeight = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>().HeightIndex;
		int enemyWidth = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>().WidthIndex;
		
		int thisHeight = curTile.GetComponent<Tile>().HeightIndex;
		int thisWidth = curTile.GetComponent<Tile>().WidthIndex;

		if(Mathf.Abs(enemyHeight - thisHeight) <= attackRange && Mathf.Abs(enemyWidth - thisWidth) <= attackRange) {

			if(GameFlow.Instance.resourcesLeft - resourcesForAttack >= 0) {
				GameFlow.Instance.resourcesLeft -= resourcesForAttack;
				GameFlow.Instance.UpdateResourceText();
				obj.GetComponent<Unit>().TakeDamage(damage);
			} else {
				Debug.Log("Should give the player a warning");
			}

			return;
		} 

		//Else I have to move the unit in order to attack.
		if(GameFlow.Instance.resourcesLeft - resourcesForAttack - resourcesForMove >= 0) {
			GameFlow.Instance.resourcesLeft -= resourcesForAttack;
			Move (attackMoveTile);
			obj.GetComponent<Unit>().TakeDamage(damage);
		} else {
			Debug.Log("Should give the player a warning");
		}
	}

	public override void TakeDamage (int damage) {
		health -= damage;

		Debug.Log("Took damage, auch!");
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

		int tmpHeight = enemyUnitTile.HeightIndex;
		int tmpWidth = enemyUnitTile.WidthIndex;

		for(int i = -attackRange; i <= attackRange; i++) {
			int y = tmpHeight + i;

			if(y >= 0 && y < GridController.Instance.gridHeight) {
				for(int j = -attackRange; j <= attackRange; j++) {
					int x = tmpWidth + j;

					if(x >= 0 && x < GridController.Instance.gridWidth) {
						if(GridController.Instance.tileArray[y,x].available) {
							returnList.Add(GridController.Instance.gridArray[y,x]);
						}
					}
				}
			}
		}

		return returnList;
	}	
}
