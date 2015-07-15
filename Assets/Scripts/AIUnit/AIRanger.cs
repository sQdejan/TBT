using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIRanger : AIUnit {

	public override void Attack (GameStateUnit moveTo, GameStateUnit attack) {
		if(moveTo != null)
			Move(moveTo);

		attack.occupier.TakeDamage(damage);
	}

	public override System.Collections.Generic.List<MCTSNode> GetPossibleMoves (MCTSNode parent) {
		List<MCTSNode> rList = new List<MCTSNode>();

		//First I get the possible moves
		for(int i = 0; i < MCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
				if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
					if(Mathf.Abs(i - curgsUnit.h) <= possibleMoves && Mathf.Abs(j - curgsUnit.w) <= possibleMoves) {
						rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, i, j));
					}
				}
			}
		}

		//Then I get the attacks

		//first I check up/down
		int y = curgsUnit.h;
		//up
		while(y < MCTS.Instance.gameState.GetLength(0) - 1) {
			y++;

			if(MCTS.Instance.gameState[y, curgsUnit.w].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, -1, -1, y, curgsUnit.w));
				break;
			} else if (MCTS.Instance.gameState[y, curgsUnit.w].state == curgsUnit.state) {
				break;
			}
		}

		y = curgsUnit.h;
		//down
		while(y > 0) {
			y--;
			
			if(MCTS.Instance.gameState[y, curgsUnit.w].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, -1, -1, y, curgsUnit.w));
				break;
			} else if (MCTS.Instance.gameState[y, curgsUnit.w].state == curgsUnit.state) {
				break;
			}
		}

		int x = curgsUnit.w;
		//right
		while(x < MCTS.Instance.gameState.GetLength(1) - 1) {
			x++;
			
			if(MCTS.Instance.gameState[curgsUnit.h, x].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, -1, -1, curgsUnit.h, x));
				break;
			} else if (MCTS.Instance.gameState[curgsUnit.h, x].state == curgsUnit.state) {
				break;
			}
		}

		x = curgsUnit.w;
		//left
		while(x > 0) {
			x--;
			
			if(MCTS.Instance.gameState[curgsUnit.h, x].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, -1, -1, curgsUnit.h, x));
				break;
			} else if (MCTS.Instance.gameState[curgsUnit.h, x].state == curgsUnit.state) {
				break;
			}
		}

		return rList;
	}

	public override AIUnit Copy (GameStateUnit unit) {
		AIRanger nRan = new AIRanger();
		nRan.possibleMoves = this.possibleMoves;
		nRan.attackRange = this.attackRange;
		nRan.health = this.health;
		nRan.damage = this.damage;
		nRan.possibleTarget = this.possibleTarget;
		
		nRan.curgsUnit = unit;
		
		return nRan;
	}
}
