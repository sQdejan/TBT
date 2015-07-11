using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ClassType {WARRIOR, RANGED};

public abstract class Unit : MonoBehaviour {

	public ClassType classType;

	public int possibleMoves;
	public int attackRange;
	public int health;
	public int damage;

	public Sprite originalSprite;
	public Sprite hoverSprite;

	//The tile that will be moved to, in order to attack. I need this variable in order to reset the sprite if not
	//hovered above anymore. It can be static because only one unit will be active at any given time
	public static GameObject attackMoveTile;

	//I need this in order to reset it's stats once a Unit is moving
	[HideInInspector] public GameObject curTile;

	#region Methods to be overridden

	public abstract bool IsAttackPossible(GameObject obj);
	public abstract void TakeDamage(int damage);

	public abstract void Attack(GameObject moveToObj, GameObject attackObj);

	#endregion

	void Start() {

		//Make the start tile occupied
		LayerMask tileLayer = 1 << LayerMask.NameToLayer("Tile");
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 0, tileLayer);
		if(hit.collider != null) {
			curTile = hit.collider.gameObject;
			curTile.GetComponent<Tile>().occupied = true;
			curTile.GetComponent<Tile>().available = false;
			curTile.GetComponent<Tile>().occupier = gameObject;
		}
	}

	//Shared and implemented functionality for Move, ShowPossibleMoves and Death
	public void Move (GameObject nextTile) {
		
		curTile.GetComponent<Tile>().occupied = false;
		curTile.GetComponent<Tile>().occupier = null;

		StartCoroutine(SlidingMove(nextTile.transform.position));
		curTile = nextTile;

		curTile.GetComponent<Tile>().occupied = true;
		curTile.GetComponent<Tile>().available = false;
		curTile.GetComponent<Tile>().occupier = gameObject;
	}

	IEnumerator SlidingMove(Vector3 pos) {
		float travelTime = 1f;
		float time = 0;

		while(time < travelTime) {
			transform.position = Vector3.Lerp(transform.position, pos, time/travelTime);
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
	}

	//Show possible moves on the grid
	public void ShowPossibleMoves() {

		int curUnitHeightInd = curTile.GetComponent<Tile>().HeightIndex;
		int curUnitWidthInd = curTile.GetComponent<Tile>().WidthIndex;
		
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				if(!GridController.Instance.tileArray[i,j].occupied) {
					if(Mathf.Abs(j - curUnitWidthInd) <= possibleMoves && Mathf.Abs(i - curUnitHeightInd) <= possibleMoves) {
						GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[i,j].spriteTilePossibleMove;
						GridController.Instance.tileArray[i,j].available = true;
					}
				}
			}
		}

	}
	
	//Remember to announce somewhere that I died
	protected void Death () {
		GameFlow.Instance.KillUnit(gameObject);
		curTile.GetComponent<Tile>().occupied = false;
		gameObject.SetActive(false);
	}
}
