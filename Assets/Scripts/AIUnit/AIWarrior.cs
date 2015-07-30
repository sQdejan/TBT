using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIWarrior : AIUnit {

	public override void Attack (GameStateUnit moveTo, GameStateUnit attack) {

		Move(moveTo);

		if(attack.state == AIGameFlow.GS_EMPTY) {
			AIGameFlow.PrintGameState(MCTS.Instance.gameState);
			Debug.Log("It's empty and shouldn't be - is the occupier null as well? " + (attack.occupier == null));
			Debug.Log("I am myself at state " + curgsUnit.state + " with h, w " + curgsUnit.h + ", " + curgsUnit.w);
		}

		attack.occupier.TakeDamage(damage);
	}

	public override List<MCTSNode> GetPossibleMoves (MCTSNode parent) {

		List<MCTSNode> rList = new List<MCTSNode>();

		//Just add the one it's standing on as it is also a viable move but not empty
		rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, curgsUnit.h, curgsUnit.w));

		//First I get the possible moves
		if(moveDirection == Direction.DOWN) {
			for(int i = curgsUnit.h; i < MCTS.Instance.gameState.GetLength(0); i++) {
				for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
					if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
						if(Mathf.Abs(i - curgsUnit.h) <= possibleMovesStraight && Mathf.Abs(j - curgsUnit.w) <= possibleMovesStrafe) {
							MCTS.Instance.gameState[i,j].state = AIGameFlow.GS_MOVE;
							rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, i, j));
						}
					}
				}
			}
		} else {
			for(int i = curgsUnit.h; i >= 0; i--) {
				for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
					if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
						if(Mathf.Abs(i - curgsUnit.h) <= possibleMovesStraight && Mathf.Abs(j - curgsUnit.w) <= possibleMovesStrafe) {
							MCTS.Instance.gameState[i,j].state = AIGameFlow.GS_MOVE;
							rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, i, j));
						}
					}
				}
			}
		}

		//Then I get the possible attacks
		int checkDirection = 1;
		
		if(attackDirection == Direction.DOWN)
			checkDirection = -1;

		for(int i = 0; i < MCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
				if(MCTS.Instance.gameState[i,j].state == possibleTarget) {
					int y = i + checkDirection;
					
					if(y >= 0 && y < MCTS.Instance.gameState.GetLength(0)) {
						for(int k = -attackRange; k <= attackRange; k++) {
							int x = j + k;
							if(x >= 0 && x < MCTS.Instance.gameState.GetLength(1)) {
								if(MCTS.Instance.gameState[y,x].state == AIGameFlow.GS_MOVE || MCTS.Instance.gameState[y,x].Equals(curgsUnit)) {
									rList.Add(new MCTSNode(parent, Action.ATTACK, y, x, i, j));
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

	//This function is used to avoid the "new" keyword all the time which
	//slows down the above function A LOT!
	public override int GetPossibleMovesDefaultPolicy (List<MCTSNode> defaultList) {

		int index = 0;

		defaultList[index].action = Action.MOVE;
		defaultList[index].gsH = curgsUnit.h;
		defaultList[index].gsW = curgsUnit.w;

		//First I get the possible moves
		if(moveDirection == Direction.DOWN) {
			for(int i = curgsUnit.h; i < MCTS.Instance.gameState.GetLength(0); i++) {
				for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
					if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
						if(Mathf.Abs(i - curgsUnit.h) <= possibleMovesStraight && Mathf.Abs(j - curgsUnit.w) <= possibleMovesStrafe) {
							MCTS.Instance.gameState[i,j].state = AIGameFlow.GS_MOVE;
							
							index++;
							
							defaultList[index].action = Action.MOVE;
							defaultList[index].gsH = i;
							defaultList[index].gsW = j;
						}
					}
				}
			}
		} else {
			for(int i = curgsUnit.h; i >= 0; i--) {
				for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
					if(MCTS.Instance.gameState[i,j].state == AIGameFlow.GS_EMPTY) {
						if(Mathf.Abs(i - curgsUnit.h) <= possibleMovesStraight && Mathf.Abs(j - curgsUnit.w) <= possibleMovesStrafe) {
							MCTS.Instance.gameState[i,j].state = AIGameFlow.GS_MOVE;
							
							index++;
							
							defaultList[index].action = Action.MOVE;
							defaultList[index].gsH = i;
							defaultList[index].gsW = j;
						}
					}
				}
			}
		}

		int checkDirection = 1;
		
		if(attackDirection == Direction.DOWN)
			checkDirection = -1;

		//Then I get the possible attacks
		for(int i = 0; i < MCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < MCTS.Instance.gameState.GetLength(1); j++) {
				if(MCTS.Instance.gameState[i,j].state == possibleTarget) {
					int y = i + checkDirection;
					
					if(y >= 0 && y < MCTS.Instance.gameState.GetLength(0)) {
						for(int k = -attackRange; k <= attackRange; k++) {
							int x = j + k;
							if(x >= 0 && x < MCTS.Instance.gameState.GetLength(1)) {
								if(MCTS.Instance.gameState[y,x].state == AIGameFlow.GS_MOVE || MCTS.Instance.gameState[y,x].Equals(curgsUnit)) {
									index++;
									
									defaultList[index].action = Action.ATTACK;
									defaultList[index].mbagsH = y;
									defaultList[index].mbagsW = x;
									defaultList[index].gsH = i;
									defaultList[index].gsW = j;
								}
							}
						}
					}
				}
			}
		}

		AIGameFlow.ClearGameState(MCTS.Instance.gameState);
		return index;
	}

	public override AIUnit Copy (GameStateUnit unit) {

		AIWarrior nWar = new AIWarrior();
		nWar.possibleMovesStraight = this.possibleMovesStraight;
		nWar.possibleMovesStrafe = this.possibleMovesStrafe;
		nWar.attackRange = this.attackRange;
		nWar.health = this.health;
		nWar.damage = this.damage;
		nWar.possibleTarget = this.possibleTarget;
		nWar.attackDirection = this.attackDirection;
		nWar.moveDirection = this.moveDirection;

		nWar.curgsUnit = unit;

		return nWar;
	}

}
