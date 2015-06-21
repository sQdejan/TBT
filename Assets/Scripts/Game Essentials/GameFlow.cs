using UnityEngine;
using System.Collections;

public class GameFlow : MonoBehaviour {

#region Singleton
	
	private static GameFlow instance;
	
	public static GameFlow Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<GameFlow>();
			}
			
			return instance;
		}
	}
	
#endregion

	public int amountOfResourcesPerTurn = 10;
	public bool playerWillStart = true;


}
