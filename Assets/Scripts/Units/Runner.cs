using UnityEngine;
using System.Collections;

public class Runner : Unit {

	public override void ShowPossibleMoves () {

		int y = curTile.GetComponent<Tile>().HeightIndex;
		int x = curTile.GetComponent<Tile>().WidthIndex;

		//Just mark the tile under the unit
		GridController.Instance.gridArray[y,x].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[y,x].spriteTilePossibleMove;
		GridController.Instance.tileArray[y,x].available = true;

		//Check up-left
		CheckPossibleDirection(y, x, -1, -1);

		//Check up-right
		CheckPossibleDirection(y, x, -1, 1);

		//Check down-left
		CheckPossibleDirection(y, x, 1, -1);

		//Check down-right
		CheckPossibleDirection(y, x, 1, 1);

	}

	void CheckPossibleDirection(int y, int x, int yDirection, int xDirection) {

		y += yDirection;
		x += xDirection;

		while(y >= 0 && y < GridController.Instance.gridHeight && x >= 0 && x < GridController.Instance.gridWidth) {

			if(!GridController.Instance.tileArray[y,x].occupied) {
				GridController.Instance.gridArray[y,x].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[y,x].spriteTilePossibleMove;
				GridController.Instance.tileArray[y,x].available = true;
			} else {
				break;
			}

			y += yDirection;
			x += xDirection;
		}
	}

	public override void Attack (GameObject moveToObj, GameObject attackObj) {
		throw new System.NotImplementedException ();
	}

	public override bool IsAttackPossible (GameObject obj) {
		throw new System.NotImplementedException ();
	}

	protected override void ChangeDirection (GameObject nextTile) {
	}
}
