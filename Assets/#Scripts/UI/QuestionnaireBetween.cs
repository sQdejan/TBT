using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QuestionnaireBetween : MonoBehaviour {

	#region Singleton
	
	private static QuestionnaireBetween instance;
	
	public static QuestionnaireBetween Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<QuestionnaireBetween>();
			}
			
			return instance;
		}
	}
	
	#endregion

	public GameObject[] groups;
	public GameObject[] nextButtons;
	public InputField[] inputFields;

	public GameObject buttonEndGame, buttonInfo, panelInfo;
	public Text textGameOver;


	public ToggleGroup t1, t2;
	
	public Toggle[] t1Group, t2Group;
	
	int curIndex = 0;
	string url = "http://janolesen.org/mid.php?playerid=%0&enjoyment=%1&enjoycomment=%2&skill=%3&skillcomment=%4&general=%5&opponent=%6&outcome=%7&playerhp=%8&aihp=%9";

//	void Start() {
//		ResetQuestionnaire();
//	}
	
	void Update() {
		if(t1.AnyTogglesOn())
			nextButtons[0].SetActive(true);

		if(t2.AnyTogglesOn())
			nextButtons[1].SetActive(true);
	}

	public void ResetQuestionnaire() {

		UIFlow.UIActive = true;

		if(GameFlow.Instance.IsGameOver()) {
			if(GameFlow.gameOutcome == 2)
				textGameOver.text = "Nice, you won! Good job. Please answer the following questions before playing against the next AI opponent.";
			else if(GameFlow.gameOutcome == 1)
				textGameOver.text = "A draw! That's good. Please answer the following questions before playing against the next AI opponent.";
			else
				textGameOver.text = "You can do it next game! Please answer the following questions before playing against the next AI opponent.";
		} else {
			textGameOver.text = "You can do it next game! Please answer the following questions before playing against the next AI opponent.";
		}

		panelInfo.SetActive(false);
		buttonEndGame.SetActive(false);
		buttonInfo.SetActive(false);
	
		curIndex = 0;

		groups[curIndex].SetActive(true);
		inputFields[curIndex].text = "";
		t1.SetAllTogglesOff();

		for(int i = 1; i < groups.Length; i++) {
			groups[i].SetActive(true);
			inputFields[i].text = "";
			t2.SetAllTogglesOff();
			groups[i].SetActive(false);
		}

		nextButtons[0].SetActive(false);
		nextButtons[1].SetActive(false);
	}
	
	public void GoNext() {
		if(curIndex < groups.Length - 1) {
			groups[curIndex].SetActive(false);
			groups[++curIndex].SetActive(true);
		}
	}

	public void GoPrev() {

	}

	public void StartNextGame() {
		URLSender.Instance.SendURL(URL ());
		GameFlow.Instance.DelayRestart();
		UIFlow.UIActive = false;
		buttonEndGame.SetActive(true);
		buttonInfo.SetActive(true);
		gameObject.SetActive(false);
	}
	
	string URL() {
		string tmpUrl;
		
		tmpUrl = url;
		
		tmpUrl = tmpUrl.Replace("%0", GameFlow.playerid.ToString());
		tmpUrl = tmpUrl.Replace("%1", ActiveIndex(t1Group).ToString());
		tmpUrl = tmpUrl.Replace("%2", inputFields[0].text);
		tmpUrl = tmpUrl.Replace("%3", ActiveIndex(t2Group).ToString());
		tmpUrl = tmpUrl.Replace("%4", inputFields[1].text);
		tmpUrl = tmpUrl.Replace("%5", inputFields[2].text);

		if(GameFlow.btAI) {
			if(GameFlow.firstGame) {
				tmpUrl = tmpUrl.Replace("%6", "behaviourAI(first)");
				GameFlow.firstGame = false;
			} else {
				tmpUrl = tmpUrl.Replace("%6", "behaviourAI");
			}
		} else {
			if(GameFlow.firstGame) {
				tmpUrl = tmpUrl.Replace("%6", "MCTSAI(first)");
				GameFlow.firstGame = false;
			} else {
				tmpUrl = tmpUrl.Replace("%6", "MCTS");
			}
		}

		if(GameFlow.Instance.IsGameOver())
			tmpUrl = tmpUrl.Replace("%7", GameFlow.gameOutcome.ToString());
		else
			tmpUrl = tmpUrl.Replace("%7", "-99");

		int aiHP = 0;
		int playerHP = 0;
		
		for(int i = 0; i < GameFlow.Instance.UnitTurnOrderList.Count; i++) {
			int tmpHP = GameFlow.Instance.UnitTurnOrderList[i].GetComponent<Unit>().health;
			
			if(GameFlow.Instance.UnitTurnOrderList[i].tag == "PlayerUnit")
				playerHP += tmpHP;
			else
				aiHP += tmpHP;
		}
		
		tmpUrl = tmpUrl.Replace("%8", playerHP.ToString());
		tmpUrl = tmpUrl.Replace("%9", aiHP.ToString());

		tmpUrl = tmpUrl.Replace(" ", "%20");

		return tmpUrl;
	}

	
	int ActiveIndex(Toggle[] group) {
		for(int i = 0; i < group.Length; i++) {
			if(group[i].isOn)
				return i + 1;
		}
		
		return -1;
	}

}
