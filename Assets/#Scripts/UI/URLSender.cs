using UnityEngine;
using System.Collections;

public class URLSender : MonoBehaviour {

	#region Singleton
	
	private static URLSender instance;
	
	public static URLSender Instance {
		get {
			if(instance == null) {
				instance = GameObject.FindObjectOfType<URLSender>();
			}
			
			return instance;
		}
	}
	
	#endregion

	public GameObject ques;

	string url = "";

	public void SendURL(string url) {
		this.url = url;
		StartCoroutine(SendResults());
	}

	IEnumerator SendResults() {

		WWW www = new WWW(url);
		
		yield return www;
	}

	public void ResetQuiest() {
		StartCoroutine(Test());
	}

	IEnumerator Test () {
		yield return new WaitForSeconds(3);
		ques.SetActive(true);
		QuestionnaireBetween.Instance.ResetQuestionnaire();
	}
}
