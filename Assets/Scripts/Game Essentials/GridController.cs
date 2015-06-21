using UnityEngine;
using System.Collections;

public class GridController : MonoBehaviour {

#region Singleton

	private static GridController instance;

	public static GridController Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<GridController>();
			}

			return instance;
		}
	}

#endregion

	public GameObject gridParent;
	public GameObject gridPrefab;
	public int gridWidth, gridHeight;
	
	public int startRowIndex;
	public Transform[] rowsToBeUsed;

	/// <summary>
	/// Width, height.
	/// </summary>
	public GameObject[,] gridArray;
	
	//Initialise grid
	void Awake() {

		GameObject[,] tmpArray = AddRowsToArray(); //For positions for the tiles

		gridArray = new GameObject[gridWidth, gridHeight];

		for(int i = 0; i < gridWidth; i++) {
			for(int j = 0; j < gridHeight; j++) {
				gridArray[i,j] = (GameObject)Instantiate(gridPrefab, tmpArray[i,j].transform.position + Vector3.up * 0.6f, Quaternion.identity);
				gridArray[i,j].GetComponent<Tile>().SetHeightWidthIndex(j, i);
				gridArray[i,j].transform.parent = gridParent.transform;
			}
		}

	}

	//Adding the rows to an temporary array in order to place the tiles
	GameObject[,] AddRowsToArray() {

		int tmpHInd = -1;
		int tmpWInd = 1;

		GameObject[,] returnArray = new GameObject[gridWidth, gridHeight];

		for(int i = 0; i < rowsToBeUsed.Length; i++) {

			if(i % 2 == 0) {
				tmpHInd += 1;
				tmpWInd = 1;
			} else {
				tmpWInd = 0;
			}

			//I need to add one extra if the row length is an odd number
			int range = gridWidth;
			if(range % 2 == 1 && tmpWInd == 0)
				range++;

			range /= 2;

			for(int j = 0; j < range; j++) {
				returnArray[tmpWInd, tmpHInd] = rowsToBeUsed[i].GetChild(j + startRowIndex).gameObject;
				tmpWInd += 2;
			}
		}

		return returnArray;
	}
}
