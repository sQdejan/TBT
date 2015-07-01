using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour {

	#region Singleton
	
	private static AIController instance;
	
	public static AIController Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<AIController>();
			}
			
			return instance;
		}
	}
	
	#endregion

	//Create the root node for MCTS by copying the current state
	public void CopyCurrentState() {

	}

	//Each time the MCTS restarts we need to reinitialise the root node state
	public void SetupState(MCTSNode node) {

	}

	public void PossibleMoves() {

	}
}
