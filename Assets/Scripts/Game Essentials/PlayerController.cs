using UnityEngine;
using System.Collections;

public enum MouseState {NORMAL, MOVE, NOMOVE, ATTACK, NOATTACK}

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
	public Sprite spriteTileOriginal;
	public Sprite spriteTileMove;

	public Texture2D cursorNormal;
	public Texture2D cursorMove;
	public Texture2D cursorNoMove;
	public Texture2D cursorAttack;
	public Texture2D cursorNoAttack;
	
	private LayerMask layerTile;
	private LayerMask layerUnit;
	private LayerMask layerEnemy;

	private GameObject activeTile;
	private GameObject activeUnit;
	private GameObject activeEnemy;

	private Vector3 tileSelObjPos;

	private bool selectedUnit = false;
	private bool tookAction = false;

	private MouseState mouseState;

	void Start () {
		layerTile = 1 << LayerMask.NameToLayer("Tile");
		layerUnit = 1 << LayerMask.NameToLayer("Unit");
		layerEnemy = 1 << LayerMask.NameToLayer("Enemy");

		tileSelObjPos = tileSelectorObject.transform.position;
	}
	
	void Update () {

		UnitCast();

		if(selectedUnit) {
			UnitSelectedCast();
		}

		switch(mouseState) {
		case MouseState.NORMAL:
			Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
			break;
		case MouseState.MOVE:
			Cursor.SetCursor(cursorMove, Vector2.zero, CursorMode.Auto);
			break;
		case MouseState.NOMOVE:
			Cursor.SetCursor(cursorNoMove, Vector2.zero, CursorMode.Auto);
			break;
		case MouseState.ATTACK:
			Cursor.SetCursor(cursorAttack, Vector2.zero, CursorMode.Auto);
			break;
		case MouseState.NOATTACK:
			Cursor.SetCursor(cursorNoAttack, Vector2.zero, CursorMode.Auto);
			break;
		default:
			break;
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

			tileSelectorObject.transform.position = tileSelObjPos;
			mouseState = MouseState.NORMAL;
		}
		
		//Select the unit if hovered above one
		if(activeUnit && Input.GetMouseButtonUp(0)) {
			ShowPossibleTiles(activeUnit.GetComponent<Unit>().possibleMoves);
			selectedUnit = true;
			mouseState = MouseState.MOVE;
		}
		
		//Deselect
		if((selectedUnit && Input.GetKeyDown(KeyCode.Escape)) || tookAction) {
			tookAction = false;
			selectedUnit = false;
			ClearGrid();
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

			mouseState = MouseState.ATTACK;
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
			mouseState = MouseState.NOATTACK;
		}

		//As enemy is first priority we just return if we found one
		if(foundEnemy)
			return;

		//Check for tile
		RaycastHit2D tileHit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0, layerTile);
		
		if(tileHit.collider != null) {
			activeTile = tileHit.collider.gameObject;

			if(activeTile.GetComponent<Tile>().occupied) {
				mouseState = MouseState.NOMOVE;
				return;
			}

			if(!activeTile.GetComponent<Tile>().available) {
				mouseState = MouseState.NOMOVE;	
			} else {
				mouseState = MouseState.MOVE;
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

	//Change the sprite showing which tiles can be used
	void ShowPossibleTiles(int possibleMoves) {

		//Finding the tile the current unit is standing on
		RaycastHit2D hit = Physics2D.Raycast(activeUnit.transform.position, Vector2.zero, 0, layerTile);

		GameObject tmpTile = hit.collider.gameObject;

		int curUnitWidthInd = tmpTile.GetComponent<Tile>().WidthIndex;
		int curUnitHeightInd = tmpTile.GetComponent<Tile>().HeightIndex;

		for(int i = 0; i < GridController.Instance.gridWidth; i++) {
			for(int j = 0; j < GridController.Instance.gridHeight; j++) {
				if(!GridController.Instance.gridArray[i,j].GetComponent<Tile>().occupied) {
					if(Mathf.Abs(i - (curUnitWidthInd + curUnitHeightInd - j)) <= possibleMoves && 
					   Mathf.Abs(j - (curUnitHeightInd - curUnitWidthInd + i)) <= possibleMoves) {
						GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = spriteTileMove;
						GridController.Instance.gridArray[i,j].GetComponent<Tile>().available = true;
					}
				}
			}
		}
	}

	//To reset grid if unit is deselected
	void ClearGrid() {
		for(int i = 0; i < GridController.Instance.gridWidth; i++) {
			for(int j = 0; j < GridController.Instance.gridHeight; j++) {
				GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = spriteTileOriginal;
				GridController.Instance.gridArray[i,j].GetComponent<Tile>().available = false;
			}
		}
	}
}
