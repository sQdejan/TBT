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
	public float gridSpacing;

	[HideInInspector]
	public GameObject[,] gridArray;

	//Initialise grid
	void Awake() {

		gridArray = new GameObject[gridWidth, gridHeight];

		for(int i = 0; i < gridWidth; i++) {
			for(int j = 0; j < gridHeight; j++) {
				gridArray[i,j] = (GameObject)Instantiate(gridPrefab, gridPrefab.transform.position + (Vector3.right * i * gridSpacing) + (-Vector3.up * j * gridSpacing), Quaternion.identity);
				gridArray[i,j].GetComponent<Tile>().SetHeightWidthIndex(j, i);
				gridArray[i,j].transform.parent = gridParent.transform;
			}
		}
	}
}
