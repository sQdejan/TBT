using UnityEngine;
using System.Collections;

public delegate void PerformAction(GameObject obj);

public abstract class Unit : MonoBehaviour {

	public int possibleMoves;
	public int attackRange;
	public int health;
	public int damage;
	public int resourcesForAttack;
	public int resourcesForMove;

	public Sprite originalSprite;
	public Sprite hoverSprite;

	public PerformAction CurrentAction; //To assign different methods depending on what should be called

	//The tile that will be moved to, in order to attack. I need this variable in order to reset the sprite if not
	//hovered above anymore. It can be static because only one unit will be active at any given time
	public static GameObject attackMoveTile;

	//I need this in order to reset it's stats once a Unit is moving
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
		//Make the start tile occupied
		LayerMask tileLayer = 1 << LayerMask.NameToLayer("Tile");
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.zero, 0, tileLayer);
		if(hit.collider != null) {
			curTile = hit.collider.gameObject;
			curTile.GetComponent<Tile>().occupied = true;
			curTile.GetComponent<Tile>().available = false;
		}
	}

	protected void Move(GameObject nextTile) {

		if(GameFlow.Instance.resourcesLeft - resourcesForMove >= 0) {

			GameFlow.Instance.resourcesLeft -= resourcesForMove;
			GameFlow.Instance.UpdateResourceText();

			curTile.GetComponent<Tile>().occupied = false;
			curTile.GetComponent<Tile>().available = true;

			transform.position = nextTile.transform.position;
			curTile = nextTile;
			curTile.GetComponent<Tile>().occupied = true;
			curTile.GetComponent<Tile>().available = false;
		} else {
			Debug.Log("Should give the player a warning");
		}
	}

	protected void Death() {
		Debug.Log("Death");
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
