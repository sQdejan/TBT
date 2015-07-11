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

	[HideInInspector]
	public GameObject currentUnit;

	private CursorState cursorState;

	private LayerMask layerTile;
	private LayerMask layerEnemy;

	private GameObject activeTile;
	private GameObject activeEnemy;

	private Vector3 tileSelObjPos;

	void Start () {
		layerTile = 1 << LayerMask.NameToLayer("Tile");
		layerEnemy = 1 << LayerMask.NameToLayer("Enemy");

		tileSelObjPos = tileSelectorObject.transform.position;
	}
	
	void Update () {

		CursorIcon();

		if(!GameFlow.playersCurrentTurn)
			return;

		UnitSelectedCast();

	}

	//If a unit is selected, one can either move or attack depending on what is on the tile. First check for
	//enemy unit, if hit then show attack, else move.
	void UnitSelectedCast() {

		cursorState = CursorState.NORMAL;

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
		if(activeEnemy && Input.GetMouseButtonUp(0) && currentUnit.GetComponent<Unit>().IsAttackPossible(activeEnemy)) {
			currentUnit.GetComponent<Unit>().Attack(null, activeEnemy);
			EndTurn();
		} else if (activeEnemy && !currentUnit.GetComponent<Unit>().IsAttackPossible(activeEnemy)) {
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
			currentUnit.GetComponent<Unit>().Move(activeTile);
			EndTurn();
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

	//This function is public to be used when the "end turn" button is pressed
	void EndTurn() {

		tileSelectorObject.transform.position = tileSelObjPos;
		activeTile = null;
		Unit.attackMoveTile = null;
		GridController.Instance.ClearGrid();
		
		//Just reset sprites
		if(activeEnemy) {
			activeEnemy.GetComponent<SpriteRenderer>().sprite = activeEnemy.GetComponent<Unit>().originalSprite;
			activeEnemy = null;
		}

		if(currentUnit) {
			currentUnit.GetComponent<SpriteRenderer>().sprite = currentUnit.GetComponent<Unit>().originalSprite;
			currentUnit = null;
		}

		cursorState = CursorState.NORMAL;

		GameFlow.Instance.EndTurn();
	}

}
