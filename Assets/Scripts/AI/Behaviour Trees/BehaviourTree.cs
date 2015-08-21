using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BehaviourTree : MonoBehaviour {

	const float WAIT_TIME = 1;

	protected Unit unit;
	protected MCTSNode move;
	protected List<MCTSNode> poss;

	void Start() {
		unit = GetComponent<Unit>();
	}

	public abstract void TakeMove();

	protected IEnumerator EndMove() {
		yield return new WaitForSeconds(WAIT_TIME);

		if(move.action == Action.MOVE) {
			unit.Move(GridController.Instance.gridArray[move.gsH,move.gsW]);
			GameFlow.Instance.SetAILastMove(Action.MOVE, -1, -1, move.gsH, move.gsW);
		} else {
			unit.Attack(GridController.Instance.gridArray[move.mbagsH,move.mbagsW], GridController.Instance.tileArray[move.gsH, move.gsW].occupier);
			GameFlow.Instance.SetAILastMove(Action.ATTACK, move.mbagsH, move.mbagsW, move.gsH, move.gsW);
		}

		GridController.Instance.ResetGrid();
		GameFlow.Instance.EndTurn();
	}

	protected float Distance(int h, int w, Points2D p2D) {
		return (float)(Mathf.Sqrt(Mathf.Pow(h - p2D.h, 2) + Mathf.Pow(w - p2D.w, 2)));
	}
}

public class Points2D {
	public int h, w;
	
	public Points2D (int h, int w) {
		this.h = h;
		this.w = w;
	}
} 
