using UnityEngine;
using System.Collections;

public class UIFlow : MonoBehaviour {

	public GameObject[] groups;
	public GameObject panelWelcome, buttonEndGame, buttonClose, buttonStartGame, buttonInfo, buttonNext, buttonPrev;

	public static bool UIActive = true;

	int curIndex = 0;

	void Start() {
		SetupBox();
	}

	public void GoNextGroup() {
		if(curIndex < groups.Length - 1) {
			groups[curIndex].SetActive(false);
			groups[++curIndex].SetActive(true);
		}

		if(curIndex == groups.Length - 1) {
			buttonNext.SetActive(false);
		} else {
			buttonNext.SetActive(true);
		}

		buttonPrev.SetActive(true);
	}

	public void GoPrevGroup() {
		if(curIndex > 0) {
			groups[curIndex].SetActive(false);
			groups[--curIndex].SetActive(true);
		}

		if(curIndex == 0) {
			buttonPrev.SetActive(false);
		} else {
			buttonPrev.SetActive(true);
		}

		buttonNext.SetActive(true);
	}

	public void OpenInfoBox() {
		panelWelcome.SetActive(true);
		buttonInfo.SetActive(false);
		SetupBox();
		UIActive = true;
	}

	public void CloseInfoBox() {
		buttonInfo.SetActive(true);
		panelWelcome.SetActive(false);
		UIActive = false;
	}

	public void ActivateQuestionnaire() {
		
	}

	public void DeactivateButtonStartGame() {
		buttonStartGame.SetActive(false);
		ActivateButtonCloseAndEndGame();
		CloseInfoBox();
	}

	void ActivateButtonCloseAndEndGame() {
		buttonClose.SetActive(true);
		buttonEndGame.SetActive(true);
	}

	void SetupBox() {
		curIndex = 0;

		groups[curIndex].SetActive(true);

		buttonNext.SetActive(true);
		buttonPrev.SetActive(false);

		for(int i = 1; i < groups.Length; i++) {
			groups[i].SetActive(false);
		}
	}

}
