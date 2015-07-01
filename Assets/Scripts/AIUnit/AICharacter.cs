using UnityEngine;
using System.Collections;

public abstract class AICharacter : Unit {

	protected override void Move (GameObject nextTile) {
			curTile.GetComponent<Tile>().occupied = false;
			curTile.GetComponent<Tile>().available = true;
			
			transform.position = nextTile.transform.position;
			curTile = nextTile;
			curTile.GetComponent<Tile>().occupied = true;
			curTile.GetComponent<Tile>().available = false;
	}
	
	//Remember to announce somewhere that I died
	protected override void Death ()
	{
		Debug.Log("Death");
	}
}
