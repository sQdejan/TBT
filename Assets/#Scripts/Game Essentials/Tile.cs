using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public Sprite spriteTileOriginal;
	public Sprite spriteTilePossibleMove;
	public Sprite spriteAttackMove;

	[HideInInspector] public bool occupied = false;
	[HideInInspector] public bool available = false;

	[HideInInspector] public GameObject occupier = null;

	private int heightIndex, widthIndex;

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

	#endregion

	public void SetHeightWidthIndex(int h, int w) {
		heightIndex = h;
		widthIndex = w;
	}

}
