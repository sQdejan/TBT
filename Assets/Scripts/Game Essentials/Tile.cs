using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public Sprite spriteTileOriginal;
	public Sprite spriteTileMove;
	public Sprite spriteMoveTo;

	[HideInInspector] public bool occupied = false;
	[HideInInspector] public bool available = false;

	public int heightIndex, widthIndex;

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

	public void SetHeightWidthIndex(int heightInd, int widthInd) {
		heightIndex = heightInd;
		widthIndex = widthInd;
	}
}
