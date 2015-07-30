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
		if(moveToObj)
			Move(moveToObj);
		else
			Move(attackMoveTile);
		
		attackObj.GetComponent<Unit>().TakeDamage(damage);;
	}

	public override bool IsAttackPossible (GameObject obj) {
		int y = 0, x = 0;

		Tile thisTile = curTile.GetComponent<Tile>();
		Tile eneTile = obj.GetComponent<Unit>().curTile.GetComponent<Tile>();

		if(thisTile.HeightIndex < eneTile.HeightIndex)
			y = 1;
		else if (thisTile.HeightIndex > eneTile.HeightIndex)
			y = -1;

		if(thisTile.WidthIndex < eneTile.WidthIndex)
			x = 1;
		else if (thisTile.WidthIndex > eneTile.WidthIndex)
			x = -1;

		if(y == 0 || x == 0)
			return false;


		GameObject closestTile = ClosestTile(thisTile.HeightIndex, thisTile.WidthIndex, y, x, obj);

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

	GameObject ClosestTile(int y, int x, int yDirection, int xDirection, GameObject ene) {
		y += yDirection;
		x += xDirection;
		
		while(y >= 0 && y < GridController.Instance.gridHeight && x >= 0 && x < GridController.Instance.gridWidth) {
			
			if(GridController.Instance.tileArray[y,x].occupied && GridController.Instance.tileArray[y,x].occupier.Equals(ene)) {
				return GridController.Instance.gridArray[y - yDirection, x - xDirection];
			} else if (GridController.Instance.tileArray[y,x].occupied) {
				return null;
			}
			
			y += yDirection;
			x += xDirection;
		}

		return null;
	}

	protected override void ChangeDirection (GameObject nextTile) {
	}

	//------------------------------- For testing ------------------------------
	public override void AttacksForAutomation (System.Collections.Generic.List<MCTSNode> list, GameObject ene) {
		throw new System.NotImplementedException ();
	}
	//---------------------------- Testing end ------------------
}
