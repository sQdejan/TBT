using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	[HideInInspector] public bool occupied = false;
	[HideInInspector] public bool available = false;

	private int heightIndex, widthIndex;

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
