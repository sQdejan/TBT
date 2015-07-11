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
	/// Height, width.
	/// </summary>
	public GameObject[,] gridArray;

	/// <summary>
	/// Height, width.
	/// </summary>
	public Tile[,] tileArray;

	
	//Initialise grid
	void Awake() {

		GameObject[,] tmpArray = AddRowsToArray(); //For positions for the tiles
		gridArray = new GameObject[gridHeight, gridWidth];
		tileArray = new Tile[gridHeight, gridWidth];

		for(int i = 0; i < gridHeight; i++) {
			for(int j = 0; j < gridWidth; j++) {
				gridArray[i,j] = (GameObject)Instantiate(gridPrefab, tmpArray[i,j].transform.position + Vector3.up * 0.6f, Quaternion.identity);
				gridArray[i,j].GetComponent<Tile>().SetHeightWidthIndex(i, j);
				gridArray[i,j].transform.parent = gridParent.transform;
				tileArray[i,j] = gridArray[i,j].GetComponent<Tile>();
			}
		}
	}

	//Adding the rows to an temporary array in order to place the tiles.
	//For easy setup.
	GameObject[,] AddRowsToArray() {

		GameObject[,] returnArray = new GameObject[gridHeight, gridWidth];

		for(int i = 0; i < gridHeight; i++) {
			for(int j = startRowIndex; j < gridWidth + startRowIndex; j++) {
				returnArray[i,j - startRowIndex] = rowsToBeUsed[i].GetChild(j).gameObject;
			}
		}

		return returnArray;
	}

	//To reset grid when turn is over
	public void ClearGrid() {
		for(int i = 0; i < GridController.Instance.gridHeight; i++) {
			for(int j = 0; j < GridController.Instance.gridWidth; j++) {
				GridController.Instance.gridArray[i,j].GetComponent<SpriteRenderer>().sprite = GridController.Instance.gridArray[i,j].GetComponent<Tile>().spriteTileOriginal;
				GridController.Instance.gridArray[i,j].GetComponent<Tile>().available = false;
			}
		}
	}
}
