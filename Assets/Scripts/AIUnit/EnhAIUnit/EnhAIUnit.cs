using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class EnhAIUnit {
	
	public int possibleMovesStraight;
	public int possibleMovesStrafe;
	public int attackRange;
	public int health;
	public int damage;
	public Direction attackDirection;
	public Direction moveDirection;
	
	//If this for instance is a player unit it can attack AI and vice versa
	public char possibleTarget;
	
	public EnhGameStateUnit curgsUnit;
	
	#region Methods to be overridden
	
	public abstract void Attack(EnhGameStateUnit moveTo, EnhGameStateUnit attack);
	public abstract List<MCTSNode> GetPossibleMoves(MCTSNode parent);
	public abstract int GetPossibleMovesDefaultPolicy(List<MCTSNode> defaultList);
	public abstract EnhAIUnit Copy(EnhGameStateUnit unit);
	
	#endregion
	
	public void Move (EnhGameStateUnit to) {
		
		//If it's the same place anyways
		if(to.h == curgsUnit.h && to.w == curgsUnit.w)
			return;
		
		to.state = curgsUnit.state;
		to.occupier = curgsUnit.occupier;
		
		curgsUnit.state = EnhAIGameFlow.GS_EMPTY;
		curgsUnit.occupier = null;
		
		curgsUnit = to;
		
		ChangeDirection();
	}
	
	void ChangeDirection() {
		
		if(curgsUnit.h == EnhMCTS.Instance.gameState.GetLength(0) - 1) {
			moveDirection = Direction.UP;
			attackDirection = Direction.UP;
		} else if (curgsUnit.h == 0) {
			moveDirection = Direction.DOWN;
			attackDirection = Direction.DOWN;
		}
		
	}
	
	public void TakeDamage (int damage) {
		health -= damage;
		
		if(health <= 0) {
			EnhAIGameFlow.Instance.KillUnit(this);
		}
	}
}
