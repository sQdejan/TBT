using UnityEngine;
using System.Collections;

public delegate void PerformAction(GameObject obj);

public abstract class Unit : MonoBehaviour {

	public int possibleMoves;
	public int health;
	public int damage;
	public Sprite originalSprite;
	public Sprite hoverSprite;

	public PerformAction CurrentAction; //To assign different methods depending on what should be called

	private GameObject curTile;

	#region Methods to be overridden

	public abstract bool IsAttackPossible(GameObject obj);
	public abstract void TakeDamage(int damage);

	protected abstract void Attack(GameObject obj);

	#endregion

	#region Properties

	public GameObject CurTile {
		get {
			return curTile;
		}
	}

	#endregion

	void Start() {
		//Just to make the tile occupied from start
		LayerMask tileLayer = 1 << LayerMask.NameToLayer("Tile");
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 0, tileLayer);
		if(hit.collider != null) {
			curTile = hit.collider.gameObject;
			curTile.GetComponent<Tile>().occupied = true;
			curTile.GetComponent<Tile>().available = false;
		}
	}

	void Move(GameObject nextTile) {
		curTile.GetComponent<Tile>().occupied = false;
		curTile.GetComponent<Tile>().available = true;

		transform.position = nextTile.transform.position;
		curTile = nextTile;
		curTile.GetComponent<Tile>().occupied = true;
		curTile.GetComponent<Tile>().available = false;
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
