using UnityEngine;
using System.Collections;

public class MCTS : MonoBehaviour {

#region Singleton
	
	private static MCTS instance;
	
	public static MCTS Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<MCTS>();
			}
			
			return instance;
		}
	}
	
#endregion

	MCTSNode rootNode;

	public void TakeTurn() {

	}

	void TreePolicy() {

	}

	void BestChild() {

	}

	void Expand() {

	}

	void FullyExpanded() {

	}

	void UntriedAction() {

	}

	void DefaultPolicy() {

	}

	void UCTValue() {

	}

	void BackPropagate() {

	}

}
