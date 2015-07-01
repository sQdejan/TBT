using UnityEngine;
using System.Collections;

public class AIGridController : MonoBehaviour {

	#region Singleton
	
	private static AIGridController instance;
	
	public static AIGridController Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<AIGridController>();
			}
			
			return instance;
		}
	}
	
	#endregion
	
	public GameObject gridParent;
	public GameObject gridPrefab;

	/// <summary>
	/// Height, width.
	/// </summary>
	public GameObject[,] gridArray;
	
	/// <summary>
	/// Height, width.
	/// </summary>
	public Tile[,] tileArray;

	private int gridHeight, gridWidth;
	
	//Copy the original grid and place it -100 on the y-axis
 	void Start() {

		gridHeight = GridController.Instance.gridHeight;
		gridWidth = GridController.Instance.gridWidth;

		gridArray = new GameObject[gridHeight, gridWidth];
		tileArray = new Tile[gridHeight, gridWidth];
		
		for(int i = 0; i < gridHeight; i++) {
			for(int j = 0; j < gridWidth; j++) {
				gridArray[i,j] = (GameObject)Instantiate(gridPrefab, GridController.Instance.gridArray[i,j].transform.position - Vector3.up * 100, Quaternion.identity);
				gridArray[i,j].GetComponent<Tile>().SetHeightWidthIndex(i, j);
				gridArray[i,j].transform.parent = gridParent.transform;
				tileArray[i,j] = gridArray[i,j].GetComponent<Tile>();
			}
		}
	}
}
