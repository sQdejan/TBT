using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warrior : Unit {

	public override bool IsAttackPossible (GameObject obj) {

		int attackRange = possibleMoves + 1;
		Tile thisUnitTile = CurTile.GetComponent<Tile>();
		Tile enemyUnitTile = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>();

		int thisWidthIndex = thisUnitTile.WidthIndex;
		int thisHeightIndex = thisUnitTile.HeightIndex;

		int enemyWidthIndex = enemyUnitTile.WidthIndex;
		int enemyHeightIndex = enemyUnitTile.HeightIndex;

		if(Mathf.Abs(enemyWidthIndex - (thisWidthIndex + thisHeightIndex - enemyHeightIndex)) <= attackRange && Mathf.Abs(enemyHeightIndex - (thisHeightIndex - thisWidthIndex + enemyWidthIndex)) <= attackRange) {
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


	}

	public override void TakeDamage (int damage) {
		health -= damage;
	}

	private List<GameObject> AvailableTiles(GameObject obj) {

		Tile enemyUnitTile = obj.GetComponent<Unit>().CurTile.GetComponent<Tile>();
		
		int enemyWidthIndex = enemyUnitTile.WidthIndex;
		int enemyHeightIndex = enemyUnitTile.HeightIndex;

		//I know that there is a max of eight neighbours because of square setup
		for(int i = 0; i < 8; i++) {

		}

		return null;
	}
}
