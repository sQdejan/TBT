using UnityEngine;
using System.Collections;

//I've created this class because there is some shared commonalities between
//units that are to be on the real board, but should not be shared by the units
//that will be used to simulate states in the AI
public abstract class PlayableCharacter : Unit {

	protected override void Move (GameObject nextTile) {
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

	//Remember to announce somewhere that I died
	protected override void Death () {
		Debug.Log("Death");
	}
}
