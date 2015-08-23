using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class QuestionnaireBefore : MonoBehaviour {

	public GameObject buttonNext;
	public GameObject panelWelcome;
	public ToggleGroup t1, t2;

	public Toggle[] t1Group, t2Group;

	string url = "http://janolesen.org/before.php?playerid=%1&level=%2&experience=%3";

	bool buttonOn = false;

	void Update() {
		if(t1.AnyTogglesOn() && t2.AnyTogglesOn() && !buttonOn) {
			ShowNext();
			buttonOn = true;
		}
	}

	public void GoNext() {
		panelWelcome.SetActive(true);
		URLSender.Instance.SendURL(URL());
		gameObject.SetActive(false);
	}

	void ShowNext() {
		buttonNext.SetActive(true);
	}

	string URL() {
		string tmpUrl;

		tmpUrl = url;

		tmpUrl = tmpUrl.Replace("%1", GameFlow.playerid.ToString());
		tmpUrl = tmpUrl.Replace("%2", ActiveIndex(t1Group).ToString());
		tmpUrl = tmpUrl.Replace("%3", ActiveIndex(t2Group).ToString());

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
