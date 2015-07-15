using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIWarrior : AIUnit {

	public override void Attack (GameStateUnit moveTo, GameStateUnit attack) {
		if(moveTo != null)
			Move(moveTo);

		attack.occupier.TakeDamage(damage);
	}

	public override List<MCTSNode> GetPossibleMoves (MCTSNode parent) {

		List<MCTSNode> rList = new List<MCTSNode>();

		//First I get the possible moves
		for(int i = 0; i < MCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
				if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
					if(Mathf.Abs(i - curgsUnit.h) <= possibleMoves && Mathf.Abs(j - curgsUnit.w) <= possibleMoves) {
						MCTS.Instance.gameState[i,j].state = AIGameFlow.GS_MOVE;
						rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, i, j));
					}
				}
			}
		}

		//Then I get the possible attacks
		for(int i = 0; i < MCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
				if(MCTS.Instance.gameState[i,j].state == possibleTarget) {
					if(Mathf.Abs(i - curgsUnit.h) <= possibleMoves + attackRange && Mathf.Abs(j - curgsUnit.w) <= possibleMoves + attackRange) {

						if(Mathf.Abs(i - curgsUnit.h) <= attackRange && Mathf.Abs(j - curgsUnit.w) <= attackRange) {
							rList.Add(new MCTSNode(parent, Action.ATTACK, -1, -1, i, j));
						} else {
							for(int k = -attackRange; k <= attackRange; k++) {
								int x = i + k;
								
								if(x >= 0 && x < MCTS.Instance.gameState.GetLength(0)) {
									for(int l = -attackRange; l <= attackRange; l++) {
										int y = j + l;
										
										if(y >= 0 && y < MCTS.Instance.gameState.GetLength(1)) {
											if(MCTS.Instance.gameState[x,y].state == AIGameFlow.GS_MOVE) {
												rList.Add(new MCTSNode(parent, Action.ATTACK, x, y, i, j));
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

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
