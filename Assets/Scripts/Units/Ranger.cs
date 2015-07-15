using UnityEngine;
using System.Collections;

public class Ranger : Unit {

	public override void Attack (GameObject moveToObj, GameObject attackObj)
	{
		attackObj.GetComponent<Unit>().TakeDamage(damage);
	}

	//For this attack I check horizontally/diagonally if the enemy is in line of sight
	//and also if an "unit" is standing between this unit and the enemy. If so, it can't shoot
	//through that target.
	public override bool IsAttackPossible (GameObject obj)
	{
		int enemyHeight = obj.GetComponent<Unit>().curTile.GetComponent<Tile>().HeightIndex;
		int enemyWidth = obj.GetComponent<Unit>().curTile.GetComponent<Tile>().WidthIndex;

		int thisHeight = curTile.GetComponent<Tile>().HeightIndex;
		int thisWidth = curTile.GetComponent<Tile>().WidthIndex;

		//If thisWidth == enemyWidth we go up/down to check
		if(thisWidth == enemyWidth) {
			int y = thisHeight;
			int i = 1;

			//Check if the enemy is below or above
			if(thisHeight - enemyHeight > 0) 
				i *= -1;

			while(y > 0 || y < GridController.Instance.gridHeight - 1) {
				y += i;

				if(GridController.Instance.tileArray[y, thisWidth].occupied && GridController.Instance.tileArray[y, thisWidth].occupier.Equals(obj))
					return true;
				else if (GridController.Instance.tileArray[y, thisWidth].occupied)
					return false;
			}
		}

		//If thisHeight == enemyHeight we go left/right to check
		if(thisHeight == enemyHeight) {
			int x = thisWidth;
			int i = 1;
			
			//Check if the enemy is below or above
			if(thisWidth - enemyWidth > 0) 
				i *= -1;
			
			while(x > 0 || x < GridController.Instance.gridWidth - 1) {
				x += i;
				
				if(GridController.Instance.tileArray[thisHeight, x].occupier.Equals(obj))
					return true;
				else if (GridController.Instance.tileArray[thisHeight, x].occupied)
					return false;
			}
		}

		return false;
	}



}
