using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class AIUnit {

	public int possibleMoves;
	public int attackRange;
	public int health;
	public int damage;

	//If this for instance is a player unit it can attack AI and vice versa
	public char possibleTarget;

	public GameStateUnit curgsUnit;

	#region Methods to be overridden
	
	public abstract void TakeDamage(int damage);
	public abstract void Attack(GameStateUnit moveTo, GameStateUnit attack);
	public abstract List<MCTSNode> GetPossibleMoves(MCTSNode parent);
	public abstract AIUnit Copy(GameStateUnit unit);

	#endregion
	
	public void Move (GameStateUnit to) {
		to.state = curgsUnit.state;
		to.occupier = curgsUnit.occupier;

		curgsUnit.state = AIGameFlow.GS_EMPTY;
		curgsUnit.occupier = null;

		curgsUnit = to;
	}
	
	//Remember to announce somewhere that I died
	protected void Death (int h, int w) {

	}


}
