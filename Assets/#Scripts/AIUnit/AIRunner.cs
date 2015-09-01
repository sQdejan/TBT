using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIRunner : AIUnit {

	public override void Attack (GameStateUnit moveTo, GameStateUnit attack)
	{
		Move(moveTo);
		
		if(attack.state == AIGameFlow.GS_EMPTY) {
			AIGameFlow.PrintGameState(MCTS.Instance.gameState);
			Debug.Log("It's empty and shouldn't be - is the occupier null as well? " + (attack.occupier == null));
			Debug.Log("I am myself at state " + curgsUnit.state + " with h, w " + curgsUnit.h + ", " + curgsUnit.w);
		}
		
		attack.occupier.TakeDamage(damage);
	}

	public override List<MCTSNode> GetPossibleMoves (MCTSNode parent)
	{
		List<MCTSNode> rList = new List<MCTSNode>();
		
		//Just add the one it's standing on as it is also a viable move but not empty
		rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, curgsUnit.h, curgsUnit.w));

		int y = curgsUnit.h;
		int x = curgsUnit.w;

		//Check up-left
		CheckMoveAndAttack(y, x, -1, -1, rList, parent);
		//Check up-right
		CheckMoveAndAttack(y, x, -1, 1, rList, parent);
		//Check down-left
		CheckMoveAndAttack(y, x, 1, -1, rList, parent);
		//Check down-right
		CheckMoveAndAttack(y, x, 1, 1, rList, parent);

		return rList;
	
	}

	void CheckMoveAndAttack(int y, int x, int yDirection, int xDirection, List<MCTSNode> nodeList, MCTSNode parent) {
		
		y += yDirection;
		x += xDirection;
		
		while(y >= 0 && y < MCTS.Instance.gameState.GetLength(0) && x >= 0 && x <  MCTS.Instance.gameState.GetLength(1)) {
			
			if(MCTS.Instance.gameState[y,x].state == AIGameFlow.GS_EMPTY) {
				nodeList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, y, x));
			} else if (MCTS.Instance.gameState[y,x].state == possibleTarget) {
				nodeList.Add(new MCTSNode(parent, Action.ATTACK, y - yDirection, x - xDirection, y, x));
				break;
			} else {
				break;
			}
			
			y += yDirection;
			x += xDirection;
		}
	}

	public override int GetPossibleMovesDefaultPolicy (List<MCTSNode> defaultList)
	{
		int index = 0;
		
		defaultList[index].action = Action.MOVE;
		defaultList[index].gsH = curgsUnit.h;
		defaultList[index].gsW = curgsUnit.w;

		int y = curgsUnit.h;
		int x = curgsUnit.w;

		//Check up-left
		CheckMoveAndAttackDefault(y, x, -1, -1, defaultList, ref index);
		//Check up-right
		CheckMoveAndAttackDefault(y, x, -1, 1, defaultList, ref index);
		//Check down-left
		CheckMoveAndAttackDefault(y, x, 1, -1, defaultList, ref index);
		//Check down-right
		CheckMoveAndAttackDefault(y, x, 1, 1, defaultList, ref index);

		return index;
	}

	void CheckMoveAndAttackDefault(int y, int x, int yDirection, int xDirection, List<MCTSNode> defaultList, ref int index) {
		
		y += yDirection;
		x += xDirection;
		
		while(y >= 0 && y < MCTS.Instance.gameState.GetLength(0) && x >= 0 && x <  MCTS.Instance.gameState.GetLength(1)) {
			
			if(MCTS.Instance.gameState[y,x].state == AIGameFlow.GS_EMPTY) {
				index++;
				
				defaultList[index].action = Action.MOVE;
				defaultList[index].gsH = y;
				defaultList[index].gsW = x;
			} else if (MCTS.Instance.gameState[y,x].state == possibleTarget) {
				index++;
				
				defaultList[index].action = Action.ATTACK;
				defaultList[index].mbagsH = y - yDirection;
				defaultList[index].mbagsW = x - xDirection;
				defaultList[index].gsH = y;
				defaultList[index].gsW = x;
				break;
			} else {
				break;
			}
			
			y += yDirection;
			x += xDirection;
		}
	}

	public override AIUnit Copy (GameStateUnit unit)
	{
		AIRunner nRun = new AIRunner();
		nRun.possibleMovesStraight = this.possibleMovesStraight;
		nRun.possibleMovesStrafe = this.possibleMovesStrafe;
		nRun.attackRange = this.attackRange;
		nRun.health = this.health;
		nRun.damage = this.damage;
		nRun.possibleTarget = this.possibleTarget;
		nRun.attackDirection = this.attackDirection;
		nRun.moveDirection = this.moveDirection;
		
		nRun.curgsUnit = unit;
		
		return nRun;;
	}
}
