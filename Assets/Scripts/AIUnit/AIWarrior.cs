using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIWarrior : AIUnit {

	public override void Attack (GameStateUnit to)
	{
		throw new System.NotImplementedException ();
	}

	public override void TakeDamage (int damage) {
		throw new System.NotImplementedException ();
	}

	public override List<MCTSNode> GetPossibleMoves (MCTSNode parent) {

		List<MCTSNode> rList = new List<MCTSNode>();

		//First I get the possible moves
		for(int i = 0; i < MCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
				if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
					if(Mathf.Abs(i - curgsUnit.h) <= possibleMoves && Mathf.Abs(j - curgsUnit.w) <= possibleMoves) {
						MCTS.Instance.gameState[i,j].state = AIGameFlow.GS_MOVE;
						rList.Add(new MCTSNode(parent, Action.MOVE, MCTS.Instance.gameState[i,j]));
					}
				}
			}
		}
		
		//Then I get the possible attacks



		AIGameFlow.ClearGameState(MCTS.Instance.gameState);
		return rList;
	}

	public override AIUnit Copy (GameStateUnit unit) {

		AIWarrior nWar = new AIWarrior();
		nWar.possibleMoves = this.possibleMoves;
		nWar.attackRange = this.attackRange;
		nWar.health = this.health;
		nWar.damage = this.damage;
		nWar.possibleTarget = this.possibleTarget;

		nWar.curgsUnit = unit;

		return nWar;
	}

}
