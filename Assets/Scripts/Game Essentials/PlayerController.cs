using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CursorState {NORMAL, MOVE, NOMOVE, ATTACK, NOATTACK}

public class PlayerController : MonoBehaviour {

#region Singleton
	
	private static PlayerController instance;
	
	public static PlayerController Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<PlayerController>();
			}
			
			return instance;
		}
	}
	
#endregion

	public GameObject tileSelectorObject;

	public Texture2D cursorNormal;
	public Texture2D cursorMove;
	public Texture2D cursorNoMove;
	public Texture2D cursorAttack;
	public Texture2D cursorNoAttack;

	private CursorState cursorState;

	private LayerMask layerTile;
	private LayerMask layerUnit;
	private LayerMask layerEnemy;

	private GameObject activeTile;
	private GameObject activeUnit;
	private GameObject activeEnemy;

	private Vector3 tileSelObjPos;

	private bool selectedUnit = false;
	private bool tookAction = false;

	void Start () {
		layerTile = 1 << LayerMask.NameToLayer("Tile");
		layerUnit = 1 << LayerMask.NameToLayer("Unit");
		layerEnemy = 1 << LayerMask.NameToLayer("Enemy");

		tileSelObjPos = tileSelectorObject.transform.position;
	}
	
	void Update () {

		CursorIcon();

		if(!GameFlow.playersCurrentTurn)
			return;

		UnitCast();

		if(selectedUnit) {
			UnitSelectedCast();
		}
	}

	//For seeing which unit the player is hovering above, and for selecting that unit
	void UnitCast() {

		RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, layerUnit);
		
		//If no unit is selected, you should be able to
		if(!selectedUnit) {
			if(hit.collider != null) {
				if(activeUnit) {
					activeUnit.GetComponent<SpriteRenderer>().sprite = activeUnit.GetComponent<Unit>().originalSprite;
				}
				
				activeUnit = hit.collider.gameObject;
				activeUnit.GetComponent<SpriteRenderer>().sprite = activeUnit.GetComponent<Unit>().hoverSprite;
			} else {
				if(activeUnit) {
					activeUnit.GetComponent<SpriteRenderer>().sprite = activeUnit.GetComponent<Unit>().originalSprite;
					activeUnit = null;
				}
			}

			cursorState = CursorState.NORMAL;
		}
		
		//Select the unit if hovered above one
		if(activeUnit && Input.GetMouseButtonUp(0) && !selectedUnit) {
			ShowPossibleTiles(activeUnit.GetComponent<Unit>().possibleMoves);
			selectedUnit = true;
			cursorState = CursorState.MOVE;
		}
		
		//Deselect
		if((selectedUnit && Input.GetKeyDown(KeyCode.Escape)) || tookAction) {
			Reset ();
		}
	}

	//If a unit is selected, one can either move or attack depending on what is on the tile. First check for
	//enemy unit, if hit then show attack, else move.
	void UnitSelectedCast() {

		//Check for enemy
		RaycastHit2D enemyHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, layerEnemy);

		bool foundEnemy = false;

		if(enemyHit.collider != null) {
			if(activeEnemy) {
				activeEnemy.GetComponent<SpriteRenderer>().sprite = activeEnemy.GetComponent<Unit>().originalSprite;
			}

			activeEnemy = enemyHit.collider.gameObject;
			activeEnemy.GetComponent<SpriteRenderer>().sprite = activeEnemy.GetComponent<Unit>().hoverSprite;
			foundEnemy = true;

			cursorState = CursorState.ATTACK;
		} else {
			if(activeEnemy) {
				activeEnemy.GetComponent<SpriteRenderer>().sprite = activeEnemy.GetComponent<Unit>().originalSprite;
				activeEnemy = null;
			}
		}

		//If an enemy is hovered above and mouse clicked, attack it if possible.
		if(activeEnemy && Input.GetMouseButtonUp(0) && activeUnit.GetComponent<Unit>().IsAttackPossible(activeEnemy)) {
			activeUnit.GetComponent<Unit>().SetAttackAsAction();
			activeUnit.GetComponent<Unit>().CurrentAction(activeEnemy);
			activeEnemy.GetComponent<SpriteRenderer>().sprite = activeEnemy.GetComponent<Unit>().originalSprite;
			tookAction = true;
			activeEnemy = null;
		} else if (activeEnemy && !activeUnit.GetComponent<Unit>().IsAttackPossible(activeEnemy)) {
			cursorState = CursorState.NOATTACK;
		} else if (!activeEnemy && Unit.attackMoveTile != null) {
			//Reset the tile sprite if not hovered above anymore
			Unit.attackMoveTile.GetComponent<SpriteRenderer>().sprite = Unit.attackMoveTile.GetComponent<Tile>().spriteTilePossibleMove;
			Unit.attackMoveTile = null;
		}

		//As enemy is first priority we just return if we found one
		if(foundEnemy)
			return;

		//Check for tile
		RaycastHit2D tileHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, layerTile);
		
		if(tileHit.collider != null) {
			activeTile = tileHit.collider.gameObject;

			if(activeTile.GetComponent<Tile>().occupied) {
				cursorState = CursorState.NOMOVE;
				return;
			}

			if(!activeTile.GetComponent<Tile>().available) {
				cursorState = CursorState.NOMOVE;	
			} else {
				cursorState = CursorState.MOVE;
			}

			tileSelectorObject.transform.position = activeTile.transform.position;
		} else {
			if(activeTile) {
				tileSelectorObject.transform.position = tileSelObjPos;
				activeTile = null;
			}
		}

		if(activeTile && Input.GetMouseButtonUp(0) && activeTile.GetComponent<Tile>().available) {
			activeUnit.GetComponent<Unit>().SetMoveAsAction();
			activeUnit.GetComponent<Unit>().CurrentAction(activeTile);
			tookAction = true;
			activeTile = null;
		} 
	}
	
	void ShowPossibleTiles(int possibleMoves) {
		
		//Finding the tile the current unit is standing on
		RaycastHit2D hit = Physics2D.Raycast(activeUnit.transform.position, Vector2.zero, 0, layerTile);
		
		GameObject tmpTile = hit.collider.gameObject;

		int curUnitHeightInd = tmpTile.GetComponent<Tile>().HeightIndex;
		int curUnitWidthInd = tmpTile.GetComponent<Tile>().WidthIndex;
		
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				if(!GridController.Instance.tileArray[i,j].occupied) {
					if(Mathf.Abs(j - curUnitWidthInd) <= possibleMoves && 
					   Mathf.Abs(i - curUnitHeightInd) <= possibleMoves) {
						GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.tileArray[i,j].spriteTilePossibleMove;
						GridController.Instance.tileArray[i,j].available = true;
					}
				}
			}
		}
	}

	//To reset grid if unit is deselected
	void ClearGrid() {
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.gridArray[i,j].GetComponent<Tile>().spriteTileOriginal;
				GridController.Instance.gridArray[i,j].GetComponent<Tile>().available = false;
			}
		}
	}

	void CursorIcon() {
		switch(cursorState) {
		case CursorState.NORMAL:
			Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
			break;
		case CursorState.MOVE:
			Cursor.SetCursor(cursorMove, Vector2.zero, CursorMode.Auto);
			break;
		case CursorState.NOMOVE:
			Cursor.SetCursor(cursorNoMove, Vector2.zero, CursorMode.Auto);
			break;
		case CursorState.ATTACK:
			Cursor.SetCursor(cursorAttack, Vector2.zero, CursorMode.Auto);
			break;
		case CursorState.NOATTACK:
			Cursor.SetCursor(cursorNoAttack, Vector2.zero, CursorMode.Auto);
			break;
		default:
			break;
		}
	}


	public void Reset() {

		tookAction = false;
		selectedUnit = false;
		tileSelectorObject.transform.position = tileSelObjPos;
		activeTile = null;
		ClearGrid();
		
		//Just reset sprites
		if(activeEnemy) {
			activeEnemy.GetComponent<SpriteRenderer>().sprite = activeEnemy.GetComponent<Unit>().originalSprite;
			activeEnemy = null;
		}

		if(activeUnit) {
			activeUnit.GetComponent<SpriteRenderer>().sprite = activeUnit.GetComponent<Unit>().originalSprite;
			activeUnit = null;
		}

		cursorState = CursorState.NORMAL;
	}

}
