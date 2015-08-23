using UnityEngine;
using System.Collections;

public class MakeSure : MonoBehaviour {

	public GameObject buttonChoice, buttonInfo, panelInfo;

	public void AreYouSure() {
		buttonInfo.SetActive(false);
		buttonChoice.SetActive(true);
		panelInfo.SetActive(false);
		UIFlow.UIActive = true;
	}

	public void No() {
		UIFlow.UIActive = false;
		buttonInfo.SetActive(true);
		buttonChoice.SetActive(false);
	}

	public void Yes() {
		buttonChoice.SetActive(false);
		GameFlow.Instance.ButtonQuestionnaire();
	}
}
