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

//		StartCoroutine(DelayedStart());
	}

	//Only for testing
	IEnumerator DelayedStart() {
		yield return new WaitForSeconds(0.5f);

		currentUnit.GetComponent<Unit>().ShowPossibleMoves();
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
				if(activeEnemy.GetComponentInChildren<SpriteRenderer>())
					activeEnemy.GetComponentInChildren<SpriteRenderer>().color = activeEnemy.GetComponent<Unit>().oriSpriteColor;
			}

			activeEnemy = enemyHit.collider.gameObject;
			if(activeEnemy.GetComponentInChildren<SpriteRenderer>())
				activeEnemy.GetComponentInChildren<SpriteRenderer>().color = activeEnemy.GetComponent<Unit>().activeSpriteColor;
			foundEnemy = true;

			cursorState = CursorState.ATTACK;
		} else {
			if(activeEnemy) {
				if(activeEnemy.GetComponentInChildren<SpriteRenderer>())
					activeEnemy.GetComponentInChildren<SpriteRenderer>().color = activeEnemy.GetComponent<Unit>().oriSpriteColor;
				activeEnemy = null;
			}
		}

		//If an enemy is hovered above and mouse clicked, attack it if possible.
		if(activeEnemy && Input.GetMouseButtonUp(0) && currentUnit.GetComponent<Unit>().IsAttackPossible(activeEnemy)) {

			//Setting the move that the player is taking in order to help the MCTS
			Tile actEne = activeEnemy.GetComponent<Unit>().curTile.GetComponent<Tile>();
			if(Unit.attackMoveTile == null) {
				Tile curUnitTile = currentUnit.GetComponent<Unit>().curTile.GetComponent<Tile>();
				GameFlow.Instance.SetPlayerLastMove(Action.ATTACK, curUnitTile.HeightIndex, curUnitTile.WidthIndex, actEne.HeightIndex, actEne.WidthIndex);
			} else {
				Tile mTile = Unit.attackMoveTile.GetComponent<Tile>();
				GameFlow.Instance.SetPlayerLastMove(Action.ATTACK, mTile.HeightIndex, mTile.WidthIndex, actEne.HeightIndex, actEne.WidthIndex);
			}

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

			//Setting the move that the player is taking in order to help the MCTS
			Tile aTile = activeTile.GetComponent<Tile>();
			GameFlow.Instance.SetPlayerLastMove(Action.MOVE, -1, -1, aTile.HeightIndex, aTile.WidthIndex);

			currentUnit.GetComponent<Unit>().Move(activeTile);
			EndTurn();
		} 

		//Just taking an automatic random move for testing purposes ------------------------------------------ !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! --------------------------------------

//		List<MCTSNode> poss = new List<MCTSNode>();
//
//		//possible moves
//		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
//			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
//				if(GridController.Instance.tileArray[i,j].available) { 
//					poss.Add(new MCTSNode(null, Action.MOVE, 1, 1, i, j));
//				}
//			}
//		}
//
//		//possible attacks - if any attacks available I remove the "moves" inside the function and only send back attacks
//		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
//			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
//				if(GridController.Instance.tileArray[i,j].occupied && GridController.Instance.tileArray[i,j].occupier.tag == "EnemyUnit") { 
//					currentUnit.GetComponent<Unit>().AttacksForAutomation(poss, GridController.Instance.tileArray[i,j].occupier);
//				}
//			}
//		}
//
//		MCTSNode n = poss[Random.Range(0, poss.Count - 1)]; 
//
//		if(n.action == Action.MOVE)
//			currentUnit.GetComponent<Unit>().Move(GridController.Instance.gridArray[n.gsH,n.gsW]);
//		else {
//			currentUnit.GetComponent<Unit>().Attack(GridController.Instance.gridArray[n.mbagsH,n.mbagsW], GridController.Instance.tileArray[n.gsH, n.gsW].occupier);
//		}
//
//		EndTurn();

		//Tiilllll here ------------------------------------------------------------------------
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
			if(activeEnemy.GetComponentInChildren<SpriteRenderer>())
				activeEnemy.GetComponentInChildren<SpriteRenderer>().color = activeEnemy.GetComponent<Unit>().oriSpriteColor;
			activeEnemy = null;
		}

		if(currentUnit) {
			currentUnit.GetComponentInChildren<SpriteRenderer>().color = currentUnit.GetComponentInChildren<Unit>().oriSpriteColor;
			currentUnit = null;
		}

		cursorState = CursorState.NORMAL;

//		currentUnit.GetComponent<Unit>().ShowPossibleMoves();

		GameFlow.Instance.EndTurn();
	}

}
