using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ClassType {WARRIOR, RANGED, RUNNER};
public enum Direction {DOWN, UP};

public abstract class Unit : MonoBehaviour {

	public ClassType classType;
	public Direction attackDirection;
	public Direction moveDirection;

	public int possibleMovesStraight;
	public int possibleMovesStrafe;
	public int attackRange;
	public int health;
	public int damage;

	public Vector3 spritePosUp;
	public Vector3 spritePosDown;

	public GameObject spriteChild;

	public Color activeSpriteColor;
	public Color damageSpriteColor;
	[HideInInspector] public Color oriSpriteColor;

	public Sprite upSprite;
	public Sprite downSprite;

	//The tile that will be moved to, in order to attack. I need this variable in order to reset the sprite if not
	//hovered above anymore. It can be static because only one unit will be active at any given time
	public static GameObject attackMoveTile;

	//I need this in order to reset it's stats once a Unit is moving
	[HideInInspector] public GameObject curTile;

	#region Methods to be overridden

	public abstract void ShowPossibleMoves();
	public abstract void Attack(GameObject moveToObj, GameObject attackObj);
	public abstract bool IsAttackPossible(GameObject obj);
	protected abstract void ChangeDirection(GameObject nextTile);

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

		oriSpriteColor = GetComponentInChildren<SpriteRenderer>().color;
		GetComponentInChildren<SpriteRenderer>().sortingOrder = curTile.GetComponent<Tile>().HeightIndex;
	}

	public void Move (GameObject nextTile) {

		ChangeDirection(nextTile);

		curTile.GetComponent<Tile>().occupied = false;
		curTile.GetComponent<Tile>().occupier = null;

		StartCoroutine(SlidingMove(nextTile.transform.position));
		curTile = nextTile;

		GetComponentInChildren<SpriteRenderer>().sortingOrder = curTile.GetComponent<Tile>().HeightIndex;

		curTile.GetComponent<Tile>().occupied = true;
		curTile.GetComponent<Tile>().available = false;
		curTile.GetComponent<Tile>().occupier = gameObject;

		attackMoveTile = null;
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

	public void TakeDamage(int damage) {
		health -= damage;
		StartCoroutine(DamageEffect());

		if(health <= 0)
			Death();
	}

	IEnumerator DamageEffect() {
		float effectTime = 0.2f;
		float time = 0;

		SpriteRenderer tmpSprRen = GetComponentInChildren<SpriteRenderer>();

		while(time < effectTime / 2) {
			tmpSprRen.color = Color.Lerp(tmpSprRen.color, damageSpriteColor, time/(effectTime/2));
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}

		time = 0;
		while(time < effectTime / 2) {
			tmpSprRen.color = Color.Lerp(tmpSprRen.color, oriSpriteColor, time/(effectTime/2));
			time += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
	}

	protected void Death () {
		GameFlow.Instance.KillUnit(gameObject);
		curTile.GetComponent<Tile>().occupied = false;
		gameObject.SetActive(false);
	}
}
