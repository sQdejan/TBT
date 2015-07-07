using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void PerformAction(GameObject obj);
public enum ClassType {WARRIOR, RANGED};

public abstract class Unit : MonoBehaviour {

	public ClassType classType;

	public int possibleMoves;
	public int attackRange;
	public int health;
	public int damage;

	public Sprite originalSprite;
	public Sprite hoverSprite;

	public PerformAction CurrentAction; //To assign different methods depending on what should be called

	//The tile that will be moved to, in order to attack. I need this variable in order to reset the sprite if not
	//hovered above anymore. It can be static because only one unit will be active at any given time
	public static GameObject attackMoveTile;

	//I need this in order to reset it's stats once a Unit is moving
	[HideInInspector] public GameObject curTile;

	#region Methods to be overridden

	public abstract bool IsAttackPossible(GameObject obj);
	public abstract void TakeDamage(int damage);

	protected abstract void Attack(GameObject obj);

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

	//Shared and implemented functionality for Move and Death
	protected void Move (GameObject nextTile) {
		
		curTile.GetComponent<Tile>().occupied = false;
		curTile.GetComponent<Tile>().available = true;
		curTile.GetComponent<Tile>().occupier = null;
		
		transform.position = nextTile.transform.position;
		curTile = nextTile;

		curTile.GetComponent<Tile>().occupied = true;
		curTile.GetComponent<Tile>().available = false;
		curTile.GetComponent<Tile>().occupier = gameObject;
	}
	
	//Remember to announce somewhere that I died
	protected void Death () {
		GameFlow.Instance.KillUnit(gameObject);
		curTile.GetComponent<Tile>().occupied = false;
		gameObject.SetActive(false);
	}

	//I need the two following methods for automatically
	//setting the two "standard" possibilities.
	public void SetAttackAsAction() {
		CurrentAction = Attack;
	}

	public void SetMoveAsAction() {
		CurrentAction = Move;
	}
}
