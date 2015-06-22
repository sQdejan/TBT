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

		AssignNeighbours();
	}

	//Assign their "true" neighbours according to ISO view.
	//Just for helping other functions.
	void AssignNeighbours() {

		for(int i = 0; i < gridWidth; i++) {
			for(int j = 0; j < gridHeight; j++) {
				
				//If odd width
				if(i % 2 == 1) {
					//top left
					int x = i - 1, y = j - 1;
					if(x >= 0 && y >= 0)
						gridArray[i,j].GetComponent<Tile>().neighbourUpLeft = gridArray[x,y];
					
					//top right
					x = i + 1;
					if(x < gridWidth && y >= 0) 
						gridArray[i,j].GetComponent<Tile>().neighbourUpRight = gridArray[x,y];
					
					//bot left
					x = i - 1;
					y = j;
					if(x >= 0) 
						gridArray[i,j].GetComponent<Tile>().neighbourDownLeft = gridArray[x,y];
					
					//bot right
					x = i + 1;
					if(x < gridWidth)
						gridArray[i,j].GetComponent<Tile>().neighbourDownRight = gridArray[x,y];
					
				} else { //If even
					
					//top left
					int x = i - 1, y = j;
					if(x >= 0)
						gridArray[i,j].GetComponent<Tile>().neighbourUpLeft = gridArray[x,y];
					
					//top right
					x = i + 1;
					if(x < gridWidth) 
						gridArray[i,j].GetComponent<Tile>().neighbourUpRight = gridArray[x,y];
					
					//bot left
					x = i - 1;
					y = j + 1;
					if(y < gridHeight && x >= 0) 
						gridArray[i,j].GetComponent<Tile>().neighbourDownLeft = gridArray[x,y];
					
					//bot right
					x = i + 1;
					if(y < gridHeight && x < gridWidth)
						gridArray[i,j].GetComponent<Tile>().neighbourDownRight = gridArray[x,y];
					
				}
			}
		}
	}

	//Adding the rows to an temporary array in order to place the tiles.
	//For easy setup.
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
