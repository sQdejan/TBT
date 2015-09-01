using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NNAIRanger : NNAIUnit {
	
	public override void Attack (NNGameStateUnit moveTo, NNGameStateUnit attack) {
		
		attack.occupier.TakeDamage(damage);
	}
	
	public override List<MCTSNode> GetPossibleMoves (MCTSNode parent) {
		List<MCTSNode> rList = new List<MCTSNode>();
		
		rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, curgsUnit.h, curgsUnit.w));
		
		//First I get the possible moves
		for(int i = 0; i < NNMCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < NNMCTS.Instance.gameState.GetLength(1); j++) {
				if(NNMCTS.Instance.gameState[i,j].state == NNAIGameFlow.GS_EMPTY) {
					if(Mathf.Abs(i - curgsUnit.h) <= possibleMovesStraight && Mathf.Abs(j - curgsUnit.w) <= possibleMovesStrafe) {
						rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, i, j));
					}
				}
			}
		}
		
		
		//		int yM = curgsUnit.h - possibleMovesStraight;
		//		
		//		while(yM <= curgsUnit.h + possibleMovesStraight) {
		//			if(yM >= 0 && yM < NNMCTS.Instance.gameState.GetLength(0)) {
		//				if(NNMCTS.Instance.gameState[yM,curgsUnit.w].state == NNAIGameFlow.GS_EMPTY) {
		//					rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, yM, curgsUnit.w));
		//				}
		//			}
		//			
		//			yM++;
		//		}
		//		
		//		int xM = curgsUnit.w - possibleMovesStrafe;
		//		
		//		while(xM <= curgsUnit.w + possibleMovesStraight) {
		//			if(xM >= 0 && xM < NNMCTS.Instance.gameState.GetLength(0)) {
		//				if(NNMCTS.Instance.gameState[curgsUnit.h,xM].state == NNAIGameFlow.GS_EMPTY) {
		//					rList.Add(new MCTSNode(parent, Action.MOVE, -1, -1, curgsUnit.h, xM));
		//				}
		//			}
		//			
		//			xM++;
		//		}
		
		//Then I get the attacks
		
		//first I check up/down
		int y = curgsUnit.h;
		//up
		while(++y < NNMCTS.Instance.gameState.GetLength(0)) {
			
			if(NNMCTS.Instance.gameState[y, curgsUnit.w].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, 1, 1, y, curgsUnit.w));
				break;
			} else if (NNMCTS.Instance.gameState[y, curgsUnit.w].state == curgsUnit.state) {
				break;
			}
		}
		
		y = curgsUnit.h;
		//down
		while(--y >= 0) {
			
			if(NNMCTS.Instance.gameState[y, curgsUnit.w].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, 1, 1, y, curgsUnit.w));
				break;
			} else if (NNMCTS.Instance.gameState[y, curgsUnit.w].state == curgsUnit.state) {
				break;
			}
		}
		
		int x = curgsUnit.w;
		//right
		while(++x < NNMCTS.Instance.gameState.GetLength(1)) {
			
			if(NNMCTS.Instance.gameState[curgsUnit.h, x].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, 1, 1, curgsUnit.h, x));
				break;
			} else if (NNMCTS.Instance.gameState[curgsUnit.h, x].state == curgsUnit.state) {
				break;
			}
		}
		
		x = curgsUnit.w;
		//left
		while(--x >= 0) {
			
			if(NNMCTS.Instance.gameState[curgsUnit.h, x].state == possibleTarget) {
				rList.Add(new MCTSNode(parent, Action.ATTACK, 1, 1, curgsUnit.h, x));
				break;
			} else if (NNMCTS.Instance.gameState[curgsUnit.h, x].state == curgsUnit.state) {
				break;
			}
		}
		
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
		for(int i = 0; i < NNMCTS.Instance.gameState.GetLength(0); i++) {
			for(int j = 0; j < NNMCTS.Instance.gameState.GetLength(1); j++) {
				if(NNMCTS.Instance.gameState[i,j].state == NNAIGameFlow.GS_EMPTY) {
					if(Mathf.Abs(i - curgsUnit.h) <= possibleMovesStraight && Mathf.Abs(j - curgsUnit.w) <= possibleMovesStrafe) {
						index++;
						
						defaultList[index].action = Action.MOVE;
						defaultList[index].gsH = i;
						defaultList[index].gsW = j;
					}
				}
			}
		}
		
		//		int yM = curgsUnit.h - possibleMovesStraight;
		//		
		//		while(yM <= curgsUnit.h + possibleMovesStraight) {
		//			if(yM >= 0 && yM < NNMCTS.Instance.gameState.GetLength(0)) {
		//				if(NNMCTS.Instance.gameState[yM,curgsUnit.w].state == NNAIGameFlow.GS_EMPTY) {
		//					index++;
		//					
		//					defaultList[index].action = Action.MOVE;
		//					defaultList[index].gsH = yM;
		//					defaultList[index].gsW = curgsUnit.w;
		//				}
		//			}
		//			
		//			yM++;
		//		}
		//		
		//		int xM = curgsUnit.w - possibleMovesStrafe;
		//		
		//		while(xM <= curgsUnit.w + possibleMovesStraight) {
		//			if(xM >= 0 && xM < NNMCTS.Instance.gameState.GetLength(0)) {
		//				if(NNMCTS.Instance.gameState[curgsUnit.h,xM].state == NNAIGameFlow.GS_EMPTY) {
		//					index++;
		//					
		//					defaultList[index].action = Action.MOVE;
		//					defaultList[index].gsH = curgsUnit.h;
		//					defaultList[index].gsW = xM;
		//				}
		//			}
		//			
		//			xM++;
		//		}
		
		//Then I get the attacks
		
		//first I check up/down
		int y = curgsUnit.h;
		//up
		while(++y < NNMCTS.Instance.gameState.GetLength(0)) {
			if(NNMCTS.Instance.gameState[y, curgsUnit.w].state == possibleTarget) {
				
				index++;
				
				defaultList[index].action = Action.ATTACK;
				defaultList[index].mbagsH = 1;
				defaultList[index].mbagsW = 1;
				defaultList[index].gsH = y;
				defaultList[index].gsW = curgsUnit.w;
				
				break;
			} else if (NNMCTS.Instance.gameState[y, curgsUnit.w].state == curgsUnit.state) {
				break;
			}
		}
		
		y = curgsUnit.h;
		//down
		while(--y >= 0) {
			
			if(NNMCTS.Instance.gameState[y, curgsUnit.w].state == possibleTarget) {
				
				index++;
				
				defaultList[index].action = Action.ATTACK;
				defaultList[index].mbagsH = 1;
				defaultList[index].mbagsW = 1;
				defaultList[index].gsH = y;
				defaultList[index].gsW = curgsUnit.w;
				
				break;
			} else if (NNMCTS.Instance.gameState[y, curgsUnit.w].state == curgsUnit.state) {
				break;
			}
		}
		
		int x = curgsUnit.w;
		//right
		while(++x < NNMCTS.Instance.gameState.GetLength(1)) {
			
			if(NNMCTS.Instance.gameState[curgsUnit.h, x].state == possibleTarget) {
				
				index++;
				
				defaultList[index].action = Action.ATTACK;
				defaultList[index].mbagsH = 1;
				defaultList[index].mbagsW = 1;
				defaultList[index].gsH = curgsUnit.h;
				defaultList[index].gsW = x;
				
				break;
			} else if (NNMCTS.Instance.gameState[curgsUnit.h, x].state == curgsUnit.state) {
				break;
			}
		}
		
		x = curgsUnit.w;
		//left
		while(--x >= 0) {
			
			if(NNMCTS.Instance.gameState[curgsUnit.h, x].state == possibleTarget) {
				
				index++;
				
				defaultList[index].action = Action.ATTACK;
				defaultList[index].mbagsH = 1;
				defaultList[index].mbagsW = 1;
				defaultList[index].gsH = curgsUnit.h;
				defaultList[index].gsW = x;
				
				break;
			} else if (NNMCTS.Instance.gameState[curgsUnit.h, x].state == curgsUnit.state) {
				break;
			}
		}
		
		return index;
	}
	
	public override NNAIUnit Copy (NNGameStateUnit unit) {
		NNAIRanger nRan = new NNAIRanger();
		nRan.possibleMovesStraight = this.possibleMovesStraight;
		nRan.possibleMovesStrafe = this.possibleMovesStrafe;
		nRan.attackRange = this.attackRange;
		nRan.health = this.health;
		nRan.damage = this.damage;
		nRan.possibleTarget = this.possibleTarget;
		nRan.attackDirection = this.attackDirection;
		nRan.moveDirection = this.moveDirection;
		
		nRan.curgsUnit = unit;
		
		return nRan;
	}
}
