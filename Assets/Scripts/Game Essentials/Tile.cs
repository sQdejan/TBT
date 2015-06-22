using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public Sprite spriteTileOriginal;
	public Sprite spriteTileMove;
	public Sprite spriteMoveTo;

	[HideInInspector] public bool occupied = false;
	[HideInInspector] public bool available = false;

	public GameObject neighbourUpLeft, neighbourUpRight, neighbourDownLeft, neighbourDownRight;

	private int heightIndex, widthIndex;
	private GameObject[] neighboursArray;

	void Start() {
		neighboursArray = new GameObject[] {neighbourUpLeft, neighbourUpRight, neighbourDownRight, neighbourDownLeft};
	} 

	#region Properties

	public int HeightIndex {
		get {
			return heightIndex;
		}
	}

	public int WidthIndex {
		get {
			return widthIndex;
		}
	}

	public GameObject[] NeighboursArray {
		get {
			return neighboursArray;
		}
	}

	#endregion

	public void SetHeightWidthIndex(int heightInd, int widthInd) {
		heightIndex = heightInd;
		widthIndex = widthInd;
	}


}
